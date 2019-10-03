using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Theme
{
    public class ProductionPageModel : BasePageModel
    {
        public int CompilerVersion { get; set; }
        public string HtmlCompiledString { get; set; }
        public ulong PageCode { get; set; }
        
    }
}
