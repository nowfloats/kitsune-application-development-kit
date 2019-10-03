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
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KitsuneWebsiteCrawlerService.Stages
{
    class PlaceHolderReplacerHelper
    {
        public static bool StopReplacer = false;
        public static void ReplacePlaceHolder(string projectId, Uri uri)
        {
            try
            {
                List<FindAndReplace> findAndReplaces = new List<FindAndReplace>();
                var projectConfig = APIHelper.GetProjectConfig(projectId);
                if (projectConfig != null)
                {
                    var customSourceSyncSettings = projectConfig["custom_source_sync"];
                    if (customSourceSyncSettings == null)
                    {
                        //Unable to get the customsource settings
                    }
                    else
                    {
                        ProjectCustomSourceSyncSettings customSourceSyncDetails = JsonConvert.DeserializeObject<ProjectCustomSourceSyncSettings>(customSourceSyncSettings.ToString());
                        if(customSourceSyncDetails != null && customSourceSyncDetails.FindAndReplace!=null)
                        {
                            findAndReplaces = customSourceSyncDetails.FindAndReplace;
                        }
                    }
                }
                

                ResoucesDetails resources = MongoHelper.GetAllTheResourcesOfWebite(projectId);
                List<AssetDetails> allResources = new List<AssetDetails>();
                allResources.AddRange(resources.Styles);
                allResources.AddRange(resources.Scripts);
                allResources.AddRange(resources.Links);
                allResources.AddRange(resources.Assets);

                Parallel.ForEach(resources.Links, new ParallelOptions { MaxDegreeOfParallelism = 20 }, (Link, loopState) =>
                {
                    Replace(Link, allResources, projectId, findAndReplaces, true);
                    Batchcompleted(projectId);
                    if(StopReplacer)
                        loopState.Stop();
                });

                if(StopReplacer)
                {
                    throw new Exception("Replacer force stop");
                }

                Parallel.ForEach(resources.Styles, new ParallelOptions { MaxDegreeOfParallelism = 20 }, (Style, loopState) =>
                {
                    Replace(Style, allResources, projectId, findAndReplaces, false);
                    Batchcompleted(projectId);
                    if (StopReplacer)
                        loopState.Stop();
                });

                Parallel.ForEach(resources.Scripts, new ParallelOptions { MaxDegreeOfParallelism = 20 }, (Script, loopState) =>
                {
                    Replace(Script, allResources, projectId, findAndReplaces, false);
                    Batchcompleted(projectId);
                    if (StopReplacer)
                        loopState.Stop();
                });
                if (StopReplacer)
                {
                    throw new Exception("Replacer force stop");
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"ProjectId:{projectId}, Message:Error Replacing PlaceHolder");
                if (StopReplacer)
                {
                    StopReplacer = false;
                    throw ex;
                }
                    
            }
        }

        public static void Replace(AssetDetails Link, List<AssetDetails> allResources, string projectId, List<FindAndReplace> findAndReplaces, bool updateDB)
        {
            try
            {
                if (Link.NewUrl == null)
                    return;
                if (!Link.ResponseStatusCode.Equals(HttpStatusCode.OK))
                    return;

                string originalHtml = string.Empty;
                string downloadString = String.Empty;

                WebClient webClient = new WebClient
                {
                    Encoding = System.Text.Encoding.Default
                };
                downloadString = String.Format("{0}/{1}{2}", EnvironmentConstants.ApplicationConfiguration.AWSBuckets.SourceBucket.Url, projectId, Link.NewUrl);
                originalHtml = webClient.DownloadString(downloadString);

                var count = Link.NewUrl.Count(x => x == '/');
                var charToAdd = "";
                if (count == 0 || count == 1)
                {
                    charToAdd = ".";
                }
                else if (count >= 2)
                {
                    charToAdd = "..";
                    count = count - 2;
                    while (count > 0)
                    {
                        charToAdd = "../" + charToAdd;
                        count--;
                    }
                }
                
                foreach (var file in allResources)
                {
                    try
                    {
                        if (file.NewUrl == null)
                            originalHtml = originalHtml.Replace(file.PlaceHolder, file.LinkUrl);
                        else
                            originalHtml = originalHtml.Replace(file.PlaceHolder, charToAdd + file.NewUrl);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"ProjectId:{projectId}, Message:Error Replacing PlaceHolder for Url: {Link.LinkUrl}");
                    }
                }

                #region Find and replace Regex

                if (findAndReplaces != null)
                {
                    foreach (var regex in findAndReplaces)
                    {
                        try
                        {
                            originalHtml = Regex.Replace(originalHtml, regex.Find, regex.Replace, RegexOptions.IgnoreCase);
                        }
                        catch(Exception ex)
                        {
                            Log.Error(ex, $"ProjectId:{projectId}, Message:Error Replacing PlaceHolder for Url: {Link.LinkUrl}");
                        }
                    }
                }

                #endregion

                var byteArray = Encoding.Default.GetBytes(originalHtml);
                Task task = null;
                if (updateDB)
                    task = MongoHelper.UpdateDBForPlaceHolderReplacer(projectId);

                AmazonS3Helper.SaveTheFileInS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSAccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSSecretKey,
                    String.Format("{0}{1}", projectId, Link.NewUrl), byteArray, EnvironmentConstants.ApplicationConfiguration.AWSBuckets.SourceBucket.Name);

                if (updateDB)
                    task.Wait();

            }
            catch (Exception ex)
            {
                string url = Link == null ? "url was null" : Link.LinkUrl;
                Log.Error(ex, $"ProjectId:{projectId}, Message:Error Replacing PlaceHolder for Url: {url}");
            }
        }

        public static bool Batchcompleted(string projectId)
        {
            try
            {
                //Check DB
                KitsuneKrawlerStats statsDetails = MongoHelper.GetCrawlStatsDetails(projectId);
                if (statsDetails.StopCrawl)
                {
                    StopReplacer = true;
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
