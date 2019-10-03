using System;
using System.Collections.Generic;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Models
{
    public class Appsettings
    {
        public MongoDBConfiguration MongoDBConfiguration { get; set; }
        public AWSConfiguration AWSS3Configuration { get; set; }
        public AWSConfiguration AWSSQSConfiguration { get; set; }
        public AWSConfiguration AWSCloudWatchConfiguration { get; set; }
        public MongoDBCollections MongoDBCollections { get; set; }
        public AWSBuckets AWSBuckets { get; set; }
        public string APIDomain { get; set; }
        public int ScreenShotTimeOut { get; set; }
        public string KitsuneUserAgent { get; set; }
        public string ScreenShotUrl { get; set; }
        public string CrawlerSQSUrl { get; set; }
        public string CloudWatchLogGroup { get; set; }
    }

    public class MongoDBConfiguration
    {
        public string MongoConnectionUrl { get; set; }
        public string DataBaseName { get; set; }
    }

    public class AWSConfiguration
    {
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public string AWSProfileName { get; set; }
    }

    public class MongoDBCollections
    {
        public string KitsuneKrawlStatsCollection { get; set; }
    }

    public class AWSBuckets
    {
        public BucketDetails SourceBucket { get; set; }
    }
    
    public class BucketDetails
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

}
