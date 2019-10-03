using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KitsuneAdminDashboard.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KitsuneAdminDashboard.Web.Controllers
{
    [Route("k-admin/GWTAnalytics")]
    public class GWTAnalyticsController : Controller
    {

        [Route("GetDetailedAnalyticsForDate")]
        [Authorize, HttpPost]
        public IActionResult GetDetailedAnalyticsForDate([FromBody]dynamic data)
        {
            try
            {
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;
                var day = data["day"];
                var month = data["day"];
                var year = data["year"];

                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.KitsuneSearchAnalyticsEndpoints.GetDetailedSearchAnalyticsForDate,
                    Constants.RiaWithfloatsServerUrl, websiteId, year, month, day)));
                request.Method = "GET";
                request.ContentType = "application/json";

                var ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var response = JsonConvert.DeserializeObject<List<DetailedSearchAnalyticsData>>(sr.ReadToEnd().ToString());
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                BadRequest(ex.ToString());
            }
            return null;
        }

        [Route("GetDetailedAnalyticsForDateRange")]
        [Authorize, HttpPost]
        public IActionResult GetDetailedAnalyticsForDateRange([FromBody]dynamic data )
        {
            try
            {
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;
                var startDate = data["startDate"];
                var endDate = data["endDate"];

                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.KitsuneSearchAnalyticsEndpoints.GetDetailedSearchAnalyticsForDateRange,
                    Constants.RiaWithfloatsServerUrl, websiteId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"))));
                request.Method = "GET";
                request.ContentType = "application/json";

                var ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var response = JsonConvert.DeserializeObject<List<DetailedSearchAnalyticsData>>(sr.ReadToEnd().ToString());
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                BadRequest(ex.ToString());
            }
            return null;
        }

        [Route("GetDailySearchAnalytics")]
        [Authorize, HttpPost]
        public IActionResult GetDailySearchAnalytics([FromBody]dynamic data)
        {
            try
            {
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;
                var year = data["year"];
                var month = data["month"];

                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.KitsuneSearchAnalyticsEndpoints.GetDailySearchAnalytics,
                    Constants.RiaWithfloatsServerUrl, websiteId, year, month)));
                request.Method = "GET";
                request.ContentType = "application/json";

                var ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var response = JsonConvert.DeserializeObject<List<DailySearchAnalytics>>(sr.ReadToEnd().ToString());
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                BadRequest(ex.ToString());
            }
            return null;
        }

        [Route("GetMonthlySearchAnalytics")]
        public IActionResult GetMonthlySearchAnalytics([FromBody]dynamic data)
        {
            try
            {
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;
                var year = data["year"];

                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.KitsuneSearchAnalyticsEndpoints.GetMonthlySearchAnalytics,
                    Constants.RiaWithfloatsServerUrl, websiteId, year)));
                request.Method = "GET";
                request.ContentType = "application/json";

                var ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var response = JsonConvert.DeserializeObject<List<MonthlySearchAnalytics>>(sr.ReadToEnd().ToString());
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                BadRequest(ex.ToString());
            }
            return null;
        }

        [Route("GetKitsunePaymentsData")]
        [Authorize, HttpPost]
        public IActionResult GetKitsunePaymentsData([FromBody] dynamic data)
        {
            try
            {
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;
                
                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.KitsunePaymentsEndpoints.GetMetrics,
                    Constants.KitsunePayments, websiteId)));

                request.Method = "POST";
                request.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(data);
                    streamWriter.Write(jsonData);
                }

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
            }
            catch (Exception ex)
            {
                BadRequest(ex.ToString());
            }
            return null;
        }

        
    }
}