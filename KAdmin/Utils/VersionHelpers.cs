using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KitsuneAdminDashboard.Web.IRazorPageExtensions
{
    public static class IRazorPageExtensions
    {
        public static string AddFileVersionToPath(this IRazorPage page, string path)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json");

                var configuration = builder.Build();
                var version = configuration["FileVersions"];
                var versionPath = $"{path}?v={version}";
                return versionPath;
            }
            catch (Exception ex) {
                return path;
            }
        }
    }
}
