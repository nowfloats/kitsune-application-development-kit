using Kitsune.API2.DataHandlers.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.EnvConstants
{
	public class MongoDBCollection
	{
		public static void InitialiseCollection(bool isProd)
		{
			if (isProd)
			{
				MongoConnector.KitsuneUserCollectionName = "users";
				MongoConnector.KitsuneWebsiteCollectionName = "KitsuneWebsites";
				MongoConnector.KitsuneWebsiteUserCollectionName = "KitsuneWebsiteUsers";
                MongoConnector.KitsuneWebsiteCacheStatusCollectionName = "KitsuneWebsiteCacheStatus";
				MongoConnector.KitsuneWebsiteDNSInfoCollectionName = "KitsuneWebsiteDNS";

				MongoConnector.KitsuneLanguageCollectionName = "KitsuneLanguages";
				MongoConnector.TaskDownloadQueueCollectionName = "new_KitsuneDownloadTask";
				MongoConnector.KitsunePublishStatsCollectionName = "new_KitsunePublishStats";

				MongoConnector.KitsuneProjectsCollectionName = "new_KitsuneProjects";
				MongoConnector.KitsuneResourceCollectionName = "new_KitsuneResources";
				MongoConnector.AuditProjectCollectionName = "new_KitsuneProjectsAudit";
				MongoConnector.AuditResourcesCollectionName = "new_KitsuneResourcesAudit";
				MongoConnector.ProductionProjectCollectionName = "new_KitsuneProjectsProduction";
				MongoConnector.ProductionResorcesCollectionName = "new_KitsuneResourcesProduction";

				MongoConnector.BuildStatusCollectionName = "new_KitsuneBuildStatus";
				MongoConnector.EnquiryCollectionName = "new_KitsuneEnquiryCollection";

				MongoConnector.KitsuneKrawlStatsCollection = "new_KitsuneKrawlStats";
				MongoConnector.KitsuneBillingCollectionName = "KitsuneBillingRecords";
				MongoConnector.InstamojoTransactionLogCollectionName = "PaymentTransactionLogs";
				MongoConnector.KitsuneOptimizationReportsCollection = "KitsuneOptimizationReports";
				MongoConnector.KitsuneWordPressCollection = "KitsuneWordPress";
			}
		}
	}
}
