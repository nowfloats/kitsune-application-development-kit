using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.API2.EnvConstants
{
    public class AmazonAWSConstants
    {

        #region Amazon Access and Secret Key

        public static string S3AccessKey { get { return EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey; } }
        public static string S3SecretKey { get { return EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey; } }
        public static string SQSAccessKey { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey; } }
        public static string SQSSecretKey { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey; } }

        #endregion

        #region S3 Buckets

        public static string ResourceBucketUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Resource.Url; } }
        public static string ResourceBucketName { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Resource.Name; } }

        public static string SourceBucketUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Source.Url; } }
        public static string SourceBucketName { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Source.Name; } }

        public static string ApplicationBucketUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Application.Url; } }
        public static string ApplicationBucketName { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Application.Name; } }

        public static string DemoBucketUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Demo.Url; } }
        public static string DemoBucketName { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Demo.Name; } }

        public static string PlaceHolderBucketUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.PlaceHolder.Url; } }
        public static string PlaceHolderBucketName { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.PlaceHolder.Name; } }

        public static string ProductionBucketUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Production.Url; } }
        public static string ProductionBucketName { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.Production.Name; } }

        public static string WebsiteFilesBucketUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.WebsiteFiles.Url; } }
        public static string WebsiteFilesBucketName { get { return EnvironmentConstants.ApplicationConfiguration.AWSBuckets.WebsiteFiles.Name; } }

        #endregion

        #region SQS Urls

        public static string KrawlerSQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.Crawler; } }
        public static string DownloadSQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.Downloader; } }
        public static string CompilerSQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.Compiler; } }
        public static string ApplicationBuildSQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.ApplicationBuild; } }
        public static string PublishSQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.Publish; } }
        public static string MigrationReportSQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.MigrationReport; } }
        public static string TranspilerSQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.Transpiler; } }
        public static string ActivitySQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.SitemapService; } }
        public static string SitemapServiceSQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.SitemapService; } }
        public static string S3FolderCopySQSUrl { get { return EnvironmentConstants.ApplicationConfiguration.AWSSQSUrls.S3FolderCopy; } }

        #endregion

    }
}
