using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
using Kitsune.API2.Models;
using System.Text.RegularExpressions;

namespace Kitsune.API2.EnvConstants
{
	public static class Constants
	{
		public static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Unix Epoch

		#region Database Collection
		public static string ProductionThemeCollection { get { return "ProductionThemes"; } }
		public static string ProductionPageCollection { get { return "ProductionThemePages"; } }
		public static string PendingThemesCollection { get { return "PendingThemes"; } }
		public static string PendingPageCollection { get { return "PendingThemePages"; } }
		public static string DraftThemesCollection { get { return "DraftThemes"; } }
		public static string DraftPageCollection { get { return "DraftThemePages"; } }

		#region Kitsune Merge

		//public static string ProductionKitsuneProjectCollection { get { return ConfigurationManager.AppSettings["ProductionKitsuneProjectCollection"]; } }
		//public static string ProductionKitsuneResourcesCollection { get { return ConfigurationManager.AppSettings["ProductionKitsuneResourcesCollection"]; } }
		//public static string AuditKitsuneProjectCollection { get { return ConfigurationManager.AppSettings["AuditKitsuneProjectCollection"]; } }
		//public static string AuditKitsuneResourcesCollection { get { return ConfigurationManager.AppSettings["AuditKitsuneResourcesCollection"]; } }
		//public static string KitsuneProjectCollection { get { return ConfigurationManager.AppSettings["KitsuneProjectCollection"]; } }
		//public static string KitsuneResourcesCollection { get { return ConfigurationManager.AppSettings["KitsuneResourcesCollection"]; } }
		//public static string KitsuneKrawlStatsCollection { get { return ConfigurationManager.AppSettings["KitsuneKrawlStatsCollection"]; } }

		#endregion


		public static string CompilerVersion { get { return "CompilerVersion"; } }
		public static string KLMAuditLogCollection { get { return "KLMAuditLogCollection"; } }
		public static string KitsuneEnquiryCollection { get { return "KitsuneEnquiryCollection"; } }
		public static string MongoCompilerConnectionUrlKey { get { return "MongoConnectionUrl"; } }
		public static string MongoUserConnectionUrlKey { get { return "UserMongoConnectionUrl"; } }

		public static string PiwikVisitorsData { get { return "PiwikVisitorsData"; } }
		public static string PiwikActionData { get { return "PiwikActionData"; } }


		public static string WebsiteCollection { get { return "websites"; } }
		public static string InvoiceCollection { get { return "invoices"; } }
		public static string UserCollection { get { return "users"; } }
		public static string WalletStats { get { return "walletStats"; } }
		public static string PaymentTransactionCollectionLog { get { return "PaymentTransactionLogs"; } }
		public static string WebActionCollection { get { return "WebActions"; } }
		public static string LanguageCollection { get { return "KitsuneLanguages"; } }
		public static string WebsiteWebActionsCollection { get { return "WebsiteWebActions"; } }
		public static string WebsiteSchemasCollection { get { return "WebsiteSchemas"; } }
		#endregion

		#region Language Expressions

		public static string WidgetRegulerExpression { get { return @"\[\[+(.*?)\]\]+"; } }
		public static string PropertyWithFunctionRegulerExpression { get { return @"\[\[+\((.*?)\)\]\]+"; } }
		public static string PropertyArrayExpression { get { return @"\[(.*?)\]"; } }
		public static string FunctionRegularExpression { get { return @"\[\[+\w+.\w+(\((?:[()]|[^()])*\))\]\]"; } }
		public static string SetObjectRegularExpression { get { return @"\[\[+\w+.\w+.\w+(\((?:\(\)[()]|[^()])*\))\]\]"; } }
		public static string PartialPageRegularExpression { get { return @"\[\[Partial(\((?:[()]|[^()])[_\w\.\'\/\-]+\))\]\]"; } }
		public static string ViewClassRegularExpression { get { return @"View(\((?:[()]|[^()])[_\w\.\'\-]+\))"; } }
		public static Regex LanguageClassNameValidator { get { return new Regex(@"[^0-9a-zA-Z_]+|^\d", RegexOptions.Compiled); } }

        #endregion

        #region Defaults

        public static string KitsuneContentCDN { get { return "kitsune-content-cdn-dev"; } }
		public static string DefaultCustomer { get { return "{0}_default"; } }

		#endregion

		#region Email Constants

		public static string EmailParam_AmountAdded { get { return "AmountAdded"; } }
		public static string EmailParam_WalletAmount { get { return "WalletAmount"; } }
		public static string EmailParam_PaymentId { get { return "PaymentId"; } }
		public static string EmailParam_CrawlId { get { return "CrawlId"; } }
		public static string EmailParam_Subject { get { return "EmailSubject"; } }
		public static string EmailParam_Body { get { return "EmailBody"; } }
		public static string EmailParam_PaymentPartyName { get { return "PaymentPartyName"; } }
		public static string EmailParam_KAdminUserName { get { return "KAdminUserName"; } }
		public static string EmailParam_KAdminPassword { get { return "KAdminPassword"; } }
		public static string EmailParam_CustomerName { get { return "CustomerName"; } }
		public static string EmailParam_DeveloperName { get { return "DeveloperName"; } }
        public static string EmailParam_ProjectName { get { return "ProjectName"; } }
		public static string EmailParam_KAdminUrl { get { return "KAdminUrl"; } }
		public static string KAdminBaseUrl { get { return "http://{0}/k-admin"; } }
		public static string BillingActivationFailedCustomers { get { return "BillingActivationFailedCustomers"; } }
		public static string BillingActivationFailedReportMail { get { return "chirag.m@getkitsune.com"; } }


