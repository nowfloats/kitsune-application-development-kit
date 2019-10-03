using Crawler.Helper;
using Crawler.HtmlParser;
using Crawler.Models;
using Kitsune.Models.Krawler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Krawler
{
    public class Krawler
    {
        public KrawlContext KrawlContext { get; set; }

        /// <summary>
        /// Constructor, rootUri is required
        /// </summary>
        /// <param name="rootUri"></param>
        public Krawler(Uri rootUri)
        {
            KrawlContext = new KrawlContext(rootUri);
        }

        /// <summary>
        /// Starts Crawling the given Root Url
        /// </summary>
        public void Krawl()
        {
            try
            {
                if (this.KrawlContext.RootUri == null)
                    throw new Exception("Error Message : Root Uri cannot be null.");

                bool stopCrawl = false;

                #region Initiate the Queue

                KrawlContext.UniqueWebPageQueue.Enqueue(KrawlContext.RootUri.AbsoluteUri);

                AssetDetails linkMapObj = new AssetDetails
                {
                    LinkUrl = KrawlContext.RootUri.AbsoluteUri,
                    PlaceHolder = "[kitsune_" + KrawlContext.RootUri.AbsoluteUri + "]"
                };

                //try adding the root url to Qeueue
                if (!KrawlContext.Resources.UniqueWebpagesDictionary.TryAdd(KrawlContext.RootUri.AbsoluteUri, linkMapObj))
                {
                    throw new Exception("Error Message : Unable to add the root url to Queue.");
                }
                
                #endregion

                #region Threads to Start crawl

                while (KrawlContext.UniqueWebPageQueue.Count != 0)
                {
                    try
                    {
                        int numberOfLinksInQueue = KrawlContext.UniqueWebPageQueue.Count;
                        int maxThreads = KrawlContext.Configuration.MaxConcurrentThreads;
                        int maxThreadsToCreate = (numberOfLinksInQueue >= maxThreads) ? maxThreads : numberOfLinksInQueue;

                        List<string> linksToProcess = new List<string>();
                        lock (KrawlContext.UniqueWebPageQueue)
                        {
                            for (int i = 0; i < maxThreadsToCreate; i++)
                            {
                                string link = String.Empty;
                                if (KrawlContext.UniqueWebPageQueue.TryDequeue(out link))
                                {
                                    if (!String.IsNullOrEmpty(link))
                                        linksToProcess.Add(link);
                                    else
                                        KrawlContext.ErrorLogMethod(LOGTYPE.ERROR,
                                            "Error Message : Link was Empty or Null While getting the link from Webpagequeue", null);
                                }
                                else
                                {
                                    KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, "Error Message : Unable to Dequeue from WebPageQueue.", null);
                                }
                            }
                        }

                        Parallel.ForEach(linksToProcess, (link) =>
                        {
                            try
                            {
                                Uri uri = new Uri(link);
                                KrawlContext.CrawledPagesCount++;
                                ProcessUri(uri);
                            }
                            catch (Exception ex)
                            {
                                if (!String.IsNullOrEmpty(link))
                                    KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error Message : Error while processing the Link : {0}", link), ex);
                                else
                                    KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error Message : Link was Empty or null while processing it"), ex);
                            }
                        });
                        
                        KrawlContext.BatchCompletedCallBackMethod(KrawlContext.CrawledPagesCount);
                        if(KrawlContext.Configuration.IsStopCrawlEnabled)
                        {
                            stopCrawl = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, ex.Message, ex);
                    }


                }

                #region PROCESS INCLUDE LIST

                ProcessIncludeList();

                try
                {
                    KrawlContext.UpdatedResoucesCallBackMethod(KrawlContext.Resources);
                }
                catch (Exception ex)
                {
                    KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, $"Error Message : Error while updating the DB", ex);
                }

                #endregion

                if (stopCrawl)
                    throw new Exception("Crawl Stopped");

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Process the given uri
        /// </summary>
        /// <param name="uri"></param>
        public void ProcessUri(Uri uri)
        {
            if (uri == null)
                throw new Exception("Error : Uri cannot be null.");
            try
            {
                KrawlContext.ErrorLogMethod(LOGTYPE.USERINFO, String.Format($"Processing Url:{uri.AbsoluteUri}"),null);
                string htmlContent = DownloadHtmlAndUpdateResources(uri);

                if (htmlContent != null)
                {
                    #region Initialise Html Document

                    KHtmlDocument htmlDocument = new KHtmlDocument();
                    htmlDocument.LoadHtml(htmlContent);
                    htmlDocument.IdentifyBaseTagAndSetValue();

                    #endregion

                    ProcessHtml(uri, htmlDocument);

                    #region Generate relative Url and call html callback function

                    if (KrawlContext.ProcessedHtmlCallBackMethod != null)
                    {
                        try
                        {
                            string path = KrawlerUtility.GenerateHtmlLocalPath(uri);
                            AssetDetails linkMap = new AssetDetails();
                            if (KrawlContext.Resources.UniqueWebpagesDictionary.TryGetValue(uri.AbsoluteUri, out linkMap))
                            {
                                linkMap.NewUrl = path;
                                KrawlContext.Resources.UniqueWebpagesDictionary[uri.AbsoluteUri] = linkMap;
                            }
                            KrawlContext.ProcessedHtmlCallBackMethod(path, htmlDocument.DocumentNode.InnerHtml);
                        }
                        catch (Exception ex)
                        {
                            KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, "Error Message : Error Generating relative Url or in html callback function", ex);
                        }
                    }

                    #endregion

                }

                #region Resouces updated Method call

                try
                {
                    KrawlContext.UpdatedResoucesCallBackMethod(KrawlContext.Resources);
                }
                catch (Exception ex)
                {
                    KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, $"Error Message : Error while updating the DB value of Url : {uri.AbsoluteUri}", ex);
                }

                #endregion

                KrawlContext.ErrorLogMethod(LOGTYPE.USERINFO, $"Processing Url : {uri.AbsoluteUri} completed.", null);
            }
            catch (Exception ex)
            {
                KrawlContext.ErrorLogMethod(LOGTYPE.USERINFO, $"Something went wrong while processing : {uri.AbsoluteUri}.", null);
                throw new Exception($"Error while processing the Url : {uri.AbsoluteUri}", ex);
            }
        }

        /// <summary>
        /// Process the given Html file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="htmlDocument"></param>
        public void ProcessHtml(Uri uri, KHtmlDocument htmlDocument)
        {
            if (uri == null)
                throw new Exception("Error : Uri cannot be null");
            try
            {
                #region Parse Html

                KHtmlParser htmlParser = new KHtmlParser(uri, htmlDocument)
                {
                    Resources = KrawlContext.Resources,
                    UniqueWebPageQueue = KrawlContext.UniqueWebPageQueue,
                    ErrorLogMethod = KrawlContext.ErrorLogMethod
                };
                htmlParser.Parse();

                #endregion

                #region Parse Css

                //TODO Parse inner styles
                try
                {
                    htmlParser.IdentifyInternalStyles();
                }
                catch (Exception ex)
                {
                    KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, "Error while parsing inner styles", ex);
                }

                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Adds Include list to the Resources
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="pathsToDownload"></param>
        public void ProcessIncludeList()
        {
            try
            {
                if(KrawlContext.Resources.IncludeStaticAssetList != null)
                {
                    foreach (var path in KrawlContext.Resources.IncludeStaticAssetList)
                    {
                        try
                        {
                            if (String.IsNullOrEmpty(path))
                                continue;
                            Uri absoluteUri = new Uri(KrawlContext.RootUri, path);
                            string placeHolder = KHtmlParser.CheckUrlPresentInAssetOrNot(absoluteUri, KrawlContext.Resources);
                            if (placeHolder == null || placeHolder.Equals("IGNORE", StringComparison.InvariantCultureIgnoreCase))
                            {
                                placeHolder = String.Format("[kitsune_{0}]", absoluteUri.AbsoluteUri);
                                AssetDetails linkMap = new AssetDetails
                                {
                                    PlaceHolder = placeHolder,
                                    LinkUrl = absoluteUri.AbsoluteUri
                                };

                                string fileExtension = Path.GetExtension(path).ToLower();
                                if (fileExtension == null)
                                    fileExtension = String.Empty;

                                switch (fileExtension.ToLower())
                                {
                                    case ".css":
                                        KrawlContext.Resources.UniqueStylesDictionary.TryAdd(absoluteUri.AbsoluteUri, linkMap);
                                        break;
                                    case ".js":
                                        KrawlContext.Resources.UniqueScriptsDictionary.TryAdd(absoluteUri.AbsoluteUri, linkMap);
                                        break;
                                    default:
                                        KrawlContext.Resources.UniqueAssetsDictionary.TryAdd(absoluteUri.AbsoluteUri, linkMap);
                                        break;
                                }

                            }
                        }
                        catch(Exception ex)
                        {
                            //Log Error
                            KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, $"Error Message : Error adding the include_asset_list_file path : {path}", ex);
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Download the Html File and update the Resources according to response
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string DownloadHtmlAndUpdateResources(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            try
            {
                string content = String.Empty;
                string contentType = String.Empty;
                Uri responseUri = null;
                Byte[] rawByte = null;

                #region Static Request

                var response = HttpRequest.HttpStaticCrawlGetRequest(uri, KrawlContext.Configuration.UserAgentString);

                if (response == null)
                {
                    throw new Exception(String.Format("Error : Response was null while requesting the Url : {0}", uri.AbsoluteUri));
                }

                #endregion

                #region Update linkmap of given Url(update status code)

                AssetDetails linkMapObject = null;
                try
                {
                    KrawlContext.Resources.UniqueWebpagesDictionary.TryGetValue(uri.AbsoluteUri, out linkMapObject);
                    linkMapObject.ResponseStatusCode = response.StatusCode;
                    KrawlContext.Resources.UniqueWebpagesDictionary[uri.AbsoluteUri] = linkMapObject;
                }
                catch (Exception ex)
                {
                    KrawlContext.ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error while Updating(linkmap) the Url : {0}", uri.AbsoluteUri), ex);
                }

                #endregion

                #region Checking Response of the Request Url and Deep Crawl if activated

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    contentType = response.ContentType;
                    responseUri = response.ResponseUri;

                    if (contentType.StartsWith("text/html", StringComparison.InvariantCultureIgnoreCase))
                    {
                        rawByte = response.RawBytes;
                        content = Encoding.Default.GetString(rawByte);
                    
                        #region Deep Crawling

                        if (KrawlContext.Configuration.IsDeepCrawl)
                        {
                            try
                            {
                                KrawlContext.ErrorLogMethod(LOGTYPE.USERINFO, $"Deep crawling Url : {uri.AbsoluteUri}", null);

                                string splashUrl = string.Format(KrawlerConstants.SplashApi, uri.AbsoluteUri);
                                Uri splashUri = new Uri(splashUrl);
                                response = HttpRequest.HttpRequestWithReadAndWriteTimeOut(
                                    splashUri, KrawlContext.Configuration.DeepCrawlingTimeOut, KrawlContext.Configuration.UserAgentString);

                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    if (response.RawBytes != null)
                                    {
                                        rawByte = response.RawBytes;
                                        content = Encoding.Default.GetString(rawByte);
                                    }
                                }
                                else
                                {
                                    KrawlContext.ErrorLogMethod(LOGTYPE.INFORMATION,
                                        String.Format("status code : {0}, Whie hitting splash API for Url : {1}",response.StatusCode, uri.AbsoluteUri), null);
                                }
                            }
                            catch (Exception ex)
                            {
                                KrawlContext.ErrorLogMethod(LOGTYPE.ERROR,
                                        String.Format("Error while hitting splash API for Url : {0}", uri.AbsoluteUri), ex);
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        #region remove the link from anchor dictionary and add it to asset dictionary

                        try
                        {
                            if (linkMapObject != null)
                            {
                                AssetDetails assetLink = new AssetDetails();
                                assetLink.LinkUrl = uri.AbsoluteUri;
                                assetLink.PlaceHolder = linkMapObject.PlaceHolder;

                                KrawlContext.Resources.UniqueAssetsDictionary.TryAdd(uri.AbsoluteUri, assetLink);

                                AssetDetails linkmap = new AssetDetails();
                                KrawlContext.Resources.UniqueWebpagesDictionary.TryRemove(uri.AbsoluteUri, out linkmap);
                            }
                        }
                        catch (Exception ex)
                        {
                            KrawlContext.ErrorLogMethod(LOGTYPE.ERROR,
                                        String.Format("Error while copying the linkmap to pathmap for Url : {0}", uri.AbsoluteUri), ex);
                        }
                        return null;

                        #endregion
                    }
                }
                else
                {
                    // Error Status code was not OK
                    KrawlContext.ErrorLogMethod(LOGTYPE.USERINFO, $"Error Message : {response.ErrorMessage} while requesting : {uri.AbsoluteUri}",null);
                    throw new Exception(String.Format("Error : Status Code was {0} and Error Message : {1}, while Requesting Url : {2}",
                        response.StatusCode, response.ErrorMessage, uri.AbsoluteUri));
                }

                #endregion                

                return content;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
