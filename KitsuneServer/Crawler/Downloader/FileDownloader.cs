using Crawler.Helper;
using Crawler.Models;
using Kitsune.Models.Krawler;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crawler.Downloader
{
    public class FileDownloader
    {
        public FileDownloader(Uri uri)
        {
            Context = new FileDownloaderContext(uri);
        }

        public FileDownloaderContext Context { get; set; }

        public void Execute()
        {

            bool stopDownload = false;

            try
            {

                #region Initiate the Qeueue

                if (Context.Resources.Styles != null)
                    Context.Resources.Styles.ForEach(style => Context.FileDownloadQueue.Enqueue(new Tuple<AssetDetails, FileType>(style, FileType.STYLE)));
                if (Context.Resources.Scripts != null)
                    Context.Resources.Scripts.ForEach(script => Context.FileDownloadQueue.Enqueue(new Tuple<AssetDetails, FileType>(script, FileType.SCRIPT)));
                if (Context.Resources.Assets != null)
                    Context.Resources.Assets.ForEach(asset => Context.FileDownloadQueue.Enqueue(new Tuple<AssetDetails, FileType>(asset, FileType.ASSET)));
                
                #endregion

                //Start downloading the Files
                while (Context.FileDownloadQueue.Count != 0)
                {
                    int numberOfFilesInQueue = Context.FileDownloadQueue.Count;
                    int maxThreads = Context.Configuration.MaxConcurrentThreads;
                    int maxThreadsToCreate = (numberOfFilesInQueue >= maxThreads) ? maxThreads : numberOfFilesInQueue;

                    //Create the List of Files to download
                    List<Tuple<AssetDetails, FileType>> filesToProcess = new List<Tuple<AssetDetails, FileType>>();
                    Tuple<AssetDetails, FileType> link;
                    lock (Context.FileDownloadQueue)
                    {
                        for (int i = 0; i < maxThreadsToCreate; i++)
                        {
                            if (Context.FileDownloadQueue.TryDequeue(out link))
                            {
                                if (link != null)
                                    filesToProcess.Add(link);
                                else
                                    Context.ErrorLogMethod(LOGTYPE.INFORMATION,
                                        "Link was Emptyornull While getting the link from Webpagequeue", null);
                            }
                            else
                            {
                                Context.ErrorLogMethod(LOGTYPE.INFORMATION, "Unable to Dequeue from FileDownloadQueue", null);
                            }
                        }
                    }

                    //Download the Files in parallel
                    Parallel.ForEach(filesToProcess, (links) =>
                    {
                        try
                        {
                            Context.DownloadedFilesCount++;
                            Uri uri = null;
                            string url = links.Item1.LinkUrl;
                            
                            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                            {
                                if (Context.Resources.SelectedDomains.Contains(uri.Host.ToUpper()))
                                {
                                    Context.ErrorLogMethod(LOGTYPE.USERINFO, $"Downloading Url : {url}", null);
                                    ProcessTheResource(links.Item1, links.Item2,uri);
                                    Context.ErrorLogMethod(LOGTYPE.USERINFO, $"Downloading completed for url : {url}", null);
                                }
                            }
                            else
                            {
                                Context.ErrorLogMethod(LOGTYPE.ERROR, $"Error Message : Error while creating uri for url : {url}", null);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (links == null)
                                Context.ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error While processing the File"), ex);
                            else
                            {
                                Context.ErrorLogMethod(LOGTYPE.USERINFO, $"Error downloading url : {links.Item1.LinkUrl}", null);
                                Context.ErrorLogMethod(LOGTYPE.INFORMATION, String.Format("Error downloading script for Url: {0}", links.Item1.LinkUrl), ex);
                            }   
                        }
                    });

                    Context.BatchCompletedCallBackMethod();
                    if (Context.Configuration.IsStopDownloadEnabled)
                    {
                        stopDownload = true;
                        throw new Exception("Download stopped");
                    }

                }

                

            }
            catch (Exception ex)
            {
                Context.ErrorLogMethod(LOGTYPE.INFORMATION, String.Format("Error running the thread for downloading the styles"), ex);
                if (stopDownload)
                    throw new Exception("Crawl Stopped");
            }

        }

        public void ProcessTheResource(AssetDetails asset, FileType type,Uri uri)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));
            if (String.IsNullOrEmpty(asset.LinkUrl)) throw new ArgumentException(nameof(asset.LinkUrl));

            try
            {
                #region Download File
                
                string url = asset.LinkUrl;
                IRestResponse result = null;

                //Download the File
                result = HttpRequest.HttpRequestWithReadAndWriteTimeOut(uri, Context.Configuration.ReadAndWriteTimeOut, Context.Configuration.UserAgentString);

                
                //Check the StatuCode
                if (result.StatusCode.Equals(HttpStatusCode.OK))
                {
                    asset.ResponseStatusCode = HttpStatusCode.OK;
                }
                else
                {
                    asset.ResponseStatusCode = result.StatusCode;
                    Context.DownloadedFileCallBackMethod(asset, type, null,null);
                    throw new Exception(String.Format("Error downloading the File : {0}, Status Code : {1}",
                        asset.LinkUrl, result.StatusCode));
                }

                #endregion

                #region Process File

                var byteArray = result.RawBytes;

                if (type.Equals(FileType.STYLE) && Context.Configuration.CrawlCssEnabled)
                {
                    //CrawlCss
                    //and also update the new files found and push it to queue
                    String cssText = result.Content;
                    cssText = CrawlCss(cssText, uri);
                    byteArray=Encoding.Default.GetBytes(cssText);
                }

                var extension = KrawlerUtility.GetExtensionFromContentType(uri.LocalPath, result.ContentType);
                if (String.IsNullOrEmpty(extension))
                {
                    if (type.Equals(FileType.SCRIPT))
                        extension = ".js";
                    else if (type.Equals(FileType.STYLE))
                        extension = ".css";
                }
                string filePath = KrawlerUtility.GenerateFileLocalPath(uri, extension);
                if (String.IsNullOrEmpty(filePath))
                {
                    filePath = uri.LocalPath;
                    Context.ErrorLogMethod(LOGTYPE.INFORMATION, String.Format("file path generate was nulll or empty for url : {0}", uri.LocalPath), null);
                }

                #endregion

                #region Save File

                //Callback method to save the file
                asset.NewUrl = filePath;
                Context.DownloadedFileCallBackMethod(asset, type, byteArray,result.ContentType);


                #endregion

            }
            catch (Exception ex)
            {
                Context.ErrorLogMethod(LOGTYPE.INFORMATION, String.Format("Error downloading Url: {0}", asset.LinkUrl), ex);
            }
        }

        public string CrawlCss(string cssText, Uri RootUrl)
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
                        {
                            if (!Uri.TryCreate(RootUrl, url.Key, out absoluteUri))
                            {
                                continue;
                            }
                        }
                        else
                            continue;
                        string placeHolder = CheckUrlPresentInAssetOrNot(absoluteUri.AbsoluteUri);

                        //  If asset not present create placeholder and add it to asset
                        if (placeHolder == null)
                            placeHolder = AddNewAsset(absoluteUri.AbsoluteUri, FileType.ASSET);
                        cssText = cssText.Replace(url.Key, placeHolder);
                    }
                    catch (Exception ex)
                    {
                        Context.ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error while crawling css of Url : {0}", url), ex);
                    }
                }

                urls = CssParser.GetAllImportUris(cssText);
                foreach (var url in urls)
                {
                    try
                    {
                        Uri absoluteUri = null;
                        if (!String.IsNullOrEmpty(url.Key))
                        {
                            if (!Uri.TryCreate(RootUrl, url.Key, out absoluteUri))
                            {
                                continue;
                            }
                        }
                        else
                            continue;

                        string placeHolder = CheckUrlPresentInAssetOrNot(absoluteUri.AbsoluteUri);

                        //  If asset not present create placeholder and add it to asset
                        if (placeHolder == null)
                            placeHolder = AddNewAsset(absoluteUri.AbsoluteUri, FileType.STYLE);
                        cssText = cssText.Replace(url.Value, "@import \"" + placeHolder + "\"");
                    }
                    catch (Exception ex)
                    {
                        Context.ErrorLogMethod(LOGTYPE.ERROR, String.Format("Error while processing the Url:{0}", url), ex);
                    }
                }
                return cssText;
            }
            catch (Exception ex)
            {
                Context.ErrorLogMethod(LOGTYPE.INFORMATION, String.Format("Error while Crawling css for csstext:{0}", cssText), ex);
                return cssText;
            }
        }

        public string AddNewAsset(string url, FileType type)
        {
            try
            {
                string placeHolder = String.Format("[Kitsune_{0}]", url);

                AssetDetails cssFileLink = new AssetDetails
                {
                    LinkUrl = url,
                    PlaceHolder = placeHolder
                };

                AssetDetails asset = new AssetDetails()
                {
                    LinkUrl = url,
                    PlaceHolder = placeHolder
                };
                Context.FileDownloadQueue.Enqueue(new Tuple<AssetDetails, FileType>(asset, FileType.ASSET));
                Context.NewAssetFoundDetailsCallBackMethod(asset, type);
                Context.Resources.Assets.Add(asset);
                return placeHolder;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string CheckUrlPresentInAssetOrNot(string url)
        {
            try
            {
                AssetDetails result=Context.Resources.Assets.FirstOrDefault(x => x.LinkUrl.Equals(url));
                if (result != null)
                    return result.PlaceHolder;
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while checking if the url present or not", ex);
            }
        }

    }
}
