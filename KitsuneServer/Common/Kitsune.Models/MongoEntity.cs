using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class MongoEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    [BsonIgnoreExtraElements]
    public class MongoCollectionStatus
    {
        [BsonElement("ns")]
        public string NameSpace { get; set; }
        [BsonElement("size")]
        public long Size { get; set; }
        [BsonElement("avgObjSize")]
        public long AvgObjSize { get; set; }
        [BsonElement("storageSize")]
        public long StorageSize { get; set; }
        [BsonElement("totalIndexSize")]
        public long TotalIndexSize { get; set; }
        [BsonElement("nindexes")]
        public long NumberOfIndexes { get; set; }
        [BsonElement("count")]
        public long Count { get; set; }
    }

}
