using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class GetThemeModel
    {
        public string Id { get; set; }
        public string ThemeName { get; set; }
        public string PageName { get; set; }
        public Dictionary<string, string> Categories { get; set; }
        public string HtmlString { get; set; }
    }
}
