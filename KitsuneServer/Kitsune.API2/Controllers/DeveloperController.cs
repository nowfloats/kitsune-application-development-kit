using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Microsoft.AspNetCore.Cors;

namespace Kitsune.API2.Controllers
{
    [Route("api/Developer")]
    public class DeveloperController : Controller
    {
        //TODO:
        //Check if used anywhere, if not reomve else update the DraftthemeModel DB
        //Somebody updated the UserLevel too, check it out
        [HttpGet("v1/GetUserId")]
        public IActionResult GetUserId([FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new GetUserIdRequestModel { UserEmail = userEmail };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetUserIdFromUserEmail(requestModel));
            }
            catch (Exception ex)
            {
                return Ok(new GetUserIdResult { IsError = false, Message = "user not found" });
            }
        }

        [HttpGet("v1/Details")]
        public IActionResult Get([FromQuery]string developerId)
        {
            try
            {
                return Ok(MongoConnector.GetDeveloperDetailsFromId(developerId));
            }
            catch (Exception ex)
            {
                return NotFound("User not found");
            }
        }

        [HttpGet("v1/UserProfile")]
        public IActionResult GetUserProfile([FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new GetDeveloperProfileRequestModel { UserEmail = userEmail };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetDeveloperProfileDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("v1/PaymentDetails")]
        public IActionResult GetPaymentDetails([FromQuery]string useremail)
        {
            try
            {
                var requestModel = new GetUserPaymentStatsRequestModel { UserEmail = useremail };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetDeveloperPaymentTransactionLogs(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("v1/GetDebitDetails")]
        public IActionResult GetDebitDetails([FromQuery]string useremail, [FromQuery]string component, [FromQuery]DateTime monthAndYear)
        {
            try
            {
                var requestModel = new GetDeveloperDebitDetailsRequestModel { UserEmail = useremail, MonthAndYear = monthAndYear, Component = component };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetDeveloperDebitDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("v1/UpdateWallet")]
        public IActionResult UpdateUserWallet([FromQuery]string useremail, [FromQuery]double amount, [FromQuery]bool add = true)
        {
            try
            {
                var requestModel = new UpdateDeveloperWalletRequestModel { UserEmail = useremail, Add = add, Amount = amount };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateDeveloperWalletDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("v1/UpdateuserDetails")]
        public IActionResult UpdateUserDetails([FromBody]UpdateDeveloperDetailsRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateDeveloperDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("v1/CreateUser")]
        public IActionResult CreateUser([FromBody]CreateDeveloperProfileRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateDeveloperProfile(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("v1/Login")]
        public IActionResult DeveloperLogin([FromQuery]string username, [FromQuery]string password)
        {
            try
            {
                if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return BadRequest("Username or password can not be empty");
                }
                var result = MongoConnector.DeveloperLogin(username, password);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost("v1/ResetPassword")]
        public IActionResult DeveloperResetPassword([FromQuery]string username, [FromQuery]string oldpassword, [FromQuery]string newpassword)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(oldpassword) || string.IsNullOrEmpty(newpassword))
                {
                    return BadRequest("Invalid request parameter");
                }
                var result = MongoConnector.DeveloperPasswordReset(username, oldpassword, newpassword);
                if (result)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest("Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet("v1/Invoice")]
        public IActionResult GetInvoice([FromQuery]string userEmail, [FromQuery]int month = 0, [FromQuery]int year = 0)
        {
            try
            {
                var requestModel = new GetInvoiceRequestModel { UserEmail = userEmail, month = month, year = year };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetInvoice(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}