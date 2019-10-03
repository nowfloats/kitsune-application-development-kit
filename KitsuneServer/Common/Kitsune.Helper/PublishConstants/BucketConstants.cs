using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper.PublishConstants
{
    public class PublishBucketConstants
    {
        public static string KitsunePublishedBucketAccelratedLink { get { return ConfigurationManager.AppSettings["KitsunePublishedBucketAccelratedLink"]; } }
        public static string KitsuneDemoBucketName { get { return ConfigurationManager.AppSettings["KitsuneDemoBucketName"]; } }
        public static string KitsunePublishedBucketName { get { return ConfigurationManager.AppSettings["KitsunePublishedBucketName"]; } }

        public static string KitsunePublishSQSQueue { get { return ConfigurationManager.AppSettings["PublishSQSUrl"]; } }
    }
}
