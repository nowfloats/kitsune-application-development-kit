using KitsuneAdminDashboard.Web.Controllers;
using KitsuneAdminDashboard.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace KitsuneAdminDashboard.Web.Utils
{
    public class Helpers
    {
        public static bool GetAndUpdateTabStatus(HttpContext httpContext, ControllerContext controllerContext, ISession session) {
            try

            {
                var hasOrders = Helpers.HasOrders(httpContext, controllerContext);
                var hasCallLogs = Helpers.HasCallLogs(httpContext, controllerContext);

                Helpers.SetTabStatusInSession(session, new TabsVisisbilityStatus()
                {
                    CallLogs = !hasCallLogs.IsError ? hasCallLogs.DoesExists : false,
                    Orders = !hasOrders.IsError ? hasOrders.DoesExists : false
                });
                return true;
            }
            catch (Exception ex)
            {
                // todo logger
            }
            return false;
        }

        public static TabExistsResponse HasOrders(HttpContext httpContext, ControllerContext controllerContext)
        {
            var response = new TabExistsResponse()
            {
                DoesExists = false,
                IsError = false
            };

            try
            {
                #region Arguments Validation

                if (httpContext == null)
                {
                    throw new ArgumentNullException("httpContext cannot be null.");
                }

                if (controllerContext == null)
                {
                    throw new ArgumentNullException("controllerContext cannot be null.");
                }

                #endregion

                #region Variable Declarations
                bool gatewaysconfigured = false;
                bool hasOrders = false;

                var transactions = new TransactionsController
                {
                    ControllerContext = controllerContext
                };

                var gatewayResponse = transactions.GetGateways();
                var orders = transactions.GetOrdersData();
                #endregion

                #region Checking Avaliable Data
                if (gatewayResponse.GetType() == typeof(JsonResult))
                {
                    gatewaysconfigured = true;
                }

                if (orders.GetType() == typeof(JsonResult))
                {
                    var result = (JsonResult)orders;
                    var value = JsonConvert.DeserializeObject<dynamic>(result.Value.ToString());
                    var extra = value["Extra"];
                    var dataLength = (extra != null ? (extra["TotalCount"] != null ? (int)extra["TotalCount"] : 0) : 0);
                    if (dataLength > 0)
                    {
                        hasOrders = true;
                    }
                }
                #endregion

                response.DoesExists = gatewaysconfigured || hasOrders;
            }
            catch (Exception ex)
            {
                response.IsError = true;
            }

            return response;
        }

        public static TabExistsResponse HasCallLogs(HttpContext httpContext, ControllerContext controllerContext)
        {
            var response = new TabExistsResponse()
            {
                DoesExists = false,
                IsError = false
            };

            try
            {
                #region Arguments Validation

                if (httpContext == null)
                {
                    throw new ArgumentNullException("httpContext cannot be null.");
                }

                if (controllerContext == null)
                {
                    throw new ArgumentNullException("controllerContext cannot be null.");
                }

                #endregion

                #region Variable Declarations
                bool hasVirtualMobileNumbers = false;
                bool hasCallLogs = false;

                var transactions = new CallTrackerController
                {
                    ControllerContext = controllerContext
                };

                var vmnDetailsResponse = transactions.GetVMNDetails();
                var callLogsResponse = transactions.GetCallLogs(new CallLogRequest() {
                    DataforAllNumbers = true,
                    Limit = 10000
                });
                #endregion

                #region Checking Avaliable Data
                if (vmnDetailsResponse.GetType() == typeof(JsonResult))
                {
                    var result = (JsonResult)vmnDetailsResponse;
                    var parsedResponse = JsonConvert.DeserializeObject<dynamic>(result.Value.ToString());
                    var dataLength = parsedResponse["Data"] != null ? (new JArray(parsedResponse["Data"])).Count : 0;
                    hasVirtualMobileNumbers = dataLength > 0 ? true : false;
                }

                if (callLogsResponse.GetType() == typeof(JsonResult))
                {
                    var result = (JsonResult)callLogsResponse;
                    var parsedResponse = JsonConvert.DeserializeObject<dynamic>(result.Value.ToString());
                    var dataLength = parsedResponse != null ? ((new JArray(parsedResponse)).Count) : 0;
                    hasCallLogs = dataLength > 0 ? true : false;
                }
                #endregion

                response.DoesExists = hasVirtualMobileNumbers || hasCallLogs;
            }
            catch (Exception ex)
            {
                response.IsError = true;
            }

            return response;
        }

        public static void SetTabStatusInSession(ISession session, TabsVisisbilityStatus status) {
            try
            {
                session.Set("showOrders", BitConverter.GetBytes(status.Orders));
                session.Set("showCallLogs", BitConverter.GetBytes(status.CallLogs));
            }
            catch(Exception ex)
            {
                // todo : handle exception
            }
        }

        public static TabsVisisbilityStatus GetTabStatusInSession(ISession session)
        {
            var response = new TabsVisisbilityStatus()
            {
                CallLogs = false,
                Orders = false
            };
            try
            {
                var showOrders = session.Get("showOrders") != null
               ? BitConverter.ToBoolean(session.Get("showOrders"), 0)
               : false;

                var showCallLogs = session.Get("showCallLogs") != null
                    ? BitConverter.ToBoolean(session.Get("showCallLogs"), 0)
                    : false;

                response.CallLogs = showCallLogs;
                response.Orders = showOrders;
            }
            catch (Exception ex)
            {
                // todo : handle exception
            }
            return response;
        }

        public static CustomerDetails GetCustomerDetails(ISession session, ClaimsPrincipal user)
        {
            var response = new CustomerDetails()
            {
                CustomerName = "",
                WebsiteUrl = ""
            };

            try
            {

                #region Customer Name
                try
                {
                    var customerFromClaims = user.Claims.FirstOrDefault(x => x.Type == "CustomerName");
                    var customerFromSession = session.Get("CustomerName");

                    if (customerFromSession != null)
                    {
                        response.CustomerName = customerFromSession != null ? Encoding.Default.GetString(customerFromSession) : "";
                    }
                    else if (customerFromClaims != null)
                    {
                        response.CustomerName = customerFromClaims.Value;
                    }
                }
                catch(Exception ex)
                {

                }
                #endregion

                #region Domain Url
                try
                {
                    var websiteUrlFromClaims = user.Claims.FirstOrDefault(x => x.Type == "WebsiteUrl");
                    var websiteUrlFromSession = session.Get("WebsiteUrl");

                    if (websiteUrlFromSession != null)
                    {
                        response.WebsiteUrl = websiteUrlFromSession != null ? Encoding.Default.GetString(websiteUrlFromSession) : "";
                    }
                    else if (websiteUrlFromClaims != null)
                    {
                        response.WebsiteUrl = websiteUrlFromClaims.Value;
                    }
                }
                catch (Exception ex)
                {

                }
                #endregion
                
            }
            catch (Exception ex)
            {

            }

            return response;
        }

        public static KitsuneStatusResponse KitsuneApiStatusCheck(ITempDataDictionary TempData)
        {
            var response = new KitsuneStatusResponse() {
                IsDown = false,
                Success = false,
                IsApiDown = false,
                IsMaintenanceBreak = true,
                Detail =  new KitsuneDowntimeDescription() {
                    Title = "Unable to reach Servers.",
                    Description = "Please contact your developer." 
                }
            };

            try
            {
                var kitsuneStatusRequest = (HttpWebRequest)WebRequest.Create(new Uri(Constants.KitsuneStatusCheckUrl));
                kitsuneStatusRequest.Method = WebRequestMethods.Http.Get;
                kitsuneStatusRequest.ContentType = "application/json";
                kitsuneStatusRequest.Timeout = 5000;

                var ws = kitsuneStatusRequest.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream());

                var stringifiedResponse = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(stringifiedResponse))
                {
                    return JsonConvert.DeserializeObject<KitsuneStatusResponse>(stringifiedResponse);
                }
            }
            catch (Exception ex)
            {

            }
            TempDataExtensions.Put<KitsuneStatusResponse>(TempData, "kitsuneStatus", response);
            return response;
        }

    }

    public class LoginModeSessionHelpers
    {
        public static String SESSION_CONSOLE_MODE_TOKEN = "ConsoleModeToken";

        public static String SESSION_LOGIN_MODE = "LoginMode";

        public static void SetConsoleModeToken(ISession session, string token)
        {
            try
            {
                session.SetString(SESSION_CONSOLE_MODE_TOKEN, token);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static String GetConsoleModeToken(ISession session)
        {
            try
            {
                String token = session.GetString(SESSION_CONSOLE_MODE_TOKEN);
                return token;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void SetLoginMode(ISession session, String mode)
        {
            try
            {
                session.SetString(SESSION_LOGIN_MODE, mode);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static String GetLoginMode(ISession session)
        {
            try
            {
                String loginMode = session.GetString(SESSION_LOGIN_MODE);
                return loginMode;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
