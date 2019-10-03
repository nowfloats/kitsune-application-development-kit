using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KitsuneAdminDashboard.Web.Utils
{
    public class AllowCORSFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)context.HttpContext.Request.Headers["Origin"] });
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept, Cache-Control, DeveloperId, WebsiteId" });
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });
            context.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
            context.HttpContext.Response.StatusCode = 200;
            
        }
    }
}
