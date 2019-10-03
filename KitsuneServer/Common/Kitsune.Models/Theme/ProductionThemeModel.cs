using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Theme
{
    public class ProductionThemeModel : BaseThemeModel
    {
        public string ThemeId { get; set; }
        public int CompilerVersion { get; set; }
        public ulong ThemeCode { get; set; }
    }
}
