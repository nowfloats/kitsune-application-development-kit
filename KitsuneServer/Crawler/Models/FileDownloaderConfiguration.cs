using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler.Models
{
    public class FileDownloaderConfiguration
    {
        public FileDownloaderConfiguration()
        {
            MaxConcurrentThreads = 20;
            UserAgentString = "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko";
            CrawlCssEnabled = true;
            ReadAndWriteTimeOut = 10;
            IsStopDownloadEnabled = false;
            IsCriticalStopDownloadEnabled = false;
        }
        /// <summary>
        /// Max concurrent threads to use for http requests
        /// </summary>
        public int MaxConcurrentThreads { get; set; }

        /// <summary>
        /// The user agent string to use for http requests
        /// </summary>
        public string UserAgentString { get; set; }

        /// <summary>
        /// If selected crawl the css after downloading
        /// </summary>
        public bool CrawlCssEnabled { get; set; }

        /// <summary>
        /// Set the server waiting time
        /// </summary>
        public int ReadAndWriteTimeOut { get; set; }

        /// <summary>
        /// Stops download more page (Continue download the existing one)
        /// </summary>
        public bool IsStopDownloadEnabled { get; set; }

        /// <summary>
        /// Stops download (Even the existing one)
        /// </summary>
        public bool IsCriticalStopDownloadEnabled { get; set; }
    }
}
