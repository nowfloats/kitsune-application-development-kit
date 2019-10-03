using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Language.Models
{
    /// <summary>
    /// K-DisplayAs : Used for formating the output and specifying the type of element for output
    /// <para>Name : name of the property  </para>
    /// <para>Displa</para>
    /// </summary>
    public class KDisplayAs
    {
        public string Name { get; set; }
        public List<string> DisplayFormats { get; set; }
    }
}
