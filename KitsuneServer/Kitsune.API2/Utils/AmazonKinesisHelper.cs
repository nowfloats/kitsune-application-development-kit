using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Kitsune.API2.EnvConstants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
	public class AmazonKinesisHelper
	{
		private static AmazonKinesisClient kinesisClient;

		static AmazonKinesisHelper()
		{
			InitiateAWSClient();
		}

		internal static void InitiateAWSClient()
		{
			try
			{
				if (kinesisClient == null)
				{
					//For dev the region is SouthEast2 and for prod SouthEast1
					//#if DEBUG
					//                    kinesisClient = new AmazonKinesisClient(EnvironmentConstants.ApplicationConfiguration.AWSKinesisConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSKinesisConfiguration.AWS_SecretKey, Amazon.RegionEndpoint.APSoutheast2);

					//#else
					kinesisClient = new AmazonKinesisClient(EnvironmentConstants.ApplicationConfiguration.AWSKinesisConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSKinesisConfiguration.AWS_SecretKey, Amazon.RegionEndpoint.APSoutheast1);

					//#endif
				}
			}
			catch (Exception ex)
			{
				//EventLogger.Write(ex, "FlowLayoutManager exception occured while processing the request for InitiateAWSClient");
			}
		}
		internal static string LogRecord<T>(T data, string streamName)
		{
			try
			{
				if (data != null && !String.IsNullOrEmpty(streamName))
				{
					if (kinesisClient == null)
					{
						InitiateAWSClient();
					}

					byte[] dataAsBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

					string sequenceNumber = string.Empty;
					using (MemoryStream memoryStream = new MemoryStream(dataAsBytes))
					{
						try
						{
							PutRecordRequest requestRecord = new PutRecordRequest();
							requestRecord.StreamName = streamName;
							requestRecord.PartitionKey = "KitsuneAPI";
							requestRecord.Data = memoryStream;

							PutRecordResponse responseRecord = kinesisClient.PutRecordAsync(requestRecord).Result;
							sequenceNumber = responseRecord.SequenceNumber;
							return sequenceNumber;
						}
						catch (Exception ex)
						{
							//EventLogger.Write(ex, "FlowLayoutManager Exception occured while processing the request LogFPRequestDetailsIntoKinesis");
							throw ex;
						}
					}
				}
			}
			catch (Exception ex)
			{
				//Log exception;

				//EventLogger.Write(ex, "FlowLayoutManager Exception occured while processing the request LogFPRequestDetailsIntoKinesis");
			}
			return null;

		}
	}
}
