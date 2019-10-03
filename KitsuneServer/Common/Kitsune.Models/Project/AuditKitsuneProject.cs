using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Project
{
    public class AuditKitsuneProject : BaseKitsuneProject
    {
        public DateTime PublishedOn { get; set; }
    }
}
