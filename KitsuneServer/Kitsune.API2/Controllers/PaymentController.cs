using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.Utils;

namespace Kitsune.API2.Controllers
{
    [Route("api/Payment")]
    public class PaymentController : Controller
    {
        InstamojoHelper helper = new InstamojoHelper();
        StripeHelper sHelper = new StripeHelper();
        
        [HttpPost]
        [Route("v1/CreatePaymentRequest")]
        public IActionResult CreatePaymentRequest([FromBody]CreatePaymentRequestModel requestModel)
        {
            //Generate the instamojo payment request id, udate the instamojo log collection, return the redirect url
            try
            {
                var result = helper.CreatePaymentRequest(requestModel.username, requestModel.amount, requestModel.responseurl);
                if (result != null && !string.IsNullOrEmpty(result.AbsoluteUri))
                    return Ok(result.AbsoluteUri);
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        [HttpPost]
        [Route("v1/CreateInternationalPaymentRequest")]
        public IActionResult CreateInternationalPaymentRequest([FromBody]CreateInternationalPaymentRequestModel requestModel)
        {
            //Generate the instamojo payment request id, udate the instamojo log collection, return the redirect url
            try
            {
                sHelper.ProcessPaymentRequest(requestModel.username, 
                                            requestModel.amount, 
                                            requestModel.currency, 
                                            requestModel.token,
                                            requestModel.responseurl);
                return Ok();
            }
            catch
            {
                return null;
            }
        }
        
        [HttpGet]
        [Route("v1/IsWalletCritical")]
        public IActionResult IsWalletCritical([FromQuery]string userEmail)  //, [FromQuery] int days = 0, [FromQuery]int extraWebsites = 0
        {
            try
            {
                return Ok(MongoConnector.IsWalletCritical(userEmail));  //, days, extraWebsites
                //return true;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost]
        [Route("v1/InstaMojoWebHook")]
        public IActionResult InstaMojoWebHook([FromForm]InstamojoWebhookRequestModel requestModel)
        {
            try
            {
                if (requestModel != null)
                {
                    //Console.WriteLine("Payment Request:"+requestModel.amount+","+requestModel.status);
                    helper.UpdateWebHook(requestModel);
                    return Ok("success");
                }
                return null;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        [HttpGet]
        [Route("v1/GetPaymentStatus")]
        public IActionResult GetPaymentStatus([FromQuery]string payment_request_id, [FromQuery]string payment_id)
        {
            try
            {
                if (payment_request_id != null && payment_id != null)
                {
                    return Ok(InstamojoHelper.CheckInstaMojoPaymentStatus(payment_request_id, payment_id));
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}