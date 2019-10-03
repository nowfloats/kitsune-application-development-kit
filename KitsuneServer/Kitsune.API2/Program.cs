using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;

namespace Kitsune.API2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CloudWatchSinkOptions options = new CloudWatchSinkOptions { LogGroupName = "Kitsune-API2" , MinimumLogEventLevel = LogEventLevel.Debug };  //, LogEventRenderer = MyCustomRenderer
            AWSCredentials credentials = new BasicAWSCredentials("[[KIT_CLOUD_AWS_ACCESS_KEY]]", "[[KIT_CLOUD_AWS_SECRET_KEY]]");
            IAmazonCloudWatchLogs client = new AmazonCloudWatchLogsClient(credentials, RegionEndpoint.APSouth1);

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

            Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Debug()
             .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
             .MinimumLevel.Override("System", LogEventLevel.Warning)
             .Enrich.WithProperty("IP", myIP)
             .WriteTo.AmazonCloudWatch(options, client)
             .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                BuildWebHost(args).Run();
                return;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseKestrel()
            .UseSetting("System.GC.Concurrent", "true")
                .UseSetting("System.GC.Server", "true")
                .UseUrls("http://*:80")
                .UseStartup<Startup>()
                .Build();
    }
}
