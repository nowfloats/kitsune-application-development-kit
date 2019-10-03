using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Theme
{
    public class PendingPageModel : BasePageModel
    {
        public string HtmlSourceString { get; set; }
        public IList<CompilerError> Errors { get; set; }
        public IList<String> Comments { get; set; }
        public bool Verified { get; set; }
        public int CompilerVersion { get; set; }
        public string HtmlCompiledString { get; set; }
        public ulong PageCode { get; set; }
    }
}
