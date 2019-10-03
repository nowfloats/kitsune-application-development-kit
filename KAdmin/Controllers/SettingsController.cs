using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KitsuneAdminDashboard.Web.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using KitsuneAdminDashboard.Web.Utils;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace KitsuneAdminDashboard.Web.Controllers
{
    [Route("/k-admin/Settings")]
    public class SettingsController : BaseController
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

            ViewBag.ShowOrders = tabsVisibilitystatus.Orders;
            ViewBag.showCallLogs = tabsVisibilitystatus.CallLogs;

            return View();
        }

        [Route("UpdateCustomerDetails")]
        [HttpPost, Authorize]
        public IActionResult UpdateCustomerDetails([FromBody]UpdateCustomerRequestModel requestModel)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId").Value;
                var websiteUserId = User.Claims.FirstOrDefault(x => x.Type == "WebsiteUserId").Value;
                var accessType = User.Claims.FirstOrDefault(x => x.Type == "AccessType").Value;

                requestModel.AccessType = accessType;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.CustomerEndpoints.UpdateCustomerDetailsEndpoint, Constants.KitsuneServerUrl, websiteId, websiteUserId)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(requestModel);
                    streamWriter.Write(jsonData);
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    UpdateCustomerName(requestModel.ContactDetails.FullName);
                    return new JsonResult(streamReader.ReadToEnd());
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("GetWebsiteUserDetails")]
        [HttpGet, Authorize]
        public WebsiteUserDetails GetWebsiteUserDetails() {
            try
            {
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId").Value;
                var websiteUserId = User.Claims.FirstOrDefault(x => x.Type == "WebsiteUserId").Value;
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.CustomerEndpoints.GetCustomerDetailsEndpoint, Constants.KitsuneServerUrl, websiteId, websiteUserId)));
                request.Method = "GET";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<WebsiteUserDetails>(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Route("UpdatePassword")]
        [HttpPost, Authorize]
        public IActionResult UpdatePassword([FromBody]UserPasswordUpdate requestModel)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteUserId = User.Claims.FirstOrDefault(x => x.Type == "WebsiteUserId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                requestModel.DeveloperId = authId.Value;
                requestModel.WebsiteId = websiteId.Value;
                requestModel.WebsiteUserId = websiteUserId.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.CustomerEndpoints.UpdateCustomerPasswordEndpoint, Constants.KitsuneServerUrl, websiteId.Value, websiteUserId.Value)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(requestModel);
                    streamWriter.Write(jsonData);
                }
                
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return new JsonResult(streamReader.ReadToEnd());
                }

            }
            catch (WebException ex)
            {
                try
                {
                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            var error = JsonConvert.DeserializeObject<UserPasswordUpdateErrorResponse>(reader.ReadToEnd());
                            if (error != null && error.Errors.Count > 0)
                            {
                                return BadRequest(new UserPasswordUpdateResponse()
                                {
                                    IsUpdationError = true,
                                    ErrorMessage = error.Errors[0].ErrorMessage
                                });
                            }
                            else
                            {
                                return BadRequest(new UserPasswordUpdateResponse()
                                {
                                    IsUpdationError = false,
                                    ErrorMessage = "error occured"
                                });
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    return BadRequest(new UserPasswordUpdateResponse()
                    {
                        IsUpdationError = false,
                        ErrorMessage = ex.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new UserPasswordUpdateResponse() {
                    IsUpdationError = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        private void UpdateCustomerName(string customerName)
        {
            try
            {
                if(!String.IsNullOrEmpty(customerName) && !String.IsNullOrWhiteSpace(customerName))
                {
                    HttpContext.Session.Set("CustomerName", Encoding.ASCII.GetBytes(customerName));
                }
            }
            catch(Exception ex)
            {

            }
        }
    }
}
