using KitsuneAdminDashboard.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Net;

namespace KitsuneAdminDashboard.Web.Utils
{
    public class ConsoleModeFilter : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            String loginMode = LoginModeSessionHelpers.GetLoginMode(context.HttpContext.Session);
             if (loginMode != null && loginMode.Equals("data_console_mode"))
            {
                string path = context.HttpContext.Request.Path;
                if (path != null)
                {
                    if (IsAccessingProtectedRoute(path))
                    {
                        LogOutUser(context);
                    }
                }

                var token = LoginModeSessionHelpers.GetConsoleModeToken(context.HttpContext.Session);
                if (token != null)
                {
                    bool isTokenValid = ValidateConsoleModeToken(token);
                    if (isTokenValid)
                    {
                        var controller = (Controller)context.Controller;
                        controller.ViewBag.LoginMode = "data_console_mode";
                    } else
                    {
                        LogOutUser(context);
                    }

                } else
                {
                    LogOutUser(context);
                }
            }
        }

        public void LogOutUser(ActionExecutingContext context)
        {
            var controller = (Controller)context.Controller;
            context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "Logout" }));
            context.Result.ExecuteResultAsync(controller.ControllerContext);
        }

        private bool IsAccessingProtectedRoute(string path)
        {
            bool isProtectedRoute = false;
            string[] protectedRoutes = new String[] 
                { "/k-admin", "/k-admin/Inbox", "/k-admin/CallTrackerLog", "/k-admin/Orders", "/k-admin/Settings",
                    "/k-admin/", "/k-admin/Inbox/", "/k-admin/CallTrackerLog/", "/k-admin/Orders/", "/k-admin/Settings/"};
            if (Array.IndexOf(protectedRoutes, path) >= 0)
            {
                isProtectedRoute = true;
            }
            return isProtectedRoute;
        }
        

        internal static bool ValidateConsoleModeToken(string token)
        {
            bool isValid = false;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.CustomerEndpoints.ValidateConsoleModeToken,
                    Constants.KitsuneServerUrl, token)));
                request.Method = "GET";
                request.ContentType = "application/json";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                isValid = ((int)response.StatusCode == 200) ? true : false;
            }
            catch (Exception ex)
            {
                isValid = false;
            }
            return isValid;
        }
    }
}
