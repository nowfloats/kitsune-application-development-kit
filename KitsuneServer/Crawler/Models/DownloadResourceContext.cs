using Kitsune.Models.Krawler;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler.Models
{
    public class DownloadResourceContext
    {
        /// <summary>
        /// Dictionary of all Styles found in given Url Domain
        /// </summary>
        public List<AssetDetails> Styles;
        /// <summary>
        /// Dictionary of all Styles found in given Url Domain
        /// </summary>
        public List<AssetDetails> Scripts;
        /// <summary>
        /// Dictionary of all Styles found in given Url Domain
        /// </summary>
        public List<AssetDetails> Assets;
        /// <summary>
        /// Dictionary of all Styles found in given Url Domain
        /// </summary>
        public List<string> SelectedDomains;
    }
}
