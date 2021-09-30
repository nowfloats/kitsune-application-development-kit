using System;
using System.Collections.Generic;
using System.Linq;
using Kitsune.AntlrLibrary;
using Kitsune.AntlrLibrary.Model;
using HtmlAgilityPack;
using Kitsune.Compiler.Helpers;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;

namespace Kitsune.Compiler.TagProcessors
{
    /// <summary>
    /// Process nodes with k-repeat tags.
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
    public class KRepeatProcessor : Processor
    {
        public override void ProcessNode(CompileResourceRequest request, List<CompilerError> compileErrors, Dictionary<string, int> customVariables, string rootUrl, string filePath, Dictionary<string, string> classNameAlias, Dictionary<int, string> classNameAliasdepth, int depth, HtmlNode node, HtmlAttribute dynamicAttribute, List<MatchNode> objectNamesToValidate, DocumentValidator documentValidator)
        {
            Node result = LexerGenerator.Parse(dynamicAttribute.Value.Trim('[', ']'));

            if (result?.Token?.Value == ACTIONS.Loop)
            {
                string referenceObject = result.Children[0].Children[0].Token.Value;
                string iterator = result.Children[2].Children[0].Token.Value;
                //Fix issue with same iterator being used at same depth.
                if (classNameAlias.ContainsKey(iterator) && classNameAliasdepth.ContainsKey(depth) && classNameAliasdepth[depth].Contains(iterator))
                {
                    classNameAlias.Remove(iterator);
                    string oldAliasDepth = classNameAliasdepth[depth];
                    classNameAliasdepth.Remove(depth);
                    oldAliasDepth = oldAliasDepth.Replace(iterator, "").Replace(",,", ",");
                    classNameAliasdepth.Add(depth, oldAliasDepth);
                }
                if (!classNameAlias.ContainsKey(iterator))
                {
                    classNameAlias.Add(iterator, "");
                    if (classNameAliasdepth.ContainsKey(depth))
                    {
                        string oldAliasDepth = classNameAliasdepth[depth];
                        classNameAliasdepth.Remove(depth);
                        classNameAliasdepth.Add(depth, oldAliasDepth + ',' + iterator);
                    }
                    else
                    {
                        classNameAliasdepth.Add(depth, iterator);
                    }
                    objectNamesToValidate.Add(new MatchNode { Value = referenceObject, Line = dynamicAttribute.Line, Column = dynamicAttribute.LinePosition });
                    try
                    {
                        if (result?.Children[4]?.Token?.Value?.ToString() == "ViewProperty" && result?.Children[4]?.Children[1]?.Token?.Value?.ToLower() == "offset")
                        {
                            //Added list type in condition as there might update in the offset and reference object so we have to update the meta also
                            if (request.PageType == Models.Project.KitsunePageType.LIST || request.PageType == Models.Project.KitsunePageType.DEFAULT || request.PageType == Models.Project.KitsunePageType.DETAILS)
                            {
                                request.PageType = Models.Project.KitsunePageType.LIST;
                                request.Offset = result.Children[6]?.Children[0]?.Token?.Value;
                                request.KObject = referenceObject;
                            }
                        }
                    }
                    catch { }
                    
                    string[] classHierarchyList = referenceObject.Split('.');
                    if (classHierarchyList[0].StartsWith("kresult", System.StringComparison.InvariantCultureIgnoreCase) || classHierarchyList[0].Equals("search", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        //pass
                    }
                    else if (classHierarchyList.Length >= 2)
                    {
                        KProperty kProperty = GetProperty(compileErrors, dynamicAttribute, documentValidator, referenceObject, classNameAlias);
                        if (kProperty == null)
                        {
                            compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KRepeatVariableNotArray, dynamicAttribute.Line, dynamicAttribute.LinePosition));
                        }
                        if (iterator.Split('.').Length > 1)
                        {
                            compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KRepeatInvalidIterator, dynamicAttribute.Line, dynamicAttribute.LinePosition));
                        }

                        KEntity entity = documentValidator?.GetKEntityFromEntityName(classHierarchyList[0]) ?? documentValidator?.GetKEntityFromEntityName(documentValidator.defaultEntity);
                        if (entity?.Classes?.Where(x => x.ClassType == KClassType.BaseClass && x.Name.ToLower() == iterator.ToLower()).FirstOrDefault() != null)
                        {
                            compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.BaseClassAsKRepeatIterator, dynamicAttribute.Line, dynamicAttribute.LinePosition));
                        }
                    }
                }
                else
                {
                    compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KRepeatVariableAlreadyInUse, dynamicAttribute.Line, dynamicAttribute.LinePosition));
                }
            }
            else if (result?.Token?.Value == ACTIONS.InLoop)
            {
                string loopVariable = result.Children[0].Children[0].Token.Value.ToLower();
                string referenceObject = result.Children[2].Children[0].Token.Value.ToLower() + $"[{GenerateVariableName(5)}]";
                objectNamesToValidate.Add(new MatchNode { Value = referenceObject, Line = dynamicAttribute.Line, Column = dynamicAttribute.LinePosition });

                if (classNameAlias.ContainsKey(loopVariable))
                {
                    compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KRepeatVariableAlreadyInUse, dynamicAttribute.Line, dynamicAttribute.LinePosition));
                    return;
                }
                KEntity entity = documentValidator?.GetKEntityFromEntityName(loopVariable) ?? documentValidator?.GetKEntityFromEntityName(documentValidator.defaultEntity);
                if (entity.Classes.Where(x => x.ClassType == KClassType.BaseClass && x.Name.ToLower() == loopVariable).Any())
                {
                    compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.BaseClassAsKRepeatVariable, dynamicAttribute.Line, dynamicAttribute.LinePosition));
                    return;
                }
               
                classNameAlias.Add(loopVariable, referenceObject);

                if (classNameAliasdepth.ContainsKey(depth))
                {
                    string oldAliasDepth = classNameAliasdepth[depth];
                    classNameAliasdepth.Remove(depth);
                    classNameAliasdepth.Add(depth, oldAliasDepth + ',' + loopVariable);
                }
                else
                {
                    classNameAliasdepth.Add(depth, loopVariable);
                }
            }
            else
            {
                compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.InvalidKRepeatSyntax, dynamicAttribute.Line, dynamicAttribute.LinePosition));
            }
            dynamicAttribute.Value = "";
        }
    }
}
