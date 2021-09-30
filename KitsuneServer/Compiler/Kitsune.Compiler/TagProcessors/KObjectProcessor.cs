using Kitsune.AntlrLibrary;
using Kitsune.AntlrLibrary.Model;
using HtmlAgilityPack;
using Kitsune.Compiler.Helpers;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kitsune.Compiler.TagProcessors
{
    public class KObjectProcessor : Processor
    {
        /// <summary>
        /// Process nodes with k-object tags.
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
            Node result = LexerGenerator.Parse(dynamicAttribute.Value.Trim('[', ']'));
            if (result?.Children?.Count == 3)
            {
                string referenceNames = result.Token.Value == ACTIONS.KObject ? result.Children[0].Token.Value : result.Children[0].Children[0].Token.Value.ToLower();
                string referenceObject = result.Children[2].Children[0].Token.Value.ToLower();
                var referenceKObjectArray = referenceNames.Split(',');

                KEntity kentity = documentValidator.GetKEntityFromEntityName(documentValidator.defaultEntity);
                var referenceObjectWithIterator = UpdateIterator(referenceObject, documentValidator);

                var tempRefObj = referenceObjectWithIterator.Trim();
                for (int i = referenceKObjectArray.Length - 1, j = 0; i >= 0; i--, j++)
                {
                    KEntity commEntity = documentValidator.GetKEntityFromEntityName(referenceKObjectArray[i]);
                    if (commEntity != null || (kentity != null && kentity.Classes.Where(x => x.Name?.ToLower() == referenceObject?.Split('.')?[0] && x.ClassType == KClassType.BaseClass).Count() == 0))
                    {
                        compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.BaseClassAsKObjectVariable, dynamicAttribute.Line, dynamicAttribute.LinePosition));
                        return;
                    }
                    else
                    {
                        if (commEntity != null)
                        {
                            kentity = commEntity;
                        }
                        if (j > 0)
                            tempRefObj = tempRefObj.Substring(0, tempRefObj.LastIndexOf('.'));
                        KProperty property = GetProperty(compileErrors, dynamicAttribute, documentValidator, tempRefObj, classNameAlias);


                        if (property == null)
                        {
                            compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.InvalidKObjectClass, dynamicAttribute.Line, dynamicAttribute.LinePosition));

                            return;
                        }
                        else if (property.Type == PropertyType.array)
                        {
                            classNameAlias.Add(referenceKObjectArray[i], tempRefObj);
                            objectNamesToValidate.Add(new MatchNode { Value = tempRefObj, Line = dynamicAttribute.Line, Column = dynamicAttribute.LinePosition });
                        }
                        else
                        {
                            classNameAlias.Add(referenceKObjectArray[i], tempRefObj);
                            objectNamesToValidate.Add(new MatchNode { Value = tempRefObj, Line = dynamicAttribute.Line, Column = dynamicAttribute.LinePosition });
                        }
                    }
                }
                request.PageType = Models.Project.KitsunePageType.DETAILS;
                var searchableProperty = IsSearchableProperty(referenceObject, out KClass kobjClass, documentValidator);

                if (referenceObject.StartsWith("webactions.") || kobjClass != null)
                    request.KObject = referenceNames.Trim() + ":" + referenceObject;
                else
                    request.KObject = referenceNames.Trim();


            }
            else
            {
                compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.InvalidKObjectSyntax, dynamicAttribute.Line, dynamicAttribute.LinePosition));
            }
            dynamicAttribute.Value = "";
        }
        private static string UpdateIterator(string referenceObject, DocumentValidator documentValidator)
        {
            var properties = referenceObject.Split('.');
            var kobjClass = new KClass();
            var result = properties[0];
            var searchableProperty = properties[0];
            for (var i = 0; i < properties.Length; i++)
            {
                if (IsSearchableProperty(searchableProperty, out kobjClass, documentValidator))
                {
                    result += "[k_obj_" + i + "]";
                }
                if(i < properties.Length - 1)
                {
                    searchableProperty += "." + properties[i + 1];
                    result += ("." + properties[i + 1]);
                }
            }
            return result;
        }
    }
}
