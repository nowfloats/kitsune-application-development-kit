using Crawler.Downloader;
using Crawler.Models;
using Kitsune.Models.Krawler;
using KitsuneWebsiteCrawlerService.AWSHelpers;
using KitsuneWebsiteCrawlerService.Constants;
using KitsuneWebsiteCrawlerService.Helpers;
using KitsuneWebsiteCrawlerService.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Stages
{
    public class ResourcesStageHelper
    {
        public static void DownloadTheResources(string projectId, Uri uri)
        {
            try
            {
                #region Get Resources

                ResoucesDetails resources = MongoHelper.GetAllTheResourcesOfWebite(projectId);
                if (resources == null)
                    throw new Exception("Resources Object was Null");

                //Get all the selected Domains (If unable to find, then only download the Root Uri resources)
                List<string> selectedDomains = new List<string>();
                try
                {
                    selectedDomains = MongoHelper.GetSelectedDomainNames(projectId);
                    if (selectedDomains == null)
                        throw new Exception("Selected Domains is null");
                }
                catch (Exception ex)
                {
                    //TODO: Log the Error
                    selectedDomains = new List<string>();
                }
                selectedDomains.Add(uri.Host.ToUpper());

                #endregion

                #region Start Download

                FileDownloader downloader = new FileDownloader(uri);
                downloader.Context.DownloadedFileCallBackMethod = (AssetDetails asset, FileType fileType, Byte[] data,string contentType) => SaveInS3AndUpdateDB(projectId, asset, fileType, data,contentType);
                downloader.Context.NewAssetFoundDetailsCallBackMethod = (AssetDetails asset, FileType type) => NewAssetFound(projectId, asset, type);
                downloader.Context.Resources.Assets = resources.Assets;
                downloader.Context.Resources.Scripts = resources.Scripts;
                downloader.Context.Resources.Styles = resources.Styles;
                downloader.Context.Resources.SelectedDomains = selectedDomains;
                downloader.Context.Configuration.UserAgentString = EnvironmentConstants.ApplicationConfiguration.KitsuneUserAgent;
                downloader.Context.Configuration.ReadAndWriteTimeOut = 60000;
                downloader.Context.ErrorLogMethod = (LOGTYPE type, string message, Exception innerException) => LogError(type, message, innerException, projectId);
                downloader.Context.BatchCompletedCallBackMethod = () => Batchcompleted(downloader.Context, projectId);
                downloader.Execute();

                #endregion
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error while downloading the resources for projectId : {projectId}");
                throw ex;
            }
        }

        public static void NewAssetFound(string projectId, AssetDetails asset, FileType type)
        {
            try
            {
                MongoHelper.UpdateNewAssetToDB(projectId, asset, type);
            }
            catch (Exception ex)
            {
                if(asset!=null)
                    Log.Error(ex, $"Error updating the resources for projectId : {projectId} and asset : {asset.LinkUrl}");
            }
        }

        public static bool SaveInS3AndUpdateDB(string projectId, AssetDetails asset, FileType fileType, Byte[] bytes,string contentType)
        {
            try
            {
                //update the details
                var dbResult = MongoHelper.UpdateDownloadDetailsAsync(projectId, asset, fileType);

                //save the file
                if (bytes != null)
                    AmazonS3Helper.SaveTheFileInS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSAccessKey, 
                                                   EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSSecretKey,
                                                   String.Format("{0}{1}", projectId, asset.NewUrl),
                                                   bytes, 
                                                   EnvironmentConstants.ApplicationConfiguration.AWSBuckets.SourceBucket.Name,
                                                   contentType);


                return dbResult.Result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error saving in s3 for projectId : {projectId} and file : {asset.LinkUrl}");
                return false;
            }
        }

        public static void LogError(LOGTYPE type, string message, Exception innerException, string projectId)
        {
            try
            {
                switch (type)
                {
                    case LOGTYPE.ERROR:
                        Log.Error(innerException, $"ProjectId:{projectId}, Message:{message}");
                        break;
                    case LOGTYPE.INFORMATION:
                        Log.Information(innerException, $"ProjectId:{projectId}, Message:{message}");
                        break;
                    case LOGTYPE.USERINFO:
                        Log.Information($"[UserInfo] ProjectId:{projectId}, Message:(\"{message}\")");
                        break;
                    case LOGTYPE.WARNING:
                        Log.Warning(innerException, $"ProjectId:{projectId}, Message:{message}");
                        break;
                    case LOGTYPE.DEBUG:
                        Log.Debug(innerException, $"ProjectId:{projectId}, Message:{message}");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static bool Batchcompleted(FileDownloaderContext fileDownloaderContext,string projectId)
        {
            try
            {
                //Check DB
                KitsuneKrawlerStats statsDetails = MongoHelper.GetCrawlStatsDetails(projectId);
                if (statsDetails.StopCrawl)
                {
                    fileDownloaderContext.Configuration.IsStopDownloadEnabled = true;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
