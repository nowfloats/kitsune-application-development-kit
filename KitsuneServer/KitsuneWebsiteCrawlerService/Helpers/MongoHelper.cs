using Crawler.Models;
using Kitsune.Models.Krawler;
using Kitsune.Models.Project;
using KitsuneWebsiteCrawlerService.Constants;
using KitsuneWebsiteCrawlerService.Models;
using MongoDB.Driver;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitsuneWebsiteCrawlerService.Helpers
{
    public class MongoHelper
    {
        private static IMongoClient _server;
        private static IMongoDatabase _kitsuneDB;
        private static string serverUrl = EnvironmentConstants.ApplicationConfiguration.MongoDBConfiguration.MongoConnectionUrl;
        private static string dbName = EnvironmentConstants.ApplicationConfiguration.MongoDBConfiguration.DataBaseName;

        internal static void InitiateConnection()
        {
            try
            {
                if (_server == null)
                {
                    _server = new MongoClient(serverUrl);
                    _kitsuneDB = _server.GetDatabase(dbName);
                }
            }
            catch (Exception ex)
            {
                //EventLogger.Write(ex, "Error while Connecting with DB");
            }
        }

        /// <summary>
        /// Update the Resources Details to KitsuneProjects DB
        /// </summary>
        /// <param name="crawlId"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
        public static bool UpdateTheAnalyseStageDetailsInDB(string crawlId, ResourcesContext resources)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();

                var urlCollection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);

                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filter = fdb.Where(x => x.ProjectId == crawlId);


                //Update Defination
                var udb = new UpdateDefinitionBuilder<KitsuneKrawlerStats>();
                var update = udb.Set(x => x.Links, resources.UniqueWebpagesDictionary.Values.ToList())
                                .Set(x => x.Scripts, resources.UniqueScriptsDictionary.Values.ToList())
                                .Set(x => x.Styles, resources.UniqueStylesDictionary.Values.ToList())
                                .Set(x => x.Assets, resources.UniqueAssetsDictionary.Values.ToList())
                                .Set(x => x.DomainsFound, resources.ExternalDomains.ToList())
                                .Set(x => x.LinksFound, resources.UniqueWebpagesDictionary.Values.Count())
                                .Set(x => x.StylesFound, resources.UniqueStylesDictionary.Values.Count())
                                .Set(x => x.ScriptsFound, resources.UniqueScriptsDictionary.Values.Count())
                                .Set(x => x.AssetsFound, resources.UniqueAssetsDictionary.Values.Count());

                //Process
                var result = urlCollection.UpdateOne(filter, update, new UpdateOptions { IsUpsert = true });


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Update the Styles,Scripts and Assets Details to KitsuneProjects DB
        /// </summary>
        /// <param name="crawlId"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
        public static bool UpdateTheDownloadStageDetailsInDB(string crawlId, DownloadResourceContext resources)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();

                var urlCollection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);

                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filter = fdb.Where(x => x.ProjectId == crawlId);

                //Update Defination
                var udb = new UpdateDefinitionBuilder<KitsuneKrawlerStats>();
                var update = udb.Set(x => x.Scripts, resources.Scripts.ToList())
                                .Set(x => x.Styles, resources.Styles.ToList())
                                .Set(x => x.Assets, resources.Assets.ToList());

                //Process
                var result = urlCollection.UpdateOne(filter, update, new UpdateOptions { IsUpsert = true });


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Update Stage in DB
        /// and lock the project
        /// </summary>
        /// <param name="crawlId"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
        public static bool UpdateCrawlStatsStage(string crawlId, KitsuneKrawlerStatusCompletion stage)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();

                var urlCollection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);

                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filter = fdb.Where(x => x.ProjectId == crawlId);

                //Update Defination
                var udb = new UpdateDefinitionBuilder<KitsuneKrawlerStats>();
                var update = udb.Set(x => x.Stage, stage)
                                .Set(x => x.IsLocked, false);

                //Process
                var result = urlCollection.UpdateOne(filter, update);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Update Download details
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="asset"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async static Task<bool> UpdateDownloadDetailsAsync(string projectId, AssetDetails asset, FileType type)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();

                var urlCollection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);

                //Filters Initialise
                var projectFilter = Builders<KitsuneKrawlerStats>.Filter.Where(x => x.ProjectId == projectId);
                var listFilter = Builders<KitsuneKrawlerStats>.Filter.Empty;
                var filter = Builders<KitsuneKrawlerStats>.Filter.Empty;

                //Update Initialise
                var update = Builders<KitsuneKrawlerStats>.Update.Set(x => x.ProjectId, projectId);

                switch (type)
                {
                    case FileType.ASSET:
                        listFilter = Builders<KitsuneKrawlerStats>.Filter.ElemMatch(x => x.Assets, x => x.LinkUrl.Equals(asset.LinkUrl));
                        filter = Builders<KitsuneKrawlerStats>.Filter.And(projectFilter, listFilter);
                        update = Builders<KitsuneKrawlerStats>.Update.Set(x => x.Assets[-1], asset)
                                                                     .Inc(x => x.AssetsDownloaded, 1);
                        break;
                    case FileType.SCRIPT:
                        listFilter = Builders<KitsuneKrawlerStats>.Filter.ElemMatch(x => x.Scripts, x => x.LinkUrl.Equals(asset.LinkUrl));
                        filter = Builders<KitsuneKrawlerStats>.Filter.And(projectFilter, listFilter);
                        update = Builders<KitsuneKrawlerStats>.Update.Set(x => x.Scripts[-1], asset)
                                                                     .Inc(x => x.ScriptsDownloaded, 1);
                        break;
                    case FileType.STYLE:
                        listFilter = Builders<KitsuneKrawlerStats>.Filter.ElemMatch(x => x.Styles, x => x.LinkUrl.Equals(asset.LinkUrl));
                        filter = Builders<KitsuneKrawlerStats>.Filter.And(projectFilter, listFilter);
                        update = Builders<KitsuneKrawlerStats>.Update.Set(x => x.Styles[-1], asset)
                                                                     .Inc(x => x.StylesDownloaded, 1);
                        break;
                    default:
                        return false;
                }

                var result = await urlCollection.UpdateOneAsync(filter, update);

                if (result.IsAcknowledged && result.IsModifiedCountAvailable)
                {
                    if (result.MatchedCount <= 0)
                    {
                        //Not Present (Add a new one)
                        switch (type)
                        {
                            case FileType.ASSET:
                                update = Builders<KitsuneKrawlerStats>.Update.Push(x => x.Assets, asset);
                                break;
                            case FileType.SCRIPT:
                                update = Builders<KitsuneKrawlerStats>.Update.Push(x => x.Scripts, asset);
                                break;
                            case FileType.STYLE:
                                update = Builders<KitsuneKrawlerStats>.Update.Push(x => x.Styles, asset);
                                break;
                            default:
                                return false;
                        }

                        result = await urlCollection.UpdateOneAsync(projectFilter, update);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        /// <summary>
        /// Add new Asset found to DB
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="asset"></param>
        /// <param name="type"></param>
        public static void UpdateNewAssetToDB(string projectId, AssetDetails asset, FileType type)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();

                var urlCollection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);

                var projectFilter = Builders<KitsuneKrawlerStats>.Filter.Where(x => x.ProjectId == projectId);
                var update = Builders<KitsuneKrawlerStats>.Update.Set(x => x.ProjectId, projectId);

                switch (type)
                {
                    case FileType.ASSET:
                        update = Builders<KitsuneKrawlerStats>.Update.Push(x => x.Assets, asset)
                                                                     .Inc(x => x.AssetsFound, 1);
                        break;
                    case FileType.SCRIPT:
                        update = Builders<KitsuneKrawlerStats>.Update.Push(x => x.Scripts, asset)
                                                                     .Inc(x => x.ScriptsFound, 1);
                        break;
                    case FileType.STYLE:
                        update = Builders<KitsuneKrawlerStats>.Update.Push(x => x.Styles, asset)
                                                                     .Inc(x => x.StylesFound, 1);
                        break;
                    case FileType.LINK:
                        update = Builders<KitsuneKrawlerStats>.Update.Push(x => x.Links, asset)
                                                                     .Inc(x => x.LinksFound, 1);
                        break;
                    default:
                        return;
                }

                urlCollection.UpdateOneAsync(projectFilter, update);
            }
            catch (Exception ex)
            {
                //TODO: LOG
            }
        }


        /// <summary>
        /// Update Error status to the Krawler DB
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="error"></param>
        public static void UpdateCrawlErrorMessage(string projectId, KrawlError error)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();

                var urlCollection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);

                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filter = fdb.Where(x => x.ProjectId == projectId);

                //Update Defination
                var udb = new UpdateDefinitionBuilder<KitsuneKrawlerStats>();
                var update = udb.Set(x => x.Stage, KitsuneKrawlerStatusCompletion.Error)
                                .Set(x => x.IsLocked, false)
                                .Set(x => x.Error, error);

                //Process
                var result = urlCollection.UpdateOne(filter, update);


                APIHelper.UpdateKitsuneProjectsStatus(projectId, ProjectStatus.ERROR);


            }
            catch (Exception ex)
            {
                //TODO:Log
            }
        }


        /// <summary>
        /// Get all the Resouces(Styles, Scripts and Assets) details from KitsuneProjects DB
        /// </summary>
        /// <param name="crawlId"></param>
        /// <returns></returns>
        public static ResoucesDetails GetAllTheResourcesOfWebite(string crawlId)
        {
            try
            {
                if (crawlId == null)
                    throw new Exception("CrawlId cannot be Null");
                if (_server == null)
                    InitiateConnection();
                var urlCollection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);

                //Filter Defination
                var filterDefinationBuilder = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filterDefination = filterDefinationBuilder.Where(x => x.ProjectId == crawlId);

                //Projection Defination
                var projectDefinationBuilder = new ProjectionDefinitionBuilder<KitsuneKrawlerStats>();
                ProjectionDefinition<KitsuneKrawlerStats> projectDefination = projectDefinationBuilder.Include(x => x.Assets)
                                                                                                  .Include(x => x.Scripts)
                                                                                                  .Include(x => x.Styles)
                                                                                                  .Include(x => x.Links)
                                                                                                  .Exclude(x => x._id);

                //Process
                var result = urlCollection.Find(filterDefination)
                                       .Project<ResoucesDetails>(projectDefination)
                                       .FirstOrDefaultAsync()
                                       .Result;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get list of all selected Domains
        /// </summary>
        /// <mparam name="crawlId"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static List<string> GetSelectedDomainNames(string crawlId)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();
                var collection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);


                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filter = fdb.Where(x => x.ProjectId == crawlId);

                //Projection Defination
                var projectionDefination = Builders<KitsuneKrawlerStats>.Projection.Include(x => x.SelectedDomains)
                                                                                   .Exclude(x => x._id);

                //Process
                var SelectedDomainsObj = collection.Find(filter)
                                                    .Project<ListOfSelectedDomains>(projectionDefination)
                                                    .FirstOrDefault();

                if (SelectedDomainsObj != null)
                    return SelectedDomainsObj.SelectedDomains;
                else
                    throw new Exception("Object returned by DB was null");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get Crawling DB Details
        /// </summary>
        /// <param name="crawlId"></param>
        /// <returns></returns>
        public static CrawlProjectDetails GetCrawlingDetails(string crawlId)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();
                var collection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);


                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filter = fdb.Where(x => x.ProjectId == crawlId &&
                x.Stage != KitsuneKrawlerStatusCompletion.Error &&
                x.Stage != KitsuneKrawlerStatusCompletion.IdentifyingExternalDomains &&
                x.Stage != KitsuneKrawlerStatusCompletion.Completed &&
                !x.IsLocked);

                //Projection Defination
                var projectionDefination = Builders<KitsuneKrawlerStats>.Projection.Include(x => x.Url)
                                                                               .Include(x => x.Stage)
                                                                               .Include(x => x.CrawlType)
                                                                               .Exclude(x => x._id);

                //Process
                var projectDetails = collection.Find(filter)
                                               .Project<CrawlProjectDetails>(projectionDefination)
                                               .FirstOrDefault();

                if (projectDetails != null)
                {
                    try
                    {
                        var updateBuilder = new UpdateDefinitionBuilder<KitsuneKrawlerStats>();
                        var update = updateBuilder.Set(x => x.IsLocked, true);
                        collection.UpdateOneAsync<KitsuneKrawlerStats>(x => x.ProjectId.Equals(crawlId), update);
                    }
                    catch { }
                }
                return projectDetails;
            }
            catch (Exception ex)
            {
                //LOG
                Log.Error($"ProjectId:{crawlId}, Message:Error while Getting crawling details with Error : {ex.Message}");
                return null;
            }
        }

        public static void UpdateErrorDetails()
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        public static async Task UpdateDBForPlaceHolderReplacer(string projectId)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();

                var urlCollection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);

                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filter = fdb.Where(x => x.ProjectId == projectId);

                //Update Defination
                var udb = new UpdateDefinitionBuilder<KitsuneKrawlerStats>();
                UpdateDefinition<KitsuneKrawlerStats> update = udb.Inc(x => x.LinksReplaced, 1);

                //Process
                var result = await urlCollection.UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                Log.Error(ex, $"ProjectId:{projectId}, Message:Error updating the Link processed");
            }
        }

        public static KitsuneKrawlerStats GetCrawlStatsDetails(string projectId)
        {
            try
            {
                if (_server == null)
                    InitiateConnection();
                var collection = _kitsuneDB.GetCollection<KitsuneKrawlerStats>(EnvironmentConstants.ApplicationConfiguration.MongoDBCollections.KitsuneKrawlStatsCollection);


                //Filter Defination
                var fdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                var filter = fdb.Where(x => x.ProjectId == projectId);

                //Projection Defination
                var projectionDefination = Builders<KitsuneKrawlerStats>.Projection.Include(x=>x.LinksLimit)
                                                                                   .Include(x=>x.StopCrawl)
                                                                                   .Exclude(x => x._id);

                //Process
                var projectDetails = collection.Find(filter)
                                               .Project<KitsuneKrawlerStats>(projectionDefination)
                                               .FirstOrDefault();

                if (projectDetails == null)
                {
                    throw new Exception("Unable to fetch the project");
                }
                return projectDetails;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
