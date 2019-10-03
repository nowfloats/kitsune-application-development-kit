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
    [Route("k-admin/Orders")]
    public class TransactionsController : BaseController
    {
        [ConsoleModeFilter]
        [Authorize]
        public override IActionResult Index()
        {
            var kitsuneStatus = Helpers.KitsuneApiStatusCheck(TempData);
            if (kitsuneStatus.Success && kitsuneStatus.IsDown)
            {
                return RedirectToAction("Maintenance", "Home");
            }

            var isSuccess = Helpers.GetAndUpdateTabStatus(this.HttpContext, this.ControllerContext, HttpContext.Session);
            var tabsVisibilitystatus = Helpers.GetTabStatusInSession(HttpContext.Session);
            var developerEmail = User.Claims.FirstOrDefault(x => x.Type == "DeveloperEmail");

            ViewBag.ShowOrders = tabsVisibilitystatus.Orders;
            ViewBag.showCallLogs = tabsVisibilitystatus.CallLogs;

            if (developerEmail != null)
            {
                ViewBag.DeveloperEmail = developerEmail.Value;
            }

            return View();
        }


        [Route("GetOrders")]
        [HttpPost, Authorize]
        public IActionResult GetOrdersData()
        {
            try
            {
                var customerId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");
                // todo : ||||
                //var customerId = "5a5f74a2f473150505ae8cb2";

                string apiLink = String.Format(Constants.TransactionsEnspoints.TransactionsData, Constants.WebActionServerUrl, Constants.KitsunePaymentsWebactionName, customerId.Value);

                var ordersDataRequest = (HttpWebRequest)WebRequest.Create(new Uri(apiLink));
                ordersDataRequest.Method = "GET";
                ordersDataRequest.ContentType = "application/json";
                ordersDataRequest.Headers.Add(HttpRequestHeader.Authorization, Constants.KitsuneTransactionsClientId);

                var ws = ordersDataRequest.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }


        [Route("GetGateways")]
        [HttpPost, Authorize]
        public IActionResult GetGateways()
        {
            try
            {
                var projectId = User.Claims.FirstOrDefault(x => x.Type == "ProjectId");
                var customerId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                string apiLink = String.Format(Constants.TransactionsEnspoints.PaymentGateways, Constants.KPaymentsServerUrl, projectId.Value, customerId.Value);

                var ordersDataRequest = (HttpWebRequest)WebRequest.Create(new Uri(apiLink));
                ordersDataRequest.Method = "GET";
                ordersDataRequest.ContentType = "application/json";

                var ws = ordersDataRequest.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }


    }


}