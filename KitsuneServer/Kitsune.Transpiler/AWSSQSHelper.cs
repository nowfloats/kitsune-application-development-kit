using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.Transpiler
{
    internal class AWSSQSHelper
    {
        internal static void DeleteMessageFromQueue(SQSEvent.SQSMessage message, ILambdaContext context)
        {
            try
            {
                if (message.ReceiptHandle != null)
                {
                    context.Logger.LogLine($"Deleting message : {message.ReceiptHandle} from queue");
                    IAmazonSQS sqs = new AmazonSQSClient(RegionEndpoint.GetBySystemName(message.AwsRegion));

                    DeleteMessageRequest deleteRequest = new DeleteMessageRequest
                    {
                        QueueUrl = TranspilerConstants.QueueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };
                    var response = sqs.DeleteMessageAsync(deleteRequest).Result;

                    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        context.Logger.LogLine($"Deleted message : {message.ReceiptHandle} from queue");
                        return;
                    }
                    context.Logger.LogLine($"Couldn't delete the message : {message.Body} from queue");

                }
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Delete queue Exception : {ex.Message}");

            }
        }

        public static string PushMessageToQueue(object model, ILambdaContext context, RegionEndpoint regionEndpoint)
        {
            try
            {
                if (model != null)
                {
                    IAmazonSQS sqs = new AmazonSQSClient(regionEndpoint ?? RegionEndpoint.APSouth1);

                    SendMessageRequest sendMessageRequest = new SendMessageRequest();

                    var messageStringBody = JsonConvert.SerializeObject(model);

                    sendMessageRequest.MessageBody = messageStringBody;
                    sendMessageRequest.QueueUrl = TranspilerConstants.QueueUrl;
                    var response = sqs.SendMessageAsync(sendMessageRequest).Result;
                    return response.HttpStatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogLine(ex.ToString());
            }
            return null;
        }

    }
}