		public static string EmailAPI { get { return "https://api2.kitsune.tools/Internal/v1/PushEmailToQueue/4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587CC93FC322"; } }
		public static string KitsuneDevClientId { get { return "4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587CC93FC322"; } }

		#endregion

		#region Payment Constants

		public static string ConvertToKitsuneDbUrl { get { return EnvironmentConstants.ApplicationConfiguration.DBConnectionStrings.KitsuneDBMongoConnectionUrl; } }
		public static string ConvertToKitsuneDatabaseName { get { return EnvironmentConstants.ApplicationConfiguration.DatabaseName; } }

		public static string InstamojoWebhook { get { return EnvironmentConstants.ApplicationConfiguration.InstamojoDetails.InstamojoWebhook; } }

		public static string InstaMojoAPIKey { get { return EnvironmentConstants.ApplicationConfiguration.InstamojoDetails.InstaMojoAPIKey; } }
		public static string InstaMojoAPIToken { get { return EnvironmentConstants.ApplicationConfiguration.InstamojoDetails.InstaMojoAPIToken; } }
		public static string InstaMojoAPIUrl { get { return EnvironmentConstants.ApplicationConfiguration.InstamojoDetails.InstaMojoAPIUrl; } }
		public static string WalletUpdateUrl { get { return EnvironmentConstants.ApplicationConfiguration.InstamojoDetails.WalletUpdateUrl; } }

		public static string StripeAPIKey { get { return EnvironmentConstants.ApplicationConfiguration.StripeDetails.StripeAPIKey; } }
		public static string StripeAPISecretKey { get { return EnvironmentConstants.ApplicationConfiguration.StripeDetails.StripeAPISecretKey; } }
		public static string StripeAPIUrl { get { return EnvironmentConstants.ApplicationConfiguration.StripeDetails.StripeAPIUrl; } }

		#endregion

		#region Identifier

		public static string KitsuneIdentiferDistributionId { get { return EnvironmentConstants.ApplicationConfiguration.KitsuneIdentifierDistributionId; } }

		public static string KitsuneRedirectIPAddress { get { return EnvironmentConstants.ApplicationConfiguration.KitsuneRedirectIPAddress; } }

		public static string KitsuneIdentifierKitsuneSubDomain { get { return EnvironmentConstants.ApplicationConfiguration.KitsuneIdentifierKitsuneSubDomain; } }

		public static string KitsuneIdentifierKitsuneDemoDomain { get { return EnvironmentConstants.ApplicationConfiguration.KitsuneDemoDomain; } }

		#endregion

		#region Billing
		public static class BillingProcessUrl
		{
			public static string Activate { get { return "activate"; } }
			public static string Deactivate { get { return "deactivate"; } }
			public static string Update { get { return "update"; } }
		}
		public static class BillingComponents
		{
			public static string WebRequests { get { return "WEBREQUESTS"; } }
		}
		#endregion

		#region Project Constants

		// 0 : projectId
		public static string ProjectConfigurationFilePath { get { return "/kitsune-settings.json"; } }
		public static string ProjectConfigurationS3FilePath { get { return "{0}" + Constants.ProjectConfigurationFilePath; } }

		public static string ProjectDefaultIndexFile { get { return "static/defaults/index.html"; } }
		public static List<string> ProjectDefaultFiles
        {
            get
            {
                return new List<string>
                {
                    "/index.html",
                    "/assets/css/styles.css",
                    "/assets/images/bg.svg",
                    "/assets/images/favicon.ico",
                    "/assets/js/app.js",
                };
            }
        }
        public static class ComponentId
        {
            public static string reports { get { return "5ab5190ba35c3b04e9817cb5"; } }
            public static string paymentGateway { get { return "5afec0b12fa18e05039b4bd6"; } }
            public static string callTracker { get { return "5b18f68ed9481d0508afb176"; } }
            public static string partialViews { get { return "5afec0cc8a3007055acaabb3"; } }
        }
        
        public static string VMNBaseUrl { get { return "[[KIT_VMN_API]]"; } }


        public static string AssignVMNApiParam { get { return "/Discover/v1/Kitsune/MapCallTracker?customerId={0}&clientId=4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587CC93FC322&primaryNumber={1}"; } }
        public static string FetchVMNApiParam { get { return "/Discover/v1/Kitsune/GetCustomerVMNByCustomerId?clientId=4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587CC93FC322&customerId={0}"; } }
        public static string RemoveVMNApiParam { get { return "/Discover/v1/Kitsune/RemoveCallTracker?clientId=4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587CC93FC322&customerId={0}&primaryNumber={1}"; } }
        public static string DisableVMNApiParam { get { return "/Discover/v1/Kitsune/RemoveCallTracker?clientId=4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587CC93FC322"; } }
        public static string UpdateVMNApiParam { get { return "/Discover/v1/Kitsune/UpdateCustomerNumber?customerId={0}&clientId=4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587CC93FC322&virtualNumber={1}&newPrimaryNumber={2}"; } }
        public static string ProjectDefaultSettingsFile { get { return "static/defaults/kitsune-settings.json"; } }
        public static string callTrackerAllWebsitesIdentifier { get { return "*"; } }

        #endregion

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
		public const int BUFFER_SIZE = 4096;
		public static string KitsuneFilesCDNUrl { get { return "https://cdn.kitsune.tools"; } }

		public static byte[] ReadInputStream(IFormFile stream)
		{
			using (var ms = new MemoryStream())
			using (var inputStream = stream.OpenReadStream())
			{
				var buffer = new byte[BUFFER_SIZE];
				var pos = 0;
				do
				{
					pos = inputStream.Read(buffer, 0, BUFFER_SIZE);
					ms.Write(buffer, 0, pos);
				} while (pos > 0);
				return ms.ToArray();
			}
		}

	}

}
