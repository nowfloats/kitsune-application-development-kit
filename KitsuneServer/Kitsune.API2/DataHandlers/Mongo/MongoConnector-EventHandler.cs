
using Kitsune.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Kitsune.Models.PublishModels;
using Kitsune.Models.BuildAndRunModels;
using Kitsune.API2.EnvConstants;
using Kitsune.Models.ReportGeneratorModels;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.Utils;
using Kitsune.Models.WebsiteModels;
using Kitsune.Models;

namespace Kitsune.API2.DataHandlers.Mongo
{
    public static partial class MongoConnector
    {
        public static void OptimisationCompleted(string projectId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var buildStatsCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);
              

                #region Send Migration Report

                var amazonSqsHanlder = new AmazonSQSQueueHandlers<TranspilerFunctionSQSModel>(AmazonAWSConstants.TranspilerSQSUrl);
                var project = GetProjectDetails(projectId);
                TranspilerFunctionSQSModel sqsModel = new TranspilerFunctionSQSModel()
                {
                    ProjectId = projectId,
                    UserEmail = project.UserEmail
                };
                amazonSqsHanlder.PushMessageToQueue(sqsModel, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey);

                #endregion
               
            }
            catch (Exception ex)
            {
                //LOG
            }
        }
        public static void TranspilationFailed(string projectId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                //Update the project status to error in case of transpilation failed
                UpdateKitsuneProjectStatus(new UpdateKitsuneProjectStatusRequestModel
                {
                    ProjectId = projectId,
                    ProjectStatus = ProjectStatus.BUILDINGERROR
                });
            }
            catch { }
        }

        public static void TranspilationCompleted(string projectId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                //Update the proejct status to IDLE on transpilation success
                UpdateKitsuneProjectStatus(new UpdateKitsuneProjectStatusRequestModel
                {
                    ProjectId = projectId,
                    ProjectStatus = ProjectStatus.IDLE
                });

                #region Send Migration Report

                var amazonSqsHanlder = new AmazonSQSQueueHandlers<ReportGeneratorSQSModel>(AmazonAWSConstants.MigrationReportSQSUrl);
                ReportGeneratorSQSModel sqsModel = new ReportGeneratorSQSModel()
                {
                    ProjectId = projectId
                };
                amazonSqsHanlder.PushMessageToQueue(sqsModel, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey);

                #endregion

                //Call screenshot API
                CreateAndUpdateDemoScreenShot(projectId);

                //Create Routing Tree
                APIHelper.CreateProjectRoute(projectId, true);

                try
                {
                    ProjectConfigHelper projectConfigHelper = new ProjectConfigHelper(projectId);
                    var isPublish = projectConfigHelper.IsAutoPublishEnabled();
                    //Auto Publish
                    if (isPublish)
                    {

                        //Call publish API
                    }
                }
                catch { }

            }
            catch (Exception ex)
            {
                //LOG
            }
        }


        public static void CrawlAnalyserPhaseCompleted(string projectId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var kitsuneProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                KitsuneProject kitsuneProjectDetails = kitsuneProjectCollection.Find(x => x.ProjectId.Equals(projectId)).Limit(1).FirstOrDefault();

                switch (kitsuneProjectDetails.ProjectType)
                {
                    case ProjectType.WORDPRESS:
                        SaveSelectedDomainRequestModel SaveAndStartSecondStageModel = new SaveSelectedDomainRequestModel()
                        {
                            ProjectId = projectId
                        };
                        SaveSelectedDomain(SaveAndStartSecondStageModel);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                //LOG
            }
        }

        public static void CreateAndUpdateDemoScreenShot(string projectId)
        {
            //Get Project Details 
            //Create absolute Url of demo
            //Call screenshot API
            //Update KitsuneProject Collection with new Screenshot

            if (string.IsNullOrEmpty(projectId))
                throw new ArgumentNullException(nameof(projectId));

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                #region INITIALISE

                var kitsuneProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var defaultProjectCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

                #endregion

                #region CREATE DEMO URI

                KitsuneWebsiteCollection websiteCollection = defaultProjectCollection.Find(x => x.ProjectId.Equals(projectId)).FirstOrDefault();
                Uri uri = Helpers.GetDemoUri(websiteCollection._id);

                #endregion

                #region CALL SCREENSHOT API

                var screenShotUrl = APIHelper.TakeScreenShotForDemoWebsite(projectId, uri);

                #endregion

                #region UPDATE SCREENSHOT URL IN DB

                var updateDefination = Builders<KitsuneProject>.Update.Set(x => x.ScreenShotUrl, screenShotUrl);
                kitsuneProjectCollection.UpdateOne(x => x.ProjectId.Equals(projectId), updateDefination);

                #endregion
            }
            catch (Exception ex)
            {
                //LOG: Error with ProjectId
            }
        }

        //public static void CreateLiveWebsiteScreenShot(List<string> websiteId)
        //{
        //    //Get Project Details 
        //    //Create absolute Url of demo
        //    //Call screenshot API
        //    //Update KitsuneProject Collection with new Screenshot

        //    if (websiteId==null)
        //        throw new ArgumentNullException(nameof(websiteId));

        //    try
        //    {
        //        if (_kitsuneServer == null)
        //            InitializeConnection();

        //        #region INITIALISE

        //        var kitsuneProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
        //        var defaultProjectCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

        //        #endregion

        //        #region CREATE DEMO URI

        //        KitsuneWebsiteCollection websiteCollection = defaultProjectCollection.Find(x => x.ProjectId.Equals(projectId)).FirstOrDefault();
        //        Uri uri = Helpers.GetDemoUri(websiteCollection._id);

        //        #endregion

        //        #region CALL SCREENSHOT API

        //        var screenShotUrl = APIHelper.TakeScreenShotForDemoWebsite(projectId, uri);

        //        #endregion

        //        #region UPDATE SCREENSHOT URL IN DB

        //        var updateDefination = Builders<KitsuneProject>.Update.Set(x => x.ScreenShotUrl, screenShotUrl);
        //        kitsuneProjectCollection.UpdateOne(x => x.ProjectId.Equals(projectId), updateDefination);

        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        //LOG: Error with ProjectId
        //    }
        //}

    }
}
