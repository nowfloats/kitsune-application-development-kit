
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Models
{
    //TODO : shift the class and webactionproperty model in 2 places
    public class GetWebActionsCommandResult
    {
        public IEnumerable<WebActionResultItem> WebActions { get; set; }
        public Pagination Extra { get; set; }
    }

    public class WebActionResultItem
    {
        public string ActionId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string WebsiteId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<WebActionProperty> Properties { get; set; }
    }

	public class KAppDetails
	{
		public string KAppName { get; set; }
		public string KAppProjectId { get; set; }
	}

    public class CommonBusinessSchemaNFXEntityModel
    {
        public string Content;
        public long Index;
        public string _id;
    }

    #region GWT SEARCH ANALYTICS MODEL

    public class SearchAnalyticsAuditModel
    {
        public string Identifier { get; set; }
        public DateTime LastDateForRecord { get; set; }
    }

    public class SearchAnalyticsSummary
    {
        public long TotalNoOfSearchQueries { get; set; }
        public long TotalNoOfImpressions { get; set; }
        public long TotalNoOfClicks { get; set; }

        public double CTR
        {
            get { return Math.Round(TotalNoOfImpressions > 0 ? TotalNoOfClicks * 1.0 / TotalNoOfImpressions : 0, 2); }
        }
    }

    public class YearlySearchAnalytics : SearchAnalyticsSummary
    {
        public int Year { get; set; }
    }

    public class MonthlySearchAnalytics : SearchAnalyticsSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class DailySearchAnalytics : SearchAnalyticsSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }

    public class DetailedSearchAnalytics
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string Keyword { get; set; }
        public string PageURL { get; set; }
        public string Country { get; set; }
        public string Device { get; set; }
        public double AveragePosition { get; set; }
        public double Impressions { get; set; }
        public double Clicks { get; set; }
        public SearchQueryFeedbackEnum? Feedback { get; set; }

        public double CTR
        {
            get { return Math.Round(Impressions > 0 ? Clicks * 1.0 / Impressions : 0, 2); }
        }
    }

    public class KitsuneDetailedSearchAnalytics : DetailedSearchAnalytics
    {
        public string WebsiteId { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchQueryFeedbackEnum
    {
        RELEVANT,
        SPAM
    }

    #endregion
}
