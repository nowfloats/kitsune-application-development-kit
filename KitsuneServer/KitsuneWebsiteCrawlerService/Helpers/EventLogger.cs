using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Helpers
{
    public class EventLogRenderer : ILogEventRenderer
    {
        private readonly string hostname;

        public EventLogRenderer()
        {
            hostname = Dns.GetHostName();
        }

        public string RenderLogEvent(LogEvent logEvent)
        {
            try
            {
                using (var writer = new StringWriter())
                {
                    logEvent.RenderMessage(writer);
                    var message = String.Format("{0} [{1}] {2}{3}", logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), logEvent.Level.ToString(), logEvent.Exception?.ToString(), writer.ToString());
                    return message;
                }
            }
            catch (Exception exception)
            {
                try
                {
                    var message = new { RenderedMessage = "Failed to render log message.", MessageTemplate = logEvent.MessageTemplate.Text, Exception = exception.ToString() };
                    return JsonConvert.SerializeObject(message);
                }
                catch (Exception ex)
                {
                    return "Unable to render log message. Reason was " + ex;
                }
            }
        }
    }

    public static class Logger
    {
        public static void InitLogger(string awsAccessKey,string awsSecretKey,string logName,string streamPrefix=null)
        {
            var loggerTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Exception}{Message}{NewLine}";

            var logGroupName = logName;
            var AWS_ACCESS_KEY = awsAccessKey;
            var AWS_SECRET_KEY = awsSecretKey;
            Amazon.RegionEndpoint REGION = Amazon.RegionEndpoint.APSoutheast2;

            Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
            CloudWatchSinkOptions options = new CloudWatchSinkOptions { LogGroupName = logGroupName, LogEventRenderer = new EventLogRenderer()};

            if(streamPrefix!=null)
            {
                ILogStreamNameProvider streamNameProvider = new ConstantLogStreamNameProvider(streamPrefix);
                options = new CloudWatchSinkOptions { LogGroupName = logGroupName, LogEventRenderer = new EventLogRenderer(), LogStreamNameProvider = streamNameProvider };
            }

            // setup AWS CloudWatch client
            AWSCredentials credentials = new BasicAWSCredentials(AWS_ACCESS_KEY, AWS_SECRET_KEY);
            IAmazonCloudWatchLogs client = new AmazonCloudWatchLogsClient(credentials, REGION);


            Log.Logger = new LoggerConfiguration().WriteTo.AmazonCloudWatch(options, client)
                                                  .WriteTo.Console(outputTemplate: loggerTemplate)
                                                  .MinimumLevel.Verbose()
                                                  .CreateLogger();
            
        }
    }
}
