using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.API2.DataHandlers.MySQL
{
    public partial class MySQLConnector
    {
        private readonly static string GWTConnectionString = EnvironmentConstants.ApplicationConfiguration.DBConnectionStrings.GWTMySQLConnectionUrl;

        public static Dictionary<string, SearchAnalyticsAuditModel> FetchMaxDateForGWTSearchAnalytics()
        {
            string query = $"select websiteId, max(date) from searchAnalytics.gwtSearchAnalytics_kitsune group by 1";
            try
            {
                using (MySqlConnection mysqlConnectionObj = new MySqlConnection())
                {
                    mysqlConnectionObj.Open();
                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnectionObj))
                    {
                        using (var mysqlDataReaderObj = command.ExecuteReader())
                        {
                            Dictionary<string, SearchAnalyticsAuditModel> fpSearchAuditDict = new Dictionary<string, SearchAnalyticsAuditModel>(StringComparer.OrdinalIgnoreCase);
                            while (mysqlDataReaderObj.Read())
                            {
                                try
                                {
                                    string identifier = mysqlDataReaderObj.IsDBNull(0) ? "" : mysqlDataReaderObj.GetString(0);
                                    DateTime? maxDate = mysqlDataReaderObj.IsDBNull(1) ? new DateTime?() : mysqlDataReaderObj.GetDateTime(1);

                                    if (string.IsNullOrWhiteSpace(identifier) || !maxDate.HasValue)
                                    {
                                        continue;
                                    }

                                    fpSearchAuditDict[identifier] = new SearchAnalyticsAuditModel()
                                    {
                                        Identifier = identifier,
                                        LastDateForRecord = maxDate.Value
                                    };
                                }
                                catch (Exception e)
                                {
                                    //EventLogger.PrintAndLogError(e);
                                }
                            }

                            if (fpSearchAuditDict?.Count > 0)
                                return fpSearchAuditDict;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //EventLogger.PrintAndLogError(e);
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public static Dictionary<string, SearchAnalyticsSummary> GetSearchAnalyticsSummary(string[] websiteIds)
        {
            if (websiteIds == null || websiteIds.Length == 0)
                return null;

            try
            {
                string query = $"select websiteId,count(distinct keyword) as numberOfKeywords,sum(impressions) as numberOfImpressions,sum(clicks) as numberOfClicks from searchAnalytics.gwtSearchAnalytics_kitsune where websiteId in ('{string.Join("','", websiteIds)}') group by 1";

                using (MySqlConnection mysqlConnectionObj = new MySqlConnection(GWTConnectionString))
                {
                    mysqlConnectionObj.Open();
                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnectionObj))
                    {
                        using (var mysqlDataReaderObj = command.ExecuteReader())
                        {
                            Dictionary<string, SearchAnalyticsSummary> websiteSearchAnalyticsSummaryDict = new Dictionary<string, SearchAnalyticsSummary>(StringComparer.OrdinalIgnoreCase);

                            while (mysqlDataReaderObj.Read())
                            {
                                try
                                {
                                    string identifier = mysqlDataReaderObj.IsDBNull(0) ? "" : mysqlDataReaderObj.GetString(0).ToUpper();
                                    long numberOfKeywords = Convert.ToInt64(mysqlDataReaderObj["numberOfKeywords"]);
                                    long numberOfImpressions = Convert.ToInt64(mysqlDataReaderObj["numberOfImpressions"]);
                                    long numberOfClicks = Convert.ToInt64(mysqlDataReaderObj["numberOfClicks"]);

                                    if (string.IsNullOrWhiteSpace(identifier))
                                        continue;

                                    websiteSearchAnalyticsSummaryDict[identifier] = new SearchAnalyticsSummary()
                                    {
                                        TotalNoOfClicks = numberOfClicks,
                                        TotalNoOfImpressions = numberOfImpressions,
                                        TotalNoOfSearchQueries = numberOfKeywords
                                    };
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }

                            if (websiteSearchAnalyticsSummaryDict?.Count > 0)
                                return websiteSearchAnalyticsSummaryDict;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public static Dictionary<string, List<YearlySearchAnalytics>> GetYearlySearchAnalytics(string[] websiteIds)
        {
            if (websiteIds == null || websiteIds.Length == 0)
                return null;

            try
            {
                string query = $"select websiteId,year(date) as year,count(distinct keyword) as numberOfKeywords,sum(impressions) as numberOfImpressions,sum(clicks) as numberOfClicks from searchAnalytics.gwtSearchAnalytics_kitsune where websiteId in ('{string.Join("','", websiteIds)}') group by 1,2";

                using (MySqlConnection mysqlConnectionObj = new MySqlConnection(GWTConnectionString))
                {
                    mysqlConnectionObj.Open();
                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnectionObj))
                    {
                        using (var mysqlDataReaderObj = command.ExecuteReader())
                        {
                            Dictionary<string, List<YearlySearchAnalytics>> fpYearlySearchAnalytics = new Dictionary<string, List<YearlySearchAnalytics>>(StringComparer.OrdinalIgnoreCase);

                            while (mysqlDataReaderObj.Read())
                            {
                                try
                                {
                                    string identifier = mysqlDataReaderObj.IsDBNull(0) ? "" : mysqlDataReaderObj.GetString(0).ToUpper();
                                    int year = Convert.ToInt32(mysqlDataReaderObj["year"]);
                                    long numberOfKeywords = Convert.ToInt64(mysqlDataReaderObj["numberOfKeywords"]);
                                    long numberOfImpressions = Convert.ToInt64(mysqlDataReaderObj["numberOfImpressions"]);
                                    long numberOfClicks = Convert.ToInt64(mysqlDataReaderObj["numberOfClicks"]);

                                    if (string.IsNullOrWhiteSpace(identifier))
                                        continue;

                                    if (!fpYearlySearchAnalytics.ContainsKey(identifier))
                                        fpYearlySearchAnalytics[identifier] = new List<YearlySearchAnalytics>();

                                    fpYearlySearchAnalytics[identifier].Add(new YearlySearchAnalytics()
                                    {
                                        Year = year,
                                        TotalNoOfClicks = numberOfClicks,
                                        TotalNoOfImpressions = numberOfImpressions,
                                        TotalNoOfSearchQueries = numberOfKeywords
                                    });
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }

                            if (fpYearlySearchAnalytics?.Count > 0)
                                return fpYearlySearchAnalytics;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public static Dictionary<string, List<MonthlySearchAnalytics>> GetMonthlySearchAnalytics(string[] websiteIds, int year)
        {
            if (websiteIds == null || websiteIds.Length == 0)
                return null;

            try
            {
                string query = $"select websiteId,month(date) as month,count(distinct keyword) as numberOfKeywords,sum(impressions) as numberOfImpressions,sum(clicks) as numberOfClicks from searchAnalytics.gwtSearchAnalytics_kitsune where websiteId in ('{string.Join("','", websiteIds)}') and year(date)=@year group by 1,2";

                using (MySqlConnection mysqlConnectionObj = new MySqlConnection(GWTConnectionString))
                {
                    mysqlConnectionObj.Open();
                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnectionObj))
                    {
                        command.Parameters.AddWithValue("@year", year);
                        using (var mysqlDataReaderObj = command.ExecuteReader())
                        {
                            Dictionary<string, List<MonthlySearchAnalytics>> fpMonthlySearchAnalytics = new Dictionary<string, List<MonthlySearchAnalytics>>(StringComparer.OrdinalIgnoreCase);

                            while (mysqlDataReaderObj.Read())
                            {
                                try
                                {
                                    string identifier = mysqlDataReaderObj.IsDBNull(0) ? "" : mysqlDataReaderObj.GetString(0).ToUpper();

                                    int month = Convert.ToInt32(mysqlDataReaderObj["month"]);
                                    long numberOfKeywords = Convert.ToInt64(mysqlDataReaderObj["numberOfKeywords"]);
                                    long numberOfImpressions = Convert.ToInt64(mysqlDataReaderObj["numberOfImpressions"]);
                                    long numberOfClicks = Convert.ToInt64(mysqlDataReaderObj["numberOfClicks"]);

                                    if (string.IsNullOrWhiteSpace(identifier))
                                        continue;

                                    if (!fpMonthlySearchAnalytics.ContainsKey(identifier))
                                        fpMonthlySearchAnalytics[identifier] = new List<MonthlySearchAnalytics>();

                                    fpMonthlySearchAnalytics[identifier].Add(new MonthlySearchAnalytics()
                                    {
                                        Year = year,
                                        Month = month,
                                        TotalNoOfClicks = numberOfClicks,
                                        TotalNoOfImpressions = numberOfImpressions,
                                        TotalNoOfSearchQueries = numberOfKeywords
                                    });
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }

                            if (fpMonthlySearchAnalytics?.Count > 0)
                                return fpMonthlySearchAnalytics;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public static Dictionary<string, List<DailySearchAnalytics>> GetDailySearchAnalytics(string[] websiteIds, int year, int month)
        {
            if (websiteIds == null || websiteIds.Length == 0)
                return null;

            try
            {
                string query = $"select websiteId,day(date) as day,count(distinct keyword) as numberOfKeywords,sum(impressions) as numberOfImpressions,sum(clicks) as numberOfClicks from searchAnalytics.gwtSearchAnalytics_kitsune where websiteId in ('{string.Join("','", websiteIds)}') and year(date)=@year and month(date)=@month group by 1,2";

                using (MySqlConnection mysqlConnectionObj = new MySqlConnection(GWTConnectionString))
                {
                    mysqlConnectionObj.Open();
                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnectionObj))
                    {
                        command.Parameters.AddWithValue("@year", year);
                        command.Parameters.AddWithValue("@month", month);
                        using (var mysqlDataReaderObj = command.ExecuteReader())
                        {
                            Dictionary<string, List<DailySearchAnalytics>> fpDailySearchAnalytics = new Dictionary<string, List<DailySearchAnalytics>>(StringComparer.OrdinalIgnoreCase);

                            while (mysqlDataReaderObj.Read())
                            {
                                try
                                {
                                    string identifier = mysqlDataReaderObj.IsDBNull(0) ? "" : mysqlDataReaderObj.GetString(0).ToUpper();

                                    int day = Convert.ToInt32(mysqlDataReaderObj["day"]);
                                    long numberOfKeywords = Convert.ToInt64(mysqlDataReaderObj["numberOfKeywords"]);
                                    long numberOfImpressions = Convert.ToInt64(mysqlDataReaderObj["numberOfImpressions"]);
                                    long numberOfClicks = Convert.ToInt64(mysqlDataReaderObj["numberOfClicks"]);

                                    if (string.IsNullOrWhiteSpace(identifier))
                                        continue;

                                    if (!fpDailySearchAnalytics.ContainsKey(identifier))
                                        fpDailySearchAnalytics[identifier] = new List<DailySearchAnalytics>();

                                    fpDailySearchAnalytics[identifier].Add(new DailySearchAnalytics()
                                    {
                                        Year = year,
                                        Month = month,
                                        Day = day,
                                        TotalNoOfClicks = numberOfClicks,
                                        TotalNoOfImpressions = numberOfImpressions,
                                        TotalNoOfSearchQueries = numberOfKeywords
                                    });
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }

                            if (fpDailySearchAnalytics?.Count > 0)
                                return fpDailySearchAnalytics;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public static List<DetailedSearchAnalytics> GetDetailedSearchAnalytics(string websiteId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrWhiteSpace(websiteId))
                return null;

            try
            {
                string query = $"select id, date, websiteId, keyword, pageurl, country, device, avgposition, impressions, clicks, feedback from searchAnalytics.gwtSearchAnalytics_kitsune where websiteId = @identifier and  date>=@startDate and date<@endDate";

                using (MySqlConnection mysqlConnectionObj = new MySqlConnection(GWTConnectionString))
                {
                    mysqlConnectionObj.Open();

                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnectionObj))
                    {
                        command.Parameters.AddWithValue("@identifier", websiteId);
                        command.Parameters.AddWithValue("@startDate", startDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@endDate", endDate.ToString("yyyy-MM-dd"));
                        List<DetailedSearchAnalytics> detailedSearchAnalyticsList = new List<DetailedSearchAnalytics>();
                        using (var mysqlDataReaderObj = command.ExecuteReader())
                        {
                            while (mysqlDataReaderObj.Read())
                            {
                                try
                                {
                                    string id = mysqlDataReaderObj.IsDBNull(0) ? "" : Convert.ToString(mysqlDataReaderObj["id"]);
                                    DateTime date = Convert.ToDateTime(mysqlDataReaderObj["date"]);
                                    string identifierVal = mysqlDataReaderObj.IsDBNull(2) ? "" : mysqlDataReaderObj.GetString(2);
                                    string keyword = mysqlDataReaderObj.IsDBNull(3) ? "" : Convert.ToString(mysqlDataReaderObj["keyword"]);
                                    string pageurl = mysqlDataReaderObj.IsDBNull(4) ? "" : Convert.ToString(mysqlDataReaderObj["pageurl"]);
                                    string country = mysqlDataReaderObj.IsDBNull(5) ? "" : Convert.ToString(mysqlDataReaderObj["country"]);
                                    string device = mysqlDataReaderObj.IsDBNull(6) ? "" : Convert.ToString(mysqlDataReaderObj["device"]);
                                    int avgposition = Convert.ToInt32(mysqlDataReaderObj["avgposition"]);
                                    int impressions = Convert.ToInt32(mysqlDataReaderObj["impressions"]);
                                    int clicks = Convert.ToInt32(mysqlDataReaderObj["clicks"]);
                                    string feedback = mysqlDataReaderObj.IsDBNull(10) ? "" : Convert.ToString(mysqlDataReaderObj["feedback"]);

                                    if (string.IsNullOrWhiteSpace(identifierVal) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(keyword))
                                        continue;

                                    DetailedSearchAnalytics detailedSearchAnalytics = new KitsuneDetailedSearchAnalytics()
                                    {
                                        WebsiteId = identifierVal,
                                        Id = id,
                                        Date = date,
                                        Keyword = keyword,
                                        PageURL = pageurl,
                                        Country = country,
                                        Device = device,
                                        AveragePosition = avgposition,
                                        Impressions = impressions,
                                        Clicks = clicks,
                                        Feedback = null
                                    }; 

                                    if (detailedSearchAnalytics != null)
                                    {
                                        if (String.Compare(feedback, ("relevant"), true) == 0)
                                            detailedSearchAnalytics.Feedback = SearchQueryFeedbackEnum.RELEVANT;
                                        else if (String.Compare(feedback, ("spam"), true) == 0)
                                            detailedSearchAnalytics.Feedback = SearchQueryFeedbackEnum.SPAM;

                                        detailedSearchAnalyticsList.Add(detailedSearchAnalytics);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }

                            if (detailedSearchAnalyticsList?.Count > 0)
                                return detailedSearchAnalyticsList;
                        }
                    }

                    mysqlConnectionObj.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return null;
        }
        
        /// <summary>
        /// Returns Search Keywords returned from GWT
        /// </summary>
        /// <param name="websiteIds">List of websiteids</param>
        /// <param name="limit">Min Limit is 10, Max Limit is 50</param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static List<DetailedSearchAnalytics> GetSearchAnalytics(List<string> websiteIds, int limit = 10, int offset = 0)
        {
            if (websiteIds == null || websiteIds.Count() == 0)
                return null;

            if (limit > 50)
                limit = 50;

            try
            {
                #region Joining the websiteids
                var websiteIdsKey = new StringBuilder(String.Empty);
                foreach (var websiteId in websiteIds)
                {
                    if (!String.IsNullOrEmpty(websiteIdsKey.ToString()))
                    {
                        websiteIdsKey.Append(", '");
                        websiteIdsKey.Append(websiteId);
                        websiteIdsKey.Append("'");
                    }
                    else
                    {
                        websiteIdsKey.Append("'");
                        websiteIdsKey.Append(websiteId);
                        websiteIdsKey.Append("'");
                    }
                }
                #endregion

                string query = $"select id, date, websiteId, keyword, pageurl, country, device, avgposition, impressions, clicks, feedback from searchAnalytics.gwtSearchAnalytics_kitsune where websiteId IN ({websiteIdsKey}) ORDER BY date DESC LIMIT {limit} OFFSET {offset}";

                using (MySqlConnection mysqlConnectionObj = new MySqlConnection(GWTConnectionString))
                {
                    mysqlConnectionObj.Open();

                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnectionObj))
                    {
                        List<DetailedSearchAnalytics> detailedSearchAnalyticsList = new List<DetailedSearchAnalytics>();
                        using (var mysqlDataReaderObj = command.ExecuteReader())
                        {
                            while (mysqlDataReaderObj.Read())
                            {
                                try
                                {
                                    string id = mysqlDataReaderObj.IsDBNull(0) ? "" : Convert.ToString(mysqlDataReaderObj["id"]);
                                    DateTime date = Convert.ToDateTime(mysqlDataReaderObj["date"]);
                                    string identifierVal = mysqlDataReaderObj.IsDBNull(2) ? "" : mysqlDataReaderObj.GetString(2);
                                    string keyword = mysqlDataReaderObj.IsDBNull(3) ? "" : Convert.ToString(mysqlDataReaderObj["keyword"]);
                                    string pageurl = mysqlDataReaderObj.IsDBNull(4) ? "" : Convert.ToString(mysqlDataReaderObj["pageurl"]);
                                    string country = mysqlDataReaderObj.IsDBNull(5) ? "" : Convert.ToString(mysqlDataReaderObj["country"]);
                                    string device = mysqlDataReaderObj.IsDBNull(6) ? "" : Convert.ToString(mysqlDataReaderObj["device"]);
                                    int avgposition = Convert.ToInt32(mysqlDataReaderObj["avgposition"]);
                                    int impressions = Convert.ToInt32(mysqlDataReaderObj["impressions"]);
                                    int clicks = Convert.ToInt32(mysqlDataReaderObj["clicks"]);
                                    string feedback = mysqlDataReaderObj.IsDBNull(10) ? "" : Convert.ToString(mysqlDataReaderObj["feedback"]);

                                    if (string.IsNullOrWhiteSpace(identifierVal) || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(keyword))
                                        continue;

                                    DetailedSearchAnalytics detailedSearchAnalytics = new KitsuneDetailedSearchAnalytics()
                                    {
                                        WebsiteId = identifierVal,
                                        Id = id,
                                        Date = date,
                                        Keyword = keyword,
                                        PageURL = pageurl,
                                        Country = country,
                                        Device = device,
                                        AveragePosition = avgposition,
                                        Impressions = impressions,
                                        Clicks = clicks,
                                        Feedback = null
                                    };

                                    if (detailedSearchAnalytics != null)
                                    {
                                        if (String.Compare(feedback, ("relevant"), true) == 0)
                                            detailedSearchAnalytics.Feedback = SearchQueryFeedbackEnum.RELEVANT;
                                        else if (String.Compare(feedback, ("spam"), true) == 0)
                                            detailedSearchAnalytics.Feedback = SearchQueryFeedbackEnum.SPAM;

                                        detailedSearchAnalyticsList.Add(detailedSearchAnalytics);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                }
                            }

                            if (detailedSearchAnalyticsList?.Count > 0)
                                return detailedSearchAnalyticsList;
                        }
                    }

                    mysqlConnectionObj.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return null;
        }

        public static bool SubmitFeedbackForSearchAnalytics(string id, string identifier, string feedback)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(identifier))
                return false;

            try
            {
                string query = $"update searchAnalytics.gwtSearchAnalytics_kitsune set feedback=@feedback where id=@id and websiteId = @identifier";

                using (MySqlConnection mysqlConnectionObj = new MySqlConnection(GWTConnectionString))
                {
                    mysqlConnectionObj.Open();
                    using (MySqlCommand command = new MySqlCommand(query, mysqlConnectionObj))
                    {
                        command.Parameters.AddWithValue("@feedback", feedback);
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@identifier", identifier);
                        int n = command.ExecuteNonQuery();
                        if (n > 0)
                            return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return false;
        }

    }
}
