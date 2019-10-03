using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class EventLogger
    {


        /// <summary>
        /// For using this class simply add reference to this class and few configurations to the config file : 
        /// 
        /// Reffer to log4net : 
        /// 
        /// <configSections>
        /// <section name = "log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
        /// </configSections>
        /// 
        ///  For Log4net Configuration : 
        ///  
        /// <log4net>
        ///  <root>
        ///    <level value = "ALL" />
        ///    < appender -ref ref="AWS" />
        ///   <appender-ref ref="RollingFileAppender" />
        ///  </root>
        /// 
        ///  For appending logs to the File name log.txt :
        /// 
        ///  <appender name = "RollingFileAppender" type="log4net.Appender.RollingFileAppender, log4net">
        ///    <file value = "{File Name}" />
        ///    < appendToFile value="true" />
        ///    <rollingStyle value = "Size" />
        ///    < maxSizeRollBackups value="20" />
        ///    <maximumFileSize value = "30000KB" />
        ///    < staticLogFileName value="true" />
        ///    <layout type = "log4net.Layout.PatternLayout" >
        ///      < conversionPattern value="%date [%thread] %-5level %logger - %message%newline" />
        ///    </layout>
        ///  </appender>
        ///  
        ///     
        ///  For sending logs to the cloud watch:
        ///  
        ///  
        ///  <appender name = "AWS" type="AWS.Logger.Log4net.AWSAppender,AWS.Logger.Log4net">
        ///    <LogGroup>{Log Group Name}</LogGroup>
        ///    <Region>{Region Of the Log Group}</Region>
        ///    <layout type = "log4net.Layout.PatternLayout" >
        ///      < conversionPattern value="[%thread] %-5level %logger - %message%newline" />
        ///    </layout>
        ///  </appender>
        ///  </log4net>
        /// 
        /// Note : For sending logs to the cloudwatch 
        ///  Install Following Nuget Packages  : 
        ///  1) AWS.Logger.Log4net
        ///  2) AWSSDK.CloudWatchLogs
        ///  3) AWSSDK.CloudFront
        ///     
        ///  Usage :
        ///  
        ///  using Logger;
        ///  
        ///  EventLogger.EventLogName = "{LOG_NAME}";
        ///  EventLogger.LogTrace({message});
        ///  EventLogger.Write({exception},{message},{any number of arguments});
        /// 
        /// 
        /// </summary>

        public static string EventLogName { get; set; }
        const int ErrorNumber = 999;
        static readonly EventLog PSXEventLog = InitializeLog();
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        static EventLogger()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        private static EventLog InitializeLog()
        {
            try
            {
                EventLog log = new EventLog();
                log.Source = EventLogName;
                return log;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static void Write(Exception ex, string message, params object[] args)
        {
            try
            {
                try
                {
                    //Send an exception
                    var logger = LogManager.GetLogger(EventLogName);
                    string errors = FormatArgsObjects(args);
                    logger.Error($"Timestamp : {DateTime.UtcNow}\nLevel : ERROR\nLoggerName : {EventLogName}\nMessage : {message}\nErrors : {errors}\nStack Trace : {ex}");
                }
                catch { }
            }
            catch { }
        }

        public static void Write(TraceLevel logType, string message, params object[] args)
        {
            try
            {
                //Send an exception
                var logger = LogManager.GetLogger(EventLogName);
                var errors = FormatArgsObjects(args);
                logger.Error($"Timestamp : {DateTime.UtcNow}\nLoggerName : {EventLogName}\nMessage : {message}\nDetails : {errors}");
            }
            catch { }

        }

        public static void LogTrace(string message, params object[] args)
        {
            var logger = LogManager.GetLogger(EventLogName);
            var errors = FormatArgsObjects(args);
            logger.Info($"Timestamp : {DateTime.UtcNow}\nLoggerName : {EventLogName}\nMessage : {message}\nDetails : {errors}");
        }

        private static string FormatArgsObjects(params object[] args)
        {
            try
            {
                StringBuilder errors = new StringBuilder();
                if (args!=null && args.Length != 0)
                {

                    foreach(var v in args)
                    {
                        errors.Append($"\n{JsonConvert.SerializeObject(v)}");
                    }
                }
                return errors.ToString();
            }
            catch
            { return String.Empty; }
        }

    }
}
