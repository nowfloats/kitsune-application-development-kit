using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Constants
{
    public class Configuration
    {
        public static IConfigurationRoot ConfigurationSettings;

        public Configuration(IConfigurationRoot configurationRoot)
        {
            ConfigurationSettings = configurationRoot;
        }
        
        public static string GetSetting(string key)
        {
            return ConfigurationSettings.GetValue<string>(key);
        }
    }
}
