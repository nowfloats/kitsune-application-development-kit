using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KitsuneAdminDashboard.Web.Models
{
    public static class Constants
    {
        #region DEV
        // public static string KitsuneServerUrl = "http://api2.kitsunedev.com";
        // public static string NFServerUrl = "http://withfloatsapi2-dev.ap-south-1.elasticbeanstalk.com";
        // public static string WebActionServerUrl = "http://webactions.kitsunedev.com";
        // public static string KitsunePayments = "http://payments.kitsunedev.com";
        #endregion

        #region ClientIds
        public static string KadminClientId = "4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587C47C6ACDD";
        public static string NFClientId = "[[NF_CLIENTID]]";
        public static string KitsuneTransactionsClientId = "5959ec985d643701d48ee8ab";
        #endregion

        public static string KitsuneServerUrl = "https://api2.kitsune.tools";
        public static string WebActionServerUrl = "https://webactions.kitsune.tools";
        
        
        public static string KPaymentsServerUrl = "https://payments.kitsune.tools";
    

        public static string KitsunePaymentsWebactionName = "kpay_transactions_prod";

        public static string KitsuneStatusCheckUrl = "https://status.api.kitsune.tools/kitsuneapi";
        
        public static string KitsunePayments = "http://payments.kitsune.tools";


        public static class SchemaEndpoints
        {
            #region Schema Endpoints
            public const string GetLanguageSchemaByIdEndpoint = "{0}/language/v1/{1}";
            public const string CreateOrUpdateSchemaEndpoint = "{0}/language/v1/CreateOrUpdateLanguageEntity";
            public const string GetDataForSchemaEndpoint = "{0}/language/v1/{1}/get-data?website={2}";
            // double '{{' are used as escape sequence they will be rendred as single '{' only
            public const string GetDataForSchemaWithReferenceId = "{0}/language/v1/{1}/get-data?website={2}&query={{ _parentClassName: '{3}', _propertyName: '{4}', _parentClassId: '{5}'}}";
            public const string AddDataForSchemaEndpoint = "{0}/language/v1/{1}/add-data";
            public const string UpdateDataForSchemaEndpoint = "{0}/language/v1/{1}/update-data";
            public const string DeleteDataForSchemaEndpoint = "{0}/language/v1/{1}/delete-data";
            public const string GetSchemaForWebsiteEndPoint = "{0}/language/v1/GetWebsiteSchema?websiteid={1}";
            public const string UploadFileForWebsiteEndpoint = "{0}/language/v1/{1}/upload-file?assetFileName={2}";
            public const string GetClassesByClassType = "{0}/language/v1/{1}/GetType?className={2}&websiteId={3}";
            public const string GetDataByClassName = "{0}/language/v1/{1}/get-data-by-type?website={2}&className={3}";
            public const string GetDataByProperty = "{0}/language/v1/{1}/get-data-by-property";
            public const string GetDataByPropertyBulk = "{0}/language/v1/{1}/get-bulk-data-by-property";
            #endregion
        }

        public static class CustomerEndpoints
        {
            public const string VerifyUserEndpoint = "{0}/api/Website/v1/Login";
            public const string UpdateCustomerDetailsEndpoint = "{0}/api/Website/v1/{1}/WebsiteUser/{2}";
            public const string GetCustomerDetailsEndpoint = "{0}/api/Website/v1/{1}/WebsiteUser/{2}";
            public const string UpdateCustomerPasswordEndpoint = "{0}/api/Website/v1/{1}/WebsiteUser/{2}/ChangePassword";
            public const string GetLoginDetailsEndpoint = "{0}/api/Website/v1/KAdminDecodeToken?token={1}";
            public const string GetWebsiteDetails = "{0}/api/website/v1/{1}?clientId={2}";
            public const string IsCallTrackerEnabled = "{0}/api/Website/v1/CallTrackerEnabled?WebsiteId={1}";
            public const string ValidateConsoleModeToken = "{0}/api/website/v1/KAdminValidateToken?token={1}";
        }

        public static class AnalyticsEndpoints
        {
            public const string GetVisitorsEndpoint = "{0}/api/WebAnalytics/v1/GetVisitors/{1}?website={2}&fromDate={3}&toDate={4}";
            public const string GetDevicesEndpoint = "{0}/api/WebAnalytics/v1/GetRequestsByDevices?website={1}&fromDate={2}&toDate={3}";
            public const string GetBrowserEndpoint = "{0}/api/WebAnalytics/v1/GetRequestsByBrowsers?website={1}&fromDate={2}&toDate={3}";
            public const string GetReferralsEndpoint = "{0}/api/WebAnalytics/v1/GetTrafficSources?website={1}&fromDate={2}&toDate={3}";
        }

        public static class KitsuneSearchAnalyticsEndpoints
        {
            public const string GetDetailedSearchAnalyticsForDate = "{0}/market/api/KitsuneSearchAnalytics/GetDetailedSearchAnalyticsForDate?websiteId={1}&year={2}&month={3}&day={4}";
            public const string GetDetailedSearchAnalyticsForDateRange = "{0}/market/api/KitsuneSearchAnalytics/GetDetailedSearchAnalyticsForDateRange?websiteId={1}&startDate={2}&endDate={3}";

            public const string GetDailySearchAnalytics = "{0}/market/api/KitsuneSearchAnalytics/GetDailySearchAnalytics?websiteId={1}&year={2}&month={3}";
            public const string GetMonthlySearchAnalytics = "{0}/market/api/KitsuneSearchAnalytics/GetMonthlySearchAnalytics?websiteId={1}&year={2}";

        }

        public static class WebactionsEndpoints
        {
            public const string WebActionsListEndpoint = "{0}/api/v1/List?type=webform";
            public const string QueryBasedWebActionsListEndpoint = "{0}/api/v1/List?userId={1}";
            public const string WebActionsDataEndpoint = "{0}/api/v1/{1}/get-data?{2}";
            public const string WebActionUploadEndpoint = "{0}/api/v1/{1}/upload-file?assetFileName={2}";
            public const string UpdateWebActionDataEndpoint = "{0}/api/v1/{1}/update-data?id={2}";
            public const string AddWebActionDataEndpoint = "{0}/api/v1/{1}/add-data";
            public const string UploadFile = "{0}/api/v1/{1}/upload-file?assetFileName={2}";
            public const string GetWebActionDataCount = "{0}/api/v1/Admin/GetDataSizeByWebsiteId?userId={1}&clientId={2}&startDate={3}&endDate={4}&type={5}&websiteId={6}";
            public const string CreateOrUpdateWebaction = "{0}/api/v1/CreateOrUpdate";
            public const string GetWebActionsListWithConfig = "{0}/api/v1/List?type=webform&includeConfig=true";
        }

        public static class TransactionsEnspoints
        {
            public const string TransactionsData = "{0}/api/v1/{1}/get-data?query={{ 'WebsiteId' : '{2}', '_env': 'live' }}&limit=10000";
            public const string PaymentGateways = "{0}/api/v1/InitGateway?projectId={1}&websiteId={2}&env=live";
        }

        public static class InternalEndpoints
        {
            public const string SendEmail = "{0}/api/Internal/v1/SendEmail?configType=0&clientId={1}";
        }

        public static class DeveloperEndPoints
        {
            public const string GetDeveloperDetails = "{0}/api/Developer/v1/Details?developerId={1}";
        }

        public static class CallTrackerEndPoints
        {
            public const string GetVMNDetails = "{0}/Language/v1/{1}/get-data-by-type?website={2}&className=phonenumber";
            public const string GetAllCallLogs = "{0}/wildfire/v1/calls/tracker?clientId={1}&fpId={2}&identifierType=SINGLE&offset=0&limit={3}";
            public const string GetCallLogs = "{0}/wildfire/v1/calls/tracker?clientId={1}&fpId={2}&identifierType=SINGLE&offset=0&limit={3}&virtualNumber={4}";
        }

        public static class KitsunePaymentsEndpoints
        {
            public const string GetMetrics = "{0}/api/v1/Metrics/{1}";
        }
    }
}
