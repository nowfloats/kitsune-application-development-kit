using Kitsune.Language.Models;
using System;
using System.Collections.Generic;

namespace Kitsune.Models
{
    public class KLMAuditLogModel
    {
        public string _id { get; set; }
        public string fpTag { get; set; }
        public ulong fpCode { get; set; }
        public ulong themeCode { get; set; }
        public DateTime createdOn { get; set; }
        public long loadTime { get; set; }
        public string themeId { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public Dictionary<string, long> functionalLog { get; set; }
        public bool isCrawler { get; set; }
        public string ipAddress { get; set; }
        public string userAgent { get; set; }
        public bool ignoreInThemeSelection { get; set; }
        //public HttpRequest request { get; set; }
    }
    public class KitsuneV2KLMRequestModel
    {
        public string ProjectId { get; set; }
        public string ClientId { get; set; }
        public string DeveloperId { get; set; }
        public string SchemaId { get; set; }
        public int ProjectVersion { get; set; }
        public string WebsiteId { get; set; }
        public string WebsiteTag { get; set; }
        public string RootPath { get; set; }
        public string HostedFilePath { get; set; }
        public string Protocol { get; set; }
        public string IncomingUrl { get; set; }
        public Project.KitsunePageType PageType { get; set; }
        public bool NoCache { get; set; }
        public string ipAddress { get; set; }
    }

public class DataApiRequestObject
    {
        public string WebsiteId;
        public List<PropertyPathSegment> PropertySegments;
    }

}
