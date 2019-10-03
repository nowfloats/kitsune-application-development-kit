using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.Models.Krawler;
using KitsuneWebsiteCrawlerService.Constants;
using KitsuneWebsiteCrawlerService.Helpers;
using KitsuneWebsiteCrawlerService.Models;
using KitsuneWebsiteCrawlerService.Stages;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Net;

namespace KitsuneWebsiteCrawlerService
{
    class Program
    {
        private static IConfigurationRoot Configuration;
        private static AWSConfiguration awsCloudWatchConfig;
        private static AWSConfiguration awsSQSConfig;
        private static string LogGroup;
        

        static void Main(string[] args)
        {
            InitialiseConfig();
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            awsCloudWatchConfig = EnvironmentConstants.ApplicationConfiguration.AWSCloudWatchConfiguration;
            awsSQSConfig = EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration;
            LogGroup= EnvironmentConstants.ApplicationConfiguration.CloudWatchLogGroup;

            Logger.InitLogger(awsCloudWatchConfig.AWSAccessKey, awsCloudWatchConfig.AWSSecretKey, LogGroup);
            Log.Information("Process Started");
            Process();

        }

        public static void Process()
        {
            try
            {
                while (true)
                {
                    string projectId = String.Empty;
                    KitsuneKrawlerStatusCompletion stage = KitsuneKrawlerStatusCompletion.Error;
                    try
                    {
                        var amazonSqsQueueHandler = new AmazonSQSQueueHandlers<KrawlSQSModel>(EnvironmentConstants.ApplicationConfiguration.CrawlerSQSUrl);
                        //var task = amazonSqsQueueHandler.ReceiveMessageFromQueue(awsSQSConfig.AWSAccessKey, awsSQSConfig.AWSSecretKey);
                        var task = new AmazonAWSHelpers.Models.AmazonSQSMessageQueueModel<KrawlSQSModel>()
                        {
                            MessageBody = new KrawlSQSModel
                            {
                                ProjectId = "5ce4ef18abc486000121acb8",
                                ReCrawl = true
                            }
                            
                        };
                        if (task != null)
                        {
                            projectId = task.MessageBody.ProjectId;
                            if (!String.IsNullOrEmpty(projectId))
                            {
                                try
                                {
                                    #region Initiate Logger

                                    Logger.InitLogger(awsCloudWatchConfig.AWSAccessKey, awsCloudWatchConfig.AWSSecretKey, LogGroup, projectId);

                                    #endregion

                                    #region Before Process

                                    try
                                    {
                                        ServiceInformationHelper serviceInfo = new ServiceInformationHelper();
                                        Log.Information($"ProjectId : {projectId}, IP: {serviceInfo.GetInstancePrivateIpAddress()}");
                                    }
                                    catch { }

                                    Uri uri = null;
                                    //Get the Details from DB
                                    var crawlDetails = MongoHelper.GetCrawlingDetails(projectId);
                                    if (crawlDetails == null)
                                    {
                                        throw new Exception("CrawlDetails was null");
                                    }
                                    if (!Uri.TryCreate(crawlDetails.Url, UriKind.Absolute, out uri))
                                    {
                                        throw new Exception(String.Format("Error Creating Uri from Url : {0}", crawlDetails.Url));
                                    }
                                    stage = crawlDetails.Stage;

                                    #endregion

                                    #region Process

                                    var isTaskCompleted = false;
                                    Log.Information($"Started, Stage: {stage.ToString()}");
                                    try
                                    {
                                        switch (stage)
                                        {
                                            case KitsuneKrawlerStatusCompletion.Initialising:
                                                InitialiseKrawlerStageHelper.InitialiseKrawler(projectId, uri);
                                                isTaskCompleted = true;
                                                break;
                                            case KitsuneKrawlerStatusCompletion.IdentifyingAllAssetsAndDownloadingWebpage:
                                                MigrationStageHelper.AnalyseTheWebsite(projectId, uri, crawlDetails.CrawlType.Equals(KrawlType.DeepKrawl));
                                                isTaskCompleted = true;
                                                break;
                                            case KitsuneKrawlerStatusCompletion.DownloadingAllStaticAssetsToStorage:
                                                ResourcesStageHelper.DownloadTheResources(projectId, uri);
                                                isTaskCompleted = true;
                                                break;
                                            case KitsuneKrawlerStatusCompletion.UpdatingWebPagesWithNewStaticAssetUri:
                                                PlaceHolderReplacerHelper.ReplacePlaceHolder(projectId, uri);
                                                isTaskCompleted = true;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MongoHelper.UpdateCrawlErrorMessage(projectId, new KrawlError { ErrorMessage = ex.Message, Stage = stage });
                                    }
                                    Log.Information($"Completed, Stage: {stage.ToString()}");

                                    #endregion

                                    #region After Process

                                    amazonSqsQueueHandler.DeleteMessageFromQueue(task, awsSQSConfig.AWSAccessKey, awsSQSConfig.AWSSecretKey);
                                    if (isTaskCompleted)
                                    {
                                        stage += 1;
                                        MongoHelper.UpdateCrawlStatsStage(projectId, stage);

                                        //Crawling completed successfully
                                        if (stage == KitsuneKrawlerStatusCompletion.Completed)
                                        {
                                            try
                                            {
                                                APIHelper.KrawlingCompletedUpdateKitsuneProjects(projectId);
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Error(ex, $"ProjectId:{projectId}, Message:Error updating DB after completion");
                                            }
                                        }

                                        //If need furthur Process again push to sqs
                                        if (stage != KitsuneKrawlerStatusCompletion.IdentifyingExternalDomains &&
                                            stage != KitsuneKrawlerStatusCompletion.Error &&
                                            stage < KitsuneKrawlerStatusCompletion.Completed)
                                            amazonSqsQueueHandler.PushMessageToQueue(task.MessageBody, awsSQSConfig.AWSAccessKey, awsSQSConfig.AWSSecretKey);
                                        
                                        //Event- Analyse Completed (select the domains to download and start next stage)
                                        if (stage == KitsuneKrawlerStatusCompletion.IdentifyingExternalDomains)
                                        {
                                            if(task.MessageBody.ReCrawl)
                                            {
                                                MongoHelper.UpdateCrawlStatsStage(projectId, stage+1);
                                                amazonSqsQueueHandler.PushMessageToQueue(task.MessageBody, awsSQSConfig.AWSAccessKey, awsSQSConfig.AWSSecretKey);
                                            }
                                            else
                                                APIHelper.RegisterAnalyseCompleteEvent(projectId);
                                        }
                                            
                                    }
                                    else
                                    {
                                        Log.Error($"ProjectId:{projectId}, Message:Error as isTaskCompleted was false for projectId: {projectId}");
                                    }

                                    #endregion
                                }
                                catch(Exception ex)
                                {
                                    //Handle if any exception rises
                                    Log.Error($"ProjectId:{projectId}, Message:Error while Processing the project with Error : {ex.Message}");
                                    MongoHelper.UpdateCrawlErrorMessage(projectId, new KrawlError { ErrorMessage = ex.Message, Stage = stage });
                                    amazonSqsQueueHandler.DeleteMessageFromQueue(task, awsSQSConfig.AWSAccessKey, awsSQSConfig.AWSSecretKey);
                                }
                                Logger.InitLogger(awsCloudWatchConfig.AWSAccessKey, awsCloudWatchConfig.AWSSecretKey, LogGroup);
                            }
                            else
                            {
                                Log.Error($"ProjectId:{projectId}, Message:Error while processing the Service as the projectId was null");
                                amazonSqsQueueHandler.DeleteMessageFromQueue(task,awsSQSConfig.AWSAccessKey, awsSQSConfig.AWSSecretKey);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        //Error picking message from sqs
                        //Error Deleting message from sqs
                        Log.Error(ex,$"ProjectId:{projectId}, Message:Error while processing the Service after getting the value");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Message:Error while polling from SQS, Exception : {ex.ToString()}");
            }
        }

        public static void InitialiseConfig()
        {
#if !DEBUG
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Console.WriteLine("Debug");
#else
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.production.json", optional: false, reloadOnChange: true);
            Console.WriteLine("Production");
#endif

            Configuration = builder.Build();
            EnvironmentConstants.ApplicationConfiguration = Configuration.Get<Appsettings>();
        }
    }
}
