using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonAWSHelpers.Models
{
    public class AmazonSQSMessageQueueModel<T>
    {
        public string MessageId, ReceiptHandle;
        public Dictionary<string, string> MessageAttributes;
        public T MessageBody;
    }
}
