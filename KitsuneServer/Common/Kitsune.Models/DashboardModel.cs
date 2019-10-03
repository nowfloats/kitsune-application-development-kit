using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class MongoVisitorData
    {
        public ObjectId _id { get; set; }
        public string idSite { get; set; }
        public string idVisit { get; set; }
        public string visitIp { get; set; }
        public string visitorId { get; set; }
        public int goalConversions { get; set; }
        public string siteCurrency { get; set; }
        public string siteCurrencySymbol { get; set; }
        public DateTime serverDate { get; set; }//changed
        public int visitServerHour { get; set; }//changed
        public long lastActionTimestamp { get; set; }//changed
        public DateTime lastActionDateTime { get; set; }//changed
        [BsonIgnore]
        public object userId { get; set; }
        public string visitorType { get; set; }
        public string visitorTypeIcon { get; set; }
        public int visitConverted { get; set; }
        [BsonIgnore]
        public object visitConvertedIcon { get; set; }
        public long visitCount { get; set; }//changed
        public long firstActionTimestamp { get; set; }//changed
        public string visitEcommerceStatus { get; set; }
        [BsonIgnore]
        public object visitEcommerceStatusIcon { get; set; }
        public int daysSinceFirstVisit { get; set; }//changed
        public int daysSinceLastEcommerceOrder { get; set; }//changed
        public long visitDuration { get; set; }
        public string visitDurationPretty { get; set; }
        public int searches { get; set; }//changed(doubt)
        public int actions { get; set; }//changed
        public string referrerType { get; set; }
        public string referrerTypeName { get; set; }
        public string referrerName { get; set; }
        public string referrerKeyword { get; set; }
        [BsonIgnore]
        public object referrerKeywordPosition { get; set; }
        [BsonIgnore]
        public object referrerUrl { get; set; }
        public object referrerSearchEngineUrl { get; set; }
        [BsonIgnore]
        public object referrerSearchEngineIcon { get; set; }
        public string languageCode { get; set; }
        public string language { get; set; }
        public string deviceType { get; set; }
        public string deviceTypeIcon { get; set; }
        public string deviceBrand { get; set; }
        public string deviceModel { get; set; }
        public string operatingSystem { get; set; }
        public string operatingSystemName { get; set; }
        public string operatingSystemIcon { get; set; }
        public string operatingSystemCode { get; set; }
        public string operatingSystemVersion { get; set; }
        public string browserFamily { get; set; }
        public string browserFamilyDescription { get; set; }
        public string browser { get; set; }
        public string browserName { get; set; }
        public string browserIcon { get; set; }
        public string browserCode { get; set; }
        public string browserVersion { get; set; }
        public int events { get; set; }//changed
        public string continent { get; set; }
        public string continentCode { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
        public string countryFlag { get; set; }
        public string region { get; set; }
        public string regionCode { get; set; }
        public string city { get; set; }
        public string location { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string visitLocalTime { get; set; }
        public string visitLocalHour { get; set; }
        public int daysSinceLastVisit { get; set; }//changed
        public string themeName { get; set; }
        public string themeid { get; set; }
        public string viewid { get; set; }
        public string logid { get; set; }
        public string fpid { get; set; }
        public string categoryid { get; set; }
        public string resolution { get; set; }
        public string plugins { get; set; }
        public double serverTimestamp { get; set; }
        public string serverTimePretty { get; set; }
        public DateTime serverDatePretty { get; set; }//change
        public DateTime serverDatePrettyFirstAction { get; set; }//change
        public string serverTimePrettyFirstAction { get; set; }
        public bool isBounced { get; set; }

        public ulong ThemeCode { get; set; }
        public bool isNotConsideredForThemePicking { get; set; }

    }

    public class ActionDetail
    {
        public ObjectId _id { get; set; }
        public string idVisit { get; set; }
        public string visitorId { get; set; }
        public string themeName { get; set; }
        public string themeId { get; set; }
        public string viewId { get; set; }
        public string logId { get; set; }
        public string fpId { get; set; }
        public string categoryId { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string pageTitle { get; set; }
        public string pageIdAction { get; set; }
        public DateTime serverTimePretty { get; set; }
        public string pageId { get; set; }
        public double generationTime { get; set; }
        public string timeSpent { get; set; }
        public string timeSpentPretty { get; set; }
        public string icon { get; set; }
        public int timestamp { get; set; }
        public string eventCategory { get; set; }
        public string eventAction { get; set; }
        public string eventName { get; set; }
    }
}
