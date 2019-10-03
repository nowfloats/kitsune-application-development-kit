using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Krawler
{
    public class AssetDetails : IEquatable<string>
    {
        public string LinkUrl { get; set; }
        public string PlaceHolder { get; set; }
        public string NewUrl { get; set; }
        public HttpStatusCode ResponseStatusCode { get; set; }

        public bool Equals(string url)
        {
            if (this.LinkUrl == url)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public enum FileType { LINK, STYLE, ASSET, SCRIPT };
}
