using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KitsuneAdminDashboard.Web.Models
{
    public class CallLogRequest
    {
        public string Number { get; set; }
        public bool DataforAllNumbers { get; set; }
        public int Limit { get; set; }

        public bool Validate() {
            if (!String.IsNullOrEmpty(this.Number) || this.DataforAllNumbers)
            {
                return true;
            }
            return false;
        }
    }

}
