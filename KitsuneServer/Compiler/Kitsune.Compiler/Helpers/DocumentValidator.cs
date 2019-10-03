using HtmlAgilityPack;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Compiler.TagProcessors;
using Kitsune.Helper;
using Kitsune.Language.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using Kitsune.SyntaxParser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kitsune.Compiler.Helpers
{
    public class DocumentValidator
    {
        internal string[] ValidObjects = { "null", "___kit_csrf_token___", "previouspage", "nextpage", "currentpagenumber", "offset", "true", "false", "_system" };
        internal Dictionary<string, KEntity> entities = new Dictionary<string, KEntity>();
        internal String defaultEntity;

        /// <summary>
        /// Validate k-tags and expressions in the document.
        /// </summary>
        /// <param name="document">Incoming document</param>
        /// <param name="request">Incoming request</param>
        /// <param name="compileErrors">list of errors during compile</param>
        /// <param name="kentity">project language</param>
        /// <param name="customVariables">custom variables found during compilation</param>
        /// <param name="rootUrl">root url</param>
        /// <param name="filePath">file path</param>
        public void ValidateDocument(HtmlDocument document, CompileResourceRequest request, List<CompilerError> compileErrors, KEntity kentity, Dictionary<string, int> customVariables, string rootUrl, string filePath, GetProjectDetailsResponseModel projectDetails, List<KEntity> componentsEntity = null)
        {
            Dictionary<string, bool> objectNamesValidated = new Dictionary<string, bool>();
            Dictionary<string, string> classNameAlias = new Dictionary<string, string>();
            Dictionary<int, string> classNameAliasLevel = new Dictionary<int, string>();
            List<MatchNode> objectNamesToValidate = new List<MatchNode>();

            entities.Add(kentity.EntityName, kentity);
            defaultEntity = kentity.EntityName;
            if (componentsEntity != null && componentsEntity.Any())
            {
                foreach (var comEntity in componentsEntity)
                {
                    if (!entities.Keys.Contains(comEntity.EntityName))
                    {
                        entities.Add(comEntity.EntityName, comEntity);
                    }
                }
            }

            var baseClassList = entities.SelectMany(x => x.Value?.Classes?.Where(y => y.ClassType == KClassType.BaseClass)?.ToList());

            HtmlNode node = document.DocumentNode;
            List<string> dynamicTagDescriptors = GetDynamicTagDescriptors(typeof(LanguageAttributes));
            int level = 0;
            while (node != null)
            {
                if (node.NodeType == HtmlNodeType.Element)
                {
                    var dynamicAttributes = node.Attributes.Where(x => dynamicTagDescriptors.Contains(x.Name.ToLower()));
                    for (int i = 0; i < dynamicAttributes.Count(); i++)
                    {
                        if (!string.IsNullOrEmpty(dynamicAttributes.ElementAt(i).Value))
                        {
                            Processor processor = ProcessorFactory.GetProcessor(dynamicAttributes.ElementAt(i).Name);
                            processor.ProcessNode(request, compileErrors, customVariables, rootUrl, filePath, classNameAlias, classNameAliasLevel, level, node, dynamicAttributes.ElementAt(i), objectNamesToValidate, this);
                        }
                        //k-partial k-norepeat attribute can be empty
                        else if(dynamicAttributes.ElementAt(i).Name?.ToLower() != LanguageAttributes.KPartial.GetDescription()?.ToLower()
                            && dynamicAttributes.ElementAt(i).Name?.ToLower() != LanguageAttributes.KNoRepeat.GetDescription()?.ToLower())
                        {
                            compileErrors.Add(CompileResultHelper.GetCompileError(string.Format(ErrorCodeConstants.InvalidKitsuneTagValue,
                                dynamicAttributes.ElementAt(i).Name,
                                dynamicAttributes.ElementAt(i).Line, dynamicAttributes.ElementAt(i).LinePosition)));
                        }
                    }
                    if (node.Name.Equals(LanguageAttributes.KScript.GetDescription(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        Processor processor = ProcessorFactory.GetProcessor(node.Name.ToLower());
                        processor.ProcessNode(request, compileErrors, customVariables, rootUrl, filePath, classNameAlias, classNameAliasLevel, level, node, null, objectNamesToValidate, this);
                    }
                }

                if (node.HasChildNodes)
                {
                    node = node.FirstChild;
                    level++;
                }
                else
                {
                    ValidateExpressions(compileErrors, entities, objectNamesValidated, classNameAlias, baseClassList, node, objectNamesToValidate, customVariables, projectDetails);
                    if (node.NextSibling != null)
                    {
                        node = node.NextSibling;
                        ClearNameAliases(classNameAlias, classNameAliasLevel, level);
                    }
                    else
                    {
                        while (node.ParentNode != null && node.ParentNode.NextSibling == null)
                        {
                            node = node.ParentNode;
                            ValidateExpressions(compileErrors, entities, objectNamesValidated, classNameAlias, baseClassList, node, objectNamesToValidate, customVariables, projectDetails);
                            level--;
                            ClearNameAliases(classNameAlias, classNameAliasLevel, level);
                        }
                        node = node?.ParentNode;
                        if (node != null)
                        {
                            ValidateExpressions(compileErrors, entities, objectNamesValidated, classNameAlias, baseClassList, node, objectNamesToValidate, customVariables, projectDetails);
                        }
                        node = node?.NextSibling;
                        level--;
                        ClearNameAliases(classNameAlias, classNameAliasLevel, level);
                    }
                }
            }
        }

        /// <summary>
        /// Get descriptors of enum values.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<string> GetDynamicTagDescriptors(Type type)
        {
            var descs = new List<string>();
            var names = Enum.GetNames(type);
            foreach (var name in names)
            {
                var field = type.GetField(name);
                var fds = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                foreach (DescriptionAttribute fd in fds)
                {
                    descs.Add(fd.Description.ToLower());
                }
            }
            return descs;
        }

        /// <summary>
        /// Validate objects used in expressions are valid.
        /// </summary>
        /// <param name="compileErrors"></param>
        /// <param name="kentity"></param>
        /// <param name="objectNamesValidated"></param>
        /// <param name="classNameAlias"></param>
        /// <param name="baseClassList"></param>
        /// <param name="node"></param>
        private void ValidateExpressions(List<CompilerError> compileErrors, Dictionary<string, KEntity> entities, Dictionary<string, bool> objectNamesValidated, Dictionary<string, string> classNameAlias, IEnumerable<KClass> baseClassList, HtmlNode node, List<MatchNode> objectNamesToValidate, Dictionary<string, int> customVariables, GetProjectDetailsResponseModel projectDetails)
        {
            if (node.NodeType != HtmlNodeType.Element)
            {
                return;
            }
            int lineNo = 0;
            CompilerError viewExist = null;
            CompilerError partialViewExist = null;
            string viewExpression = string.Empty;
            foreach (string line in node.OuterHtml.Split("\n"))
            {
                List<MatchNode> expressionNodes = HtmlHelper.GetExpressionFromElement(line, node.Line + lineNo);
                foreach (var expression in expressionNodes)
                {
                    try
                    {
                        viewExpression = expression.Value;
                        //View() function validation

                        viewExist = KitsuneCompiler.ValidateView(ref viewExpression, entities.First().Value);
                        if (viewExist != null)
                        {
                            viewExist.LineNumber = expression.Line;
                            viewExist.LinePosition = expression.Column;
                            compileErrors.Add(viewExist);
                        }
                        //Partial() view function validation
                        partialViewExist = KitsuneCompiler.ValidatePartialView(ref viewExpression, projectDetails.Resources);
                        if (partialViewExist != null)
                        {
                            partialViewExist.LineNumber = expression.Line;
                            partialViewExist.LinePosition = expression.Column;
                            compileErrors.Add(partialViewExist);
                        }

                        foreach (string raw_obj in Parser.GetObjects(viewExpression))
                        {
                            string obj = raw_obj.Replace("[[", "").Replace("]]", "").ToLower();
                            if (customVariables.ContainsKey(obj))
                            {
                                continue;
                            }
                            else
                            {
                                objectNamesToValidate.Add(new MatchNode { Value = obj, Line = node.Line + lineNo, Column = line.IndexOf(obj) });
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        compileErrors.Add(CompileResultHelper.GetCompileError(ex.Message, node.Line, node.LinePosition));
                        continue;
                    }
                }
                lineNo++;
            }
            foreach (MatchNode obj in objectNamesToValidate)
            {
                Boolean isAlias = false;
                string objectName = obj.Value.ToLower();
                if ((classNameAlias.ContainsKey(objectName) && classNameAlias[objectName] == "") || customVariables.ContainsKey(objectName))
                {
                    continue;
                }
                string baseClassName = objectName.Split('.')[0];
                while (baseClassName != "kresult" && baseClassName != "search" && classNameAlias.ContainsKey(baseClassName))
                {
                    int index = objectName.IndexOf('.');
                    objectName = classNameAlias[baseClassName] + (index >= 0 ? objectName.Substring(index) : "");
                    isAlias = true;
                    baseClassName = objectName.Split('.')[0];
                }
                if (objectNamesValidated.ContainsKey(objectName))
                {
                    if (!objectNamesValidated[objectName] && !isAlias)
                    {
                        compileErrors.Add(CompileResultHelper.GetCompileError(String.Format(ErrorCodeConstants.UnrecognizedType, objectName), obj.Line, obj.Column));
                    }
                }
                else
                {
                    string[] objectPathArray = objectName.ToLower().Split('.');
                    string basePropertyName = objectPathArray[0];
                    var baseClass = baseClassList.FirstOrDefault(x => x.Name?.ToLower() == basePropertyName);

                    if (baseClass != null)
                    {
                        var baseEntity = entities.Keys.Contains(basePropertyName) ? entities[basePropertyName] : null;

                        if (baseEntity == null)
                            baseEntity = entities.First().Value;

                        IEnumerable<CompilerError> errorList = ValidateExpression(objectPathArray, 1, basePropertyName, PropertyType.obj, obj.Line, obj.Column, baseEntity);
                        if (errorList.Count() > 0)
                        {
                            compileErrors.AddRange(errorList);
                            objectNamesValidated.Add(objectName, false);
                        }
                        else
                        {
                            objectNamesValidated.Add(objectName, true);
                        }
                    }
                    else if (!ValidObjects.Contains(basePropertyName.Trim('[').Trim(']')) && ((basePropertyName.StartsWith("kresult") && !classNameAlias.ContainsKey("kresult")) || !basePropertyName.StartsWith("kresult")) )
                    {
                        compileErrors.Add(CompileResultHelper.GetCompileError(String.Format(ErrorCodeConstants.UnrecognizedClass, basePropertyName), obj.Line, obj.Column));
                        //objectNamesValidated.Add(objectName, false);
                    }

                }
            }
            //InnerText returns text from inner html tags, need to clear so we don't run parser again. DOM is unaffected by this.
            node.InnerHtml = "";
            objectNamesToValidate.Clear();
        }

        /// <summary>
        /// validate expression.
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="ind"></param>
        /// <param name="className"></param>
        /// <param name="type"></param>
        /// <param name="line"></param>
        /// <param name="position"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private IEnumerable<CompilerError> ValidateExpression(string[] expr, int ind, string className, PropertyType type, int line, int position, KEntity entity)
        {
            if (expr.Length > ind)
            {
                var len = expr[ind].Length;
                expr[ind] = Constants.PropertyArrayExpression.Replace(expr[ind], "");
                var kClass = entity.Classes.FirstOrDefault(x => x.Name?.ToLower() == className);
                if (kClass == null)
                    return new List<CompilerError> { new CompilerError { LineNumber = line, LinePosition = position, Message = String.Format(ErrorCodeConstants.ClassDoesNotExistInSchema, className) } };

                var kProperty = kClass.PropertyList.FirstOrDefault(x => x.Name?.ToLower() == expr[ind]);
                if (kProperty != null)
                {
                    var dataType = (kProperty.Type != PropertyType.array ? kProperty.DataType.Name : kProperty.DataType.Name.Trim('[', ']'))?.ToLower();
                    if (kProperty.Type == PropertyType.function)
                    {
                        if (!Constants.FunctionRegularExpression.IsMatch(expr[ind]))
                            return new List<CompilerError> { new CompilerError { LineNumber = line, LinePosition = position, Message = String.Format(ErrorCodeConstants.FunctionUsedLikeProperty, expr[ind]) } };

                    }
                    else if (kProperty.Type == PropertyType.obj && "dynamic" == kProperty.DataType?.Name?.ToLower())
                        return new List<CompilerError>();

                    var newClassName = ((kProperty.Type == PropertyType.array && len == expr[ind].Length) ? "array" : dataType.ToLower());
                    return ValidateExpression(expr, ++ind, newClassName, kProperty.Type, line, position, entity);
                }
                else
                {
                    return new List<CompilerError> { new CompilerError { LineNumber = line, LinePosition = position, Message = String.Format(ErrorCodeConstants.PropertyDoesNotExist, expr[ind], className) } };
                }
            }
            return new List<CompilerError>();
        }

        /// <summary>
        /// Clear repeat based name aliases after traversal.
        /// </summary>
        /// <param name="classNameAlias"></param>
        /// <param name="classNameAliasLevel"></param>
        /// <param name="level"></param>
        private void ClearNameAliases(Dictionary<string, string> classNameAlias, Dictionary<int, string> classNameAliasLevel, int level)
        {
            if (classNameAliasLevel.ContainsKey(level))
            {
                string[] classNameAliasListToClear = classNameAliasLevel[level].Split(',');
                foreach (string classNameAliasToClear in classNameAliasListToClear)
                {
                    classNameAlias.Remove(classNameAliasToClear);
                }
                classNameAliasLevel.Remove(level);
            }
        }

        internal KEntity GetKEntityFromEntityName(string entityName)
        {
            try
            {
                if (!String.IsNullOrEmpty(entityName))
                    return entities.Where(x => x.Value?.EntityName.ToLower() == entityName.ToLower()).FirstOrDefault().Value;
            }
            catch { }

            return entities?.FirstOrDefault().Value;
        }

    }
}
