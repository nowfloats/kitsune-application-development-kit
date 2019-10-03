using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Primitives;
using Kitsune.API2.DataHandlers.MySQL;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace Kitsune.API2.Controllers
{
    /// <summary>
    /// API's related to analytics shown to customer
    /// </summary>
    [Produces("application/json")]
    [Route("api/WebAnalytics")]
    public class WebAnalyticsController : Controller
    {
        [HttpGet("v1/GetVisitors/{filterType}")]
        public IActionResult GetVisitors([FromRoute] VistorsFilterType filterType, [FromQuery]string website, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            try
            {
                #region Auth request
                if (Request.Headers.TryGetValue("Authorization", out StringValues headerValues))
                {
                    // TODO: Validate if valid auth header
                    headerValues.FirstOrDefault();
                }
                else
                {
                    return BadRequest("Not Authorized");
                }
                #endregion

                if (String.IsNullOrEmpty(website))
                    return BadRequest("Param website invalid");

                if (fromDate == null)
                    fromDate = Kitsune.API2.EnvConstants.Constants.epoch;

                if (toDate == null)
                    toDate = DateTime.UtcNow;

                website = website.Trim().ToLower();

                return Json(new MySQLConnector().GetVistorsForSite(website, filterType, fromDate, toDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("v1/GetAllRequestsPerDayByUserId")]
        public IActionResult GetAllRequestsPerDayByUserId([FromQuery]string developerId, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            try
            {
                string UserId = string.Empty;
                #region Auth request
                if (Request.Headers.TryGetValue("Authorization", out StringValues headerValues))
                {
                    UserId = headerValues.FirstOrDefault();
                }
                else
                {
                    return BadRequest("Not Authorized");
                }
                #endregion

                if (String.IsNullOrEmpty(developerId))
                    return BadRequest("Incorrect developerId");

                var userId = MongoConnector.GetUserIdFromUserEmail(new GetUserIdRequestModel { UserEmail = developerId });

                if (userId.IsError)
                    return BadRequest(userId.Message);

                if (fromDate == null)
                    fromDate = Kitsune.API2.EnvConstants.Constants.epoch;

                if (toDate == null)
                    toDate = DateTime.UtcNow;

                var projects = MongoConnector.GetAllProjectsPerUser(developerId);
                var websites = MongoConnector.GetAllCustomerIdsPerDeveloperId(userId.Id);

                return Json(new MySQLConnector().GetAllRequestsPerDayByUserId(projects, websites, fromDate, toDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("v1/GetStoragePerDayByUserId")]
        public IActionResult GetStoragePerDayByUserId([FromQuery]string developerId, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            try
            {
                string UserId = string.Empty;
                #region Auth request
                if (Request.Headers.TryGetValue("Authorization", out StringValues headerValues))
                {
                    UserId = headerValues.FirstOrDefault();
                }
                else
                {
                    return BadRequest("Not Authorized");
                }
                #endregion

                if (String.IsNullOrEmpty(developerId))
                    return BadRequest("Incorrect developerId");

                var userId = MongoConnector.GetUserIdFromUserEmail(new GetUserIdRequestModel { UserEmail = developerId });

                if (userId.IsError)
                    return BadRequest(userId.Message);

                if (fromDate == null)
                    fromDate = Kitsune.API2.EnvConstants.Constants.epoch;

                if (toDate == null)
                    toDate = DateTime.UtcNow;

                var projectIds = MongoConnector.GetAllProjectsPerUser(developerId);
                var websiteIds = MongoConnector.GetAllCustomerIdsPerDeveloperId(userId.Id);

                return Json(new MySQLConnector().GetStoragePerDay(projectIds, websiteIds, fromDate, toDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("v1/GetLowWalletBalanceStatus")]
        public IActionResult GetLowWalletBalanceStatus([FromQuery]string developerId)
        {
            try
            {
                string UserId = string.Empty;
                #region Auth request
                if (Request.Headers.TryGetValue("Authorization", out StringValues headerValues))
                {
                    UserId = headerValues.FirstOrDefault();
                }
                else
                {
                    return BadRequest("Not Authorized");
                }
                #endregion

                if (String.IsNullOrEmpty(developerId))
                    return BadRequest("Incorrect developerId");

                double amount = MongoConnector.GetAmountToAddBeforeNextBillingCycleForUser(developerId);
                DateTime? balanceWentZeroDate = MongoConnector.GetBalanceWentZeroDateForUser(developerId);

                Dictionary<string, Object> result = new Dictionary<string, Object> {
                    {"amount_to_add", amount },
                    {"balanceWentZeroDate", balanceWentZeroDate },
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("v1/GetRequestsByDevices")]
        public IActionResult GetRequestsByDevices([FromQuery] string website, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            try
            {
                #region Auth request
                if (Request.Headers.TryGetValue("Authorization", out StringValues headerValues))
                {
                    // TODO: Validate if valid auth header
                    headerValues.FirstOrDefault();
                }
                else
                {
                    return BadRequest("Not Authorized");
                }
                #endregion

                if (String.IsNullOrEmpty(website))
                    return BadRequest("Param website invalid");

                if (fromDate == null)
                    fromDate = Kitsune.API2.EnvConstants.Constants.epoch;

                if (toDate == null)
                    toDate = DateTime.UtcNow;

                website = website.Trim().ToLower();

                return Json(new MySQLConnector().GetRequestsByDevices(website, fromDate, toDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("v1/GetRequestsByBrowsers")]
        public IActionResult GetRequestsByBrowsers([FromQuery] string website, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            try
            {
                #region Auth request
                if (Request.Headers.TryGetValue("Authorization", out StringValues headerValues))
                {
                    // TODO: Validate if valid auth header
                    headerValues.FirstOrDefault();
                }
                else
                {
                    return BadRequest("Not Authorized");
                }
                #endregion

                if (String.IsNullOrEmpty(website))
                    return BadRequest("Param website invalid");

                if (fromDate == null)
                    fromDate = Kitsune.API2.EnvConstants.Constants.epoch;

                if (toDate == null)
                    toDate = DateTime.UtcNow;

                website = website.Trim().ToLower();

                return Json(new MySQLConnector().GetRequestsByBrowser(website, fromDate, toDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("v1/GetTrafficSources")]
        public IActionResult GetTrafficSources([FromQuery] string website, [FromQuery]DateTime? fromDate, [FromQuery]DateTime? toDate)
        {
            try
            {
                #region Auth request
                if (Request.Headers.TryGetValue("Authorization", out StringValues headerValues))
                {
                    // TODO: Validate if valid auth header
                    headerValues.FirstOrDefault();
                }
                else
                {
                    return BadRequest("Not Authorized");
                }
                #endregion

                if (String.IsNullOrEmpty(website))
                    return BadRequest("Param website invalid");

                if (fromDate == null)
                    fromDate = Kitsune.API2.EnvConstants.Constants.epoch;

                if (toDate == null)
                    toDate = DateTime.UtcNow;

                website = website.Trim().ToLower();

                return Json(new MySQLConnector().GetTrafficSources(website, fromDate, toDate));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region GWT - Search Keywords

        [HttpGet("GetSearchAnalyticsSummary")]
		public IActionResult GetSearchAnalyticsSummary([FromQuery]string websiteId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(websiteId))
					return BadRequest();

                var searchAnalyticsDict = MySQLConnector.GetSearchAnalyticsSummary(new string[] { websiteId });
                if (searchAnalyticsDict?.ContainsKey(websiteId) == true)
                {
                    if (searchAnalyticsDict[websiteId] != null)
                        return Ok(searchAnalyticsDict[websiteId]);
                }
			}
			catch (Exception e)
			{
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok(null);
        }

		[HttpGet("GetYearlySearchAnalytics")]
		public IActionResult GetYearlySearchAnalytics([FromQuery]string websiteId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(websiteId))
					return BadRequest();

                var searchAnalyticsDict = MySQLConnector.GetYearlySearchAnalytics(new string[] { websiteId });
                if (searchAnalyticsDict?.ContainsKey(websiteId) == true)
                {
                    if (searchAnalyticsDict[websiteId]?.Count > 0)
                        return Ok(searchAnalyticsDict[websiteId]);
                }
			}
			catch (Exception e)
			{
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok(null);
        }

		[HttpGet("GetMonthlySearchAnalytics")]
		public IActionResult GetMonthlySearchAnalytics([FromQuery]string websiteId, [FromQuery]int year)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(websiteId))
					return BadRequest();

				DateTime dt;
				try
				{
					dt = new DateTime(year, 1, 1);
				}
				catch (Exception)
				{
					return BadRequest();
				}

                var searchAnalyticsDict = MySQLConnector.GetMonthlySearchAnalytics(new string[] { websiteId }, dt.Year);
                if (searchAnalyticsDict?.ContainsKey(websiteId) == true)
                {
                    if (searchAnalyticsDict[websiteId]?.Count > 0)
                        return Ok(searchAnalyticsDict[websiteId]);
                }
			}
			catch (Exception e)
			{
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok(null);
        }

		[HttpGet("GetDailySearchAnalytics")]
		public IActionResult GetDailySearchAnalytics([FromQuery]string websiteId, [FromQuery]int year, [FromQuery]int month)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(websiteId))
					return BadRequest();
				DateTime dt;
				try
				{
					dt = new DateTime(year, month, 1);
				}
				catch (Exception)
				{
					return BadRequest();
				}

                var searchAnalyticsDict = MySQLConnector.GetDailySearchAnalytics(new string[] { websiteId }, dt.Year, dt.Month);
                if (searchAnalyticsDict?.ContainsKey(websiteId) == true)
                {
                    if (searchAnalyticsDict[websiteId]?.Count > 0)
                        return Ok(searchAnalyticsDict[websiteId]);
                }
			}
			catch (Exception e)
			{
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok(null);
        }

		[HttpGet("GetDetailedSearchAnalyticsForDate")]
		public IActionResult GetDetailedSearchAnalyticsForDate([FromQuery]string websiteId, [FromQuery]int year, [FromQuery]int month, [FromQuery]int day)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(websiteId))
					return BadRequest();
				DateTime dt;
				try
				{
					dt = new DateTime(year, month, day);
				}
				catch (Exception)
				{
					return BadRequest();
				}

				List<DetailedSearchAnalytics> detailedSearchAnalytics = MySQLConnector.GetDetailedSearchAnalytics(websiteId, dt, dt.AddDays(1));
				if (detailedSearchAnalytics != null)
					return Ok(detailedSearchAnalytics);
			}
			catch (Exception e)
			{
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok(null);
        }

		[HttpGet("GetDetailedSearchAnalyticsForDateRange")]
		public IActionResult GetDetailedSearchAnalyticsForDateRange([FromQuery]string websiteId, [FromQuery]DateTime startDate, [FromQuery]DateTime endDate)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(websiteId))
					return BadRequest();

				List<DetailedSearchAnalytics> detailedSearchAnalytics = MySQLConnector.GetDetailedSearchAnalytics(websiteId, startDate, endDate);
				if (detailedSearchAnalytics != null)
					return Ok(detailedSearchAnalytics);
			}
			catch (Exception e)
			{
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
            return Ok(null);
        }

		[HttpGet("SubmitWebsiteOwnerFeedback")]
		public IActionResult SubmitWebsiteOwnerFeedback([FromQuery]string Id, [FromQuery]string websiteId, [FromQuery]SearchQueryFeedbackEnum feedback)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(websiteId))
					return BadRequest();

				return Ok(MySQLConnector.SubmitFeedbackForSearchAnalytics(Id, websiteId, feedback.ToString()));
			}
			catch (Exception e)
			{
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
		}

        [HttpPost("v1/GetSearchAnalytics")]
        public IActionResult GetDailySearchAnalytics([FromBody]GWTSearchAnalyticsRequestModel requestModel)
        {
            try
            {
                if (requestModel == null ||  requestModel.WebsiteIds == null || requestModel.WebsiteIds?.Count() == 0 || requestModel.Offset < 0)
                    return BadRequest();

                if (requestModel.Limit < 10)
                    requestModel.Limit = 10;

                return Ok(MySQLConnector.GetSearchAnalytics(requestModel.WebsiteIds, requestModel.Limit, requestModel.Offset));
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e);
            }
        }

        #endregion
    }
}
