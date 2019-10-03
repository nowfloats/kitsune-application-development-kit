using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Theme
{
    public class PendingThemeModel : BaseThemeModel
    {
        public string ThemeId { get; set; }
        public Status Status { get; set; }
        public ulong ThemeCode { get; set; }
        public string Admin { get; set; }
        public long? VerificationPercentage { get; set; }
        public int CompilerVersion { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
