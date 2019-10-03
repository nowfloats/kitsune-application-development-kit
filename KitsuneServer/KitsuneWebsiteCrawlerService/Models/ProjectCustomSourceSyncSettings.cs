using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Models
{
    public class ProjectCustomSourceSyncSettings
    {
        [JsonProperty("include_static_asset_apis")]
        public List<ExternalAPIRequestModel> IncludeStaticAssetApis { get; set; }
        [JsonProperty("include_static_assets")]
        public List<string> IncludeStaticAssets { get; set; }
        [JsonProperty("exclude")]
        public string Exclude { get; set; }                                           //Don't delete those path from KitsuneResources after Crawling, TODO:also handle in crawling
        [JsonProperty("include")]
        public string Include { get; set; }
        [JsonProperty("ignore_link_conversion")]
        public string IgnoreLinkConversion { get; set; }
        [JsonProperty("origin_server_address")]
        public string OriginSserverAddress { get; set; }
        [JsonProperty("find_and_replace")]
        public List<FindAndReplace> FindAndReplace { get; set; }
    }

    public class FindAndReplace
    {
        [JsonProperty("find")]
        public string Find { get; set; }
        [JsonProperty("replace")]
        public string Replace { get; set; }
    }

    public class ExternalAPIRequestModel
    {
        [JsonProperty("end_point")]
        public string EndPoint { get; set; }
        [JsonProperty("method")]
        public REQUESTMETHOD Method { get; set; }
        [JsonProperty("headers")]
        public List<Header> Headers{ get; set; }
    }

    public class Header
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
    
    public enum REQUESTMETHOD { GET=0,POST=1}
}
