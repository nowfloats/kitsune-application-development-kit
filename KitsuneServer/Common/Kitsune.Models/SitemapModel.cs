using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class SitemapGenerationTaskModel
    {
        public string ProjectId;
        public string WebsiteId;
        public string WebsiteUrl;
        public List<string> ExcludePaths;
    }

    //S3-Config details
    /// <summary>
    /// key - folder/file path
    /// </summary>
    public class SitemapGenerationTaskParameterModel
    {
        public string region { get; set; }
        public string bucket { get; set; }
        public string key { get; set; }
        public string url { get; set; }
        public string acl { get; set; }
        public string access_key { get; set; }
        public string secret { get; set; }
        public string disallow_regex { get; set; }
    }

    public class SitemapGenerationTaskQueueModel
    {
        //1-API, 2-S3 config
        public int type { get; set; }
        public SitemapGenerationTaskParameterModel param { get; set; }
    }
}
