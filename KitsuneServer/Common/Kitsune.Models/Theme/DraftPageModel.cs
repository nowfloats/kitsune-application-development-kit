using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Theme
{
    public class DraftPageModel : BasePageModel
    {
        public string HtmlSourceString { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsArchived { get; set; }
        public string KObject { get; set; }
        public IList<CompilerError> Errors { get; set; }
    }
}
