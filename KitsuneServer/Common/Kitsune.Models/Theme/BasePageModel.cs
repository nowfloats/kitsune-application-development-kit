using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Theme
{
    public class BasePageModel : MongoEntity
    {
        public string ThemeId { get; set; }
        public string PageName { get; set; }
        public string ClassName { get; set; }
        public string UrlPattern { get; set; }
        public string UrlPatternRegex { get; set; }
        public bool IsStatic { get; set; }
        public bool IsDefault { get; set; }
        public int Version { get; set; }
        public KitsunePageType PageType { get; set; }
    }
    public enum KitsunePageType
    {
        Default = 0,
        List = 1,
        Details = 2,
        Search = 3,
        Partial = 4
    }
}
