using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public enum KitsuneWebMigrationStatusCompletion { NotStarted,
        IdentifyingAllAssetsAndDownloadingWebpageCompleted,
        IdentifyingExternalDomainsCompleted,
        IdentifyingKeyphrasesForWebPagesCompleted,
        DownloadingAllStaticAssetsToStorageCompleted,
        OptimisingAllStaticAssetsForPerformanceCompleted,
        UpdatingWebPagesWithNewStaticAssetUriCompleted,
        Error = -1 };

    public enum FILE { STYLES, SCRIPTS, ASSETS, LINKS };

    public class SkippedStages
    {
        public KitsuneWebMigrationStatusCompletion Stage { get; set; }
        public string Message { get; set; }
    }

    public class CrawledStats : MongoEntity
    {
        public string Url { get; set; }
        public string UserEmail { get; set; }
        public string CrawlId { get; set; }
        public KitsuneWebMigrationStatusCompletion Status { get; set; }
        public bool IsLocked { get; set; }
        public string ErrorMessage { get; set; }
        public List<SkippedStages> KitsuneWebCrawlSkippedStages { get; set; }

        //  Stage 1
        public int LinksFound { get; set; }
        public int ScriptsFound { get; set; }
        public int StylesFound { get; set; }
        public int AssetsFound { get; set; }

        //  Stage 2
        public int LinksKeywordGenerated { get; set; }
        public int TotalKeywords { get; set; }

        //  Stage 3
        public int StylesDownloaded { get; set; }
        public int ScriptsDownloaded { get; set; }
        public int AssetsDownloaded { get; set; }

        //  Stage 4
        public int NumberOfStylesProcessed { get; set; }
        public int NumberOfScriptsProcessed { get; set; }
        public int NumberOfImagesProcessed { get; set; }

        //  Stage 5
        public int LinkReplaced { get; set; }


        public long Stage1CompletionTime { get; set; }
        public long Stage2CompletionTime { get; set; }
        public long Stage3CompletionTime { get; set; }
        public long Stage4CompletionTime { get; set; }
    }
}
