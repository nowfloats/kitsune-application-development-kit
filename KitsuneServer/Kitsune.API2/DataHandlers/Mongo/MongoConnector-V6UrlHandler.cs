using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using Kitsune.API2.Utils;
using Kitsune.Models;
using Kitsune.Models.Project;
using Kitsune.Models.Theme;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.DataHandlers.Mongo
{
    //custom functions for k_business_* queries
    public static partial class MongoConnector
    {
        #region NFX - Redirection

        internal static CommonBusinessSchemaNFXEntityModel GetProductDetails(string productId)
        {
            try
            {
                if (_kitsuneSchemaServer == null)
                    InitializeConnection();

                var productCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>("k_business_product");

                var filter = Builders<BsonDocument>.Filter.Eq("_kid", productId)
                                & Builders<BsonDocument>.Filter.Eq("isarchived", false);

                var product = productCollection.Find(filter)?.FirstOrDefault();

                if (product != null)
                    return new CommonBusinessSchemaNFXEntityModel()
                    {
                        Content = product["name"].AsString,
                        Index = (long)product["index"].AsDouble,
                        _id = product["_kid"].AsString
                    };
            }
            catch (Exception ex)
            {
                Console.Write($"Unable to GetBizFloatsDetails({productId}) - {ex.ToString()}");
            }

            return null;
        }

        internal static CommonBusinessSchemaNFXEntityModel GetProductDetailsByIndex(string merchantId, string index)
        {
            try
            {
                if (_kitsuneSchemaServer == null)
                    InitializeConnection();

                var productCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>("k_business_product");

                var filter = Builders<BsonDocument>.Filter.Eq("websiteid", merchantId)
                               & Builders<BsonDocument>.Filter.Eq("isarchived", false)
                               & Builders<BsonDocument>.Filter.Eq("index", Convert.ToDouble(index));

                var product = productCollection.Find(filter)?.FirstOrDefault();

                if (product != null)
                    return new CommonBusinessSchemaNFXEntityModel()
                    {
                        Content = product["name"].AsString,
                        Index = (long)product["index"].AsDouble,
                        _id = product["_kid"].AsString
                    };
            }
            catch (Exception ex)
            {
                Console.Write($"Unable to GetProductDetailsByIndex({merchantId}, {index}) - {ex.ToString()}");
            }

            return null;
        }

        public static CommonBusinessSchemaNFXEntityModel GetBizFloatsDetails(string dealId)
        {
            try
            {
                if (_kitsuneSchemaServer == null)
                    InitializeConnection();

                var dealCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>("k_business_update");

                var filter = Builders<BsonDocument>.Filter.Eq("_kid", dealId)
                                & Builders<BsonDocument>.Filter.Eq("isarchived", false);

                var bizFloat = dealCollection.Find(filter)?.FirstOrDefault();

                if (bizFloat != null)
                    return new CommonBusinessSchemaNFXEntityModel()
                    {
                        Content = bizFloat["title"].AsString,
                        Index = (long)bizFloat["index"].AsDouble,
                        _id = bizFloat["_kid"].AsString
                    };
            }
            catch (Exception ex)
            {
                Console.Write($"Unable to GetBizFloatsDetails({dealId}) - {ex.ToString()}");
            }

            return null;
        }

        public static CommonBusinessSchemaNFXEntityModel GetBizFloatsDetailsByIndex(string merchantId, string index)
        {
            try
            {
                if (_kitsuneSchemaServer == null)
                    InitializeConnection();

                var dealCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>("k_business_update");

                var filter = Builders<BsonDocument>.Filter.Eq("websiteid", merchantId)
                                & Builders<BsonDocument>.Filter.Eq("isarchived", false)
                                & Builders<BsonDocument>.Filter.Eq("index", Convert.ToDouble(index));

                var bizFloat = dealCollection.Find(filter)?.FirstOrDefault();

                if (bizFloat != null)
                    return new CommonBusinessSchemaNFXEntityModel()
                    {
                        Content = bizFloat["title"].AsString,
                        Index = (long)bizFloat["index"].AsDouble,
                        _id = bizFloat["_kid"].AsString
                    };
            }
            catch (Exception ex)
            {
                Console.Write($"Unable to GetBizFloatsDetailsByIndex({merchantId}, {index}) - {ex.ToString()}");
            }

            return null;
        }

        internal static string GetBizFloatUrlPattern(string themeId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var temp_themeCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(MongoConnector.ProductionResorcesCollectionName);
                var temp_productDefinitionBuilder = new ProjectionDefinitionBuilder<ProductionKitsuneResource>();
                var temp_pd = temp_productDefinitionBuilder.Include(x => x._id).Include(x => x.UrlPattern).Include(x => x.IsDefault).Include(x => x.UrlPatternRegex).Include(x => x.IsStatic).Include(s => s.PageType);

                var temp_result = temp_themeCollection.Find(x => x.ProjectId == themeId && x.PageType == Kitsune.Models.Project.KitsunePageType.DETAILS && x.KObject.Contains("business.updates")).Project<ProductionKitsuneResource>(temp_pd)?.ToList();

                if (temp_result != null && temp_result.Count() > 0)
                {
                    return temp_result.FirstOrDefault().UrlPattern;
                }
                else
                {
                    temp_result = temp_themeCollection.Find(x => x.ProjectId == themeId
                                            && (x.UrlPatternRegex.ToLower().Contains("([a-zA-Z0-9\\-\\.,\\%]+)/b-([a-zA-Z0-9\\-\\.,\\%]+)") ||
                                            x.UrlPatternRegex.ToLower().Contains("([a-zA-Z0-9\\-\\.,\\%]+)/u-([a-zA-Z0-9\\-\\.,\\%]+)") ||
                                            x.UrlPatternRegex.ToLower().Contains("([a-zA-Z0-9\\-\\.,\\%]+)/u([a-zA-Z0-9\\-\\.,\\%]+)") ||
                                            (x.UrlPatternRegex.ToLower().Contains("([a-zA-Z0-9\\-\\.,\\%]+)/b([a-zA-Z0-9\\-\\.,\\%]+)")))).Project<ProductionKitsuneResource>(temp_pd)?.ToList();

                    if (temp_result != null && temp_result.Count() > 0)
                    {
                        return temp_result.FirstOrDefault().UrlPattern;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write($"Exception: GetBizFloatUrlPattern({themeId}) - {ex.ToString()}");
            }

            return null;
        }

        internal static string GetProductUrlPattern(string themeId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var temp_themeCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(MongoConnector.ProductionResorcesCollectionName);
                var temp_productDefinitionBuilder = new ProjectionDefinitionBuilder<ProductionKitsuneResource>();
                var temp_pd = temp_productDefinitionBuilder.Include(x => x._id).Include(x => x.UrlPattern).Include(x => x.IsDefault).Include(x => x.UrlPatternRegex).Include(x => x.IsStatic).Include(s => s.PageType);

                var temp_result = temp_themeCollection.Find(x => x.ProjectId == themeId && x.PageType == Kitsune.Models.Project.KitsunePageType.DETAILS && x.KObject.Contains("business.products")).Project<ProductionKitsuneResource>(temp_pd)?.ToList();

                if (temp_result != null && temp_result.Count() > 0)
                {
                    return temp_result.FirstOrDefault().UrlPattern;
                }
                else
                {
                    temp_result = temp_themeCollection.Find(x => x.ProjectId == themeId
                                    && (x.UrlPatternRegex.ToLower().Contains("([a-zA-Z0-9\\-\\.,\\%]+)/p([a-zA-Z0-9\\-\\.,\\%]+)")
                                    || (x.UrlPatternRegex.ToLower().Contains("([a-zA-Z0-9\\-\\.,\\%]+)/p-([a-zA-Z0-9\\-\\.,\\%]+)")))).Project<ProductionKitsuneResource>(temp_pd)?.ToList();

                    if (temp_result != null && temp_result.Count() > 0)
                    {
                        return temp_result.FirstOrDefault().UrlPattern;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write($"Exception: GetProductUrlPattern({themeId}) - {ex.ToString()}");
            }

            return null;
        }

        #endregion
    }
}
