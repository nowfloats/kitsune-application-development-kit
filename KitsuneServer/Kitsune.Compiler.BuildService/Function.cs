using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using AmazonAWSHelpers.Models;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.Models;
using Kitsune.Models.PublishModels;
using Newtonsoft.Json;
using Serilog;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Kitsune.Compiler.BuildService
{
    public class Function
    {
        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {

        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, context);
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            context.Logger.LogLine($"Processed message {message.Body}");
            context.Logger.LogLine(Environment.GetEnvironmentVariable("SQS_AWS_REGION"));
            context.Logger.LogLine(Directory.GetCurrentDirectory() + "/Files");

            try
            {
                var amazonCompilerSqsQueueHandler = new AmazonSQSQueueHandlers<CompilerServiceSQSModel>(Environment.GetEnvironmentVariable("SQS_URL"));

                var sqsModel = JsonConvert.DeserializeObject<CompilerServiceSQSModel>(message.Body);

                var errors = new List<BuildError>();
                List<CompileResult> buildStatus = null;

                var projectId = sqsModel.ProjectId;
                var buildVersion = sqsModel.BuildVersion;
                var user = sqsModel.UserEmail;


                //Update the kitsune project status to building 
                if (APIHelpers.UpdateProjectStatus(user, projectId, buildVersion.ToString()))
                {
                    context.Logger.LogLine(String.Format("Compilation processing started for project '{0}' with version {1}", projectId, buildVersion));

                    buildStatus = new BuildAndRunHelper().BuildProject(user, projectId, bool.Parse(Environment.GetEnvironmentVariable("IS_DEVELOPMENT_VERSION")), 1);
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
                        foreach (var err in errors)
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
                
                var response = amazonCompilerSqsQueueHandler.DeleteMessageFromQueue(new AmazonSQSMessageQueueModel<CompilerServiceSQSModel>
                {
                    MessageBody = JsonConvert.DeserializeObject<CompilerServiceSQSModel>(message.Body),
                    MessageId = message.MessageId,
                    ReceiptHandle = message.ReceiptHandle,
                    MessageAttributes = message.Attributes
                });

                context.Logger.LogLine(response);
            }
            catch { }

            await Task.CompletedTask;
        }
    }
}
