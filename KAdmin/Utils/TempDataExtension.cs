using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KitsuneAdminDashboard.Web.Utils
{
    public static class TempDataExtensions
    {
        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            if (String.IsNullOrEmpty(key) || String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("Key cannot be null or empty");
            }
            tempData[key] = JsonConvert.SerializeObject(value);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            if (String.IsNullOrEmpty(key) || String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("Key cannot be null or empty");
            }

            object o;
            tempData.TryGetValue(key, out o);
            return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
        }
    }
}
