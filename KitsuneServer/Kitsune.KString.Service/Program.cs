using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Kitsune.KString.Service
{
    class Program
    {
        static bool isDebug = true;
#if DEBUG
        static readonly string kStringQueueUrl = "https://sqs.ap-southeast-2.amazonaws.com/593693325525/KitsuneKStringQueue";
#else
        static readonly string kStringQueueUrl = "https://sqs.ap-southeast-2.amazonaws.com/593693325525/KitsuneKStringQueue";
#endif
        static void Main(string[] args)
        {
           Console.WriteLine("Kitsune Compiler Service Started");

            if (isDebug)
                Console.WriteLine("KString service started");
            while (true)
            {
                try
                {
                    var amazonCompilerSqsQueueHandler = new AmazonSQSQueueHandlers<KStringQueueModel>(kStringQueueUrl);


                    var task = amazonCompilerSqsQueueHandler.ReceiveMessageFromQueue();
                    try
                    {
                        if (task != null && task.MessageBody != null)
                        {
                            var keywordList = KStringHelper.ExtractKeyword(task.MessageBody.KString);
                            if (keywordList != null && keywordList.Any())
                            {
                                var result = KStringHelper.UpdateKeywordsToKitsuneDB(task.MessageBody.SchemaName, task.MessageBody.UserId, task.MessageBody.KID, task.MessageBody.ReferenceId, task.MessageBody.KString, keywordList);
                                if (!string.IsNullOrEmpty(result))
                                {
                                    Console.WriteLine($"Keyword Extracted : {JsonConvert.SerializeObject(task.MessageBody)}");
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(String.Format("Error during kstring queue processing, ErrorMessage : {0}, StackTrace : {1}", ex.Message, ex.StackTrace));
                    }
                    finally
                    {
                        if(task != null)
                        {
                            amazonCompilerSqsQueueHandler.DeleteMessageFromQueue(task);
                            Console.WriteLine($"Message removed : {task.MessageBody.SchemaName}");
                        }
                      
                    }
                  
                }
                catch (Exception ex)
                {
                  //  EventLogger.LogTrace(String.Format("Error during compilation, ErrorMessage : {0}, StackTrace : {1}", ex.Message, ex.StackTrace), null, null);
                    Console.WriteLine(String.Format("Error during kstring processing, ErrorMessage : {0}, StackTrace : {1}", ex.Message, ex.StackTrace));
                }
                finally
                {
                    
                }
            }
        }
    }
}
