using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.ProjectConfigModels
{
    public class CustomSourceSynModel
    {
        public string origin_server_address { get; set; }
        public string resource_removal_exclude { get; set; }
        public bool auto_publish { get; set; }
    }
}
