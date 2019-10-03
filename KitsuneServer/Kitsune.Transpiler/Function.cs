using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using AmazonAWSHelpers.Models;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Models;
using Kitsune.Models.BuildAndRunModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Kitsune.Transpiler
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
        public void FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                try
                {
                    ProcessMessageAsync(message, context);
                }
                catch (Exception ex)
                {
                    context.Logger.LogLine($"Exception : {ex.Message}");
                }
                finally
                {
                }

            }
        }

        private void ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            var api = new APIHelper();
            try
            {
                var log = context.Logger;
                System.Diagnostics.Stopwatch transpileStopWatch = new System.Diagnostics.Stopwatch();
                System.Diagnostics.Stopwatch completeProcessStopWatch = new System.Diagnostics.Stopwatch();
                completeProcessStopWatch.Start();

                //Default threshold if not provided is set to 250 sec
                var functionThreshold = System.Environment.GetEnvironmentVariable("FunctionThreshold") ?? "250000";


                if (message != null && !string.IsNullOrEmpty(message.Body))
                {
                    var request = JsonConvert.DeserializeObject<JObject>(message.Body);
                    if (request == null)
                    {
                        log.LogLine("Error : Request body can not be empty");
                        return;
                    }


                    var projectId = request["ProjectId"] != null ? (string)request["ProjectId"] : null;
                    var userEmail = request["UserEmail"] != null ? (string)request["UserEmail"] : null;
                    var resourceProcessed = request["ResourcesProcesed"] != null ? (int)request["ResourcesProcesed"] : 0;

                    if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(userEmail))
                    {
                        log.LogLine($"Invalid request paramater : {message.Body}");
                        return;
                    }
                    try
                    {
                        log.LogLine($"Transpiling the project : {projectId}");

                        log.LogLine($"Getting project details for project : {projectId}");
                        var project = api.GetProjectDetailsApi(userEmail, projectId);
                        var totalFiles = 0;
                        var completedFiles = resourceProcessed;

                        if (project != null && project.Resources != null && project.Resources.Any(x => !x.IsStatic))
                        {
                            totalFiles = project.Resources.Count(x => !x.IsStatic && x.ResourceType == Models.Project.ResourceType.LINK);
                            KitsunePage page = null;
                            log.LogLine($"Transpiling '{project.Resources.Count(x => !x.IsStatic) - resourceProcessed}' resources ");
                            string compiledFile = string.Empty;
                            foreach (var resource in project.Resources.Where(x => !x.IsStatic && x.ResourceType == Models.Project.ResourceType.LINK).OrderByDescending(x => x.UpdatedOn).Skip(resourceProcessed))
                            {
                                transpileStopWatch.Reset();
                                transpileStopWatch.Start();

                                try
                                {
                                    compiledFile = api.GetFileFromS3(new GetFileFromS3RequestModel
                                    {
                                        BucketName = project.BucketNames.demo,
                                        Compiled = true,
                                        ProjectId = project.ProjectId,
                                        SourcePath = resource.SourcePath
                                    });
                                }
                                catch (Exception ex)
                                {
                                    log.LogLine($"Exception : {ex.Message}, StackTrace : {ex.StackTrace}");
                                    return;
                                }
                                if (!string.IsNullOrEmpty(compiledFile))
                                {
                                    try
                                    {
                                        NUglify.Html.HtmlSettings settings = new NUglify.Html.HtmlSettings() { DecodeEntityCharacters = false, RemoveOptionalTags = false, ShortBooleanAttribute = false };
                                        //settings.TagsWithNonCollapsableWhitespaces.Add("p", false);
                                        NUglify.UglifyResult minify = NUglify.Uglify.Html(compiledFile, settings);
                                        if (!minify.HasErrors)
                                            page = new NodeProcessor().Process(minify.Code, resource.SourcePath, resource.KObject, resource.CustomVariables, resource.Offset);
                                        else
                                        {
                                            string errorList = "";
                                            foreach (NUglify.UglifyError error in minify.Errors)
                                            {
                                                errorList += $"[{error.StartLine},{error.StartColumn}:{error.EndLine},{error.EndColumn}]{error.ErrorCode}:{error.ErrorNumber}:{error.Message};";
                                            }
                                            log.LogLine($"Error : File minification for '{resource.SourcePath}', erorrList {errorList}");
                                            //Fallback 
                                            page = new NodeProcessor().Process(minify.Code, resource.SourcePath, resource.KObject, resource.CustomVariables, resource.Offset);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.LogLine($"Exception : File minification for '{resource.SourcePath}', Message : {ex.Message}, StackTrace : {ex.StackTrace}");
                                        //Fallback 
                                        page = new NodeProcessor().Process(compiledFile, resource.SourcePath, resource.KObject, resource.CustomVariables, resource.Offset);
                                    }
                                }



                                if (page != null)
                                {
                                    var jsonSerializeSettings = new JsonSerializerSettings();
                                    jsonSerializeSettings.TypeNameHandling = TypeNameHandling.Auto;
                                    string output = JsonConvert.SerializeObject(page, Formatting.None, jsonSerializeSettings);

                                    var resourceContent = new SaveFileContentToS3RequestModel
                                    {
                                        ProjectId = project.ProjectId,
                                        BucketName = project.BucketNames.demo,
                                        SourcePath = $"{resource.SourcePath}.kc",
                                        FileContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(output)),
                                        base64 = true
                                    };
                                    var compiledStringResult = api.SaveFileContentToS3(resourceContent);
                                    if (compiledStringResult == null) { }

                                    //Increase the completed files count
                                    completedFiles++;
                                }
                                else
                                {
                                    //Update project and build error
                                    UpdateBuildError(project.ProjectId, project.UserEmail, resource.SourcePath, context);
                                    break;
                                }

                                log.LogLine($"{completedFiles}/{totalFiles} '{resource.SourcePath}' {transpileStopWatch.ElapsedMilliseconds.ToString()}");

                                //Check if the runtime is exceeding the timelimit then push to queue with completed task.

                                if (completedFiles < totalFiles && completeProcessStopWatch.ElapsedMilliseconds > int.Parse(functionThreshold))
                                {
                                    var msgBody = new
                                    {
                                        ProjectId = projectId,
                                        UserEmail = userEmail,
                                        ResourcesProcesed = completedFiles
                                    };
                                    var response = AWSSQSHelper.PushMessageToQueue(msgBody, context, RegionEndpoint.GetBySystemName(message.AwsRegion));
                                    if (!string.IsNullOrEmpty(response))
                                    {
                                        log.LogLine($"Pushed the message to queue : {JsonConvert.SerializeObject(msgBody)}");
                                    }
                                    //Exit the loop;
                                    break;
                                }
                            }

                            
                        }
                        else
                        {
                            totalFiles = completedFiles = 0;
                            log.LogLine("No resource found to transpile");
                        }

                        if (completedFiles >= totalFiles)
                        {
                            log.LogLine($"Transpilation done successfully for project '{project.ProjectId}' in {completeProcessStopWatch.ElapsedMilliseconds} ms");
                            log.LogLine("Updating Build Status");
                            var buildStatusUpdateResult = api.UpdateBuildStatus(project.UserEmail, new CreateOrUpdateKitsuneStatusRequestModel
                            {
                                ProjectId = project.ProjectId,
                                Stage = BuildStatus.Completed,
                            });
                            log.LogLine("Success : Updating Build Status");

                            log.LogLine("Calling trigger completed event");

                            if (api.TriggerCompletedEvent(project.ProjectId).IsError)
                            {
                                log.LogLine($"Error updating transpilation complete status for project '{project.ProjectId}'");
                            }
                            log.LogLine("Success : Calling trigger completed event");

                        }
                    }
                    catch(Exception ex)
                    {
                        UpdateBuildError(projectId, userEmail, null, context);

                        if (api.TriggerFailedEvent(projectId).IsError)
                        {
                            log.LogLine($"Error updating transpilation failed status for project '{projectId}'");
                        }
                        throw;
                    }
                    // TODO: Do interesting work based on the new message
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                AWSSQSHelper.DeleteMessageFromQueue(message, context);
            }

        }
        private void UpdateBuildError(string projectId, string userEmail, string sourcePath, ILambdaContext context)
        {
            var api = new APIHelper();

            //Update the build status for error
            var buildStatusUpdateResult = api.UpdateBuildStatus(userEmail, new CreateOrUpdateKitsuneStatusRequestModel
            {
                ProjectId = projectId,
                Stage = BuildStatus.Error,
                Error = new List<Models.PublishModels.BuildError>
                                            {
                                                new Models.PublishModels.BuildError
                                                {
                                                    SourcePath = sourcePath,
                                                    SourceMethod = "Transpiler",
                                                    Message = !string.IsNullOrEmpty(sourcePath) ? $"Unable to transpile the project resource '{sourcePath}'" : $"Unable to transpile the project : {projectId}"
                                                }
                                            }

            });
            //Update the project status to error fromthe trigger failed event
            if (api.TriggerFailedEvent(projectId).IsError)
            {
                context.Logger.LogLine($"Error updating transpilation failed status for project '{projectId}'");
            }
        }
    }
}