using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class KitsuneProjects: MongoEntity
    {
        public string ProjectName { get; set; }
        public string Url { get; set; }
        public string CDNDomainName { get; set; }
        public string DistributionId { get; set; }
        public string UserEmail { get; set; }
        public List<LinksMap> Links { get; set; }
        public List<PathMap> Styles { get; set; }
        public List<PathMap> Scripts { get; set; }
        public List<PathMap> Assets { get; set; }
        public List<DomainMap> DomainList { get; set; }
        public List<string> SelectedDomains { get; set; }
        public WebsitePerformance OriginalWebsitePerformance { get; set; }
        public WebsitePerformance KitsuneWebsitePerformance { get; set; }
        public string originalRobots { get; set; }

        //Add new fields
        public bool IsCompleted { get; set; }
        public bool IsPublishing { get; set; }
        public DateTime CompletedOn { get; set; }
        public string CrawlId { get; set; }
        public string KitsuneUrl { get; set; }
        public string ImageUrl { get; set; }
        public string FaviconIconUrl { get; set; }
        public bool IsActivated { get; set; }
        public bool IsArchived { get; set; }

        public string ClientId { get; set; }
    }
}
