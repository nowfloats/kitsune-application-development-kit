using Crawler.Helper;
using Crawler.HtmlParser;
using KitsuneWebsiteCrawlerService.AWSHelpers;
using KitsuneWebsiteCrawlerService.Constants;
using KitsuneWebsiteCrawlerService.Helpers;
using KitsuneWebsiteCrawlerService.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KitsuneWebsiteCrawlerService.Stages
{
    public class InitialiseKrawlerStageHelper
    {
        public static void InitialiseKrawler(string projectId, Uri uri)
        {
            try
            {
                #region Get the Details

                var screenshot = GetScreenShotOfUrl(projectId, uri);

                var faviconUrl = GetFaviconUrl(projectId, uri);

                var robotsResult = GetAndSaveRobotsTxt(projectId, uri);
                
                #endregion

                #region Wait for result and save in DB

                var screenShotUrl = screenshot.Result;
                if (String.IsNullOrEmpty(screenShotUrl) && String.IsNullOrEmpty(faviconUrl))
                    return;
                
                UpdateWebsiteDetails details = new UpdateWebsiteDetails { FaviconUrl = faviconUrl, ScreenShotUrl = screenShotUrl, ProjectId = projectId };

                APIHelper.UpdateWebsiteFaviconUrl(details);

                #endregion

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"ProjectId:{projectId}, Message:Error in initialising phase");
            }
        }



        /// <summary>
        /// Get the FaviconUrl from the given Uri
        /// returns null if not present
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetFaviconUrl(string projectId, Uri uri)
        {
            try
            {

                #region Get the favicon Icon Url

                Uri link = new Uri(uri.AbsoluteUri);

                WebClient webClient = new WebClient();
                webClient.Encoding = System.Text.Encoding.Default;
                var originalHtml = webClient.DownloadString(uri);


                KHtmlDocument doc = new KHtmlDocument();
                doc.LoadHtml(originalHtml);
                var faviconUri = doc.GetFaviconIcon(uri);

                #endregion

                #region Download the Favicon Icon

                webClient.Encoding = System.Text.Encoding.Default;
                Byte[] favicon = webClient.DownloadData(faviconUri);
                AmazonS3Helper.SaveTheFileInS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSAccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSSecretKey,
                    projectId + faviconUri.LocalPath, favicon, EnvironmentConstants.ApplicationConfiguration.AWSBuckets.SourceBucket.Name);

                #endregion

                return $"/{projectId}{faviconUri.LocalPath}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"ProjectId:{projectId}, Message:Error while searching favicon icon for Url : {uri.AbsoluteUri}");
                //EventLogger.Write(ex, String.Format("Error while searching favicon icon for Url : {0}", uri.AbsoluteUri), projectId);
                return null;
            }
        }

        public static bool GetAndSaveRobotsTxt(string projectId, Uri uri)
        {
            try
            {

                WebClient webClient = new WebClient();
                webClient.Encoding = System.Text.Encoding.Default;

                Byte[] robots = webClient.DownloadData(uri.AbsoluteUri + "robots.txt");
                if (robots != null)
                {
                    return AmazonS3Helper.SaveTheFileInS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSAccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSSecretKey,
                        projectId + uri.LocalPath + "robots.txt", robots, EnvironmentConstants.ApplicationConfiguration.AWSBuckets.SourceBucket.Name);
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"ProjectId:{projectId}, Message:Error while searching Robots.txt for Url : {uri.AbsoluteUri}");
                try
                {
                    Byte[] robotsValue = Encoding.ASCII.GetBytes("User-agent: * /n allow: /");
                    return AmazonS3Helper.SaveTheFileInS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSAccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWSSecretKey,
                        projectId + uri.LocalPath + "robots.txt", robotsValue, EnvironmentConstants.ApplicationConfiguration.AWSBuckets.SourceBucket.Name);
                }
                catch (Exception innerEx)
                {
                    Log.Error(innerEx, $"ProjectId:{projectId}, Message:Error while created a default Robots.txt for Url : {uri.AbsoluteUri}");
                }
                return false;
            }
        }
        


        private class ScreenShotResultModel
        {
            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty("future_link")]
            public string ScreenShotUrl { get; set; }
            [JsonProperty("code")]
            public int StatusCode { get; set; }
        }

        /// <summary>
        /// Call the screenshot API
        /// returns the future Url of the screenshot
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static async Task<string> GetScreenShotOfUrl(string projectId, Uri uri)
        {
            try
            {
                var requestObject = new { project_id = projectId, url = uri.AbsoluteUri };
                Uri screenShotUri = null;
                string screenShotUrl = EnvironmentConstants.ApplicationConfiguration.ScreenShotUrl;
                if (!Uri.TryCreate(screenShotUrl, UriKind.Absolute, out screenShotUri))
                {
                    throw new Exception($"Unable to create SplashAPI Uri, Url: {screenShotUrl}");
                }
                var response = HttpRequest.HttpPostRequestWithTimeOut(screenShotUri,EnvironmentConstants.ApplicationConfiguration.ScreenShotTimeOut, EnvironmentConstants.ApplicationConfiguration.KitsuneUserAgent, requestObject);
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    var responseObject = response.Content;
                    ScreenShotResultModel screenshotResult = JsonConvert.DeserializeObject<ScreenShotResultModel>(responseObject);
                    if (screenshotResult.StatusCode.Equals(200))
                    {
                        return screenshotResult.ScreenShotUrl;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"ProjectId:{projectId}, Message:Failed to take screenshot");
                return null;
            }
        }




    }
}
