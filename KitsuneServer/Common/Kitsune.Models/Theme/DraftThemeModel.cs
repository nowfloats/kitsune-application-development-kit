using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Theme
{
    public class DraftThemeModel : BaseThemeModel
    {
        public bool IsArchived { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
