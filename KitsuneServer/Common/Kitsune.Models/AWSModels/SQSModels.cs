using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.AWSModels
{
    public class S3FolderCopyQueueModdel
    {
        public string SourceBucket { get; set; }
        public string SourceFolder { get; set; }
        public string DestinationBucket { get; set; }
        public string DestinationFolder { get; set; }
    }
}
