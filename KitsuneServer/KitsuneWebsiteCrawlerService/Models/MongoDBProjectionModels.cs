using Crawler.Models;
using Kitsune.Models.Krawler;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Models
{
    public class ResoucesDetails
    {
        public List<AssetDetails> Styles { get; set; }
        public List<AssetDetails> Scripts { get; set; }
        public List<AssetDetails> Assets { get; set; }
        public List<AssetDetails> Links { get; set; }
    }

    public class ListOfSelectedDomains
    {
        public List<string> SelectedDomains { get; set; }
    }

    public class CrawlProjectDetails
    {
        public string Url { get; set; }
        public KitsuneKrawlerStatusCompletion Stage { get; set; }
        public KrawlType CrawlType { get; set; }
        public int LinksLimit { get; set; }
        public bool StopCrawl { get; set; }
    }
}
