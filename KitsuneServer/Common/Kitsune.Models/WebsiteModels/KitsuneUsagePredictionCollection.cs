using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Kitsune.Models.WebsiteModels
{
    public class KitsuneUsagePredictionCollection : MongoEntity
    {
        public string for_user { get; set; }
        public double amount_to_add_before_next_billing_cycle { get; set; }
        public double days_left { get; set; }
        public DateTime date_created { get; set; }
        public DateTime date_updated { get; set; }
        public double wallet_balance { get; set; }
    }
}

