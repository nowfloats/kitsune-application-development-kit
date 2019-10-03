using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kitsune.API2.Models;
using Kitsune.API2.EnvConstants;
using System.Text;

namespace Kitsune.API2.Utils
{
    public class BillingAPI
    {
    #if DEBUG
            private static readonly string BillingBaseUri = "https://billing.kitsunedev.com/";
    #else
            private static readonly string BillingBaseUri = "https://billing.kitsune.tools/";
    #endif
        //Component : {"Webrequests"} , Process: {"activate", "deactivte", "update"} ---- these are present in constants file
        public static bool BillingProcess(string customerId, string Remarks, string Component, string Process)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentNullException(nameof(customerId));
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestObject = new BillingRequestModel
                {
                    component = Component,
                    resource_id = customerId,
                    remarks = Remarks
                };
                var jsonData = JsonConvert.SerializeObject(requestObject);
                var response = client.PostAsync(new Uri(new Uri(BillingBaseUri), Process),
                    new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
