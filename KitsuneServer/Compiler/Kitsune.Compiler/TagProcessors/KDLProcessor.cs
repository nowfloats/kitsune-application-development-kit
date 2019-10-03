using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Kitsune.Compiler.Helpers;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using Kitsune.SyntaxParser;

namespace Kitsune.Compiler.TagProcessors
{
    public class KDLProcessor : Processor
    {
        /// <summary>
        /// Process nodes with KDL tag.
        /// </summary>
        /// <param name="request">Incoming request</param>
        /// <param name="compileErrors">list of errors during compilation</param>
        /// <param name="kentity">project language</param>
        /// <param name="customVariables">output list to hold custom variables</param>
        /// <param name="rootUrl">root url</param>
        /// <param name="filePath">file path</param>
        /// <param name="classNameAlias">aliases used either with k-repeat or k-object</param>
        /// <param name="classNameAliasdepth">depth based storage of aliases</param>
        /// <param name="depth">depth of the node in DOM</param>
        /// <param name="dynamicAttribute">key-value pair of the k-tag</param>
        public override void ProcessNode(CompileResourceRequest request, List<CompilerError> compileErrors, Dictionary<string, int> customVariables, string rootUrl, string filePath, Dictionary<string, string> classNameAlias, Dictionary<int, string> classNameAliasdepth, int depth, HtmlNode node, HtmlAttribute dynamicAttribute, List<MatchNode> objectNamesToValidate, DocumentValidator documentValidator)
        {
            var pattern = dynamicAttribute.Value;
            if (!string.IsNullOrWhiteSpace(pattern))
            {
                pattern = pattern.Trim().Trim('/');
                if (pattern.IndexOf("currentpagenumber.urlencode()") < 0)
                {
                    pattern = pattern.Replace("currentpagenumber", "currentpagenumber.urlencode()");
                }

                KEntity kentity = documentValidator.GetKEntityFromEntityName(documentValidator.defaultEntity);

                #region Validate unique id exist for detailspage
                if (request.PageType == Models.Project.KitsunePageType.DETAILS)
                {
                    //added support for multiple kobjects, check for the last objetcts unique param
                    var kobjects = request.KObject.Split(':')[0].Split(',');
                    var kobjParam = kobjects[kobjects.Length-1].ToLower();
                    var kidRegex = Kitsune.Helper.Constants.GetKObjectIteratorRegex($"{kobjParam}._kid");
                    var _idRegex = Kitsune.Helper.Constants.GetKObjectIteratorRegex($"{kobjParam}._id");
                    var indexRegex = Kitsune.Helper.Constants.GetKObjectIteratorRegex($"{kobjParam}.index");
                    var idRegex = Kitsune.Helper.Constants.GetKObjectIteratorRegex($"{kobjParam}.id");
                    if (!(_idRegex.IsMatch(pattern.ToLower()) || kidRegex.IsMatch(pattern.ToLower()) || indexRegex.IsMatch(pattern.ToLower()) || idRegex.IsMatch(pattern.ToLower())))
                    {
                        compileErrors.Add(new CompilerError()
                        {
                            LineNumber = dynamicAttribute.Line,
                            LinePosition = dynamicAttribute.LinePosition,
                            Message = string.Format(ErrorCodeConstants.MissingUniqueIndexForDetailsPage, request.KObject.Split(':').Length > 1 ? request.KObject.Split(':')[1] : kobjParam)
                        });
                    }
                }
                #endregion

                var viewExist = KitsuneCompiler.ValidateView(ref pattern, kentity);
                if (viewExist != null)
                {
                    viewExist.LineNumber = dynamicAttribute.Line;
                    viewExist.LinePosition = dynamicAttribute.LinePosition;
                    compileErrors.Add(viewExist);
                }
                var pagination = Kitsune.Helper.Constants.PaginationRegularExpression.Match(pattern);

                if (pagination != null && pagination.Success)
                {
                    var matches = Kitsune.Helper.Constants.FunctionParameterExpression.Match(pagination.Value);
                    if (matches != null && matches.Groups?.Count > 0)
                        request.UrlPattern = string.Format("[[{0}]]/{1}-[[{2}.currentpagenumber]]", rootUrl, matches.Groups[1].Value.Substring(matches.Groups[1].Value.IndexOf('.') + 1), request.ClassName);
                }
                else
                {
                    var expressions = Helper.HtmlHelper.GetExpressionFromElement(pattern, 0);
                    //Add urlencode 
                    if (expressions != null && expressions.Any())
                    {
                        var tmp = "";
                        var tmpValue = "";
                        var groupCout = 0;
                        foreach (var exp in expressions)
                        {
                            groupCout++;
                            tmp = exp.Value.Trim('[', ']');
                            tmpValue = exp.Value;
                            if (!tmp.ToLower().EndsWith(".urlencode()"))
                                pattern = pattern.Replace(tmpValue, exp.Value.Replace(tmp, $"{tmp}.urlencode()"));

                            var objects = Parser.GetObjects(tmp);
                            if (objects != null && objects.Any())
                            {
                                foreach (var custObj in objects)
                                {
                                    if (custObj.Split('.').Length == 1)
                                    {
                                        IList<KClass> allClasses = new List<KClass>();
                                        if (documentValidator != null && documentValidator.entities != null && documentValidator.entities.Keys.Any(x => x == custObj.Trim('[', ']').ToLower()))
                                        {
                                            allClasses = documentValidator.entities[custObj.Trim('[', ']').ToLower()].Classes;
                                        }
                                        if (!allClasses.Any(x => x.ClassType == KClassType.BaseClass && x.Name.ToLower() == custObj.Trim('[', ']')))
                                        {
                                            if (!customVariables.ContainsKey(custObj.Trim('[', ']')?.ToLower()))
                                                customVariables.Add(custObj.Trim('[', ']')?.ToLower(), groupCout);
                                        }
                                    }
                                }
                            }

                        }
                    }

                    request.UrlPattern = string.Format("[[{0}]]/{1}", rootUrl, pattern);
                    if (pattern != null && pattern.ToLower().IndexOf(@"search/") != -1)
                        request.PageType = Models.Project.KitsunePageType.SEARCH;
                }
                //req.UrlPattern = pattern.Replace(kdl, req.UrlPattern);
                dynamicAttribute.Value = request.UrlPattern;
            }
            else if (filePath.ToLower().EndsWith(".dl"))
                compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.NoDLTagInDLPage));
            else if (string.IsNullOrEmpty(request.UrlPattern))
                request.UrlPattern = string.Format("[[{0}]]/{1}", rootUrl, filePath.ToLower());
        }
    }
}
