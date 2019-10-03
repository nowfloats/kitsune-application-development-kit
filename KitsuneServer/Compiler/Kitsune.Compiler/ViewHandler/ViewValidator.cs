using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Kitsune.Compiler.Helpers;
using Kitsune.Language.Models;
using Kitsune.Language.Helper;
using System.Text.RegularExpressions;
using Kitsune.SyntaxParser;
using Kitsune.Models;

namespace Kitsune.Compiler.Validators
{
    public class ViewValidator
    {

        public List<CompilerError> Validate(KEntity baseLanguageEntity, string[] resourceLines, string userEmail, string themeId, List<KEntity> componentsEntity = null)
        {
            var result = new List<CompilerError>();
            Dictionary<string, KEntity> entities = new Dictionary<string, KEntity>();
            entities.Add(baseLanguageEntity.EntityName, baseLanguageEntity);
            if(componentsEntity != null && componentsEntity.Any())
            {
                foreach(var comEntity in componentsEntity)
                {
                    if (!entities.Keys.Contains(comEntity.EntityName))
                        entities.Add(comEntity.EntityName, comEntity);
                }
            }
            var kitsuneObjects = new List<Kitsune.Helper.MatchNode>();
            var repeatIterator = string.Empty;
            try
            {
                #region Validating Resource Html
                var index = 0;
                foreach (var resourceLine in resourceLines)
                {
                    List<Helper.MatchNode> attributeValue = null;

                    attributeValue = Kitsune.Helper.HtmlHelper.GetExpressionFromElement(resourceLine, index + 1);
                    kitsuneObjects = new List<Kitsune.Helper.MatchNode>();
                    if (attributeValue != null && attributeValue.Any())
                    {
                        repeatIterator = string.Empty;
                        List<string> objects = null;
                        foreach (var attr in attributeValue)
                        {
                            kitsuneObjects = new List<Kitsune.Helper.MatchNode>();
                            if (Kitsune.Helper.Constants.KRepeatPatternRegex.IsMatch(attr.Value.Trim()))
                            {
                                repeatIterator = Kitsune.Helper.Constants.KRepeatPatternRegex.Match(attr.Value.Trim()).Groups[3].Value.Trim();
                            }
                            try
                            {
                                objects = Parser.GetObjects(attr.Value);
                            }
                            catch (Exception ex)
                            {
                                result.Add(new CompilerError { Message = ex.Message, LineNumber = index, LinePosition = resourceLine.IndexOf(attr.Value) });
                                return result;
                            }
                            foreach (var obj in objects)
                            {
                                kitsuneObjects.Add(new Kitsune.Helper.MatchNode { Value = obj.ToString().Replace("[[", "").Replace("]]", ""), Column = attr.Column, Line = attr.Line });
                            }

                            foreach (var kitsuneObject in kitsuneObjects)
                            {
                                var objectPathArray = kitsuneObject.Value.ToLower().Split('.');
                                if (objectPathArray.Length > 1)
                                {
                                    var ind = 0;
                                    var basePropertyName = objectPathArray[ind];
                                    var baseEntity = entities.Keys.Contains(basePropertyName) ? entities[basePropertyName] : null;

                                    if(baseEntity == null)
                                    {
                                        baseEntity = entities.First().Value;
                                        //result.Add(new CompilerError { Message = "Unrecognized Class of type '" + basePropertyName + "'", LineNumber = kitsuneObject.Line, LinePosition = kitsuneObject.Column });
                                        //continue;
                                    }


                                    var baseClassList = baseEntity.Classes.Where(x => x.ClassType == KClassType.BaseClass);

                                    var baseClass = baseClassList.FirstOrDefault(x => x.Name?.ToLower() == basePropertyName);

                                    if (baseClass != null)
                                    {
                                        result.AddRange(ValidateExpression(objectPathArray, ++ind, basePropertyName, PropertyType.obj, kitsuneObject.Line, kitsuneObject.Column, userEmail, baseEntity));

                                    }
                                    else if (!(basePropertyName.StartsWith("kresult") || basePropertyName.StartsWith("_system")))
                                        result.Add(new CompilerError { Message = "Unrecognized Class of type '" + basePropertyName + "'", LineNumber = kitsuneObject.Line, LinePosition = kitsuneObject.Column });
                                }
                            }
                        }
                    }


                    #endregion
                    index++;
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public IEnumerable<CompilerError> ValidateExpression(string[] expr, int ind, string className, PropertyType type, int line, int position, string userEmail, KEntity entity)
        {
            if (expr.Length > ind)
            {
                var kDataClass = entity.Classes.Where(x => x.ClassType == KClassType.DataTypeClass);
                var len = expr[ind].Length;
                expr[ind] = Kitsune.Helper.Constants.PropertyArrayExpression.Replace(expr[ind], "");
                var kClass = entity.Classes.FirstOrDefault(x => x.Name?.ToLower() == className);
                if (kClass == null)
                    return new List<CompilerError> { new CompilerError { LineNumber = line, LinePosition = position, Message = "'" + className + "' Class dose not exist in the schema" } };

                var kProperty = kClass.PropertyList.FirstOrDefault(x => x.Name?.ToLower() == expr[ind]);
                if (kProperty != null)
                {

                    var dataType = (kProperty.Type != PropertyType.array ? kProperty.DataType.Name : kProperty.DataType.Name.Trim('[', ']'))?.ToLower();
                    if (kProperty.Type == PropertyType.function)
                    {
                        if (!Kitsune.Helper.Constants.FunctionRegularExpression.IsMatch(expr[ind]))
                            return new List<CompilerError> { new CompilerError { LineNumber = line, LinePosition = position, Message = expr[ind] + " is function but used like property" } };

                    }
                    else if (kProperty.Type == PropertyType.obj && "dynamic" == kProperty.DataType?.Name?.ToLower())
                        return new List<CompilerError>();


                    var newClassName = ((kProperty.Type == PropertyType.array && len == expr[ind].Length) ? "array" : dataType.ToLower());
                    return ValidateExpression(expr, ++ind, newClassName, kProperty.Type, line, position, userEmail, entity);

                }
                else
                {
                    return new List<CompilerError> { new CompilerError { LineNumber = line, LinePosition = position, Message = "Property " + expr[ind] + " does not exist in the class " + className } };
                }
            }

            return new List<CompilerError>();
        }
        
    }
}
