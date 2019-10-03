using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler.HtmlParser
{
    public class BaseTag
    {
        public bool Exists { get; set; }
        public Uri Href { get; set; }
    }
}
