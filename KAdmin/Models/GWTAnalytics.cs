using System;

namespace KitsuneAdminDashboard.Web.Models
{
    public class DetailedSearchAnalyticsData
    {
        public string WebsiteId;
        public string Id;
        public DateTime Date;
        public string Keyword;
        public string PageURL;
        public string Country;
        public string Device;
        public Int64 AveragePosition;
        public Int64 Impressions;
        public Int64 Clicks;
        public string Feedback;
        public float CTR;
    }

    public class SearchAnalytics
    {
        public Int64 TotalNoOfSearchQueries;
        public Int64 TotalNoOfImpressions;
        public Int64 TotalNoOfClicks;
        public float CTR;
    }

    public class YearlySearchAnalytics : SearchAnalytics
    {
        public int Year;
    }

    public class MonthlySearchAnalytics : YearlySearchAnalytics
    {
        public int Month;
    }

    public class DailySearchAnalytics : MonthlySearchAnalytics
    {
        public int Day;
    }

    public class KitsunePaymentMetrics
    {
        string status;
        int total;
        KistunePaymentsState Success;
        KistunePaymentsState InProgress;
        KistunePaymentsState Fail;
    }

    public class KistunePaymentsState
    {
        int Count;
        Amount Amounts;

        public class Amount
        {
            double INR;
            double USD;
        }

    }
}
