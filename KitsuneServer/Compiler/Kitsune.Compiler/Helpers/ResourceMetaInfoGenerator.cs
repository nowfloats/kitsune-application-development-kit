using HtmlAgilityPack;
using Kitsune.Language.Helper;
using Kitsune.Language.Models;
using Kitsune.SyntaxParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kitsune.Models;
using Kitsune.API.Model.ApiRequestModels;

namespace Kitsune.Compiler.Core.Helpers
{
    public class ResourceMetaInfoGenerator
    {
        static List<SinglePositionProperty> MetaInfoList = new List<SinglePositionProperty> { };
        static List<Kitsune.Helper.MatchNode> metaList = new List<Kitsune.Helper.MatchNode>();

        public static void ExtractProperty(string[] Lines, string userEmail, string themeId, List<string> tagsToIgnore)
        {
            var result = new List<ResourceCompilationResult>();

            try
            {
                var index = 0;
                Regex rangeSeperatorRegex = new Regex(@"(\[\d*)(x)(\d*\])");
                Regex offsetSeperatorRegex = new Regex(@"(\[\d*)(x)(\d*)(x)(\d*\])");
                foreach (var line in Lines)
                {
                    List<Helper.MatchNode> attributeValue = null;

                    attributeValue = Kitsune.Helper.HtmlHelper.GetExpressionFromElement(line, index + 1);
                    var widgetTypes = new List<Kitsune.Helper.MatchNode>();
                    if (attributeValue != null && attributeValue.Any())
                    {
                        foreach (var attr in attributeValue)
                        {
                            var property = "";
                            var matches = Parser.GetObjects(attr.Value);
                            foreach (var mat in matches)
                            {
                                if (!mat.Contains("null"))
                                {
                                    property = mat.ToString().Replace("[[", "").Replace("]]", "");
                                    if (!tagsToIgnore.Any(s => s.Equals(property, StringComparison.OrdinalIgnoreCase)) && !property.StartsWith("kresult", StringComparison.OrdinalIgnoreCase))
                                    {
                                        property = rangeSeperatorRegex.Replace(property, "$1:$3");
                                        property = offsetSeperatorRegex.Replace(property, "$1:$3:$5");
                                        widgetTypes.Add(new Helper.MatchNode { Value = property, Column = attr.Column, Line = attr.Line });
                                        metaList.Add(new Helper.MatchNode { Value = property, Column = attr.Column, Line = attr.Line });
                                        MetaInfoList.Add(new SinglePositionProperty { Property = property.ToLower(), Position = new position { Column = attr.Column, Line = attr.Line } });
                                    }
                                }
                            }
                        }
                    }

                    index++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception occured while extracting properties");
            }
        }

        public static HtmlDocument GetDocumentObject()
        {
            var document = new HtmlDocument();
            document.OptionAutoCloseOnEnd = true;
            return document;
        }

        public static string MakeRegexEligible(string input)
        {
            string output = input.Replace("[", @"\[").Replace("]", @"\]").Replace(".", @"\.");
            return output;
        }

        public static ResourceMetaInfo MetaInfo(CompileResourceRequest req, string metadocument, GetProjectDetailsResponseModel projectDetails,
            KEntity kentity, List<string> tagsToIgnore)
        {
            try
            {
                if (tagsToIgnore == null)
                    tagsToIgnore = new List<string>();
                MetaInfoList = new List<SinglePositionProperty> { };
                metaList = new List<Kitsune.Helper.MatchNode>();
                var documentRef = new HtmlDocument();
                documentRef = GetDocumentObject();
                if (!string.IsNullOrEmpty(metadocument))
                {
                    var document = GetDocumentObject();
                    try
                    {
                        document.LoadHtml(metadocument);

                    }
                    catch { }
                    documentRef = document;

                    var descendants = document.DocumentNode.Descendants();

                    if (!(req.IsStatic))
                    {
                        #region K-RepeatHandler
                        var repeatNodes = descendants.Where(x => x.Attributes[LanguageAttributes.KRepeat.GetDescription()] != null && !string.IsNullOrEmpty(x.Attributes[LanguageAttributes.KRepeat.GetDescription()].Value));
                        if (repeatNodes != null && repeatNodes.Any())
                        {
                            var krepeatCount = 0;
                            var kval = "";
                            string match;
                            List<string> repeatForEachParam;
                            foreach (var node in repeatNodes)
                            {
                                node.Attributes[LanguageAttributes.KRepeat.GetDescription()].Value = string.Concat("[", node.Attributes[LanguageAttributes.KRepeat.GetDescription()].Value.Trim(), "]");
                                kval = node.Attributes[LanguageAttributes.KRepeat.GetDescription()].Value;
                                match = Regex.Match(kval, Kitsune.Helper.Constants.WidgetRegulerExpression.ToString())?.Value?.Trim('[', ']');
                                if (string.IsNullOrEmpty(match))
                                { }
                                //nfUXErrors.Add(new CompilerError { Message = "Invalid k-repeat value", LineNumber = node.Line, LinePosition = node.LinePosition });
                                else
                                {
                                    //if (match.IndexOf(" in ") > 0)
                                    //{
                                    //    repeatForEachParam = match.Split(new string[] { " in " }, StringSplitOptions.None)?.ToList();
                                    //    ///TODO : check condition for all params
                                    //    if (repeatForEachParam.Count == 2)
                                    //    {
                                    //        var regexes = Kitsune.Helper.Constants.WidgetRegulerExpression;
                                    //        var regexmatchies = regexes.Matches(node.InnerHtml);
                                    //        var regex = new Regex(MakeRegexEligible(krepeatvar[krepeatCount].Trim()));
                                    //        krepeatCount++;
                                    //        if (regexmatchies != null && regexmatchies.Count > 0)
                                    //            for (int i = 0; i < regexmatchies.Count; i++)
                                    //            {
                                    //                node.InnerHtml = node.InnerHtml.Replace(regexmatchies[i].Value, regex.Replace(regexmatchies[i].Value, "[x]"));
                                    //            }

                                    //        foreach (var attr in node.Attributes.Where(x => x.Name.ToLower() != "k-repeat"))
                                    //        {
                                    //            regexmatchies = regexes.Matches(attr.Value);
                                    //            for (int i = 0; i < regexmatchies.Count; i++)
                                    //            {
                                    //                attr.Value = attr.Value.Replace(regexmatchies[i].Value, regex.Replace(regexmatchies[i].Value, "[x]"));
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    //else
                                    //{
                                    repeatForEachParam = match.Split(',').ToList();
                                    if (repeatForEachParam.Count == 3)
                                    {
                                        var repeatCount = repeatForEachParam[2].Split(':');
                                        tagsToIgnore.Add(repeatForEachParam[0].Trim());
                                        tagsToIgnore.Add(repeatForEachParam[1].Trim());
                                        if (repeatCount[0].Contains("offset"))
                                            repeatCount[0] = "x";
                                        var regexes = Kitsune.Helper.Constants.WidgetRegulerExpression;
                                        var regexmatchies = regexes.Matches(node.InnerHtml);
                                        var regex = new Regex(MakeRegexEligible(@"[\s*" + repeatForEachParam[1].Trim() + @"\s*]"));
                                        if (regexmatchies != null && regexmatchies.Count > 0)
                                            for (int i = 0; i < regexmatchies.Count; i++)
                                            {
                                                if (repeatCount[1].Contains("length()"))
                                                    node.InnerHtml = node.InnerHtml.Replace(regexmatchies[i].Value, regex.Replace(regexmatchies[i].Value, "[x]"));
                                                else
                                                    node.InnerHtml = node.InnerHtml.Replace(regexmatchies[i].Value, regex.Replace(regexmatchies[i].Value, "[" + repeatCount[0].Trim() + "x" + repeatCount[1].Trim() + "]"));
                                            }

                                        foreach (var attr in node.Attributes.Where(x => x.Name.ToLower() != "k-repeat"))
                                        {
                                            regexmatchies = regexes.Matches(attr.Value);
                                            for (int i = 0; i < regexmatchies.Count; i++)
                                            {
                                                attr.Value = attr.Value.Replace(regexmatchies[i].Value, regex.Replace(regexmatchies[i].Value, "$1[" + repeatCount[0].Trim() + "x" + repeatCount[1].Trim() + "]$3$4"));
                                            }

                                        }

                                    }

                                    //}
                                }
                            }
                            foreach (var node in repeatNodes)
                            {
                                node.Attributes[LanguageAttributes.KRepeat.GetDescription()].Value = node.Attributes[LanguageAttributes.KRepeat.GetDescription()].Value.Substring(1, node.Attributes[LanguageAttributes.KRepeat.GetDescription()].Value.Length - 2);
                            }
                            var newHtml = document.DocumentNode.OuterHtml;
                            document = GetDocumentObject();
                            document.LoadHtml(newHtml);
                            descendants = document.DocumentNode.Descendants();

                            #endregion
                        }


                        string[] NfWidgets = document.DocumentNode.OuterHtml.Split('\n');
                        ExtractProperty(NfWidgets, req.UserEmail, req.ProjectId, tagsToIgnore.Distinct().ToList());

                        var metaObject = new List<PropertyDetails>();

                        //MetaInfoListString.Sort();

                        List<MultiplePositionProperty> distinctInput = MetaInfoList.GroupBy(i => i.Property)
                        .Select(g => new MultiplePositionProperty
                        {
                            Property = g.Key,
                            Positions = g.Select(i => i.Position).ToList()
                        }).ToList();
                        distinctInput = distinctInput.GroupBy(i => i.Property)
                        .Select(g => new MultiplePositionProperty
                        {
                            Property = g.Key,
                            Positions = g.SelectMany(i => i.Positions).ToList()
                        }).ToList();

                        //# For writing unmerged properties to file in local
                        //List<string> MetaInfoListString = new List<string>();
                        //foreach (var metainfo in distinctInput)
                        //{
                        //    List<string> write = new List<string> { metainfo.Property };
                        //    foreach (var position in metainfo.Positions)
                        //    {
                        //        write.Add(position.Column.ToString());
                        //        write.Add(position.Line.ToString());
                        //    }
                        //    MetaInfoListString.Add(string.Join(",", write));
                        //}
                        //MetaInfoListString.Sort();
                        //string outputFile = @"D:\" + req.UserEmail + @"\" + req.SourcePath.Replace("/", "");
                        //System.IO.File.WriteAllLines(outputFile + @".csv", MetaInfoListString);

                        var metaInfoMergeList = distinctInput.Where(x => x.Property.Contains('[')).ToList();
                        distinctInput.RemoveAll(x => x.Property.Contains('['));
                        var mergedMetaInfoList = ksegregation.MergeProperty(metaInfoMergeList);
                        distinctInput.AddRange(mergedMetaInfoList);
                        distinctInput = distinctInput.OrderBy(x => x.Property).ToList();

                        //# For writing merged properties to file in local
                        //List<string> MetaInfoListString = new List<string>();
                        //foreach (var metainfo in distinctInput)
                        //{
                        //    List<string> write = new List<string> { metainfo.Property };
                        //    foreach (var position in metainfo.Positions)
                        //    {
                        //        write.Add(position.Column.ToString());
                        //        write.Add(position.Line.ToString());
                        //    }
                        //    MetaInfoListString.Add(string.Join(",", write));
                        //}
                        //string optimizedOutputFile = @"D:\optimized_" + req.UserEmail + @"\" + req.SourcePath.Replace("/", "");
                        //System.IO.File.WriteAllLines(optimizedOutputFile + @".csv", MetaInfoListString);


                        var metaClassBuilder = new MetaClassBuilder();
                        var metaClass = metaClassBuilder.BuildMetaClass(kentity, distinctInput, req.SourcePath);

                        return new ResourceMetaInfo
                        {
                            IsError = false,
                            Property = distinctInput,
                            metaClass = metaClass
                        };

                    }

                    return new ResourceMetaInfo
                    {
                        IsError = true,
                        Message = "Input should not be Static"
                    };
                }
                return new ResourceMetaInfo
                {
                    IsError = true,
                    Message = "Input should not be empty"
                };
            }
            catch (Exception ex)
            {
                //TODO: throw error
                //throw new Exception(string.Format("Excepiton occured while generating metainfo: {0}", ex.Message));
                return new ResourceMetaInfo
                {
                    IsError = true,
                    Message = string.Format("Excepiton occured while generating metainfo: {0}", ex.Message)
                };
            }
        }
    }
}
