using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KitsuneAdminDashboard.Web.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using KitsuneAdminDashboard.Web.Utils;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace KitsuneAdminDashboard.Web.Controllers
{
    [Route("k-admin/Analytics")]
    public class AnalyticsController : BaseController
    {
        [Route("GetVisitors")]
        [Authorize, HttpPost]
        public IActionResult GetVisitors(VistorsFilterType filterType, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId")?.Value;
                var website = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.AnalyticsEndpoints.GetVisitorsEndpoint, Constants.KitsuneServerUrl, Convert.ToInt32(filterType), website, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId);

                var ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [Route("GetTrafficSources")]
        [Authorize, HttpPost]
        public IActionResult GetTrafficSources(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId")?.Value;
                var website = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.AnalyticsEndpoints.GetReferralsEndpoint, Constants.KitsuneServerUrl, website, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId);

                var ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [Route("GetDevices")]
        [Authorize, HttpPost]
        public IActionResult GetDevices(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId")?.Value;
                var website = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.AnalyticsEndpoints.GetDevicesEndpoint, Constants.KitsuneServerUrl, website, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId);

                var ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [Route("GetBrowsers")]
        [Authorize, HttpPost]
        public IActionResult GetBrowsers(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId")?.Value;
                var website = User.Claims.FirstOrDefault(x => x.Type == "CustomerId")?.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(string.Format(Constants.AnalyticsEndpoints.GetBrowserEndpoint, Constants.KitsuneServerUrl, website, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId);

                var ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

    }
}