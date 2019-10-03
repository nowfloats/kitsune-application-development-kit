using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KitsuneAdminDashboard.Web.Models
{
    public class TabExistsResponse
    {
        public bool DoesExists { get; set; }
        public bool IsError { get; set; }
    }

    public class TabsVisisbilityStatus
    {
        public bool Orders { get; set; }
        public bool CallLogs { get; set; }
    }
}
