using Kitsune.Models.Krawler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawler.Models
{
    public class ResourcesContext
    {
        /// <summary>
        /// Dictionary of all Styles found in given Url Domain
        /// </summary>
        public volatile ConcurrentDictionary<string, AssetDetails> UniqueStylesDictionary;
        /// <summary>
        /// Dictionary of all Assets found in given Url Domain
        /// </summary>
        public volatile ConcurrentDictionary<string, AssetDetails> UniqueAssetsDictionary;
        /// <summary>
        /// Dictionary of all Scripts found in given Url Domain
        /// </summary>
        public volatile ConcurrentDictionary<string, AssetDetails> UniqueScriptsDictionary;
        /// <summary>
        /// Dictionary of all webpages found
        /// </summary>
        public volatile ConcurrentDictionary<string, AssetDetails> UniqueWebpagesDictionary;
        /// <summary>
        /// List of all Extrenal Domains found in given Url Domain
        /// </summary>
        public volatile HashSet<string> ExternalDomains;
        /// <summary>
        /// Regex string url to ignore
        /// </summary>
        public string ExcludeFilesRegex;
        /// <summary>
        /// Regex string url to include
        /// </summary>
        public string IncludeFilesRegex;
        /// <summary>
        /// Regex string url to ignore
        /// </summary>
        public List<string> IncludeStaticAssetList;
        /// <summary>
        /// Ignore File name change
        /// </summary>
        public Regex IgnoreFileNameChangeRegex;


        public ResourcesContext(string excludeFilesRegex = null,string ignoreFileNameChangeRegex=null,List<string> include_static_asset_folder=null)
        {
            UniqueStylesDictionary = new ConcurrentDictionary<string, AssetDetails>();
            UniqueAssetsDictionary = new ConcurrentDictionary<string, AssetDetails>();
            UniqueScriptsDictionary = new ConcurrentDictionary<string, AssetDetails>();
            UniqueWebpagesDictionary = new ConcurrentDictionary<string, AssetDetails>();
            ExternalDomains = new HashSet<string>();
            ExcludeFilesRegex = excludeFilesRegex;
            IncludeStaticAssetList = include_static_asset_folder;
            if (ignoreFileNameChangeRegex != null)
                IgnoreFileNameChangeRegex = new Regex(ignoreFileNameChangeRegex);
            else
                IgnoreFileNameChangeRegex = null;
        }
    }
}
