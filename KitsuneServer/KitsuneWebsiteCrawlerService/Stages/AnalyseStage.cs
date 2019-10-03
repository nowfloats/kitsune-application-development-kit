using Crawler.Krawler;
using Crawler.Models;
using Kitsune.Models.Krawler;
using KitsuneWebsiteCrawlerService.AWSHelpers;
using KitsuneWebsiteCrawlerService.Constants;
using KitsuneWebsiteCrawlerService.Helpers;
using KitsuneWebsiteCrawlerService.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KitsuneWebsiteCrawlerService.Stages
{
    public class MigrationStageHelper
    {
        private static bool LimitCrossed = false;
        public static void AnalyseTheWebsite(string projectId, Uri uri, bool IsDeepCrawl)
        {
            try
            {
                string regexToIgnore = null;
                string regexToInclude = null;
                string ignore_link_conversion = null;
                List<string> include_static_asset_folder = null;
                Log.Information("Getting project config");
                var result = APIHelper.GetProjectConfig(projectId);
                if(result != null)
                {
                    var customSourceSyncSettings = result["custom_source_sync"];
                    if (customSourceSyncSettings == null)
                    {
                        //Unable to get the customsource settings
                    }
                    else
                    {
                    Log.Information("ProjectConfig : " + JsonConvert.SerializeObject(customSourceSyncSettings));
                        try
                        {
                            ProjectCustomSourceSyncSettings excludeList = JsonConvert.DeserializeObject<ProjectCustomSourceSyncSettings>(customSourceSyncSettings.ToString());
                            if (!String.IsNullOrEmpty(excludeList.Exclude))
                                regexToIgnore = excludeList.Exclude;
                            if (!string.IsNullOrEmpty(excludeList.Include))
                                regexToInclude = excludeList.Include;
                            ignore_link_conversion = excludeList.IgnoreLinkConversion;
                            include_static_asset_folder = excludeList.IncludeStaticAssets;
                            if (excludeList.IncludeStaticAssetApis != null)
                            {
                                var assetList = Utils.GetAllStaticAssetList(excludeList.IncludeStaticAssetApis);
                                if (assetList != null)
                                {
                                    if (include_static_asset_folder == null)
                                        include_static_asset_folder = new List<string>();
                                    include_static_asset_folder.AddRange(assetList);
                                }

                            }
                        }
                        catch(Exception ex)
                        {
                            Log.Error("ProjectCustomSourceSync : " + ex.Message);
                        }
                                           
                    }
                }
                
                Krawler krawler = new Krawler(uri);
                krawler.KrawlContext.ProcessedHtmlCallBackMethod = (filePath, htmlString) => SaveInS3(projectId, projectId + filePath, htmlString);
                krawler.KrawlContext.UpdatedResoucesCallBackMethod = (resourceObject) => UpdateAnalyseDetails(projectId, resourceObject);
                krawler.KrawlContext.BatchCompletedCallBackMethod = (CrawledPagesCount) => Batchcompleted(krawler,projectId, CrawledPagesCount);
                krawler.KrawlContext.ErrorLogMethod = (x, y, z) => LogError(x, y, z, projectId);
                krawler.KrawlContext.Configuration.MaxConcurrentThreads = 20;
                krawler.KrawlContext.Configuration.IsDeepCrawl = IsDeepCrawl;
                krawler.KrawlContext.Configuration.UserAgentString = EnvironmentConstants.ApplicationConfiguration.KitsuneUserAgent;

                if(!String.IsNullOrEmpty(ignore_link_conversion))
                    krawler.KrawlContext.Resources.IgnoreFileNameChangeRegex = new Regex(ignore_link_conversion);
                krawler.KrawlContext.Resources.ExcludeFilesRegex = regexToIgnore;
                krawler.KrawlContext.Resources.IncludeFilesRegex = regexToInclude;
                krawler.KrawlContext.Resources.IncludeStaticAssetList = include_static_asset_folder;
                krawler.Krawl();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error Analysing the Url for projectId : {projectId}");
                //TODO: Throw Error
                throw ex;
            }
        }
        public static bool UpdateAnalyseDetails(string projectId, ResourcesContext resources)
        {
            try
            {
                return MongoHelper.UpdateTheAnalyseStageDetailsInDB(projectId, resources);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating Analyse Details for ProjectId : {projectId}");
                return false;
            }
        }
        public static bool SaveInS3(string projectId, string filePath, string htmlString)
        {
            try
            {
                var bytes = Encoding.Default.GetBytes(htmlString);
                filePath = filePath.TrimStart('/');
                return AmazonS3Helper.SaveTheFileInS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSAccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSSecretKey
                    , filePath, bytes, EnvironmentConstants.ApplicationConfiguration.AWSBuckets.SourceBucket.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error saving the file : {filePath} for projectId : {projectId}");
                return false;
            }
        }
        public static bool Batchcompleted(Krawler krawlerContext,string projectId,int CrawledPagesCount)
        {
            try
            {
                //Check DB
                KitsuneKrawlerStats statsDetails =MongoHelper.GetCrawlStatsDetails(projectId);
                if((statsDetails.LinksLimit!=0 && CrawledPagesCount > statsDetails.LinksLimit) || statsDetails.StopCrawl)
                {
                    krawlerContext.KrawlContext.Configuration.IsStopCrawlEnabled = true;
                    LimitCrossed = true;
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static void LogError(LOGTYPE type, string message, Exception innerException, string projectId)
        {
            try
            {
                switch(type)
                {
                    case LOGTYPE.ERROR:
                        Log.Error(innerException,message);
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
    }
}
