using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KitsuneAdminDashboard.Web.Models;
using KitsuneAdminDashboard.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KitsuneAdminDashboard.Web.Controllers
{
    [Route("k-admin/CallTrackerLog")]
    public class CallTrackerController : BaseController
    {
        [ConsoleModeFilter]
        public override IActionResult Index()
        {
            var kitsuneStatus = Helpers.KitsuneApiStatusCheck(TempData);
            if (kitsuneStatus.Success && kitsuneStatus.IsDown)
            {
                return RedirectToAction("Maintenance", "Home");
            }


            var isSuccess = Helpers.GetAndUpdateTabStatus(this.HttpContext, this.ControllerContext, HttpContext.Session);
            var tabsVisibilitystatus = Helpers.GetTabStatusInSession(HttpContext.Session);

            ViewBag.ShowOrders = tabsVisibilitystatus.Orders;
            ViewBag.showCallLogs = tabsVisibilitystatus.CallLogs;

            return View();
        }

        [Route("GetVMNDetails")]
        [Authorize, HttpGet]
        public IActionResult GetVMNDetails()
        {
            try
            {
                // todo : use
                var authorization = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");
                var schemaName = User.Claims.FirstOrDefault(x => x.Type == "EntityName");
                //EntityName


                // todo : dev
                //string authorization = "593023815d64370c6cc90c2c";
                //string schemaName = "langtest2";
                //string websiteId = "5b1d0fe7f760e81e70d562fc";
                //string baseApiUrl = "https://api2.kitsunedev.com";

                var apiUrl = String.Format(Constants.CallTrackerEndPoints.GetVMNDetails, Constants.KitsuneServerUrl, schemaName.Value, websiteId.Value); 
                var request = (HttpWebRequest)WebRequest.Create(new Uri(apiUrl));

                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authorization.Value);

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
                return null;
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [Route("GetCallLogs")]
        [Authorize, HttpPost]
        public IActionResult GetCallLogs([FromBody]CallLogRequest callLogRequest)
        {
            try
            {
                bool isValid = callLogRequest.Validate();
                // todo : use
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");


                // todo : dev
                //string authorization = "593023815d64370c6cc90c2c";
                //string websiteId = "test-kit-v2";
                int limit = callLogRequest.Limit > 0 ? callLogRequest.Limit : 100;
                var endpoint = "";

                if (callLogRequest.DataforAllNumbers)
                {
                    endpoint = Constants.CallTrackerEndPoints.GetAllCallLogs;
                    endpoint = String.Format(endpoint, Constants.NFServerUrl, Constants.NFClientId, websiteId.Value, limit);
                }
                else
                {
                    endpoint = Constants.CallTrackerEndPoints.GetCallLogs;
                    endpoint = String.Format(endpoint, Constants.NFServerUrl, Constants.NFClientId, websiteId.Value, limit, callLogRequest.Number);
                }
                
                
                var request = (HttpWebRequest)WebRequest.Create(new Uri(endpoint));

                request.Method = "GET";
                request.ContentType = "application/json";
                

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
                return null;
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

    }
}