using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Project
{
    public class ProductionKitsuneProject : AuditKitsuneProject
    {
        public bool RuntimeOptimization { get; set; }
    }
}
