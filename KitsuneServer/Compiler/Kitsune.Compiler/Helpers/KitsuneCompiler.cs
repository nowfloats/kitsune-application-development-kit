using HtmlAgilityPack;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Helper;
using Kitsune.Language.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kitsune.Compiler.Helpers
{
    public class KitsuneCompiler
    {
        public static HtmlDocument GetDocumentObject()
        {
            var document = new HtmlDocument();
            document.OptionAutoCloseOnEnd = false;
            document.OptionCheckSyntax = false;
            return document;
        }

        public static CompileResult CompileHTML(CompileResourceRequest req, out HtmlDocument documentRef,
            GetProjectDetailsResponseModel projectDetails,
            KEntity kentity,
            GetPartialPagesDetailsResponseModel partialPages = null, bool subsequentCompilation = false, List<KLanguageModel> componentEntities = null) // partialPage is required only for the publish/preview
        {
            ///TODO : Handle the kitsune-settings.xml saperatly 
            documentRef = GetDocumentObject();
            var filePath = req.SourcePath.Trim('/');
            if (!string.IsNullOrEmpty(req.FileContent))
            {
                HtmlDocument document;
                List<CompilerError> nfUXErrors = new List<CompilerError>();
                Dictionary<string, int> customVariables = new Dictionary<string, int>();
                string rootUrl;

                DocumentPreprocessor.PreprocessDocument(req, kentity, filePath, out document, nfUXErrors, out rootUrl);
                documentRef = document;
                /*
                if (document.ParseErrors.Count() > 0)
                {
                    return CompileResultHelper.GetCompileResult(nfUXErrors, req.SourcePath, false);
                }
                */
                req.UrlPattern = req.SourcePath; //assigning default urlpattern 

                if (!req.IsStatic)
                {
                    if (kentity == null)
                    {
                        return CompileResultHelper.GetErrorCompileResult("Schema not found for this project.", req.SourcePath);
                    }
                    else if (kentity.EntityName == null)
                        kentity.EntityName = Constants.KPayDefaultSchema;

                    HtmlDocument documentClone = GetDocumentObject();
                    documentClone.LoadHtml(document.DocumentNode.OuterHtml);
                    DocumentValidator validator = new DocumentValidator();
                    validator.ValidateDocument(documentClone, req, nfUXErrors, kentity, customVariables, rootUrl, filePath, projectDetails, componentEntities?.Select(x => x.Entity)?.ToList());
                    if (nfUXErrors.Count == 0)
                    {
                        if (req.IsPublish)
                        {
                            var csrfPresent = Constants.CSRFRegularExpression.IsMatch(document.DocumentNode.OuterHtml);

                            UpdateForPublish(ref document, req, kentity, nfUXErrors, customVariables, projectDetails, partialPages, componentEntities, csrfPresent);

                            if (req.PageType != Models.Project.KitsunePageType.PARTIAL)
                            {
                                HandleScriptInjection(document, req, kentity, rootUrl, projectDetails, csrfPresent);
                            }

                        }

                    }
                    /*
                    KitsunePage myPage = (new NewRootNodeProcessor()).Process(document.DocumentNode.OuterHtml);
                    var jsonSerializeSettings = new JsonSerializerSettings();
                    jsonSerializeSettings.TypeNameHandling = TypeNameHandling.Objects;
                    string output = JsonConvert.SerializeObject(myPage, Formatting.None, jsonSerializeSettings);
                    //KitsunePage myNewPage = JsonConvert.DeserializeObject<KitsunePage>(output, jsonSerializeSettings);
                    */
                }
                UpdatePathsForPreview(req, document);
                if (nfUXErrors.Count > 0)
                    return new CompileResult { Success = false, ErrorMessages = nfUXErrors, PageName = req.SourcePath };
                else
                    return new CompileResult { Success = true, CompiledString = document.DocumentNode.OuterHtml.ToString(), ErrorMessages = nfUXErrors, PageName = req.SourcePath, CustomVariables = customVariables };
            }

            return new CompileResult
            {
                Success = false,
                ErrorMessages = new List<CompilerError> { new CompilerError { Message = "Input should not be empty" } },
                PageName = req.SourcePath,
            };
        }

        /// <summary>
        /// Update href, src etc paths for preview
        /// </summary>
        /// <param name="req">compiler request</param>
        /// <param name="document">document to update</param>
        private static void UpdatePathsForPreview(CompileResourceRequest req, HtmlDocument document)
        {
            if (req.IsPreview)
            {
                //Replace all the relative path with absolute path
                // TODO : Remove once identifier handle the relative path
                foreach (var elem in document.DocumentNode.Descendants().Where(x => x.Attributes["href"] != null || x.Attributes["src"] != null))
                {
                    var hrefAttr = elem.Attributes["href"];
                    if (hrefAttr != null && !string.IsNullOrEmpty(hrefAttr.Value) && !hrefAttr.Value.StartsWith("[[") && !hrefAttr.Value.ToLower().Trim().StartsWith("#") && !hrefAttr.Value.ToLower().Trim('/').StartsWith("http") && !hrefAttr.Value.ToLower().Trim('/').StartsWith("mailto:") && !hrefAttr.Value.ToLower().Trim('/').StartsWith("tel:") && !((hrefAttr.Value.ToLower().Trim('/').IndexOf(".html") > 0) || (hrefAttr.Value.ToLower().Trim('/').IndexOf(".htm") > 0) || (hrefAttr.Value.ToLower().Trim('/').IndexOf(".html.dl") > 0) || (hrefAttr.Value.ToLower().Trim('/').IndexOf(".php") > 0)))
                        hrefAttr.Value = string.Format(Kitsune.Helper.Constants.AssetsBasePath, req.ProjectId, hrefAttr.Value.Trim('.').Trim('/'));
                    var srcAttr = elem.Attributes["src"];
                    if (srcAttr != null && !string.IsNullOrEmpty(srcAttr.Value) && !srcAttr.Value.StartsWith("[[") && !srcAttr.Value.ToLower().Trim('/').StartsWith("http") && !((srcAttr.Value.ToLower().Trim('/').IndexOf(".html") > 0) || (srcAttr.Value.ToLower().Trim('/').IndexOf(".htm") > 0)))
                        srcAttr.Value = string.Format(Kitsune.Helper.Constants.AssetsBasePath, req.ProjectId, srcAttr.Value.Trim('.').Trim('/'));
                }
            }
        }

        private static void UpdateForPublish(ref HtmlDocument document,
                                             CompileResourceRequest request,
                                             KEntity kentity,
                                             List<CompilerError> compileErrors,
                                             Dictionary<string, int> customVariables,
                                             GetProjectDetailsResponseModel projectDetails,
                                             GetPartialPagesDetailsResponseModel partialPages = null,
                                             List<KLanguageModel> componentEntities = null,
                                             bool isCSRFPresent = false
                                             )
        {

            var htmlString = document.DocumentNode.OuterHtml.ToString();
            htmlString = htmlString.Replace("itemscope=\"\"", "itemscope");
            if (!string.IsNullOrEmpty(htmlString))
            {
                if (isCSRFPresent)
                    htmlString = Constants.CSRFRegularExpression.Replace(htmlString, Constants.CSRFReplacementToken);

                var lines = htmlString.Split('\n');

                var descendants = document.DocumentNode.Descendants();
                var kdlOb = descendants.Where(s => s.Attributes[LanguageAttributes.KDL.GetDescription()] != null);
                //store temporary original KDL value before replacement of _system object
                var tempKDL = string.Empty;
                if (kdlOb != null && kdlOb.Any())
                    tempKDL = kdlOb.FirstOrDefault().GetAttributeValue(LanguageAttributes.KDL.GetDescription(), null);

                for (int i = 0; i < lines.Length; i++)
                {
                    ReplaceCustomVariables(customVariables, lines, i);
                    InjectPartialView(request, kentity, projectDetails, partialPages, lines, i);
                    ReplaceComponentEntities(projectDetails, componentEntities, lines, i);
                    SetObject(compileErrors, kentity, projectDetails, lines, i, request.ClassName);
                }
                htmlString = string.Join("\n", lines);
                document = GetDocumentObject();
                document.LoadHtml(htmlString);

                //restore back the original k-dl attribute
                if (!string.IsNullOrEmpty(tempKDL))
                {
                    document.DocumentNode.Descendants().First(x => x.Attributes["k-dl"] != null).Attributes["k-dl"].Value = tempKDL;
                }
                htmlString = document.DocumentNode.OuterHtml;
            }
        }

        private static void ReplaceComponentEntities(GetProjectDetailsResponseModel projectDetails, List<KLanguageModel> componentEntities, string[] lines, int i)
        {
            if (componentEntities != null && componentEntities.Any())
            {
                foreach (var component in componentEntities)
                {
                    var matches = Constants.GeCustomVariableRegex($@"{component.Entity.EntityName}\.").Matches(lines[i]);
                    if (matches != null && matches.Count > 0)
                    {
                        foreach (var match in matches.ToList())
                        {
                            if (!string.IsNullOrEmpty(match.Value))
                                lines[i] = lines[i].Replace(match.Value, Constants.GetCustomVariableReplacementRegex($@"{component.Entity.EntityName}\.").Replace(match.Value, $"$1_system.components._{component._id}."));
                        }
                    }
                }
            }
        }

        private static void ReplaceCustomVariables(Dictionary<string, int> customVariables, string[] lines, int i)
        {
            //Update all the custom variables with _system.viewbag.{customvar}
            foreach (var custVar in customVariables.Keys)
            {
                var match = Constants.GeCustomVariableRegex(custVar).Match(lines[i]);
                if (match != null && !string.IsNullOrEmpty(match.Value))
                {
                    lines[i] = lines[i].Replace(match.Value, Constants.GetCustomVariableReplacementRegex(custVar).Replace(match.Value, $"$1_system.viewbag.{custVar}"));
                }
            }
        }

        private static void SetObject(List<CompilerError> compileErrors, KEntity kentity, GetProjectDetailsResponseModel projectDetails, string[] lines, int i, string currentClassName)
        {
            var pageClassResult = Helper.Constants.ViewClassRegularExpression.Matches(lines[i]);
            for (int j = 0; j < pageClassResult.Count; j++)
            {
                if (pageClassResult[j].Groups.Count > 1)
                {
                    var groupparam = pageClassResult[j].Groups[2].Value?.Trim('(', ')');
                    if (!string.IsNullOrEmpty(groupparam))
                    {
                        var partialViewParams = groupparam.Split(',');
                        var viewName = partialViewParams[0]?.Trim('\'');

                        if (!kentity.Classes.Any(x => x.ClassType == KClassType.BaseClass && x.Description == viewName.ToLower() && x.Name != null))
                            compileErrors.Add(new CompilerError { LineNumber = i + 1, LinePosition = 1, Message = string.Format("Page '{0}' dose not exist", viewName) });
                        else
                        {
                            lines[i] = lines[i].Replace(pageClassResult[j].Groups[1].Value, kentity.Classes.First(x => x.ClassType == KClassType.BaseClass && x.Description != null && x.Description.ToLower() == viewName.ToLower() && x.Name != null).Name);
                            List<MatchNode> nodes = new List<MatchNode>();
                            ResourceItemMeta pageDetails;
                            nodes = HtmlHelper.SetObjectExpressionFromElement(lines[i], i);
                            if (nodes != null && nodes.Any())
                            {
                                foreach (var node in nodes)
                                {
                                    var ob = node.Value ?? "";
                                    string className = null;
                                    pageDetails = null;

                                    // foreach (var ob in languageObjects)
                                    if (ob.ToLower().Contains("geturl()"))
                                    {
                                        className = ob.IndexOf('.') > 0 ? ob.Trim('[', ']').Substring(0, ob.Trim('[', ']').IndexOf('.')) : ob.Trim('[', ']');
                                        pageDetails = projectDetails.Resources.FirstOrDefault(x => x.ClassName == className.ToUpper());
                                        string pattern = pageDetails.UrlPattern;
                                        if (!currentClassName.Equals(className, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            string[] segments = pattern.Split('/');
                                            for (int segIndex = 0; segIndex < segments.Length; segIndex++)
                                            {
                                                if (segments[segIndex].IndexOf("currentpagenumber", StringComparison.InvariantCultureIgnoreCase) > 0)
                                                {
                                                    segments[segIndex] = segments[segIndex].Replace(className + ".currentpagenumber", "'1'", StringComparison.InvariantCultureIgnoreCase);
                                                }
                                            }
                                            pattern = String.Join("/", segments);
                                        }
                                        if (pageDetails != null)
                                            lines[i] = lines[i].Replace(ob, pattern);
                                    }
                                    else if (ob.ToLower().Contains("setobject"))
                                    {
                                        className = ob.IndexOf('.') > 0 ? ob.Trim('[', ']').Substring(0, ob.Trim('[', ']').IndexOf('.')) : ob.Trim('[', ']');
                                        pageDetails = projectDetails.Resources.FirstOrDefault(x => x.ClassName == className.ToUpper());
                                        if (pageDetails != null)
                                        {
                                            if (!string.IsNullOrEmpty(pageDetails.KObject))
                                            {
                                                var matches = Kitsune.Helper.Constants.FunctionParameterExpression.Match(ob);
                                                if (matches != null && matches.Groups?.Count > 0 && !string.IsNullOrEmpty(pageDetails.UrlPattern))
                                                {
                                                    var tempKObjectsValue = ((pageDetails.KObject.Split(':').Count() > 1) ? pageDetails.KObject.Split(':')[0] : pageDetails.KObject).Split(',');
                                                    var matchOBject = matches.Groups[1].Value.Split(',');
                                                    var pattern = pageDetails.UrlPattern;
                                                    var widgetNodeForKObj = Kitsune.Helper.Constants.WidgetRegulerExpression.Matches(pageDetails.UrlPattern);
                                                    if (matchOBject.Length == tempKObjectsValue.Length && widgetNodeForKObj != null && widgetNodeForKObj.Count > 0)
                                                    {
                                                        for(var mi = 0; mi < tempKObjectsValue.Length; mi++)
                                                        {
                                                            for (var wc = 0; wc < widgetNodeForKObj.Count; wc++)
                                                            {
                                                                pattern = pattern.Replace(widgetNodeForKObj[wc].Value, Kitsune.Helper.Constants.GetKObjectReplaceParamRegex(tempKObjectsValue[mi], true).Replace(widgetNodeForKObj[wc].Value, matchOBject[mi]));
                                                            }
                                                        }
                                                    }
                                                    //TODO : support for single object if only using one _kid
                                                    else if (widgetNodeForKObj != null && widgetNodeForKObj.Count > 0)
                                                    {
                                                        for (var mi = 0; mi < tempKObjectsValue.Length; mi++)
                                                        {
                                                            for (var wc = 0; wc < widgetNodeForKObj.Count; wc++)
                                                            {
                                                                pattern = pattern.Replace(widgetNodeForKObj[wc].Value, Kitsune.Helper.Constants.GetKObjectReplaceParamRegex(tempKObjectsValue[mi], true).Replace(widgetNodeForKObj[wc].Value, matchOBject[0]));
                                                            }
                                                        }
                                                    }
                                                    lines[i] = lines[i].Replace(ob, pattern);
                                                }
                                            }
                                            else
                                            {
                                                compileErrors.Add(new CompilerError
                                                {
                                                    LineNumber = i,
                                                    LinePosition = lines[i].IndexOf("setobject", StringComparison.InvariantCultureIgnoreCase),
                                                    Message = string.Format("K-Object not found for the page '{0}'", pageDetails.SourcePath)
                                                });
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        compileErrors.Add(new CompilerError { LineNumber = i + 1, LinePosition = 1, Message = "Page view parameters missing." });
                    }
                }
                else
                {
                    compileErrors.Add(new CompilerError { LineNumber = i + 1, LinePosition = 1, Message = "Invalid page view syntax" });
                }
            }
        }

        private static void InjectPartialView(CompileResourceRequest request, KEntity kentity, GetProjectDetailsResponseModel projectDetails, GetPartialPagesDetailsResponseModel partialPages, string[] lines, int i)
        {
            var partialregex = Constants.PartialPageRegularExpression;
            var partialresult = partialregex.Matches(lines[i]);

            if (partialPages != null && partialPages.Resources != null && partialPages.Resources.Any())
                for (int mc = 0; mc < partialresult.Count; mc++)
                {
                    if (partialresult[mc].Groups.Count > 1)
                    {
                        var groupparam = partialresult[mc].Groups[1].Value?.Trim('(', ')').Trim('\'').ToLower();
                        if (!string.IsNullOrEmpty(groupparam))
                        {
                            var partialViewParams = groupparam.Split(',');
                            var viewName = partialViewParams[0]?.Trim('\'');

                            if (projectDetails.Resources.Any(x => x.PageType == Models.Project.KitsunePageType.PARTIAL && x.SourcePath.ToLower() == viewName.ToLower()))
                            {
                                var htmlData = new HtmlDocument();
                                htmlData.LoadHtml(partialPages.Resources.FirstOrDefault(x => string.Compare(x.SourcePath, viewName, true) == 0).HtmlSourceString);
                                //Support partial page with Html body and without body tag
                                string partialbody = htmlData.DocumentNode.SelectSingleNode("/html/body")?.InnerHtml ?? htmlData.DocumentNode.OuterHtml;
                                var partialCompiledOb = KitsuneCompiler.CompileHTML(new CompileResourceRequest
                                {
                                    FileContent = partialbody,
                                    IsPublish = true,
                                    SourcePath = viewName,
                                    PageType = Models.Project.KitsunePageType.PARTIAL,
                                    ProjectId = request.ProjectId,
                                    UserEmail = request.UserEmail
                                }, out htmlData, projectDetails, kentity, partialPages);
                                if (partialCompiledOb != null && partialCompiledOb.Success)
                                    lines[i] = lines[i].Replace(partialresult[mc].Value, partialCompiledOb.CompiledString);
                            }
                        }
                    }
                }
        }

        private static void HandleScriptInjection(HtmlDocument document,
                                             CompileResourceRequest request,
                                             KEntity kentity,
                                             string rootUrl,
                                             GetProjectDetailsResponseModel projectDetails, bool isCSRF)
        {


            HtmlNode body = document.DocumentNode.SelectSingleNode("//body");
            var descendants = document.DocumentNode.Descendants();
            var kPayObject = descendants.Where(s => s.Attributes[LanguageAttributes.KPayAmount.GetDescription()] != null);


            if (body != null)
            {
                if (kPayObject != null && kPayObject.Any())
                {
                    HtmlNode kPayScript = document.CreateElement("script");
                    kPayScript.SetAttributeValue("src", Constants.KPayJsLink);
                    kPayScript.SetAttributeValue("type", "text/javascript");
                    kPayScript.SetAttributeValue("charset", "utf-8");
                    kPayScript.SetAttributeValue("defer", "defer");
                    kPayScript.SetAttributeValue("async", "async");
                    body.AppendChild(kPayScript);
                }

                if (isCSRF)
                {
                    HtmlNode csrfScript = document.CreateElement("script");
                    csrfScript.SetAttributeValue("src", Constants.CSRFTokenJSLink);
                    csrfScript.SetAttributeValue("type", "text/javascript");
                    csrfScript.SetAttributeValue("charset", "utf-8");
                    csrfScript.SetAttributeValue("defer", "defer");
                    csrfScript.SetAttributeValue("async", "async");
                    body.AppendChild(csrfScript);
                }

                HtmlNode ThemeIdWidget = HtmlNode.CreateNode(string.Format("<span style='display: none;' id='GET-VALUE(THEME-ID)'>{0}</span>", request.ProjectId));
                HtmlNode WebsiteId = HtmlNode.CreateNode("<span style='display: none;' id='GET-VALUE(WEBSITE-ID)'>[KITSUNE_WEBSITE_ID]</span>");
                body.AppendChild(ThemeIdWidget);
                body.AppendChild(WebsiteId);

                if (kentity.EntityName != Constants.KPayDefaultSchema)
                {
                    HtmlNode RootAlias = HtmlNode.CreateNode(string.Format("<a style='display: none;' href='[[{0}]]' id='GET-URL(HOME)'></a>", rootUrl));
                    body.AppendChild(RootAlias);
                }
            }
        }
        public static bool IsSearchableProperty(string propertyPath, KEntity entity, out KClass kClass)
        {
            kClass = null;
            var objectPathArray = propertyPath.ToLower().Split('.');
            if (objectPathArray.Length > 1)
            {
                var obClass = new KClass();
                var obProperty = new KProperty();
                var dataTypeClasses = Kitsune.Helper.Constants.DataTypeClasses;
                for (var i = 0; i < objectPathArray.Length; i++)
                {
                    if (i == 0)
                    {
                        obClass = entity.Classes.FirstOrDefault(x => x.ClassType == KClassType.BaseClass && x.Name == objectPathArray[i]);
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
                            obClass = entity.Classes.FirstOrDefault(x => x.ClassType == KClassType.UserDefinedClass && x.Name?.ToLower() == obProperty.DataType?.Name?.ToLower());
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
            return false;
        }

        public static CompileResult ValidateJsonConfig(ValidateConfigRequestModel requestModel)
        {
            try
            {
                JToken jsonToValidate = JToken.Parse(requestModel.File.Content);
#if DEBUG
                var Validator = ProjectConfigValidator.GetInstance(new Uri("https://s3.ap-south-1.amazonaws.com/kitsune-resources-dev/schema.json"));
#else
                var Validator = ProjectConfigValidator.GetInstance(new Uri("https://s3.ap-south-1.amazonaws.com/kitsune-resources/schema.json"));
#endif
                var result = Validator.ValidateJson(jsonToValidate);
                if (result.IsError && result.Errors != null && result.Errors.Any())
                {
                    return new CompileResult
                    {
                        Success = false,
                        ErrorMessages = result.Errors.Select(x => new CompilerError
                        {
                            LineNumber = x.LineNumber,
                            LinePosition = x.LinePosition,
                            Message = x.ToString()
                        }).ToList()
                    };
                }
            }
            catch (JsonReaderException ex)
            {
                return new CompileResult
                {
                    Success = false,
                    ErrorMessages = new List<CompilerError>
                    {
                        new CompilerError
                        {
                            LineNumber = ex.LineNumber,
                            LinePosition = ex.LinePosition,
                            Message = ex.Message
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return null;
            }
            return new CompileResult { Success = true, CompiledString = requestModel?.File?.Content };
        }

        public static CompilerError ValidateView(ref string expression, KEntity kentity)
        {
            var pageClassResult = Helper.Constants.ViewClassRegularExpression.Matches(expression);

            if (pageClassResult != null && pageClassResult.Count > 0)
            {
                foreach (var page in pageClassResult.ToList())
                {
                    var groupparam = page.Groups[2].Value?.Trim('(', ')');
                    if (!string.IsNullOrEmpty(groupparam))
                    {
                        var partialViewParams = groupparam.Split(',');
                        var viewName = partialViewParams[0]?.Trim('\'');

                        if (kentity.Classes.Any(x => x.ClassType == KClassType.BaseClass && x.Description != null && x.Description.ToLower() == viewName.ToLower() && x.Name != null))
                            expression = expression.Replace(page.Groups[1].Value, kentity.Classes.First(x => x.ClassType == KClassType.BaseClass && x.Description != null && x.Description.ToLower() == viewName.ToLower() && x.Name != null).Name);
                        else
                            return CompileResultHelper.GetCompileError(string.Format(ErrorCodeConstants.UnrecognizedView, viewName));
                    }
                }
            }
            return null;
        }
        public static CompilerError ValidatePartialView(ref string expression, List<ResourceItemMeta> resources)
        {
            var pageClassResult = Helper.Constants.PartialPageRegularExpression.Matches(expression);

            if (pageClassResult != null && pageClassResult.Count > 0)
            {
                foreach (var page in pageClassResult.ToList())
                {
                    var groupparam = page.Groups[1].Value?.Trim('(', ')');
                    if (!string.IsNullOrEmpty(groupparam))
                    {
                        var partialViewParams = groupparam.Split(',');
                        var viewName = partialViewParams[0]?.Trim('\'');

                        if (!resources.Any(x => x.PageType == Models.Project.KitsunePageType.PARTIAL && x.SourcePath.ToLower() == viewName.ToLower()))
                            return CompileResultHelper.GetCompileError(string.Format(ErrorCodeConstants.UnrecognizedView, viewName));
                    }
                }
            }
            return null;
        }

    }
}
