using System;
using System.Collections.Generic;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Models
{
    public class UpdateWebsiteDetails
    {
        public string UserEmail { get; set; }
        public string ProjectId { get; set; }
        public string FaviconUrl { get; set; }
        public string ScreenShotUrl { get; set; }
    }

    public enum EventTypeWebEngage { Optimization, Publish, CrawlerAnalysed }

    public class CreateEventRequest
    {
        public string ProjectId { get; set; }
        public EventTypeWebEngage Event { get; set; }
    }
}
