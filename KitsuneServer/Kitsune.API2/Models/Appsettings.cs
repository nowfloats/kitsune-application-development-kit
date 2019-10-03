using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Models
{
    public class Appsettings
    {
        public DBConnectionStrings DBConnectionStrings { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaDatabaseName { get; set; }
        public int MongoQueryMaxtimeOut { get; set; }
        public CloudWatchLog CloudWatchLogger { get; set; }
        public AWSConfiguration AWSSQSConfiguration { get; set; }
        public AWSConfiguration AWSS3Configuration { get; set; }
        public AWSConfiguration AWSCDNConfiguration { get; set; }
        public AWSConfiguration AWSKinesisConfiguration { get; set; }
        public AWSBucketList AWSBuckets { get; set; }
        public AWSSQSUrlList AWSSQSUrls { get; set; }

        public StripeDetails StripeDetails { get; set; }

        public InstamojoDetails InstamojoDetails { get; set; }
        public WordPressConfiguration WordPressConfiguration { get; set; }

        public string KitsuneIdentifierDistributionId { get; set; }
        public string KitsuneIdentifierKitsuneSubDomain { get; set; }
        public string KitsuneDemoDomain { get; set; }
        public string KitsuneRedirectIPAddress { get; set; }

        public string ScreenShotAPIUrl { get; set; }
        public string RoutingAPIDomain { get; set; }
        public string CloudProviderCredentialsEncryptionKey { get; set; }

        public string CloudFrontDistributionId { get; set; }
        public bool IsDev { get; set; }
        public Defaults Defaults { get; set; }
    }

    public class DBConnectionStrings
    {
        public string KitsuneDBMongoConnectionUrl { get; set; }
        public string KitsuneSchemaDBMongoConnectionUrl { get; set; }
        public string KitsuneWebLogConnectionUrl { get; set; }
        public string FPDBMongoConnectionUrl { get; set; }

        public string GWTMySQLConnectionUrl { get; set; }
    }

    public class StripeDetails
    {
        public string StripeAPIKey { get; set; }
        public string StripeAPISecretKey { get; set; }
        public string StripeAPIUrl { get; set; }
    }

    public class InstamojoDetails
    {
        public string InstamojoWebhook { get; set; }
        public string InstaMojoAPIKey { get; set; }
        public string InstaMojoAPIToken { get; set; }
        public string InstaMojoAPIUrl { get; set; }
        public string WalletUpdateUrl { get; set; }
    }

    public class AWSConfiguration
    {
        public string AWS_AccessKey { get; set; }
        public string AWS_SecretKey { get; set; }
        public string AWS_ProfileName { get; set; }
    }

    public class WordPressConfiguration
    {
        public string CreatingNewWordPressInstanceAPI { get; set; }
    }

    public class AWSBucketList
    {
        public AWSBucket Resource { get; set; }
        public AWSBucket Source { get; set; }
        public AWSBucket Application { get; set; }
        public AWSBucket Demo { get; set; }
        public AWSBucket PlaceHolder { get; set; }
        public AWSBucket Production { get; set; }
        public AWSBucket WebsiteFiles { get; set; }
    }
    public class AWSSQSUrlList
    {
        public string Downloader { get; set; }
        public string Crawler { get; set; }
        public string Compiler { get; set; }
        public string Optimizer { get; set; }
        public string Publish { get; set; }
        public string MigrationReport { get; set; }
        public string KString { get; set; }
        public string SitemapService { get; set; }
        public string S3FolderCopy { get; set; }
        public string Transpiler { get; set; }
        public string ApplicationBuild { get; set; }
    }
    public class AWSBucket
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
    public class CloudWatchLog
    {
        public string LogGroupName { get; set; }
        public string Regionn { get; set; }
    }
    public class Defaults
    {
        public string AWSSQSRegion { get; set; }
    }
}
