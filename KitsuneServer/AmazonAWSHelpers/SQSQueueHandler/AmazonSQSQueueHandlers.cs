using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using AmazonAWSHelpers.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AmazonAWSHelpers.SQSQueueHandler
{
    public class AmazonSQSQueueHandlers<T> : IMessageQueueHandlers<T>
    {
        private string QueueUrl = null;

        public AmazonSQSQueueHandlers(string queueUrl)
        {
            QueueUrl = queueUrl;
        }

        public AmazonSQSMessageQueueModel<T> ReceiveMessageFromQueue()
        {
            try
            {
                IAmazonSQS sqs = new AmazonSQSClient(RegionEndpoint.APSouth1);

                ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = QueueUrl,
                    MaxNumberOfMessages = 1,
                    WaitTimeSeconds = 20
                };

                ReceiveMessageResponse receiveMessageResponse = sqs.ReceiveMessageAsync(receiveMessageRequest).Result;


                if (receiveMessageResponse != null && receiveMessageResponse.Messages != null)
                {
                    Message message = receiveMessageResponse.Messages.FirstOrDefault();
                    if (message != null && message.Body != null)
                    {
                        var messageModel = new AmazonSQSMessageQueueModel<T>
                        {
                            MessageBody = JsonConvert.DeserializeObject<T>(message.Body),
                            MessageId = message.MessageId,
                            ReceiptHandle = message.ReceiptHandle,
                            MessageAttributes = message.Attributes
                        };
                        return messageModel;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public AmazonSQSMessageQueueModel<T> ReceiveMessageFromQueue(string awsAccessKeyId,string awsSecretAccessKey, RegionEndpoint regionEndpoint = null)
        {
            try
            {
                IAmazonSQS sqs = new AmazonSQSClient(awsAccessKeyId, awsSecretAccessKey, regionEndpoint ?? RegionEndpoint.APSouth1);

                ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = QueueUrl,
                    MaxNumberOfMessages = 1,
                    WaitTimeSeconds = 20
                };

                ReceiveMessageResponse receiveMessageResponse = sqs.ReceiveMessageAsync(receiveMessageRequest).Result;


                if (receiveMessageResponse != null && receiveMessageResponse.Messages != null)
                {
                    Message message = receiveMessageResponse.Messages.FirstOrDefault();
                    if (message != null && message.Body != null)
                    {
                        var messageModel = new AmazonSQSMessageQueueModel<T>
                        {
                            MessageBody = JsonConvert.DeserializeObject<T>(message.Body),
                            MessageId = message.MessageId,
                            ReceiptHandle = message.ReceiptHandle,
                            MessageAttributes = message.Attributes
                        };
                        return messageModel;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public string DeleteMessageFromQueue(AmazonSQSMessageQueueModel<T> model)
        {
            try
            {
                if (model.ReceiptHandle != null)
                {
                    IAmazonSQS sqs = new AmazonSQSClient(RegionEndpoint.APSouth1);

                    DeleteMessageRequest deleteRequest = new DeleteMessageRequest
                    {
                        QueueUrl = QueueUrl,
                        ReceiptHandle = model.ReceiptHandle
                    };
                    var response = sqs.DeleteMessageAsync(deleteRequest).Result;
                    return response.HttpStatusCode.ToString();
                }
            }
            catch
            {
            }
            return null;
        }

        public string DeleteMessageFromQueue(AmazonSQSMessageQueueModel<T> model, string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint regionEndpoint = null)
        {
            try
            {
                if (model.ReceiptHandle != null)
                {
                    IAmazonSQS sqs = new AmazonSQSClient(awsAccessKeyId,awsSecretAccessKey,regionEndpoint ?? RegionEndpoint.APSouth1);

                    DeleteMessageRequest deleteRequest = new DeleteMessageRequest
                    {
                        QueueUrl = QueueUrl,
                        ReceiptHandle = model.ReceiptHandle
                    };
                    var response = sqs.DeleteMessageAsync(deleteRequest).Result;
                    return response.HttpStatusCode.ToString();
                }
            }
            catch
            {
            }
            return null;
        }

        public string PushMessageToQueue(T model, RegionEndpoint regionEndpoint = null)
        {
            try
            {
                if (model != null)
                {
                    IAmazonSQS sqs = new AmazonSQSClient(regionEndpoint ?? RegionEndpoint.APSouth1);

                    SendMessageRequest sendMessageRequest = new SendMessageRequest();

                    var emailString = JsonConvert.SerializeObject(model);

                    sendMessageRequest.MessageBody = emailString;
                    sendMessageRequest.QueueUrl = QueueUrl;
                    var response = sqs.SendMessageAsync(sendMessageRequest).Result;
                    return response.HttpStatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
        public string PushMessageToQueue(T model, string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint regionEndpoint = null)
        {
            try
            {
                if (model != null)
                {
                    IAmazonSQS sqs = new AmazonSQSClient(awsAccessKeyId, awsSecretAccessKey, regionEndpoint ?? RegionEndpoint.APSouth1);

                    SendMessageRequest sendMessageRequest = new SendMessageRequest();

                    var emailString = JsonConvert.SerializeObject(model);

                    sendMessageRequest.MessageBody = emailString;
                    sendMessageRequest.QueueUrl = QueueUrl;
                    var response = sqs.SendMessageAsync(sendMessageRequest).Result;
                    return response.HttpStatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public async Task<HttpStatusCode> PushMessageToQueueAsync(T model, string awsAccessKeyId, string awsSecretAccessKey)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (String.IsNullOrEmpty(awsAccessKeyId))
                throw new ArgumentNullException(nameof(awsAccessKeyId));
            if (String.IsNullOrEmpty(awsSecretAccessKey))
                throw new ArgumentNullException(nameof(awsSecretAccessKey));
            try
            {
                IAmazonSQS sqs = new AmazonSQSClient(awsAccessKeyId, awsSecretAccessKey, RegionEndpoint.APSouth1);
                String emailString = JsonConvert.SerializeObject(model);

                SendMessageRequest sendMessageRequest = new SendMessageRequest
                {
                    MessageBody = emailString,
                    QueueUrl = QueueUrl
                };
                SendMessageResponse response =await sqs.SendMessageAsync(sendMessageRequest);
                return response.HttpStatusCode;
            }
            catch (Exception ex)
            {
                throw new Exception("Error Pushing to SQS", ex);
            }
        }
    }
}
