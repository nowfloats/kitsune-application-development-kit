using Kitsune.API2.DataHandlers.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.EnvConstants
{
    public static class MySQLStoredProcedures
    {
        /// <summary>
        /// params: (website, fromDate, toDate)
        /// </summary>
        public const string GetVisitorsByCity = "KitsuneWebLog.visitors_by_city";
        public const string GetVisitorsByCountry = "KitsuneWebLog.visitors_by_country";
        public const string GetVisitorByDay = "KitsuneWebLog.visitors_by_day";
        public const string GetVisitorByHour = "KitsuneWebLog.visitors_by_hour";
        public const string GetTotalVistor = "KitsuneWebLog.total_visitors";

        public const string GetRequestsByReferrers = "KitsuneWebLog.top_referrers";
        public const string GetRequestsByBrowsers = "KitsuneWebLog.browsers";
        public const string GetRequestsByDevices = "KitsuneWebLog.devices";
        public const string GetRequestsByOS = "KitsuneWebLog.opertating_systems";
        public const string GetAllRequestsPerDayByUserId = "KitsuneWebLog.requests_by_day_for_cids";
        public const string GetStorageByDayForProjectWebsiteIDs = "KitsuneWebLog.sp_StorageByDayForProjectWebsiteIDs";
    }
}
