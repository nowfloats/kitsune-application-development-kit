using HtmlAgilityPack;
using Kitsune.Compiler.Helpers;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune.Compiler.TagProcessors
{
    public abstract class Processor
    {
        char[] vowels = new char[] { 'a', 'e', 'i', 'o', 'u', 'y' };
        char[] consonants = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };
        Random random = new Random();

        internal string GenerateVariableName(int length)
        {
            StringBuilder sb = new StringBuilder();
            //initialize our vowel/consonant flag
            bool flag = (random.Next(2) == 0);
            for (int i = 0; i < length; i++)
            {
                sb.Append(GetChar(flag));
                flag = !flag; //invert the vowel/consonant flag
            }
            return sb.ToString();
        }

        internal char GetChar(bool vowel)
        {
            if (vowel)
            {
                return vowels[random.Next(vowels.Length)];
            }
            return consonants[random.Next(consonants.Length)];
        }

        public abstract void ProcessNode(CompileResourceRequest request, List<CompilerError> compileErrors, Dictionary<string, int> customVariables, string rootUrl, string filePath, Dictionary<string, string> classNameAlias, Dictionary<int, string> classNameAliasdepth, int depth, HtmlNode node, HtmlAttribute dynamicAttribute, List<MatchNode> objectNamesToValidate, DocumentValidator documentValidator);

        internal static bool IsSearchableProperty(string propertyPath, out KClass kClass, DocumentValidator documentValidator)
        {
            kClass = null;
            var objectPathArray = propertyPath.ToLower().Split('.');
            if (objectPathArray.Length > 1 && documentValidator != null && documentValidator.entities != null && documentValidator.entities.Count > 0)
            {
                var obClass = new KClass();
                var obProperty = new KProperty();
                IList<KClass> allClasses = null;
                if (documentValidator.entities.Keys.Any(x => x == objectPathArray[0].ToLower()))
                    allClasses = documentValidator.entities[objectPathArray[0].ToLower()].Classes;
                else if (documentValidator.entities.First().Value.Classes.Any(x => x.ClassType == KClassType.BaseClass && x.Name.ToLower() == objectPathArray[0].ToLower()))
                    allClasses = documentValidator.entities.First().Value.Classes;
                if (allClasses != null && allClasses.Any())
                {
                    var dataTypeClasses = Kitsune.Helper.Constants.DataTypeClasses;
                    for (var i = 0; i < objectPathArray.Length; i++)
                    {
                        if (i == 0)
                        {
                            obClass = allClasses.FirstOrDefault(x => x.ClassType == KClassType.BaseClass && x.Name == objectPathArray[i]);
                            if (obClass == null)
                                return false;
                        }
                        else
                        {
                            obProperty = obClass.PropertyList.FirstOrDefault(x => x.Name.ToLower() == objectPathArray[i]);
                            if (obProperty == null)
                                return false;
                            else if ((obProperty.Type == PropertyType.array && !dataTypeClasses.Contains(obProperty.DataType?.Name?.ToLower())) || obProperty.Type == PropertyType.obj)
                            {
                                obClass = allClasses.FirstOrDefault(x => x.ClassType == KClassType.UserDefinedClass && x.Name?.ToLower() == obProperty.DataType?.Name?.ToLower());
                            }
                            else
                                return false;
                        }
                    }
                    if (obClass != null)
                    {
                        kClass = obClass;
                        if (obProperty != null && obProperty.Type == PropertyType.array)
                            return true;
                    }
                }
            }
            return false;
        }

        internal KProperty GetProperty(List<CompilerError> compileErrors, HtmlAttribute dynamicAttribute, DocumentValidator documentValidator, string referenceObject, Dictionary<string, string> classNameAlias)
        {
            KProperty kProperty = null;
            KClass kClass = null;
            string expression = referenceObject;
            string baseExpression = expression.Split('.')[0].Split('[')[0].ToLower();
            while (baseExpression != "kresult" && baseExpression != "search" && classNameAlias.ContainsKey(baseExpression))
            {
                string[] expressionParts = expression.Split('.');
                expressionParts[0] = classNameAlias[baseExpression];
                expression = String.Join(".", expressionParts);
                baseExpression = expression.Split('.')[0].Split('[')[0].ToLower();
            }
            string[] classHierarchyList = expression.Split('.');
            KEntity entity = documentValidator?.GetKEntityFromEntityName(classHierarchyList[0]) ?? documentValidator?.GetKEntityFromEntityName(documentValidator.defaultEntity);
            if (entity != null)
            {
                kClass = entity.Classes.Where(x => x.ClassType == KClassType.BaseClass && x.Name.ToLower() == classHierarchyList[0].ToLower()).FirstOrDefault();
                if (kClass == null)
                {
                    compileErrors.Add(CompileResultHelper.GetCompileError(String.Format(ErrorCodeConstants.UnrecognizedType, classHierarchyList[0]), dynamicAttribute.Line, dynamicAttribute.LinePosition));
                    return null;
                }
                for (int i = 1; i < classHierarchyList.Length - 1; i++)
                {
                    string propName = classHierarchyList[i].Split('[')[0];
                    KProperty prop = kClass.PropertyList.Where(x => x.Name.ToLower() == propName.ToLower()).FirstOrDefault();
                    if (prop == null)
                    {
                        compileErrors.Add(CompileResultHelper.GetCompileError(String.Format(ErrorCodeConstants.UnrecognizedProperty, propName), dynamicAttribute.Line, dynamicAttribute.LinePosition));
                        return null;
                    }
                    kClass = entity.Classes.Where(x => x.Name.ToLower() == prop.DataType.Name.ToLower()).FirstOrDefault();
                    if (kClass == null)
                    {
                        compileErrors.Add(CompileResultHelper.GetCompileError(String.Format(ErrorCodeConstants.UnrecognizedType, prop.DataType.Name), dynamicAttribute.Line, dynamicAttribute.LinePosition));
                        return null;
                    }
                }
                string finalPropName = classHierarchyList[classHierarchyList.Length - 1].Split('[')[0].ToLower();
                kProperty = kClass.PropertyList.Where(x => x.Name == finalPropName && x.Type == PropertyType.array).FirstOrDefault();
            }
            return kProperty;
        }
    }
}
