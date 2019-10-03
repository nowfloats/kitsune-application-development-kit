using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Krawler
{
    public class KrawlSQSModel
    {
        public string ProjectId { get; set; }
        public bool ReCrawl { get; set; }
    }
}
