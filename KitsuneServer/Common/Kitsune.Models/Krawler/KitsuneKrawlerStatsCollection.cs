using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Krawler
{
    public class KitsuneKrawlerStats : MongoEntity
    {
        public bool IsLocked { get; set; }
        public string ProjectId { get; set; }
        public string Url { get; set; }
        public KrawlError Error { get; set; }
        public KitsuneKrawlerStatusCompletion Stage { get; set; }
        public KrawlType CrawlType { get; set; }
        public List<AssetDetails> Links { get; set; }
        public List<AssetDetails> Styles { get; set; }
        public List<AssetDetails> Scripts { get; set; }
        public List<AssetDetails> Assets { get; set; }
        public List<string> DomainsFound { get; set; }
        public List<string> SelectedDomains { get; set; }
        public int ScriptsDownloaded { get; set; }
        public int StylesDownloaded { get; set; }
        public int AssetsDownloaded { get; set; }
        public int LinksFound { get; set; }
        public int StylesFound { get; set; }
        public int ScriptsFound { get; set; }
        public int AssetsFound { get; set; }
        public int LinksReplaced { get; set; }
        public bool StopCrawl { get; set; }
        public int LinksLimit { get; set; }
    }



    public enum KitsuneKrawlerStatusCompletion
    {
        Initialising = 0,
        IdentifyingAllAssetsAndDownloadingWebpage = 1,
        IdentifyingExternalDomains = 2,
        DownloadingAllStaticAssetsToStorage = 3,
        UpdatingWebPagesWithNewStaticAssetUri = 4,
        Completed = 5,
        Error = -1
    };

    public enum KrawlType { ShallowKrawl, DeepKrawl };

    public enum FILESTATUS { NOCHANGE, NEW, EDIT, DELETE };

    public class KrawlError
    {
        public KitsuneKrawlerStatusCompletion Stage { get; set; }
        public string ErrorMessage { get; set; }
    }
    
}
