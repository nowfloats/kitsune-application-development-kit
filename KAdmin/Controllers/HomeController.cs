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
using System.IO;
using Newtonsoft.Json;
using System.Web;
using System.Text;

namespace KitsuneAdminDashboard.Web.Controllers
{
    public class HomeController : BaseController
    {
        public static String LOGIN_MODE_CONSOLE = "data_console_mode";

        public static String LOGIN_MODE_DEFAULT = "default";

        const string ALI_CLOUD = "ALICLOUD";

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

        public IActionResult Login()
        {

            var kitsuneStatus = Helpers.KitsuneApiStatusCheck(TempData);
            if (kitsuneStatus.Success && kitsuneStatus.IsDown)
            {
                return RedirectToAction("Maintenance", "Home");
            }
            else
            {
                return View();
            }
            
        }

        public IActionResult Maintenance()
        {
            var response = TempDataExtensions.Get<KitsuneStatusResponse>(TempData, "kitsuneStatus");
            if (response == null)
            {
                response = Helpers.KitsuneApiStatusCheck(TempData);
                if (response.Success && !response.IsDown)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.kitsuneStatus = response;
            return View("Maintenance");
        }

        #region Login
        [AllowAnonymous, HttpPost]
        public IActionResult Login(AuthUser authUser)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var loginResponse = AuthenticationHelpers.VerifyLogin(authUser);
                    if (loginResponse != null)
                    {
                        var projectDetails = loginResponse.WebsiteDetails;
                        var userDetails = loginResponse.UserDetails;
                        var developerDetails = loginResponse.DeveloperContactDetails;

                        var claims = new List<Claim> {
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim("UserAuthId", loginResponse.DeveloperId)
                    };

                        if (!string.IsNullOrEmpty(loginResponse.SchemaId))
                            claims.Add(new Claim("SchemaId", loginResponse.SchemaId));

                        if (!string.IsNullOrEmpty(loginResponse.EntityName))
                            claims.Add(new Claim("EntityName", loginResponse.EntityName));

                        if (!string.IsNullOrEmpty(loginResponse.DeveloperId))
                            claims.Add(new Claim("DeveloperId", loginResponse.DeveloperId));

                        if (userDetails != null)
                        {
                            if (!string.IsNullOrEmpty(userDetails.UserName))
                                claims.Add(new Claim("Username", userDetails.UserName));

                            if (!string.IsNullOrEmpty(userDetails.UserId))
                                claims.Add(new Claim("WebsiteUserId", userDetails.UserId));

                            if (!string.IsNullOrEmpty(userDetails.AccessType))
                                claims.Add(new Claim("AccessType", userDetails.AccessType));

                            if (userDetails.Contact != null && !string.IsNullOrEmpty(userDetails.Contact.FullName))
                                claims.Add(new Claim("CustomerName", userDetails.Contact.FullName));

                            if (userDetails.Contact != null && !string.IsNullOrEmpty(userDetails.Contact.Email))
                                claims.Add(new Claim("CustomerEmail", userDetails.Contact.Email));

                            if (userDetails.Contact != null && !string.IsNullOrEmpty(userDetails.Contact.PhoneNumber))
                                claims.Add(new Claim("CustomerPhoneNumber", userDetails.Contact.PhoneNumber));
                        }

                        if (projectDetails != null)
                        {
                            if (!string.IsNullOrEmpty(projectDetails.ProjectId))
                                claims.Add(new Claim("ProjectId", projectDetails.ProjectId));

                            if (!string.IsNullOrEmpty(projectDetails.WebsiteUrl))
                            {
                                claims.Add(new Claim("WebsiteUrl", projectDetails.WebsiteUrl));
                                claims.Add(new Claim("Domain", projectDetails.WebsiteUrl));
                            }

                            if (!string.IsNullOrEmpty(projectDetails.WebsiteId))
                                claims.Add(new Claim("CustomerId", projectDetails.WebsiteId));
                        }

                        if (developerDetails != null)
                        {
                            if (!string.IsNullOrEmpty(developerDetails.Email))
                                claims.Add(new Claim("DeveloperEmail", developerDetails.Email));
                        }

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                        if (IsHostedOnAlicloud())
                        {
                            Response.Cookies.Append("CLOUD_PROVIDER", "ALI_CLOUD");
                        }

                        return Ok("success");
                    }
                    else
                    {
                        return Ok("invalid");
                    }
                }
                else
                {
                    return Ok("invalid");
                }
            }
            catch(Exception ex)
            {
                return BadRequest("invalid");
            }
        }

        private bool VerifyLogin(AuthUser authUser)
        {
            var loginResponse = AuthenticationHelpers.VerifyLogin(authUser);
            return authUser != null;

        }

        [AllowAnonymous, HttpGet]
        public IActionResult TokenLogin([FromQuery] string token)
        {
            try {
                token = HttpUtility.UrlEncode(token);
                var loginDataRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.CustomerEndpoints.GetLoginDetailsEndpoint, Constants.KitsuneServerUrl, token)));
                loginDataRequest.Method = "GET";
                loginDataRequest.ContentType = "application/json";

                var ws = loginDataRequest.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream());
                TokenAuthUser authObject = JsonConvert.DeserializeObject<TokenAuthUser>(sr.ReadToEnd().ToString());

                if (authObject != null)
                {
                    AuthUser authUser = new AuthUser()
                    {
                        Domain = authObject.WebsiteUrl,
                        Username = authObject.UserName,
                        Pwd = authObject.Password
                    };

                    Login(authUser);

                    if (VerifyLogin(authUser))
                    {
                        if (authObject.Source != null && authObject.Source.Equals(LOGIN_MODE_CONSOLE))
                        {
                            LoginModeSessionHelpers.SetLoginMode(HttpContext.Session, "data_console_mode");
                            LoginModeSessionHelpers.SetConsoleModeToken(HttpContext.Session, token);
                            return Redirect("/k-admin/ManageWebsiteContent");
                        } else
                        {
                            return Redirect("/k-admin");
                        }
                        
                    }
                    else {
                        return LogOut();
                    }
                }
                else
                {
                    return LogOut();
                }
            }
            catch (Exception ex) {
                return LogOut();
            }
            
        }
        #endregion

        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private Boolean IsHostedOnAlicloud()
        {
            Boolean isAliCloud = false;
            try
            {
                isAliCloud = (Environment.GetEnvironmentVariable("CLOUD_PROVIDER").Equals(ALI_CLOUD));
            }
            catch(Exception ex)
            {

            }
            return isAliCloud;
        }
        
    }
}
