using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitsuneAdminDashboard.Web.Models;
using KitsuneAdminDashboard.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KitsuneAdminDashboard.Web.Controllers
{
    public class BaseController : Controller
    {
        [ConsoleModeFilter]
        [Authorize]
        public virtual IActionResult Index()
        {
            return View();
        }

        [Route("GetCustomerName")]
        public IActionResult GetCustomerName()
        {
            try
            {
                var customerDetails = Helpers.GetCustomerDetails(HttpContext.Session, this.User);
                return new JsonResult(customerDetails);
            }
            catch(Exception ex)
            {
                return Ok(null);
            }
        }
    }
}