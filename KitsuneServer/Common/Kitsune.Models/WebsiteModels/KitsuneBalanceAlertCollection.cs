using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.WebsiteModels
{
    public class KitsuneBalanceAlertCollection : MongoEntity
    {
        public string for_user { get; set; }
        public int alerted_before_days { get; set; }
        public int category { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_updated { get; set; }
        public double wallet_balance { get; set; }
    }
}
