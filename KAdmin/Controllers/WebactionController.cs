using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KitsuneAdminDashboard.Web.Models;
using KitsuneAdminDashboard.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace KitsuneAdminDashboard.Web.Controllers
{
    [Route("k-admin/Inbox")]
    public class WebactionController : BaseController
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

        #region Web Actions Region

        [Route("GetWebActionsList")]
        [HttpPost, Authorize]
        public IActionResult GetWebActionsList()
        {
            try
            {
                var customerId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");

                var fpDataRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.WebactionsEndpoints.WebActionsListEndpoint, Constants.WebActionServerUrl)));
                fpDataRequest.Method = "GET";
                fpDataRequest.ContentType = "application/json";
                fpDataRequest.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var ws = fpDataRequest.GetResponse();
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

        // Get Specific Web Actions Data by web action Name
        [Route("GetWebActionsData")]
        [HttpPost, Authorize]
        public IActionResult GetWebActionsData(string webActionName)
        {
            try
            {
                if (String.IsNullOrEmpty(webActionName))
                {
                    return BadRequest("WebactionName can't be null or empty.");
                }

                var customerId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");

                var fpDataRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.WebactionsEndpoints.WebActionsDataEndpoint, Constants.WebActionServerUrl, webActionName, "query={WebsiteId:'" + customerId.Value + "'}")));
                fpDataRequest.Method = "GET";
                fpDataRequest.ContentType = "application/json";
                fpDataRequest.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var ws = fpDataRequest.GetResponse();
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

        // Add web action data
        [Route("AddWebActionsData")]
        [HttpPost, Authorize]
        public IActionResult AddWebActionsData([FromQuery]string webActionName, [FromBody]dynamic data)
        {
            try
            {
                if (String.IsNullOrEmpty(webActionName))
                {
                    throw new ArgumentNullException("webActionName");
                }

                if (data == null)
                {
                    throw new ArgumentNullException("webActionData");
                }

                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.WebactionsEndpoints.AddWebActionDataEndpoint, Constants.WebActionServerUrl, webActionName)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var obj = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(data.ToString());
                obj["WebsiteId"] = websiteId.Value;

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(obj);
                    streamWriter.Write(jsonData);
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return new JsonResult(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("UpdateWebActionsData")]
        [HttpPost, Authorize]
        public IActionResult UpdateWebActionsData([FromQuery]string name, [FromQuery]string objectid, [FromBody]dynamic data)
        {
            try
            {
                #region Arguments validation

                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException("webActionName");
                }

                if (String.IsNullOrEmpty(objectid))
                {
                    throw new ArgumentNullException("objectid");
                }

                if (data == null)
                {
                    throw new ArgumentNullException("webActionData");
                }

                #endregion

                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.WebactionsEndpoints.UpdateWebActionDataEndpoint, Constants.WebActionServerUrl, name, objectid)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var obj = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(data.ToString());

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(obj);
                    streamWriter.Write(jsonData);
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return new JsonResult(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Route("WebActionDataUpload")]
        [HttpPost, Authorize]
        public IActionResult WebActionDataUpload([FromQuery]string name)
        {
            try
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException("name");
                }

                if (Request.Form.Files.Count > 0)
                {
                    var inputFile = Request.Form.Files[0];
                    var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                    if (inputFile != null && inputFile.Length > 0)
                    {
                        var url = String.Format(Constants.WebactionsEndpoints.WebActionUploadEndpoint, Constants.WebActionServerUrl, name, inputFile.FileName);
                        HttpContent fileStreamContent = new StreamContent(inputFile.OpenReadStream());
                        var fileLink = "";
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("Authorization", authId.Value);
                            using (var formData = new MultipartFormDataContent())
                            {
                                formData.Add(fileStreamContent, "file", inputFile.FileName);
                                var response = client.PostAsync(url, formData).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    return new JsonResult(response.Content.ReadAsStringAsync().Result);
                                }
                                return new JsonResult(fileLink);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return BadRequest("error uploading file");
        }

        [AllowCORSFilter]
        [Route("WebActionDataUploadV2")]
        [HttpOptions]
        public IActionResult WebActionDataUploadUnAuthorizedOptions()
        {
            return Ok();
        }

        [AllowCORSFilter]
        [Route("WebActionDataUploadV2")]
        [HttpPost]
        public IActionResult WebActionDataUploadUnAuthorized([FromQuery]string name)
        {
            try
            {
                if (String.IsNullOrEmpty(name))
                {
                    throw new ArgumentNullException("name");
                }

                if (Request.Form.Files.Count > 0)
                {
                    var inputFile = Request.Form.Files[0];
                    StringValues developerId = "";
                    Request.Headers.TryGetValue("DeveloperId", out developerId);
                    if (inputFile != null && inputFile.Length > 0)
                    {
                        var url = String.Format(Constants.WebactionsEndpoints.WebActionUploadEndpoint, Constants.WebActionServerUrl, name, inputFile.FileName);
                        HttpContent fileStreamContent = new StreamContent(inputFile.OpenReadStream());
                        var fileLink = "";
                        using (var client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("Authorization", Convert.ToString(developerId));
                            using (var formData = new MultipartFormDataContent())
                            {
                                formData.Add(fileStreamContent, "file", inputFile.FileName);
                                var response = client.PostAsync(url, formData).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    return new JsonResult(response.Content.ReadAsStringAsync().Result);
                                }
                                return new JsonResult(fileLink);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return BadRequest("error uploading file");
        }

        [Route("UploadFileToSystemWebaction")]
        [HttpPost, Authorize]
        public IActionResult UploadFileToSystemWebaction([FromForm]ImageUpload data) {
            try
            {
                var requestUrl = String.Format(Constants.WebactionsEndpoints.UploadFile, Constants.WebActionServerUrl, data.WebactionName, data.FileName);
                HttpContent fileStreamContent = new StreamContent(data.File.OpenReadStream());

                var fileLink = "";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", data.AuthId);
                    using (var formData = new MultipartFormDataContent())
                    {
                        formData.Add(fileStreamContent, "file", data.FileName);
                        var response = client.PostAsync(requestUrl, formData).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            return new JsonResult(response.Content.ReadAsStringAsync().Result);
                        }
                        return new JsonResult(fileLink);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [Route("AddDataToSystemWebaction")]
        [HttpPost, Authorize]
        public IActionResult AddDataToSystemWebaction([FromBody]dynamic data)
        {
            try
            {
                if (data == null)
                {
                    throw new ArgumentNullException("AddDataToSystemWebaction");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                var webactionName = dataObject["webactionName"];
                var authId = dataObject["authId"];
                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.WebactionsEndpoints.AddWebActionDataEndpoint, Constants.WebActionServerUrl, webactionName.Value)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(dataObject["webactionData"]);
                    streamWriter.Write(jsonData);
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return new JsonResult(streamReader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [Route("GetWebActionDataCount")]
        [HttpPost, Authorize]
        public IActionResult GetWebActionDataCount([FromBody]dynamic data)
        {
            try
            {
                if (data == null)
                {
                    throw new ArgumentNullException("GetWebActionDataCount");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                var startDate = dataObject["startDate"];
                var endDate = dataObject["endDate"];
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId").Value;
                var type = dataObject["type"];
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.WebactionsEndpoints.GetWebActionDataCount,
                    Constants.WebActionServerUrl, authId, Constants.KadminClientId, startDate.Value, endDate.Value, type, websiteId.Value)));
                request.Method = "GET";
                request.ContentType = "application/json";

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (String.IsNullOrWhiteSpace(response))
                {
                    response = "{ 'Actions':[]}";
                }
                var output = JsonConvert.DeserializeObject<dynamic>(response);
                return new JsonResult(output);
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [Route("GetWebActionListWithConfig")]
        [HttpPost, Authorize]
        public IActionResult GetWebActionListWithConfig([FromBody] dynamic data)
        {
            try
            {
                var customerId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");

                var webactionRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.WebactionsEndpoints.GetWebActionsListWithConfig, Constants.WebActionServerUrl)));
                webactionRequest.Method = "GET";
                webactionRequest.ContentType = "application/json";
                webactionRequest.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var ws = webactionRequest.GetResponse();
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

        [Route("CreateOrUpdateWebaction")]
        [HttpPost, Authorize]
        public IActionResult CreateOrUpdateWebaction([FromBody]dynamic data)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.WebactionsEndpoints.CreateOrUpdateWebaction,
                    Constants.WebActionServerUrl)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    //string jsonData = JsonConvert.SerializeObject(data);
                    streamWriter.Write(data.ToString());
                }

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    return new JsonResult(response);
                }
            }
            catch(Exception e)
            {
                return BadRequest(e.ToString());
            }

            return null;
        }
        #endregion
    }
}
