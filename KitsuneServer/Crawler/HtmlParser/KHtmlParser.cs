using Crawler.Helper;
using Crawler.Models;
using HtmlAgilityPack;
using Kitsune.Models.Krawler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawler.HtmlParser
{
    public class KHtmlParser
    {
        /// <summary>
        /// Root Url given by the User(Can be initialised once)
        /// </summary>
        private Uri _rootUrl;
        /// <summary>
        /// Set the value of the _rootUrl
        /// </summary>
        public Uri RootUrl
        {
            get
            {
                return _rootUrl;
            }
        }
        /// <summary>
        /// Pool of all webpages to be Crawled
        /// </summary>
        public ConcurrentQueue<string> UniqueWebPageQueue;
        /// <summary>
        /// Resource Object which stores the Resource details
        /// </summary>
        public ResourcesContext Resources;
        /// <summary>
        /// Parsed Html Document
        /// </summary>
        public KHtmlDocument KHtml;
        /// <summary>
        /// Uri Scheme allowed like: https , http , file , mailto
        /// </summary>
        public HashSet<string> AllowedScheme = new HashSet<string>() { "http", "https", "file" };
        /// <summary>
        /// Error Log
        /// </summary>
        public Action<LOGTYPE, string, Exception> ErrorLogMethod;



        public KHtmlParser(Uri rootUri, KHtmlDocument htmlDocument)
        {
            try
            {
                if (rootUri == null)
                    throw new Exception("Root Uri cannot be null");
                if (htmlDocument == null)
                    throw new Exception("HtmlDocument cannot be null");             //TODO: also check if htmlDocument is loaded with content

                UniqueWebPageQueue = new ConcurrentQueue<string>();
                Resources = new ResourcesContext();
                ErrorLogMethod = (LOGTYPE x, string y, Exception z) => Console.WriteLine("Type: {0} , Message: {1} ,Exception: {2}", x, y, z == null ? "No InnerException" : z.Message);

                _rootUrl = rootUri;
                KHtml = htmlDocument;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Start parsing the Html
        /// </summary>
        public void Parse()
        {
            try
            {
                #region Identify and Remove BaseTag

                //TODO : Remove it if done before
                KHtml.IdentifyBaseTagAndSetValue(RootUrl);
                if (KHtml.BaseTag != null && KHtml.BaseTag.Exists)
                {
                    KHtml.RemoveBaseTags();
                }

                #endregion

                #region Process webpages, styles, scripts and assets

                IdentifyAndUpdateWebpagesList();
                IdentifyStyleSheets();
                IdentifyJavaScript();
                IdentifyAssets();

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void IdentifyAndUpdateWebpagesList()
        {
            try
            {
                HtmlNodeCollection anchorNodes = KHtml.GetAnchorsNodeCollection();
                if (anchorNodes != null)
                {
                    foreach (var node in anchorNodes)
                    {
                        try
                        {
                            #region Get the attribute value

                            string attributeValue = String.Empty;
                            string attributeName = String.Empty;

                            switch (node.Name)
                            {
                                case "a":
                                    attributeValue = node.GetAttributeValue("href", string.Empty);
                                    attributeName = "href";
                                    break;
                                case "form":
                                    string formMethod = node.GetAttributeValue("method", string.Empty);
                                    string formAction = node.GetAttributeValue("action", string.Empty);
                                    if (!string.IsNullOrEmpty(formAction) && !string.IsNullOrEmpty(formMethod) && formMethod.Equals("get",StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        attributeName = "action";
                                        attributeValue = formAction;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    break;
                                default:
                                    continue;
                            }

                            #endregion

                            //  AttributeValue = WebUtility.HtmlDecode(attributeValue);
                            //  TODO: check why to decode the value

                            #region Process the attribute value

                            Uri absoluteUri = null;
                            if (!string.IsNullOrEmpty(attributeValue))
                            {
                                //  Ignore #
                                if (!attributeValue.StartsWith("#"))
                                {
                                    if (Uri.TryCreate(RootUrl, attributeValue, out absoluteUri))
                                    {
                                        if (AllowedScheme.Contains(absoluteUri.Scheme.ToLower()))
                                        {
                                            if (absoluteUri.Host.Equals(RootUrl.Host, StringComparison.OrdinalIgnoreCase))
                                            {
                                                #region Process the new Url found

                                                string fragment = absoluteUri.Fragment;
                                                string absoluteUrl = String.IsNullOrEmpty(fragment) ?
                                                    absoluteUri.AbsoluteUri : absoluteUri.AbsoluteUri.Replace(absoluteUri.Fragment, String.Empty);

                                                absoluteUri = Utils.GenerateUriToProcess(absoluteUri, Resources.IgnoreFileNameChangeRegex);
                                                string placeHolder = KHtmlParser.CheckUrlPresentInAssetOrNot(absoluteUri,Resources);
                                                if (placeHolder != null && placeHolder.Equals("IGNORE", StringComparison.InvariantCultureIgnoreCase))
                                                    continue;
                                                if (placeHolder == null)
                                                {
                                                    //create a new one
                                                    placeHolder = String.Format("[kitsune_{0}]", absoluteUri.AbsoluteUri);
                                                    AssetDetails linkMap = new AssetDetails
                                                    {
                                                        PlaceHolder = placeHolder,
                                                        LinkUrl = absoluteUri.AbsoluteUri
                                                    };

                                                    Resources.UniqueWebpagesDictionary.TryAdd(absoluteUri.AbsoluteUri, linkMap);    //  HACK: What if it is unable to add
                                                    UniqueWebPageQueue.Enqueue(absoluteUri.AbsoluteUri);
                                                }
                                                
                                                node.SetAttributeValue(attributeName, placeHolder + fragment);

                                                #endregion
                                            }
                                            else
                                            {
                                                //throw new Exception(String.Format("Different Domain Url found. Uri : {0}"
                                                //    , absoluteUri.AbsoluteUri));
                                            }
                                        }
                                        else
                                        {
                                            //throw new Exception((String.Format("Scheme of the Uri : {0} was {1}"
                                            //    , absoluteUri.AbsoluteUri, absoluteUri.Scheme)));
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception(String.Format("Unable to create absoluteUri for RootUrl : {0} and relativeUri : {1}"
                                            , RootUrl, attributeValue));
                                    }
                                }
                            }
                            else
                            {
                                //TO LOG
                                //ErrorLogMethod(LOGTYPE.ERROR, $"Attribute Value was empty for {node.Name}, for Url : {absoluteUri.AbsoluteUri}", null);
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error while processing the node."), ex);
                        }
                    }
                }
                else
                {
                    ErrorLogMethod(LOGTYPE.INFORMATION, $"No Links found in {RootUrl.AbsoluteUri}", null);
                }
            }
            catch (Exception ex)
            {
                //  TODO: LOG inner exxception
                ErrorLogMethod(LOGTYPE.ERROR, $"Error Message : Error while finding links in {RootUrl.AbsoluteUri}", null);
            }
        }
        public void IdentifyJavaScript()
        {
            try
            {
                HtmlNodeCollection scriptNodes = KHtml.GetScriptsNodeCollection();
                if (scriptNodes != null)
                {
                    foreach (var node in scriptNodes)
                    {
                        try
                        {
                            #region Get attribute value

                            string attributeValue = String.Empty;
                            string attributeName = String.Empty;

                            switch (node.Name)
                            {
                                case "script":
                                    attributeValue = node.GetAttributeValue("src", String.Empty);
                                    attributeName = "src";
                                    break;
                                default:
                                    continue;
                            }

                            #endregion

                            #region Create absolute Uri (Also consider the Base Uri)

                            Uri absoluteUri = null;
                            // For handling basetag
                            if (KHtml.BaseTag.Exists)
                            {
                                Uri.TryCreate(KHtml.BaseTag.Href, attributeValue, out absoluteUri);
                            }
                            else
                            {
                                Uri.TryCreate(RootUrl, attributeValue, out absoluteUri);
                            }

                            #endregion

                            #region Process the Uri

                            if (absoluteUri != null)
                            {
                                absoluteUri = Utils.GenerateUriToProcess(absoluteUri, Resources.IgnoreFileNameChangeRegex);
                                string placeHolder = KHtmlParser.CheckUrlPresentInAssetOrNot(absoluteUri,Resources);
                                if (placeHolder!=null && placeHolder.Equals("IGNORE", StringComparison.InvariantCultureIgnoreCase))
                                    continue;
                                if (placeHolder == null)
                                {
                                    placeHolder = String.Format("[Kitsune_{0}]", absoluteUri.AbsoluteUri);

                                    AssetDetails javaScriptFileLink = new AssetDetails
                                    {
                                        LinkUrl = absoluteUri.AbsoluteUri,
                                        PlaceHolder = placeHolder
                                    };

                                    Resources.UniqueScriptsDictionary.TryAdd(absoluteUri.AbsoluteUri, javaScriptFileLink);
                                }
                                node.SetAttributeValue(attributeName, placeHolder);
                            }
                            else
                            {
                                //TO LOG
                                //ErrorLogMethod(LOGTYPE.ERROR, $"Attribute Value was empty for {node.Name}, for Url : {absoluteUri.AbsoluteUri}", null);
                            }

                            #endregion

                            #region Add to External Domain

                            if (!absoluteUri.Host.Equals(RootUrl.Host, StringComparison.OrdinalIgnoreCase))
                            {
                                Resources.ExternalDomains.Add(absoluteUri.Host.ToUpper());
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            ErrorLogMethod(LOGTYPE.ERROR, $"Error while processing the Node(IdentifyingJavaScript Method) for Url : {RootUrl.AbsoluteUri}", ex);
                        }
                    }
                }
                else
                {
                    ErrorLogMethod(LOGTYPE.INFORMATION, $"No Script Tag found for url : {RootUrl.AbsoluteUri}", null);
                }
            }
            catch (Exception ex)
            {
                ErrorLogMethod(LOGTYPE.ERROR, "Error While finding the script tag.", ex);
            }
        }
        public void IdentifyStyleSheets()
        {
            try
            {
                HtmlNodeCollection styleNodes = KHtml.GetExternalStylesNodeCollection();
                if (styleNodes != null)
                {
                    foreach (var node in styleNodes)
                    {
                        try
                        {
                            #region Get attribute value

                            string attributeValue = String.Empty;
                            string attributeName = String.Empty;

                            switch (node.Name)
                            {
                                case "link":
                                    attributeValue = node.GetAttributeValue("href", String.Empty);
                                    attributeName = "href";
                                    break;
                                default:
                                    continue;
                            }

                            #endregion

                            #region Create absolute Uri (Also consider the Base Uri)

                            Uri absoluteUri = null;
                            // For handling basetag
                            if (KHtml.BaseTag.Exists)
                            {
                                Uri.TryCreate(KHtml.BaseTag.Href, attributeValue, out absoluteUri);
                            }
                            else
                            {
                                Uri.TryCreate(RootUrl, attributeValue, out absoluteUri);
                            }

                            #endregion

                            #region Process the Uri

                            if (absoluteUri != null)
                            {
                                var rel = node.GetAttributeValue(@"rel", String.Empty).ToLower();
                                if (rel.Equals("stylesheet") || rel.Equals(String.Empty))
                                {
                                    absoluteUri = Utils.GenerateUriToProcess(absoluteUri, Resources.IgnoreFileNameChangeRegex);
                                    string placeHolder = CheckUrlPresentInAssetOrNot(absoluteUri,Resources);
                                    if (placeHolder!=null && placeHolder.Equals("IGNORE", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        continue;
                                    }
                                    if (placeHolder == null)
                                    {
                                        placeHolder = String.Format("[Kitsune_{0}]", absoluteUri.AbsoluteUri);

                                        AssetDetails cssFileLink = new AssetDetails
                                        {
                                            LinkUrl = absoluteUri.AbsoluteUri,
                                            PlaceHolder = placeHolder
                                        };

                                        Resources.UniqueStylesDictionary.TryAdd(absoluteUri.AbsoluteUri, cssFileLink);
                                    }
                                    node.SetAttributeValue(attributeName, placeHolder);
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                //TO LOG
                                //ErrorLogMethod(LOGTYPE.ERROR, $"Attribute Value was empty for {node.Name}, for Url : {absoluteUri.AbsoluteUri}", null);
                            }
                            #endregion

                            #region Add to External Domain

                            if (!absoluteUri.Host.Equals(RootUrl.Host, StringComparison.OrdinalIgnoreCase))
                            {
                                Resources.ExternalDomains.Add(absoluteUri.Host.ToUpper());
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            ErrorLogMethod(LOGTYPE.ERROR, "Error in IdentifyStyleSheets", ex);
                        }
                    }
                }
                else
                {
                    ErrorLogMethod(LOGTYPE.INFORMATION, $"No style tag found for url : {RootUrl.AbsoluteUri}", null);
                }
            }
            catch (Exception ex)
            {
                ErrorLogMethod(LOGTYPE.ERROR, "Error While finding the style tag.", ex);
            }
        }
        public void IdentifyAssets()
        {
            try
            {
                HtmlNodeCollection nodes = KHtml.DocumentNode.SelectNodes("//*[@src] | //*[@srcset]");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        #region src value

                        try
                        {
                            String srcValue = String.Empty;
                            srcValue = node.GetAttributeValue(@"src", String.Empty);
                            if (!String.IsNullOrEmpty(srcValue))
                            {
                                string placeHolder = ProcessAssetAndCreatePlaceHolder(node, srcValue);
                                if (placeHolder != null)
                                    node.SetAttributeValue("src", placeHolder);
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogMethod(LOGTYPE.ERROR, "Error while getting attribute Value", ex);
                        }

                        #endregion

                        #region srcset value

                        try
                        {
                            string srcsetValue = node.GetAttributeValue(@"srcset", String.Empty);
                            if (!String.IsNullOrEmpty(srcsetValue))
                            {
                                List<string> srcsetArray = srcsetValue.Split(',').Select(x => x.Trim(' ').Split(' ').First()).Distinct().ToList();
                                
                                foreach (var srcset in srcsetArray)
                                {
                                    try
                                    {
                                        if (!String.IsNullOrEmpty(srcset))
                                        {
                                            string placeHolder = ProcessAssetAndCreatePlaceHolder(node, srcset);
                                            if (placeHolder != null)
                                                srcsetValue = srcsetValue.Replace(srcset, placeHolder);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorLogMethod(LOGTYPE.ERROR,
                                            String.Format("Error while processing the srcset value:{0}", srcset), ex);
                                    }
                                }
                                node.SetAttributeValue("srcset", srcsetValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogMethod(LOGTYPE.ERROR, "Error while processing the node", ex);
                        }

                        #endregion
                    }
                }

                //TODO : assets like link[@href]
            }
            catch (Exception ex)
            {
                ErrorLogMethod(LOGTYPE.ERROR, "Error while IdentifyingAssets", ex);
            }
        }
        public void IdentifyInternalStyles()
        {
            try
            {
                HtmlNodeCollection stylesNodes = KHtml.GetEmbeddedStylesNodeCollection();
                if (stylesNodes != null)
                {
                    foreach (var node in stylesNodes)
                    {
                        try
                        {
                            string styleText = node.InnerHtml;
                            styleText = CrawlCss(styleText);
                            node.InnerHtml = styleText;
                        }
                        catch (Exception ex)
                        {
                            ErrorLogMethod(LOGTYPE.ERROR, "Error while processing the Node", ex);
                        }
                    }
                }
                HtmlNodeCollection styleAttributeNodes = KHtml.GetInnerStylesNodeCollection();
                if (styleAttributeNodes != null)
                {
                    foreach (var node in styleAttributeNodes)
                    {
                        try
                        {
                            string styleText = node.GetAttributeValue("style", String.Empty);
                            styleText = CrawlCss(styleText);
                            node.SetAttributeValue("style", styleText);
                        }
                        catch (Exception ex)
                        {
                            ErrorLogMethod(LOGTYPE.ERROR, "Error while processing the Node", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogMethod(LOGTYPE.ERROR, "Error while IdentifyInternalStyles", ex);
                throw ex;
            }
        }
        public string CrawlCss(string cssText)
        {
            try
            {
                if (String.IsNullOrEmpty(cssText))
                    throw new Exception("cssText cannot be null");
                var urls = CssParser.GetAllUris(cssText);
                foreach (var url in urls)
                {
                    try
                    {
                        Uri absoluteUri = null;
                        if (!String.IsNullOrEmpty(url.Key))
                            Uri.TryCreate(RootUrl, url.Key, out absoluteUri);
                        absoluteUri = Utils.GenerateUriToProcess(absoluteUri, Resources.IgnoreFileNameChangeRegex);
                        string placeHolder = CheckUrlPresentInAssetOrNot(absoluteUri,Resources);
                        if (placeHolder!=null && placeHolder.Equals("IGNORE", StringComparison.InvariantCultureIgnoreCase))
                            continue;
                        if (placeHolder == null)
                        {
                            placeHolder = String.Format("[Kitsune_{0}]", absoluteUri.AbsoluteUri);

                            AssetDetails cssFileLink = new AssetDetails
                            {
                                LinkUrl = absoluteUri.AbsoluteUri,
                                PlaceHolder = placeHolder
                            };

                            //TODO: what if not added
                            Resources.UniqueAssetsDictionary.TryAdd(absoluteUri.AbsoluteUri, cssFileLink);
                        }
                        cssText = cssText.Replace(url.Key,  placeHolder );
                    }
                    catch (Exception ex)
                    {
                        ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error while crawling css of Url : {0}", url), ex);
                    }
                }

                urls = CssParser.GetAllImportUris(cssText);
                foreach (var url in urls)
                {
                    try
                    {
                        Uri absoluteUri = null;
                        if (!String.IsNullOrEmpty(url.Key))
                            Uri.TryCreate(RootUrl, url.Key, out absoluteUri);
                        absoluteUri = Utils.GenerateUriToProcess(absoluteUri, Resources.IgnoreFileNameChangeRegex);
                        string placeHolder = CheckUrlPresentInAssetOrNot(absoluteUri,Resources);
                        if (placeHolder!=null && placeHolder.Equals("IGNORE", StringComparison.InvariantCultureIgnoreCase))
                            continue;
                        if (placeHolder == null)
                        {
                            placeHolder = String.Format("[Kitsune_{0}]", absoluteUri.AbsoluteUri);

                            AssetDetails cssFileLink = new AssetDetails
                            {
                                LinkUrl = absoluteUri.AbsoluteUri,
                                PlaceHolder = placeHolder
                            };

                            //TODO : what if tryAdd fails
                            Resources.UniqueStylesDictionary.TryAdd(absoluteUri.AbsoluteUri, cssFileLink);
                        }
                        cssText = cssText.Replace(url.Value, "@import \"" + placeHolder + "\"");
                    }
                    catch (Exception ex)
                    {
                        ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error while processing the Url:{0}", url), ex);
                    }
                }
                return cssText;
            }
            catch (Exception ex)
            {
                ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error while Crawling css for csstext:{0}", cssText), ex);
                return cssText;
            }
        }
        public string ProcessAssetAndCreatePlaceHolder(HtmlNode node, string url)
        {
            try
            {
                string urlToProcess = url;
                String placeHolder = null;
                if (!String.IsNullOrEmpty(urlToProcess))
                {
                    urlToProcess = WebUtility.HtmlDecode(urlToProcess);
                    var tagName = node.Name.ToLower();
                    Uri uri;
                    // For handling basetag
                    if (KHtml.BaseTag.Exists)
                    {
                        Uri.TryCreate(KHtml.BaseTag.Href, urlToProcess, out uri);
                    }
                    else
                    {
                        Uri.TryCreate(RootUrl, urlToProcess, out uri);
                    }
                    if (uri != null)
                    {

                        #region Process Uri

                        if (!tagName.Equals("script") && !tagName.Equals("iframe"))
                        {
                            uri = Utils.GenerateUriToProcess(uri, Resources.IgnoreFileNameChangeRegex);
                            placeHolder = KHtmlParser.CheckUrlPresentInAssetOrNot(uri,Resources);
                            if(placeHolder!=null && placeHolder.Equals("IGNORE",StringComparison.InvariantCultureIgnoreCase))
                            {
                                return null;
                            }
                            if (placeHolder == null)
                            {
                                placeHolder = String.Format("[Kitsune_{0}]", uri.AbsoluteUri);

                                AssetDetails assetLink = new AssetDetails();
                                assetLink.LinkUrl = uri.AbsoluteUri;
                                assetLink.PlaceHolder = placeHolder;

                                //TODO : what if try add fails
                                Resources.UniqueAssetsDictionary.TryAdd(uri.AbsoluteUri, assetLink);
                            }
                        }

                        #endregion

                        #region Adding to the externalDomain list 

                        if (!uri.Host.Equals(RootUrl.Host))
                        {
                            Resources.ExternalDomains.Add(uri.Host.ToUpper());
                        }

                        #endregion

                    }
                }
                else
                {
                    ErrorLogMethod(LOGTYPE.INFORMATION, "Url was null or Empty", null);
                }
                return placeHolder;
            }
            catch (Exception ex)
            {
                ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error while processing the Url: {0}", url), ex);
                return null;
            }
        }
        public static string CheckUrlPresentInAssetOrNot(Uri absoluteUrl,ResourcesContext resourcesContext)
        {
            try
            {
                if (resourcesContext.UniqueAssetsDictionary.ContainsKey(absoluteUrl.AbsoluteUri))
                {
                    return resourcesContext.UniqueAssetsDictionary[absoluteUrl.AbsoluteUri].PlaceHolder;
                }
                else if (resourcesContext.UniqueScriptsDictionary.ContainsKey(absoluteUrl.AbsoluteUri))
                {
                    return resourcesContext.UniqueScriptsDictionary[absoluteUrl.AbsoluteUri].PlaceHolder;
                }
                else if (resourcesContext.UniqueStylesDictionary.ContainsKey(absoluteUrl.AbsoluteUri))
                {
                    return resourcesContext.UniqueStylesDictionary[absoluteUrl.AbsoluteUri].PlaceHolder;
                }
                else if (resourcesContext.UniqueWebpagesDictionary.ContainsKey(absoluteUrl.AbsoluteUri))
                {
                    return resourcesContext.UniqueWebpagesDictionary[absoluteUrl.AbsoluteUri].PlaceHolder;
                }
                else if (!String.IsNullOrEmpty(resourcesContext.IncludeFilesRegex))
                {
                    Regex regex = new Regex(resourcesContext.IncludeFilesRegex, RegexOptions.IgnoreCase);
                    if (regex.IsMatch(absoluteUrl.PathAndQuery))
                    {
                        return null;
                    }
                }
                if (!String.IsNullOrEmpty(resourcesContext.ExcludeFilesRegex))
                {
                    Regex regex = new Regex(resourcesContext.ExcludeFilesRegex, RegexOptions.IgnoreCase);
                    if (regex.IsMatch(absoluteUrl.PathAndQuery))
                    {
                        return "IGNORE";
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while checking if the url present or not", ex);
            }
        }
        
    }
}
