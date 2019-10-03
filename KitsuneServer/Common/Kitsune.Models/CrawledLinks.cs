using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{

    public class DomainMap
    {
        public string Domain { get; set; }
        public bool IsSuggested { get; set; }
    }

    public class LinksMap
    {
        public string LinkUrl { get; set; }
        public List<string> Keywords { get; set; }
        public string S3Url { get; set; }
        public string PlaceHolder { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public DateTime LastModified { get; set; }
    }
    public class PathMap
    {
        public string OriginalLinkUrl { get; set; }
        public string PlaceHolder { get; set; }
        public string KitsuneLinkUrl { get; set; }
        public string KitsuneOptimisedLinkUrl { get; set; }
        public int Version { get; set; }
        public DateTime LastModified { get; set; }

    }
    public class WebsitePerformance
    {
        public double ResponseTimeInMiliSec { get; set; }
        public double slow_connectionInSec { get; set; }
        public double fast_connectionInSec { get; set; }
    }
}
