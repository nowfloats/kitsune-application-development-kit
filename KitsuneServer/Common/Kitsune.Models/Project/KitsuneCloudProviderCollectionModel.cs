using Kitsune.Models.Cloud;
using System;

namespace Kitsune.Models.ProjectModels
{
    public class KitsuneCloudProviderCollectionModel
    {
        public MongoDB.Bson.ObjectId _id;
        public string projectid;
        public CloudProvider provider;
        public string accountid;
        public string key;
        public string region;
        //TODO: Secret always to be encrypted before saving to db.
        public string secret;
        public bool isarchived;
        public DateTime createddate;
    }
}
