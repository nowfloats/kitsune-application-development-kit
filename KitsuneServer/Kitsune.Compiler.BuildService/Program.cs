using Kitsune.Models;
using System;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.Models.PublishModels;
using System.Collections.Generic;
using Kitsune.Compiler.BuildService.Helpers;
using Amazon;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using System.Net;

namespace Kitsune.Compiler.BuildService
{
    public class Program
    {
        static bool isDebug = true;
        public static IConfiguration ServiceConfiguration { get; set; }
        static readonly int _currentCompilerVersion = 1;
        static void Main(string[] args)
        {
#if DEBUG
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", true, true)
             .AddEnvironmentVariables();
#else
                          var builder = new ConfigurationBuilder()
                         .AddJsonFile($"appsettings.Production.json", true, true)
                         .AddEnvironmentVariables();
#endif
            ServiceConfiguration = builder.Build();
            string myIP = null;
            try
            {
                string hostName = Dns.GetHostName(); // Retrive the Name of HOST  
                                                     // Get the IP  
                myIP = Dns.GetHostEntry(hostName)?.AddressList?.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.FirstOrDefault()?.ToString();
            }
            catch
            {

            }
            Logger.InitLogger(ServiceConfiguration.GetSection("AWSAccessKey").Value,
                ServiceConfiguration.GetSection("AWSSecretKey").Value,
                ServiceConfiguration.GetSection("CloudWatchLogGroup").Value,
                myIP);

            Console.WriteLine("Started");
            Log.Information("Compiler service started");
            Console.WriteLine(ServiceConfiguration.GetSection("KitsuneCompilerSQSUrl").Value);
            Log.Information(ServiceConfiguration.GetSection("KitsuneCompilerSQSUrl").Value);



            var amazonCompilerSqsQueueHandler = new AmazonSQSQueueHandlers<CompilerServiceSQSModel>(ServiceConfiguration.GetSection("KitsuneCompilerSQSUrl").Value);
            AmazonAWSHelpers.Models.AmazonSQSMessageQueueModel<CompilerServiceSQSModel> task = null;
            string projectId = string.Empty;
            int buildVersion = 0;
            string user = string.Empty;
            var errors = new List<BuildError>();
            List<CompileResult> buildStatus = null;
            while (true)
            {
                try
                {
                    Console.WriteLine($"Polling from the queue : {DateTime.UtcNow.ToString()}");
                    Log.Information($"Polling from the queue : {DateTime.UtcNow.ToString()}");

                    task = amazonCompilerSqsQueueHandler.ReceiveMessageFromQueue(ServiceConfiguration.GetSection("AWSAccessKey").Value, ServiceConfiguration.GetSection("AWSSecretKey").Value,
                        RegionEndpoint.GetBySystemName(ServiceConfiguration.GetSection("AWSRegion").Value));

                    if (task != null && task.MessageBody != null)
                    {
                        Console.Clear();
                        projectId = task.MessageBody.ProjectId;
                        buildVersion = task.MessageBody.BuildVersion;
                        user = task.MessageBody.UserEmail;


                        //Update the kitsune project status to building 
                        if (APIHelpers.UpdateProjectStatus(user, projectId, buildVersion.ToString()))
                        {
                            Console.WriteLine(String.Format("Compilation processing started for project '{0}' with version {1}", projectId, buildVersion), projectId, buildVersion.ToString());
                            Log.Information(String.Format("Compilation processing started for project '{0}' with version {1}", projectId, buildVersion), projectId, buildVersion.ToString());

                            buildStatus = new BuildAndRunHelper().BuildProject(user, projectId, bool.Parse(ServiceConfiguration.GetSection("_isDev").Value), _currentCompilerVersion);
                            if (buildStatus != null && buildStatus.Any())
                            {
                                errors = new List<BuildError>();
                                foreach (var error in buildStatus)
                                {
                                    if (!error.Success)
                                        errors.AddRange(error.ErrorMessages.Select(x => new BuildError
                                        {
                                            Column = x.LinePosition,
                                            Line = x.LineNumber,
                                            Message = x.Message,
                                            ErrorStackTrace = x.Message,
                                            SourceMethod = "KitsuneCompiler",
                                            SourcePath = error.PageName
                                        }));
                                }
                                APIHelpers.UpdateProjectErrorStatus(user, projectId, buildVersion, errors);
                                Console.WriteLine(String.Format("Compilation failed for project '{0}' with version {1}, Errors", projectId, buildVersion));
                                Log.Error(String.Format("Compilation failed for project '{0}' with version {1}, Errors", projectId, buildVersion));
                                foreach(var err in errors)
                                {
                                    Console.WriteLine(JsonConvert.SerializeObject(err));
                                    Log.Error(JsonConvert.SerializeObject(err));
                                }
                                buildStatus = null;
                            }
                            else
                            {
                                //UPDATE the project status
                                if (APIHelpers.UpdateBuildStatus(user, projectId, buildVersion))
                                {
                                    Console.WriteLine(string.Format("Compilation done successful"), projectId, buildVersion.ToString());
                                    Log.Information(string.Format("Compilation done successful"), projectId, buildVersion.ToString());
                                }
                            }
                        }
                        amazonCompilerSqsQueueHandler.DeleteMessageFromQueue(task, ServiceConfiguration.GetSection("AWSAccessKey").Value, ServiceConfiguration.GetSection("AWSSecretKey").Value, RegionEndpoint.GetBySystemName(ServiceConfiguration.GetSection("AWSRegion").Value));
                        Console.WriteLine(string.Format("Message removed : {0}", task?.MessageBody?.ProjectId));
                        Log.Information(string.Format("Message removed : {0}", task?.MessageBody?.ProjectId));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("Error during compilation, ErrorMessage : {0}, StackTrace : {1}", ex.Message, ex.StackTrace));
                    Log.Error(String.Format("Error during compilation, ErrorMessage : {0}, StackTrace : {1}", ex.Message, ex.StackTrace));
                }
            }
        }
    }

}
