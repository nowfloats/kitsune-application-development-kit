using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Language.Models
{
    /// <summary>
    /// KFunction : for language predefined functions.
    /// </summary>
    public class KFunction
    {
        /// <summary>
        /// Function name
        /// </summary>
        public string Name { get; set; }
        public string ReturnType { get; set; }
        /// <summary>
        /// Parameters of the function
        /// </summary>
        public string[] ParamsTypes { get; set; }
        public string FunctionBody { get; set; }
    }
}
