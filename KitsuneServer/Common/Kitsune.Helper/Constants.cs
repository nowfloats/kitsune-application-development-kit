using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Kitsune.Helper
{
    public static class Constants
    {

        public static string KitsuneUserAgent { get { return ConfigurationManager.AppSettings["KitsuneUserAgent"]; } }

        #region Database Collection
        public static string ProductionThemeCollection { get { return "ProductionThemes"; } }
        public static string ProductionPageCollection { get { return "ProductionThemePages"; } }
        public static string PendingThemesCollection { get { return "PendingThemes"; } }
        public static string PendingPageCollection { get { return "PendingThemePages"; } }
        public static string DraftThemesCollection { get { return "DraftThemes"; } }
        public static string DraftPageCollection { get { return "DraftThemePages"; } }

        #region Kitsune Merge

        public static string ProductionKitsuneProjectCollection { get { return ConfigurationManager.AppSettings["ProductionKitsuneProjectCollection"]; } }
        public static string ProductionKitsuneResourcesCollection { get { return ConfigurationManager.AppSettings["ProductionKitsuneResourcesCollection"]; } }
        public static string AuditKitsuneProjectCollection { get { return ConfigurationManager.AppSettings["AuditKitsuneProjectCollection"]; } }
        public static string AuditKitsuneResourcesCollection { get { return ConfigurationManager.AppSettings["AuditKitsuneResourcesCollection"]; } }
        public static string KitsuneProjectCollection { get { return ConfigurationManager.AppSettings["KitsuneProjectCollection"]; } }
        public static string KitsuneResourcesCollection { get { return ConfigurationManager.AppSettings["KitsuneResourcesCollection"]; } }
        public static string KitsuneKrawlStatsCollection { get { return ConfigurationManager.AppSettings["KitsuneKrawlStatsCollection"]; } }

        #endregion


        public static string CompilerVersion { get { return "CompilerVersion"; } }
        public static string KLMAuditLogCollection { get { return "KLMAuditLogCollection"; } }
        public static string KitsuneEnquiryCollection { get { return "KitsuneEnquiryCollection"; } }
        public static string MongoCompilerConnectionUrlKey { get { return "MongoConnectionUrl"; } }
        public static string MongoUserConnectionUrlKey { get { return "UserMongoConnectionUrl"; } }

        public static string PiwikVisitorsData { get { return "PiwikVisitorsData"; } }
        public static string PiwikActionData { get { return "PiwikActionData"; } }


        public static string WebsiteCollection { get { return "websites"; } }
        public static string InvoiceCollection { get { return "KitsuneInvoice"; } }
        public static string UserCollection { get { return "users"; } }
        public static string WalletStats { get { return "walletStats"; } }
        public static string PaymentTransactionCollectionLog { get { return "PaymentTransactionLogs"; } }
        public static string WebActionCollection { get { return "WebActions"; } }
        public static string LanguageCollection { get { return "KitsuneLanguages"; } }
        public static string WebsiteWebActionsCollection { get { return "WebsiteWebActions"; } }
        public static string WebsiteSchemasCollection { get { return "WebsiteSchemas"; } }
        #endregion

        #region APIUrls

        #region DeveloperAPI
        public static string GetUserDetails { get { return "api/Developer/v1/UserProfile?useremail={0}"; } }
        public static string UpdateUserDetails { get { return "api/Developer/v1/UpdateuserDetails"; } }
        public static string GetUserPaymentDetails { get { return "api/Developer/v1/PaymentDetails?useremail={0}"; } }
        public static string GetDeveloperDebitDetails { get { return "api/Developer/v1/GetDebitDetails?useremail={0}&component={1}&monthAndYear={2}"; } }
        public static string GetUserIdQuery { get { return "api/Developer/v1/GetUserId?useremail={0}"; } }
        #endregion

        #region ProjectAPI
        public static string CompilerAPIBaseUrl = "api/Project/";
        public static string GetProjectAndResourcesDetailsQuery { get { return CompilerAPIBaseUrl + "v1/ProjectAndResources/{0}?userEmail={1}"; } }
        public static string GetUserProjectsQuery { get { return CompilerAPIBaseUrl + "v1/Projects?userEmail={0}"; } }
        //public static string GetAllProjectsQuery { get { return CompilerAPIBaseUrl + "v1/Themes?userEmail={0}&getAll=true"; } }
        public static string GetProjectDetailsQuery { get { return CompilerAPIBaseUrl + "v1/ProjectDetails/{0}?userEmail={1}"; } }
        public static string GetProjectResourceDetailsQuery { get { return CompilerAPIBaseUrl + "v1/Project/{0}/Resource?sourcePath={1}&userEmail={2}"; } }
        public static string GetPartialPagesDetailsQuery { get { return CompilerAPIBaseUrl + "v1/Project/{0}/PartialPages?sourcePath={1}&userEmail={2}"; } }
        public static string CreateOrUpdateProjectCommand { get { return CompilerAPIBaseUrl + "v1/Project"; } }
        public static string CreateNewProjectCommand { get { return CompilerAPIBaseUrl + "v1/CreateNewProject"; } }
        public static string UpdateProjectVersionCommand { get { return CompilerAPIBaseUrl + "v1/Project/{0}/UpdateVersion?userEmail={1}&version={2}&publishedVersion={3}"; } }
        public static string CreateOrUpdateResourceCommand { get { return CompilerAPIBaseUrl + "v1/Resource"; } }
        public static string DeleteProjectCommand { get { return CompilerAPIBaseUrl + "v1/Projects/{0}?userEmail={1}"; } }
        public static string DeleteResourceCommand { get { return CompilerAPIBaseUrl + "v1/Projects/{0}/Resources?userEmail={2}&sourcePath={1}"; } }
        //public static string UpdateProjectLogCommand { get { return CompilerAPIBaseUrl + "v1/Log"; } }
        public static string KitsuneEnquiryCommand { get { return CompilerAPIBaseUrl + "v1/KitsuneEnquiry"; } }
        public static string GetCompilerVersion { get { return CompilerAPIBaseUrl + "v1/CompilerVersion"; } }
        public static string MakeReosurceAsDefaultCommand { get { return CompilerAPIBaseUrl + "v1/Project/{0}/MakeResourceDefault?sourcePath={1}&userEmail={2}"; } }
        public static string GetProjectDetailsForBuild { get { return CompilerAPIBaseUrl + "v1/ProjectDetailForBuild/{0}?userEmail={1}"; } }
        public static string GetSubmittedThemes { get { return "api/Compiler/Admin/Themes?user={0}"; } }
        public static string GetSubmittedThemesByStatus { get { return "api/Compiler/Admin/Themes/Status/{0}?user={1}"; } }
        public static string GetSubmittedThemeDetails { get { return "api/Compiler/Admin/Theme/{0}/{1}/{2}?user={3}"; } }
        public static string GetSubmittedPageDetails { get { return "api/Compiler/Admin/Page/{0}/{1}/{2}/{3}?user={4}"; } }
        public static string GetAuditProjectAndResourcesDetails { get { return CompilerAPIBaseUrl + "Admin/ProjectAndResources/{0}/{1}?userEmail={2}"; } }
        public static string GetAllAuditProjectAndResourcesDetails { get { return CompilerAPIBaseUrl + "Admin/AllProjectAndResources/{0}?userEmail={1}"; } }
        public static string GetThemeStatus { get { return "Admin/Status/Theme/{0}/{1}/{2}?user={3}"; } }
        public static string UploadResource { get { return CompilerAPIBaseUrl + "v1/Project/{0}/UploadResource?sourcePath={1}&isPath={2}"; } }
        public static string SaveFileContentToS3 { get { return CompilerAPIBaseUrl + "v1/Project/SaveFileContentToS3"; } }
        public static string GetFileFromS3 { get { return CompilerAPIBaseUrl + "v1/Project/GetFileFromS3"; } }
        public static string DeleteResource { get { return CompilerAPIBaseUrl + "/v1/Project/{0}/DeleteResource?resourceKey={1}"; } }
        public static string GetProjetResources { get { return CompilerAPIBaseUrl + "v1/Project/{0}/GetProjectResources"; } }
        public static string VersionAssets { get { return CompilerAPIBaseUrl + "v1/Project/{0}/VersionResources?version={1}"; } }
        #endregion

        #region AdminAPI
        public static string UpdateAuditProject { get { return CompilerAPIBaseUrl + "Admin/Project"; } }
        public static string UpdateSubmittedResource { get { return CompilerAPIBaseUrl + "Admin/Resource"; } }
        public static string CreateAuditProject { get { return CompilerAPIBaseUrl + "Admin/Create/Project"; } }


        public static string CreateProductionProject { get { return CompilerAPIBaseUrl + "Admin/Live/Project"; } }
        public static string DeleteProductionProject { get { return CompilerAPIBaseUrl + "Admin/UnLive/Project/{0}?userEmail={1}"; } }
        public static string GetProductionProjectDetails { get { return CompilerAPIBaseUrl + "Admin/ProductionProject/{0}?userEmail={1}"; } }
        #endregion

        #region BuildAndPublishAPI
        public static string UpdateBuildStatus { get { return CompilerAPIBaseUrl + "v1/Build?user={0}"; } }
        public static string GetLastCompletedBuild { get { return CompilerAPIBaseUrl + "v1/LastCompletedBuild?user={0}&projectid={0}"; } }
        public static string PublishProject { get { return CompilerAPIBaseUrl + "v1/Publish?userEmail={0}&customerId={1}"; } }
        #endregion

        #endregion
        public static string CSRFReplacementToken = "##KIT_CSRF_TOKEN##";

        #region Compiler regex
        /// <summary>
        /// Updated the regex to compiled regex
        /// </summary>
        public static Regex WidgetRegulerExpression = new Regex(@"\[\[+(.*?)\]\]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex PropertyWithFunctionRegulerExpression = new Regex(@"\[\[+\((.*?)\)\]\]+", RegexOptions.Compiled);
        public static Regex PropertyArrayExpression = new Regex(@"\[(.*)\]", RegexOptions.Compiled);
        public static Regex FunctionRegularExpression = new Regex(@"\[\[+\w+.\w+(\((?:[()]|[^()])*\))\]\]", RegexOptions.Compiled);
        public static Regex PaginationRegularExpression = new Regex(@"\[\[\s?paginate(\((?:[()]|[^()])*\))\s?\]\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex KDLRegularExpression = new Regex(@"(View(\((?:[()]|[^()])[_\w\.\'\-\/]+\)))", RegexOptions.Compiled);
        public static Regex FunctionParameterExpression = new Regex(@"\((.*?)\)", RegexOptions.Compiled);
        public static Regex SetObjectRegularExpression = new Regex(@"\[\[(?:\s?|\s+)+\w+.\w+.\w+(\((?:\(\)[()]|[^()])*\))(?:\s?|\s+)\]\]", RegexOptions.Compiled);
        public static Regex PartialPageRegularExpression = new Regex(@"\[\[\s*Partial(\((?:[()]|[^()])[_\w\.\'\/\-]+\))\s*\]\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex ViewClassRegularExpression = new Regex(@"\[\[.*?(View(\((?:[()]|[^()])[_\w\.\'\-\/]+\))).*?\]\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex DynamicFileExtensionRegularExpression = new Regex(@"\.(html|htm|htm.dl|html.dl)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex ConfigFileRegularExpression = new Regex(@"/(kitsune-settings.json)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex KRepeatPatternRegex = new Regex(@"\[\[(.*)(\,)(.*)(\,)(.*)(\:)(.*)\]\]", RegexOptions.Compiled);
        public static Regex SupportedResourcePathRegularExpression = new Regex(@"[^0-9A-Za-z\-\/\._]+", RegexOptions.Compiled);
        public static Regex BlockNodeUniqueIdRegex = new Regex(@"##k-([0-9]+)##", RegexOptions.Compiled);
        public static Regex CSRFRegularExpression = new Regex(@"\[\[kitsune_csrf.token\]\]|kcsrf\.getToken\(\)\;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static Regex GetKObjectIteratorRegex(string kobject)
        {
            //for k-object=[[product in business.products]]
            //[[product.name]]
            return new Regex($@"\[\[(.*?)(\b{kobject}\b)\.?(.*?)\]\]");
        }
        public static Regex GeCustomVariableRegex(string var)
        {
            return new Regex($@"\[\[(.*?)(\b{var}\b)\.?(.*?)\]\]", RegexOptions.IgnoreCase);
        }
        public static Regex GetCustomVariableReplacementRegex(string var)
        {
            return new Regex($@"([^\.])(\b{var}\b)", RegexOptions.IgnoreCase);
        }

        public static Regex GetKObjectReplaceParamRegex(string iterator, bool ignoreCase = false)
        {
            iterator = iterator.Replace(".", "\\.");
            iterator = iterator.Replace("[", "\\[");
            iterator = iterator.Replace("]", "\\]");
            iterator = iterator.Replace("(", "\\(");
            iterator = iterator.Replace(")", "\\)");
            if (ignoreCase)
                return new Regex($@"(\b{iterator}\b)", RegexOptions.IgnoreCase);
            return new Regex($@"(\b{iterator}\b)");
        }
        public static Regex GetRepeatForEachParamRegex(string iterator, bool ignoreCase = false)
        {
            //for k-repeat=[[product in business.products]]
            //[[product.name]]
            //TODO : check if this can be same as k-object regex
            iterator = iterator.Replace(".", "\\.");
            iterator = iterator.Replace("[", "\\[");
            iterator = iterator.Replace("]", "\\]");
            iterator = iterator.Replace("(", "\\(");
            iterator = iterator.Replace(")", "\\)");
            if (ignoreCase)
                return new Regex($@"({iterator})", RegexOptions.IgnoreCase);
            return new Regex($@"({iterator})");
        }
        #endregion

        #region Language

        public static string[] DataTypeClasses { get { return new string[] { "str", "date", "number", "boolean", "kstring", "phonenumber" }; } }

        public static string CreateOrUpdateLanguage { get { return "Language/v1/CreateOrUpdateLanguageEntity"; } }
        public static string GetLanguage { get { return "Language/v1/{0}"; } }
        public static string KPayDefaultSchema { get { return "_System"; } }

        #endregion

        #region KLM
        public static string GetThemesByCategory { get { return "api/KLM/v1/Category/{0}"; } }
        public static string GetThemeDetailsKLM { get { return "api/KLM/v1/Theme/{0}"; } }
        public static string GetThemeIdForUser { get { return "api/KLM/v1/GetThemeId?ipAddress={0}&tag={1}"; } }
        public static string GetProductionViewHTML { get { return "api/KLM/v1/GetHTML/Theme/{0}/Page/{1}"; } }
        public static string CreateAndUpdateKMLAuditLog { get { return "api/KLM/v1/AuditLog"; } }

        #endregion

        #region Dashboard

        public static string GetTotalCrawler { get { return "api/Dashboard/TotalCrawler?form={0}&to={1}"; } }
        public static string GetAvgCrawlers { get { return "api/Dashboard/AvgCrawlers?form={0}&to={1}"; } }
        public static string GetAvgCrawlerResponseTime { get { return "api/Dashboard/AvgCrawlerResponseTime?form={0}&to={1}"; } }

        public static string GetTotalVisitors { get { return "api/Dashboard/TotalVisitors?form={0}&to={1}"; } }
        public static string GetMedianSpentTime { get { return "api/Dashboard/MedianSpentTime?form={0}&to={1}"; } }
        public static string GetMedianLoadTime { get { return "api/Dashboard/MedianLoadTime?form={0}&to={1}"; } }
        public static string GetBounceRate { get { return "api/Dashboard/BounceRate?form={0}&to={1}"; } }

        public static string GetCrawlersData { get { return "api/Dashboard/CrawlersData?form={0}&to={1}&skip={2}&limit={3}"; } }
        public static string GetVisitorsData { get { return "api/Dashboard/VisitorsData?form={0}&to={1}&skip={2}&limit={3}"; } }

        #endregion

        
        #region ConverToKitsune
        public static string ConvertToKitsuneDbUrl { get { return ConfigurationManager.ConnectionStrings["mongoConnectionUrl"].ConnectionString; } }
        public static string ConvertToKitsuneCdnConnectionString { get { return ConfigurationManager.AppSettings["ConvertToKitsuneCdnConnectionString"]; } }
        public static string ConvertToKitsuneS3ConnectionString { get { return ConfigurationManager.AppSettings["ConverToKitsuneS3ConnectionString"]; } }
        public static string ConvertToKitsuneDatabaseName { get { return ConfigurationManager.AppSettings["databaseName"]; } }
        public static string ConvertToKitsuneUrlQueueCollection { get { return "ConvertToKitsuneQueue"; } }
        public static string ConvertToKitsuneCrawlCollection { get { return "ConvertToKitsuneDetails"; } }
        public static string KitsuneProjectsCollectionName { get { return ConfigurationManager.AppSettings["CrawlCollectionName"]; } }
        public static string WebformsCollectionName { get { return ConfigurationManager.AppSettings["WebformsCollectionName"]; ; } }
        public static string ConvertToKitsuneCrawlLinksStatsCollectionName { get { return ConfigurationManager.AppSettings["CrawlStatsCollectionName"]; } }
        public static string KitsunePublishStatsCollectionName { get { return ConfigurationManager.AppSettings["KitsunePublishStats"]; } }
        public static string KitsuneBuildStatsCollection { get { return ConfigurationManager.AppSettings["KitsuneBuildStats"]; } }
        public static string KitsuneBuildStatusCollection { get { return ConfigurationManager.AppSettings["KitsuneBuildStatusCollection"]; } }
        public static string KitsunePublishStatusCollection { get { return ConfigurationManager.AppSettings["KitsunePublishStatusCollection"]; } }
        public static string ZipFolderStatsCollection { get { return ConfigurationManager.AppSettings["ZipFolderStats"]; } }
        public static string KitsuneCustomerCollection { get { return ConfigurationManager.AppSettings["KitsuneCustomerCollection"]; } }
        public static string KitsuneBillingCollection { get { return ConfigurationManager.AppSettings["KitsuneBillingCollection"]; } }

        public static string ConvertToKitsuneUploadAsset { get { return "api/Conversion/v1/Bucket/{0}/UploadKitsuneConversionAsset?&assetFileName={1}"; } }
        public static string ConvertToKitsuneCreateNewS3Bucket { get { return "api/Conversion/v1/CreateBucket"; } }
        public static string ConvertToKitsuneCreateNewCLoudFrontDistribution { get { return "api/Conversion/v1/CreateDistribution"; } }
        public static string ConvertToKitsuneInvalidateCDN { get { return "api/Conversion/v1/InvalidateCDN"; } }

        public static string ConvertToKitsuneAnchorTagName { get { return "a"; } }
        public static string ConvertToKitsuneStyleTagName { get { return "link"; } }
        public static string ConvertToKitsuneScriptTagName { get { return "script"; } }
        public static string ConvertToKitsuneImageTagName { get { return "img"; } }
        public static string ConvertToKitsuneIndexPageName { get { return "/index"; } }
        public static string ConvertToKitsuneSubDomainName { get { return "/kitsunesubdomain"; } }
        public static string ConvertToKitsuneS3BucketName { get { return ConfigurationManager.AppSettings["ConvertToKitsuneBucketName"]; } }
        public static string ConvertToKitsuneS3BucketAccelerateUrl { get { return ConfigurationManager.AppSettings["DemoBucketAccelerateUrl"]; } }
        public static string ConvertToKitsuneDistributionId { get { return ConfigurationManager.AppSettings["DistributionId"]; } }
        public static string ConvertToKitsuneKeyPhraseExtractorConnectionString { get { return "http://13.54.108.157/"; } }
        public static List<string> ListOfUnSuggestedDomains { get { return new List<string> { "fonts.googleapis.com", "ajax.googleapis.com", "maps.googleapis.com" }; } }
        public static string ConvertToKitsuneScreenShot { get { return "http://13.54.183.50:8080/GetScreenshot"; } }
        public static string ConvertToKitsuneScreenShotSQSQueue { get { return ConfigurationManager.AppSettings["ConvertToKitsuneScreenShotSQSQueue"]; } }



        public static string ConvertToKitsuneCreateNewCrawlId { get { return "api/Conversion/v1/CreateRecord"; } }
        public static string ConvertToKitsuneGetCrawlId { get { return "api/Conversion/v1/GetCrawlId?url={0}"; } }
        public static string ConvertToKitsuneActivateSite { get { return "api/Conversion/v1/ActivateSite"; } }
        public static string ConvertToKitsuneCheckUrlCrawledOrNot { get { return "api/Conversion/v1/CheckCrawledOrNot?url={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneCheckUrlPresentOrNot { get { return "api/Conversion/v1/CheckUrlPresentOrNot?url={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneGetCrawlingDetails { get { return "api/Conversion/v1/GetCrawlingDetails?crawlId={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneGetKeywordsDetails { get { return "api/Conversion/v1/GetKeywordsDetails?crawlId={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneGetDownloadDetails { get { return "api/Conversion/v1/GetDownloadDetails?crawlId={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneGetOptimisingResourcesDetails { get { return "api/Conversion/v1/GetOptimisingResourcesDetails?crawlId={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneGetReplaceDetails { get { return "api/Conversion/v1/GetReplaceDetails?crawlId={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneGetListOfDomain { get { return "api/Conversion/v1/GetListOfDomains?crawlId={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneSaveSelectedDomains { get { return "api/Conversion/v1/SaveSelectedDomains"; } }
        public static string ConvertToKitsuneGetIndexPageCdnUrl { get { return "api/Conversion/v1/GetHomePageUrl?crawlId={0}&userEmail={1}"; } }
        public static string ConvertToKitsuneFolderS3Url { get { return "api/Conversion/v1/GetProjectZip?crawlId={0}"; } }
        public static string ConvertToKitsunePaymentUrl { get { return "api/Payment/v1/CreatePaymentRequest"; } }
        public static string ConvertToKitsunePaymentStatusUrl { get { return "api/Payment/v1/GetPaymentStatus?payment_request_id={0}&payment_id={1}"; } }
        public static string GenerateMonitorId { get { return "api/Payment/v1/GenerateMonitorId"; } }
        public static string ConvertToKitsuneUpdateKitsuneProjectDBAsComplete { get { return "api/Conversion/v1/UpdateKitsuneProjectDBAsComplete"; } }
        public static string ConvertToKitsunePaymentRedirectUrl { get { return ConfigurationManager.AppSettings["PaymentRedirectUrl"]; } }
        public static string ConvertToKitsuneInvoiceBucketName { get { return "kitsune-conversion-invoice"; } }

        public static string ConvertToKitsuneGetUserUrls { get { return "api/Conversion/v1/ListOfUrl?userEmail={0}"; } }
        public static string ConvertToKitsuneEmailApiUrl { get { return "api/Conversion/v1/SendKitsuneConversionMail"; } }

        public static string ConvertToKitsuneIsWalletCritical { get { return "api/Payment/v1/IsWalletCritical?userEmail={0}&days={1}&extraWebsites={2}"; } }

        public static string ConvertToKitsuneGetUrlForKeywords { get { return "api/Conversion/v1/GetUrlsForKeywords?domain={0}&keyword={1}"; } }
        public static string ConvertToKitsuneGetSiteMapOfWebite { get { return "api/Conversion/v1/GetSiteMapOfWebite?domain={0}"; } }
        public static string ConvertToKitsuneGetContactDetails { get { return "api/Conversion/v1/GetContactDetails?domain={0}"; } }

        #region EmailParameters
        public static string EmailParam_AmountAdded { get { return "AmountAdded"; } }
        public static string EmailParam_WalletAmount { get { return "WalletAmount"; } }
        public static string EmailParam_PaymentId { get { return "PaymentId"; } }
        public static string EmailParam_CrawlId { get { return "CrawlId"; } }
        public static string EmailParam_Subject { get { return "EmailSubject"; } }
        public static string EmailParam_Body { get { return "EmailBody"; } }
        public static string EmailParam_PaymentPartyName { get { return "PaymentPartyName"; } }
        #endregion

        #region Invoice
        public static string InvoiceKitsuneLogo { get { return "http://getkitsune.com/Images/logo.png"; } }
        public static string InvoiceOfficeAddress { get { return "3rd Floor, NowFloats Building, Jubilee Hills Road No. 36, Jubilee Hills Checkpost, Hyderababd, Telangana 500033"; } }
        public static string InvoiceStorageURL { get { return "https://s3-ap-southeast-1.amazonaws.com/nf-temp-cdn/fpweblog/UploadedFiles"; } }
        #endregion

        //public string  { get; set; }

        #endregion


        #region Monitoring constants
        public static string SITE24x7_AUTH_TOKEN { get { return ConfigurationManager.AppSettings["SITE24x7_AUTH_TOKEN"]; } }
        #endregion

        public static string InstaMojoAPIKey { get { return ConfigurationManager.AppSettings["InstaMojoKey"]; } }
        public static string InstaMojoAPIToken { get { return ConfigurationManager.AppSettings["InstaMojoToken"]; } }
        public static string InstaMojoAPIUrl { get { return ConfigurationManager.AppSettings["InstamojoAPIUrl"]; } }
        public static string WalletUpdateUrl { get { return ConfigurationManager.AppSettings["WalletUpdateurl"]; } }
        public static string ShopCluesUserName { get { return ConfigurationManager.AppSettings["ShopCluesUserName"]; } }
        public static string ShopCluesPassword { get { return ConfigurationManager.AppSettings["ShopCluesPassword"]; } }

        //public static string AssetsBasePath { get { return "https://cdn.kitsune.tools/ThemeAssets/{0}/{1}"; }} TODO : Put the CDN invalidation logic then enable the cdn link
        public static string AssetsBasePath { get { return "https://s3.ap-south-1.amazonaws.com/kitsune-resource-source/{0}/{1}"; } }
        public static string CheckAssetsBasePath { get { return "https://kitsune-content-cdn.s3.amazonaws.com/{0}/draft/"; } }
        public static string VersionAssetsBasePath { get { return "https://kitsune-content-cdn.s3.amazonaws.com/{0}/audit/v{1}/{2}"; } }
        public static string CheckVersionAssetsBasePath { get { return "https://kitsune-content-cdn.s3.amazonaws.com/{0}/audit/v"; } }
        public static string CSRFTokenJSLink { get { return "//cdn.kitsune.tools/libs/kcsrf.js"; } }

#if DEBUG
        public static string KPayJsLink { get { return "http://payments.kitsunedev.com/library/k-pay.js"; } }
#else
        public static string KPayJsLink{ get { return "//payments.kitsune.tools/library/k-pay.js"; } }
#endif

        #region WebAction
        #endregion
        public static string GenerateWebActionName(string _actionName)
        {
            try
            {
                return "wa_" + _actionName.ToLower();
            }
            catch
            {
                return null;
            }
        }
        public static string GenerateSchemaName(string _schemaName)
        {
            try
            {
                return "k_" + _schemaName.ToLower().Trim().Replace(" ", "").Replace("-", "_");
            }
            catch
            {
                return null;
            }
        }

    }
}
