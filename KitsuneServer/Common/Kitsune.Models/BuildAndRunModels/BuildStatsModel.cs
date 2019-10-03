using Kitsune.Models.PublishModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.BuildAndRunModels
{
    public enum BuildStatus
    {
        Queued = 0,
        Compiled = 1,
        QueuedBuild = 2,
        Analyzer = 3,
        Optimizer = 4,
        Replacer = 5,
        Completed = 6,
        Error = -1
    }
}
