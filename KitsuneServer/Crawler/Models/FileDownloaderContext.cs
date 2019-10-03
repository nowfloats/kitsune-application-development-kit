using Kitsune.Models.Krawler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Crawler.Models
{
    public class FileDownloaderContext
    {
        public FileDownloaderContext(Uri uri)
        {
            _rootUri = uri;
            DownloadedFilesCount = 0;
            Resources = new DownloadResourceContext();
            Configuration = new FileDownloaderConfiguration();
            FileDownloadQueue = new ConcurrentQueue<Tuple<AssetDetails, FileType>>();
            ErrorLogMethod = (LOGTYPE type, string message, Exception innerException) => Console.WriteLine("Error Message:{0}", message);
            NewAssetFoundDetailsCallBackMethod = (AssetDetails asset, FileType type) => Console.WriteLine("New File Found, Url: {0}", asset.LinkUrl);
            DownloadedFileCallBackMethod = (AssetDetails asset, FileType fileType, Byte[] data,string contentType) => Console.WriteLine("Path : {0}", asset.LinkUrl);
        }
        /// <summary>
        /// Uri given by the User(can be only initialised once while creating the object)
        /// </summary>
        private Uri _rootUri = null;

        /// <summary>
        /// Get the RootUri of the Crawler
        /// </summary>
        public Uri RootUri
        {
            get { return _rootUri; }
        }

        /// <summary>
        /// Total number of Files Downloaded
        /// </summary>
        public volatile int DownloadedFilesCount = 0;

        /// <summary>
        /// Resources Details
        /// </summary>
        public DownloadResourceContext Resources;


        /// <summary>
        /// Downloader Configurations
        /// </summary>
        public FileDownloaderConfiguration Configuration;

        /// <summary>
        /// Call back function for Downloaded file
        /// </summary>
        public Action<AssetDetails, FileType, Byte[],string> DownloadedFileCallBackMethod;

        /// <summary>
        /// New Found Asset Details
        /// </summary>
        public Action<AssetDetails, FileType> NewAssetFoundDetailsCallBackMethod;

        /// <summary>
        /// Error Log call back function
        /// </summary>
        public Action<LOGTYPE, string, Exception> ErrorLogMethod;

        /// <summary>
        /// Queue to download the files
        /// </summary>
        public ConcurrentQueue<Tuple<AssetDetails, FileType>> FileDownloadQueue;

        /// <summary>
        /// Queue to download the files
        /// </summary>
        public Func<bool> BatchCompletedCallBackMethod;
    }
}
