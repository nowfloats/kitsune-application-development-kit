using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper.CrawlerConstants
{
    public static class KitsuneKrawlerConstants
    {
        public static string KrawlerUserAgent { get { return ConfigurationManager.AppSettings["UserAgent"]; } }
        public static string KrawlerSQSUrl { get { return ConfigurationManager.AppSettings["KrawlerSQSUrl"]; } }
        public static string ScreenShotAPI { get { return "<ScreenShotAPIEndpoint>"; } }
        //timeout in millisec
        public static int ScreenShotTimeOut { get { return 10000; } }

        #region API

        public static string KrawlingCompletedUpdateKitsuneProjectsAPI { get { return "api/krawler/v1/updatekrawlcomplete?projectId={0}"; } }
        public static string UpdateKitsuneProjectStatus { get { return "api/krawler/v1/updateprojectstatus?projectId={0}&status={1}"; } }
        public static string UpdateWebsiteFaviconUrl { get { return "api/krawler/v1/UpdateWebsiteDetails"; } }

        #endregion
    }
}
    