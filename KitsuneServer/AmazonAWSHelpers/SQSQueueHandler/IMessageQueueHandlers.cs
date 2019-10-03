using Amazon;
using AmazonAWSHelpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonAWSHelpers.SQSQueueHandler
{
    public interface IMessageQueueHandlers<T>
    {
        AmazonSQSMessageQueueModel<T> ReceiveMessageFromQueue();

        AmazonSQSMessageQueueModel<T> ReceiveMessageFromQueue(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint regionEndpoint = null);

        string DeleteMessageFromQueue(AmazonSQSMessageQueueModel<T> model);

        string DeleteMessageFromQueue(AmazonSQSMessageQueueModel<T> model, string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint regionEndpoint = null);

        string PushMessageToQueue(T model, RegionEndpoint regionEndpoint = null);

        string PushMessageToQueue(T model, string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint regionEndpoint = null);
    }
}
