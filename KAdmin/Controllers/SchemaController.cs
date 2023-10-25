using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using KitsuneAdminDashboard.Web.Models;
using Microsoft.AspNetCore.Authorization;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Primitives;
using KitsuneAdminDashboard.Web.Utils;
using Microsoft.AspNetCore.Cors;

namespace KitsuneAdminDashboard.Web.Controllers
{
    [ConsoleModeFilter]
    [Route("k-admin/ManageWebsiteContent")]
    public class SchemaController : BaseController
    {
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

        #region Schema Operations

        [Route("GetLanguageSchemaById")]
        [Authorize, HttpPost]
        public IActionResult GetLanguageSchemaById()
        {
            try
            {
                var schemaId = User.Claims.FirstOrDefault(x => x.Type == "SchemaId");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");

                var fpDataRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.SchemaEndpoints.GetLanguageSchemaByIdEndpoint, Constants.KitsuneServerUrl, schemaId.Value)));
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
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

            return null;
        }

        [Route("GetDataForSchema")]
        [Authorize, HttpPost]
        public IActionResult GetDataForSchema([FromQuery]string website)
        {
            try
            {
                var entityName = User.Claims.FirstOrDefault(x => x.Type == "EntityName");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                var fpDataRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.SchemaEndpoints.GetDataForSchemaEndpoint, Constants.KitsuneServerUrl, entityName.Value, websiteId.Value)));
                fpDataRequest.Method = "GET";
                fpDataRequest.ContentType = "application/json";
                fpDataRequest.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var ws = fpDataRequest.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output["Data"]);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

            return null;
        }

        [Route("AddDataForSchema")]
        [Authorize, HttpPost]
        public IActionResult AddDataForSchema([FromBody]dynamic data)
        {
            try
            {
                var entityName = User.Claims.FirstOrDefault(x => x.Type == "EntityName");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");

                if (data == null)
                {
                    throw new ArgumentNullException("AddDataForSchema");
                }

                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                dataObject["WebsiteId"] = websiteId.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.SchemaEndpoints.AddDataForSchemaEndpoint, Constants.KitsuneServerUrl, entityName.Value)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                //var obj = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(data.ToString());
                //var dataObj = (Newtonsoft.Json.Linq.JObject)obj.Property("Data").Value;

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(dataObject);
                    streamWriter.Write(jsonData.ToString());
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                return Content(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [Route("UpdateDataForSchema")]
        [Authorize, HttpPost]
        public IActionResult UpdateDataForSchema([FromBody]dynamic data)
        {
            try
            {
                var entityName = User.Claims.FirstOrDefault(x => x.Type == "EntityName");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                // var websiteId = User.Claims.FirstOrDefault(x => x.Type == "KitsuneUrl");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                if (data == null)
                {
                    throw new ArgumentNullException("UpdateDataForSchema");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                dataObject["WebsiteId"] = websiteId.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.SchemaEndpoints.UpdateDataForSchemaEndpoint, Constants.KitsuneServerUrl, entityName.Value)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(dataObject);
                    streamWriter.Write(jsonData.ToString());
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();

                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

            return null;
        }

        [Route("DeleteDataForSchema")]
        [Authorize, HttpPost]
        public IActionResult DeleteDataForSchema([FromBody]dynamic data)
        {
            try
            {
                var entityName = User.Claims.FirstOrDefault(x => x.Type == "EntityName");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                // var websiteId = User.Claims.FirstOrDefault(x => x.Type == "KitsuneUrl");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                if (data == null)
                {
                    throw new ArgumentNullException("DeleteDataForSchema");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                dataObject["WebsiteId"] = websiteId.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.SchemaEndpoints.DeleteDataForSchemaEndpoint, Constants.KitsuneServerUrl, entityName.Value)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(dataObject);
                    streamWriter.Write(jsonData.ToString());
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return new JsonResult(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [Route("GetDataWithReferenceId")]
        [Authorize, HttpPost]
        public IActionResult GetDataWithReferenceId([FromBody]dynamic data)
        {
            try
            {
                var entityName = User.Claims.FirstOrDefault(x => x.Type == "EntityName");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                // var websiteId = User.Claims.FirstOrDefault(x => x.Type == "KitsuneUrl");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                if (data == null)
                {
                    throw new ArgumentNullException("GetDataWithReferenceId");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());

                var fpDataRequest = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(
                    Constants.SchemaEndpoints.GetDataForSchemaWithReferenceId, 
                    Constants.KitsuneServerUrl, entityName.Value, websiteId.Value,
                    dataObject["_parentClassName"], dataObject["_propertyName"], dataObject["_parentClassId"])));

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
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

            return null;
        }

        [Route("SaveUploadedFile")]
        [Authorize, HttpPost]
        public IActionResult SaveUploadedFile() {
            try
            {
                var file = Request.Form.Files[0];

                StringValues isFroala = "";
                var isUploadedFromFroalaPrameter = Request.Form.TryGetValue("froala", out isFroala);

                var entityName = User.Claims.FirstOrDefault(x => x.Type == "EntityName");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                var url = String.Format(Constants.SchemaEndpoints.UploadFileForWebsiteEndpoint, Constants.KitsuneServerUrl, websiteId.Value, file.FileName);
              
                HttpContent fileStreamContent = new StreamContent(file.OpenReadStream());
                try
                {
                    var fileLink = "";
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", authId.Value);
                        using (var formData = new MultipartFormDataContent())
                        {
                            formData.Add(fileStreamContent, "file", file.FileName);
                            var response = client.PostAsync(url, formData).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                fileLink = response.Content.ReadAsStringAsync().Result;
                            }

                            if (isUploadedFromFroalaPrameter && response.IsSuccessStatusCode)
                            {
                                return new JsonResult(new {
                                    link = fileLink.Trim(new char[] { '"' })
                                });
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
            catch(Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [AllowCORSFilter]
        [Route("SaveUploadedFileV2")]
        [HttpOptions]
        public IActionResult SaveUploadedFileUnAuthorizedOptions()
        {
            return Ok();
        }

        [AllowCORSFilter]
        [Route("SaveUploadedFileV2")]
        [HttpPost]
        public IActionResult SaveUploadedFileUnAuthorized()
        {
            try
            {
                var file = Request.Form.Files[0];
                
                StringValues isFroala = "";
                StringValues developerId = "";
                StringValues websiteId = "";
                var isUploadedFromFroalaPrameter = Request.Form.TryGetValue("froala", out isFroala);
                Request.Headers.TryGetValue("WebsiteId", out websiteId);
                Request.Headers.TryGetValue("DeveloperId", out developerId);

                var url = String.Format(Constants.SchemaEndpoints.UploadFileForWebsiteEndpoint, Constants.KitsuneServerUrl, 
                    Convert.ToString(websiteId), file.FileName);

                HttpContent fileStreamContent = new StreamContent(file.OpenReadStream());
                try
                {
                    var fileLink = "";
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", Convert.ToString(developerId));
                        using (var formData = new MultipartFormDataContent())
                        {
                            formData.Add(fileStreamContent, "file", file.FileName);
                            var response = client.PostAsync(url, formData).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                fileLink = response.Content.ReadAsStringAsync().Result;
                            }

                            if (isUploadedFromFroalaPrameter && response.IsSuccessStatusCode)
                            {
                                return new JsonResult(new
                                {
                                    link = fileLink.Trim(new char[] { '"' })
                                });
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
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        /**
         * Gets the related classes in the schema which contains the respective classType
         * */
        [Route("GetClassesByClassType")]
        [Authorize, HttpPost]
        public IActionResult GetRelatedClassesByType([FromBody]dynamic data)
        {
            try
            {
                var schemaId = User.Claims.FirstOrDefault(x => x.Type == "SchemaId");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                if (data == null)
                {
                    throw new ArgumentNullException("GetRelatedClassesByType");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                String classType = dataObject["classType"];

                var request = (HttpWebRequest)WebRequest.Create(new Uri(
                    String.Format(Constants.SchemaEndpoints.GetClassesByClassType, Constants.KitsuneServerUrl, schemaId.Value, classType, websiteId.Value)));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var ws = request.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

            return null;
        }

        /**
         * Gets all the data for the give classType
         * */
        [Route("GetDataByClassType")]
        [Authorize, HttpPost]
        public IActionResult GetDataByClassType([FromBody]dynamic data)
        {
            try
            {
                var entityName = User.Claims.FirstOrDefault(x => x.Type == "EntityName");
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");
                
                if (data == null)
                {
                    throw new ArgumentNullException("GetDataByClassType");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                String classType = dataObject["classType"];

                var request = (HttpWebRequest)WebRequest.Create(new Uri(
                    String.Format(Constants.SchemaEndpoints.GetDataByClassName, Constants.KitsuneServerUrl, entityName.Value, websiteId.Value, classType)));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var ws = request.GetResponse();
                StreamReader sr = new StreamReader(ws.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }

            return null;
        }
        

        [Route("SendEmail")]
        [Authorize, HttpPost]
        public IActionResult SendEmail([FromBody]dynamic data)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                if (data == null)
                {
                    throw new ArgumentNullException("SendEmail");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                var clientId = dataObject["clientId"];
                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.InternalEndpoints.SendEmail, Constants.KitsuneServerUrl, clientId)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    //string jsonData = JsonConvert.SerializeObject(data);
                    streamWriter.Write(data.ToString());
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

        [Route("GetDeveloperDetails")]
        [Authorize, HttpPost]
        public IActionResult GetDevloperDetails([FromBody]dynamic data)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                if (data == null)
                {
                    throw new ArgumentNullException("GetDevloperDetails");
                }
                string developerID = JsonConvert.DeserializeObject(data.ToString())["developerId"];
                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.DeveloperEndPoints.GetDeveloperDetails, Constants.KitsuneServerUrl, developerID)));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<DeveloperDetails>(response);
                    return new JsonResult(output.Email);
                }
                return null;
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }
        #endregion

        [Route("GetWebsiteDetails")]
        [Authorize, HttpPost]
        public IActionResult GetWebsiteDetails([FromBody]dynamic data)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                if (data == null)
                {
                    throw new ArgumentNullException("GetWebsiteDetails");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                var websiteId = dataObject["websiteId"];
                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.CustomerEndpoints.GetWebsiteDetails, 
                    Constants.KitsuneServerUrl, websiteId.Value, Constants.KadminClientId)));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<WebsiteDetails>(response);
                    string clientId = (output.ClientId != null && output.ClientId.Length > 0) ? output.ClientId : Constants.KadminClientId;
                    return new JsonResult(clientId);
                }
                return null;
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [Route("IsCallTrackerEnabled")]
        [Authorize, HttpPost]
        public IActionResult IsCallTrackerEnabled()
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");
                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.CustomerEndpoints.IsCallTrackerEnabled,
                    Constants.KitsuneServerUrl, websiteId.Value)));
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
                return null;
            }
            catch (Exception e)
            {
                return BadRequest(e.ToString());
            }
        }

        [Route("GetDataByProperty")]
        [Authorize, HttpPost]
        public IActionResult GetDataByProperty([FromBody] dynamic data)
        {
            var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
            var schemaId = User.Claims.FirstOrDefault(x => x.Type == "SchemaId");
            var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");
            try
            {
               

                if (data == null)
                {
                    throw new ArgumentNullException("GetDataByProperty");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                dataObject["WebsiteId"] = websiteId.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.SchemaEndpoints.GetDataByProperty,
                    Constants.KitsuneServerUrl, schemaId.Value)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(dataObject);
                    streamWriter.Write(jsonData.ToString());
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response))
                {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }

            }
            catch(Exception e)
            {
                return BadRequest(e.ToString()+ schemaId.Value + websiteId.Value+ authId.Value);
            }
            return null;
        }

        [Route("GetDataByPropertyBulk")]
        [Authorize, HttpPost]
        public IActionResult GetDataByPropertyBulk([FromBody] dynamic data)
        {
            try
            {
                var authId = User.Claims.FirstOrDefault(x => x.Type == "UserAuthId");
                var schemaId = User.Claims.FirstOrDefault(x => x.Type == "SchemaId");
                var websiteId = User.Claims.FirstOrDefault(x => x.Type == "CustomerId");

                if (data == null)
                {
                    throw new ArgumentNullException("GetDataByPropertyBulk");
                }
                var dataObject = JsonConvert.DeserializeObject(data.ToString());
                dataObject["WebsiteId"] = websiteId.Value;

                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.SchemaEndpoints.GetDataByPropertyBulk,
                    Constants.KitsuneServerUrl, schemaId.Value)));
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add(HttpRequestHeader.Authorization, authId.Value);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(dataObject);
                    streamWriter.Write(jsonData.ToString());
                }

                var httpResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(httpResponse.GetResponseStream());
                var response = sr.ReadToEnd().ToString();
                if (!String.IsNullOrEmpty(response)) {
                    var output = JsonConvert.DeserializeObject<dynamic>(response);
                    return new JsonResult(output);
                }
               
            }
            catch(Exception e)
            {
                return BadRequest(e.ToString());
            }

            return null;
        }
    }
}
