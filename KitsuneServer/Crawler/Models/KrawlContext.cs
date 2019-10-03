using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawler.Models
{
    public class KrawlContext
    {
        /// <summary>
        /// Uri given by the User(can be only initialised once while creating the object)
        /// </summary>
        private Uri _rootUri = null;
        /// <summary>
        /// 
        /// </summary>
        private Uri _seedUri = null;
        /// <summary>
        /// Get the RootUri of the Crawler
        /// </summary>
        public Uri RootUri
        {
            get { return _rootUri; }
        }
        /// <summary>
        /// Get the _seedUri of the Crawler
        /// </summary>
        public Uri SeedUri
        {
            get { return _seedUri; }
        }
        /// <summary>
        /// Total number of pages that have been crawled
        /// </summary>
        public volatile int CrawledPagesCount = 0;
        /// <summary>
        /// Pool of all webpages to be Crawled
        /// </summary>
        public ConcurrentQueue<string> UniqueWebPageQueue;
        /// <summary>
        /// Resource Object which stores the Resource details
        /// </summary>
        public ResourcesContext Resources;
        /// <summary>
        /// Crawling Configuration
        /// </summary>
        public volatile KrawlConfiguration Configuration;
        /// <summary>
        /// Give the Function which will save the html
        /// </summary>
        public Func<string, string, bool> ProcessedHtmlCallBackMethod;
        /// <summary>
        /// Calls the Method when Resources are updated
        /// </summary>
        public Func<ResourcesContext, bool> UpdatedResoucesCallBackMethod;
        /// <summary>
        /// Callback method once one batch of Analyse is Completed
        /// </summary>
        public Func<int,bool> BatchCompletedCallBackMethod;
        /// <summary>
        /// Log the Error if any occured
        /// </summary>
        public Action<LOGTYPE, string, Exception> ErrorLogMethod;
        
        public KrawlContext(Uri rootUri)
        {
            _rootUri = rootUri;
            //_seedUri=rootUri.     TODO: create seed Uri
            UniqueWebPageQueue = new ConcurrentQueue<string>();
            Resources = new ResourcesContext();
            Configuration = new KrawlConfiguration();
            ProcessedHtmlCallBackMethod = null;
            UpdatedResoucesCallBackMethod = (ResourcesContext resources) => true;
            ErrorLogMethod = (LOGTYPE x, string y, Exception z) => Console.WriteLine("Type: {0} , Message: {1} ,Exception: {2}", x, y, z == null ? "No InnerException" : z.Message);
            BatchCompletedCallBackMethod = (int CrawledPagesCount) => true;
        }
    }
}
