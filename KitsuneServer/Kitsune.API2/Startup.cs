using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Amazon;
using System.IO;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Kitsune.API2
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.Production.json", optional: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();
            EnvironmentConstants.ApplicationConfiguration = Configuration.Get<Appsettings>();

#if DEBUG
            MongoDBCollection.InitialiseCollection(true);
#else
            MongoDBCollection.InitialiseCollection(env.IsProduction());
#endif

            
            //Console.WriteLine("_configurationSettings: " + JsonConvert.SerializeObject(EnvironmentConstants.ApplicationConfiguration));


            // customer renderer (optional, defaults to a simple rendered message of Serilog's LogEvent
            //var renderer = new MyCustomRenderer();

            //CloudWatchSinkOptions options = new CloudWatchSinkOptions { LogGroupName = EnvironmentConstants.ApplicationConfiguration.CloudWatchLogger.LogGroupName };  //, LogEventRenderer = MyCustomRenderer

            //AWSCredentials credentials = new BasicAWSCredentials(EnvironmentConstants.ApplicationConfiguration.AWSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSConfiguration.AWS_SecretKey);
            //IAmazonCloudWatchLogs client = new AmazonCloudWatchLogsClient(credentials, RegionEndpoint.APSoutheast2);
            //Log.Logger = new LoggerConfiguration()
            //                .MinimumLevel.Debug()
            //                .WriteTo.AmazonCloudWatch(options, client)
            //                //.WriteTo.RollingFile(Path.Combine("D:\\VS_2015\\Log", "log-{Date}.txt"))
            //                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
               .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
               .AddNewtonsoftJson(options => options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat);

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });
            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = "kit-api-cache.g98ftg.clustercfg.aps1.cache.amazonaws.com:6379";
                option.InstanceName = "redis";
            });

            //services.Configure<MvcOptions>(options =>
            //{
            //    options.Filters.Add(new RequireHttpsAttribute());
            //});

            //var mapper = ConfigureAutoMapper.Configure();
            //services.AddOptions();
            ////services.Configure<MongoOptions>(Configuration);
            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            ////services.AddSingleton<IRepository, Repository>();
            ////services.AddSingleton(mapper);
            ////services.AddMvc();
            ////services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            ////services.AddAWSService<IAmazonSimpleEmailService>();

            //services.AddMvc()
            //    .AddJsonOptions(options =>
            //    {
            //        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
            //    }); ;
            //Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", Configuration["AWS:AccessKey"]);
            //Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", Configuration["AWS:SecretKey"]);
            //Environment.SetEnvironmentVariable("AWS_REGION", Configuration["AWS:Region"]);
            //Environment.SetEnvironmentVariable("AWS_PROFILE", Configuration["AWS:Profile"]);
            //services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            //services.AddAWSService<IAmazonS3>();
            ////services.AddAWSService<IAmazonDynamoDB>();
            
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) //, ILoggerFactory loggerFactory
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder => {
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
            
            app.UseMiddleware<LogMiddleware>();
            //Redirect to https on  production
#if !DEBUG

            //var options = new RewriteOptions()
            //                    .AddRedirectToHttps();

            //app.UseRewriter(options);

#endif

            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
            //loggerFactory.AddSerilog();
        }
    }
}
