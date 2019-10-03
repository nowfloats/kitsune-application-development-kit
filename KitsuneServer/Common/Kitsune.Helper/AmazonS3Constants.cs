using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper
{
    public class AmazonS3Constants
    {
        public static string KitsuneResourcesBucketUrl { get { return ConfigurationManager.AppSettings["ResourcesBucketUrl"]; } }
        public static string KitsuneResourcesBucketName { get { return ConfigurationManager.AppSettings["ResourceBucketName"]; } }
        
        public static string SourceBucketUrl { get { return ConfigurationManager.AppSettings["SourceBucketUrl"]; } }
        public static string SourceBucketName { get { return ConfigurationManager.AppSettings["SourceBucketName"]; } }

        public static string DemoBucketUrl { get { return ConfigurationManager.AppSettings["DemoBucketUrl"];  } }
        public static string DemoBucketName { get { return ConfigurationManager.AppSettings["DemoBucketName"];  } }

        public static string PlaceHolderBucketUrl { get { return ConfigurationManager.AppSettings["PlaceHolderBucketUrl"]; } }
        public static string PlaceHolderBucketName { get { return ConfigurationManager.AppSettings["PlaceHolderBucketName"]; } }

        public static string ProductionBucketUrl { get { return ConfigurationManager.AppSettings["ProductionBucketUrl"];  } }
        public static string ProductionBucketName { get { return ConfigurationManager.AppSettings["ProductionBucketName"];  } }

    }
}
