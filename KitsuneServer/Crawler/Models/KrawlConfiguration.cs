using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler.Models
{
    public class KrawlConfiguration
    {
        public KrawlConfiguration()
        {
            MaxConcurrentThreads = 20;
            UserAgentString = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
            IsDeepCrawl = false;
            MaxPagesToCrawl = 1000;
            DeepCrawlingTimeOut = 30000;
            IsCriticalStopCrawlEnabled = false;
            IsStopCrawlEnabled = false;
        }
        /// <summary>
        /// Max concurrent threads to use for http requests
        /// </summary>
        public int MaxConcurrentThreads { get; set; }

        /// <summary>
        /// Maximum number of pages to crawl. 
        /// If zero, this setting has no effect
        /// </summary>
        public int MaxPagesToCrawl { get; set; }

        /// <summary>
        /// The user agent string to use for http requests
        /// </summary>
        public string UserAgentString { get; set; }

        /// <summary>
        /// Set DeepCrawl true if you want Rendered DOM
        /// </summary>
        public bool IsDeepCrawl { get; set; }

        /// <summary>
        /// Set the DeepCrawl Readwrite waiting time
        /// </summary>
        public int DeepCrawlingTimeOut { get; set; }

        /// <summary>
        /// Stops crawling more page (Continue crawling the existing one)
        /// </summary>
        public bool IsStopCrawlEnabled { get; set; }

        /// <summary>
        /// Stops crawling (Even the existing one)
        /// </summary>
        public bool IsCriticalStopCrawlEnabled { get; set; }
    }
}
