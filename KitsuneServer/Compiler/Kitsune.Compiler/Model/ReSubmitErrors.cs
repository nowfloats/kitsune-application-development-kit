using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Compiler.Model
{
    public class ReSubmitErrors
    {
        public string ThemeId { get; set; }
        public int Version { get; set; }
        public int CompilerVersion { get; set; }
        public string UserEmail { get; set; }
        public string Admin { get; set; }
    }
}
