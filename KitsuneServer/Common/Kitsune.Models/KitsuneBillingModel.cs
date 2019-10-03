using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class KitsuneBillingModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string for_user { get; set; }
        public DateTime from_date { get; set; }
        public DateTime to_date { get; set; }
        public DateTime date_created { get; set; }
        public string component { get; set; }
        public int usage_quantity { get; set; }
        public string usage_unit { get; set; }
        public double usage_amount { get; set; }
        public List< BillingDetail> details { get; set; }
        public string tariff_string { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string tariff { get; set; }
    }

    public class BillingDetail
    {
        public string website { get; set; }
        public int requests { get; set; }

    }


}
