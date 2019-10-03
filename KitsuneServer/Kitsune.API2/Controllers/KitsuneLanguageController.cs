using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.Utils;
using Kitsune.API2.Validators;
using Kitsune.BasePlugin.Utils;
using Kitsune.Language.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Kitsune.API2.Controllers
{
    [Produces("application/json")]
    [Route("Language")]
    public class KitsuneLanguageController : Controller
    {
        [HttpPost]
        [Route("v1/CreateOrUpdateLanguageEntity")]
        [Route("v1/CreateOrUpdateSchema")]
        public IActionResult CreateOrUpdateLanguageEntity([FromBody]CreateOrUpdateLanguageEntityRequestModel requestModel)
        {
            try
            {
                requestModel.UserId = Request.Headers.ContainsKey("Authorization") ? Request.Headers["Authorization"].ToString() : requestModel.UserId;
                if (!string.IsNullOrEmpty(requestModel.SchemaId))
                    requestModel.LanguageId = requestModel.SchemaId;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateOrUpdateLanguageEntity(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/CreateLanguage")]
        public IActionResult CreateLanguage([FromBody]dynamic requestModel)
        {
            try
            {

                var userId = requestModel["UserId"] != null ? requestModel["UserId"].Value.ToString() : null;
                if (Request.Headers.ContainsKey("Authorization"))
                    userId = Request.Headers.ContainsKey("Authorization") ? Request.Headers["Authorization"].ToString() : userId;
                var jsonObject = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)requestModel).Last).Value;
                var entity = JsonParserHelper.ParseToKEntity(jsonObject);
                var tempCommand = new CreateOrUpdateLanguageEntityRequestModel()
                {
                    UserId = userId,
                    Entity = entity
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateOrUpdateLanguageEntity(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpPost]
        [Route("v1/UpdateLanguage/{id}")]
        public IActionResult UpdateLanguage([FromBody]dynamic requestModel, [FromRoute]string id)
        {
            try
            {
                var userId = requestModel["UserId"].Value.ToString();
                var jsonObject = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)requestModel).Last).Value;
                var entity = JsonParserHelper.ParseToKEntity(jsonObject);
                var tempCommand = new CreateOrUpdateLanguageEntityRequestModel()
                {
                    Entity = entity,
                    UserId = userId,
                    LanguageId = id
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateOrUpdateLanguageEntity(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/CreateDatatype/{languageId}")]
        public IActionResult CreateDataTypeClass([FromRoute] string languageId, [FromBody]KClass dataClas)
        {
            try
            {
                var userId = string.Empty;
                if (Request.Headers.ContainsKey("Authorization"))
                    userId = Request.Headers.ContainsKey("Authorization") ? Request.Headers["Authorization"].ToString() : userId;

                var tempCommand = new CreateDatatypeEntityRequestModel()
                {
                    Datatype = dataClas,
                    UserId = userId,
                    LanguageId = languageId
                };

                // ClassType should be UserDefined for all user defined class
                tempCommand.Datatype.ClassType = KClassType.UserDefinedClass;

                List<System.ComponentModel.DataAnnotations.ValidationResult> validationResult = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (dataClas == null)
                    validationResult.Add(new System.ComponentModel.DataAnnotations.ValidationResult("DataClass can not be null"));
                else if (string.IsNullOrEmpty(dataClas.Name))
                    validationResult.Add(new System.ComponentModel.DataAnnotations.ValidationResult("Class name can not be null"));

                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateDataClass(new CreateDatatypeEntityRequestModel { Datatype = dataClas, LanguageId = languageId, UserId = userId }));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/AddProperty/{languageId}/{classname}")]
        public IActionResult AddClassProperty([FromRoute] string languageId, [FromRoute] string classname, [FromBody]KProperty kProperty)
        {
            try
            {
                var userId = string.Empty;
                userId = AuthHelper.AuthorizeRequest(Request);

                var tempCommand = new AddPropertyRequestModel()
                {
                    Property = kProperty,
                    ClassName = classname,
                    UserId = userId,
                    LanguageId = languageId
                };

                List<System.ComponentModel.DataAnnotations.ValidationResult> validationResult = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (kProperty == null)
                    validationResult.Add(new System.ComponentModel.DataAnnotations.ValidationResult("Property can not be null"));
                else if (string.IsNullOrEmpty(kProperty.Name))
                    validationResult.Add(new System.ComponentModel.DataAnnotations.ValidationResult("Property name can not be null"));

                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.AddPropertyToClass(tempCommand));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/{languageid}")]
        public IActionResult GetLanguageEntity([FromRoute]string languageid)
        {
            try
            {
                string userId = AuthHelper.AuthorizeRequest(Request);

                var requestModel = new GetLanguageEntityRequestModel
                {
                    EntityId = languageid
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetLanguageEntity(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/{languageid}/{clientid}")]
        public IActionResult GetLanguageEntity([FromRoute]string languageid, [FromRoute]string clientid, bool propertygroup = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(clientid) && BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() == clientid.Trim().ToUpper())
                {
                    var requestModel = new GetLanguageEntityRequestModel
                    {
                        EntityId = languageid,
                    };
                    var validationResult = requestModel.Validate();
                    if (validationResult.Any())
                        return BadRequest(validationResult);
                    if (propertygroup)
                        return Ok(MongoConnector.GetLanguageEntityByPropertyGroup(requestModel));
                    else
                        return Ok(MongoConnector.GetLanguageEntity(requestModel));
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("v1/{languageid}/GetType")]
        public IActionResult GetLanguageEntityForType([FromRoute]string languageid, [FromQuery]string className, [FromQuery]string websiteId)
        {
            try
            {
                string UserId = AuthHelper.AuthorizeRequest(Request);

                var requestModel = new GetSimilarPropertiesInLanguageEntityRequestModel
                {
                    EntityId = languageid,
                    ClassName = className,
                    WebsiteId = websiteId
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetSimilarPropertiesInLanguageEntity(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("v2/{languageid}/GetType")]
        public IActionResult GetLanguageEntityForTypeBySegment([FromRoute]string languageid, [FromQuery]string className, [FromQuery]string websiteId)
        {
            try
            {
                string UserId = AuthHelper.AuthorizeRequest(Request);

                var requestModel = new GetSimilarPropertiesInLanguageEntityRequestModel
                {
                    EntityId = languageid,
                    ClassName = className,
                    WebsiteId = websiteId
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetSimilarPropertyPathSegments(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("v1/get-schemas")]
        public IActionResult GetLanguageSchemas([FromQuery]string userid, [FromQuery]string projectid)
        {
            try
            {
                var requestModel = new GetLanguageSchemaRequestModel
                {
                    UserId = userid,
                    ProjectId = projectid
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetLanguageSchemaForUser(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("v1/get-defaultclass")]
        public IActionResult GetLanguageDefaults([FromQuery]string userid)
        {
            try
            {
                var requestModel = new GetLanguageSchemaRequestModel
                {
                    UserId = userid,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetDataTypeClasses());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/{schemaname}/add-data")]
        [Route("v1/{schemaid}/add-data/{clientid}")]
        public IActionResult AddOrUpdateWebsite([FromRoute]string schemaname, [FromBody]AddOrUpdateWebsiteRequestModel requestModel, [FromRoute]string clientid = null, [FromRoute]string schemaid = null)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);

                if (string.IsNullOrEmpty(userId) && (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper()))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }

                requestModel.UserId = userId;
                requestModel.SchemaId = schemaid;
                requestModel.SchemaName = schemaname;

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.AddDataForWebsite(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public IActionResult AddOrUpdateWebsite([FromRoute]string schemaid, [FromBody]AddOrUpdateWebsiteRequestModel requestModel)
        {
            try
            {
                requestModel.SchemaId = schemaid;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.AddDataForWebsite(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Get website data based on schema name and websiteid
        /// Get website data based on websiteid only if called form clientid
        /// </summary>
        /// <param name="website"></param>
        /// <param name="id"></param>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="include"></param>
        /// <param name="clientid"></param>
        /// <param name="schemaid"></param>
        /// <param name="schemaname"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/{schemaname}/get-data")]
        [Route("v1/get-data/{clientid}")]
        public IActionResult GetWebsiteData([FromQuery]string website, [FromQuery]string id = null,
            [FromQuery]int skip = 0, [FromQuery]int limit = 100, [FromQuery]string query = null, [FromQuery]string sort = null,
            [FromQuery]string include = null, [FromRoute]string clientid = null, [FromRoute]string schemaid = null, [FromRoute]string schemaname = null)
        {
            try
            {
                var requestModel = new GetWebsiteDataRequestModel
                {
                    Limit = limit,
                    Query = query,
                    SchemaName = schemaname,
                    Skip = skip,
                    Sort = sort,
                    WebsiteId = website,
                    Include = include,
                    ClientId = clientid,
                    SchemaId = schemaid
                };

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId) && (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper()))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                requestModel.UserId = userId;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);
                return Ok(MongoConnector.GetWebsiteData(requestModel));

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/{schemaname}/get-data-by-type")]
        public IActionResult GetWebsiteDataByType([FromRoute]string schemaname, [FromQuery]string website, [FromQuery]string className)
        {
            try
            {
                var requestModel = new GetWebsiteDataByTypeRequestModel
                {
                    SchemaName = schemaname,
                    WebsiteId = website,
                    ClassName = className,
                };

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                else
                    requestModel.UserId = userId;

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);
                return Ok(MongoConnector.GetWebsiteDataByType(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/{schemaid}/get-data-by-property")]
        [Route("v1/{schemaid}/get-data-by-property/{clientid}")]
        public IActionResult GetWebsiteDataByPropertyPath([FromRoute]string schemaid, [FromBody]GetWebsiteDataByPropertyPath requestModel, [FromRoute]string clientid = null)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId) && (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper()))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                requestModel.SchemaId = schemaid;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);
                var result = MongoConnector.GetWebsiteDataByPropertyPath(requestModel);
                return new CommonActionResult(CommonAPIResponse.OK(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("v2/{schemaid}/get-data-by-property")]
        [Route("v2/{schemaid}/get-data-by-property/{clientid}")]
        public IActionResult GetWebsiteDataByPropertyPathV2([FromRoute]string schemaid, [FromQuery]string requestObject, [FromRoute]string clientid = null)
        {
            try
            {

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId) && (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper()))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                var requestModel = JsonConvert.DeserializeObject<GetWebsiteDataByPropertyPath>(requestObject);
                requestModel.SchemaId = schemaid;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);
                var result = MongoConnector.GetWebsiteDataByPropertyPath(requestModel);
                return new CommonActionResult(CommonAPIResponse.OK(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost]
        [Route("v1/{schemaid}/get-bulk-data-by-property")]
        [Route("v1/{schemaid}/get-bulk-data-by-property/{clientid}")]
        public IActionResult GetWebsiteDataByPropertyPath([FromRoute]string schemaid, [FromBody]GetWebsiteDataByPropertyPathV2 requestModel, [FromRoute]string clientid = null)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId) && (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper()))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                requestModel.SchemaId = schemaid;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);
                var result = new List<GetWebsiteDataByPropertyResponseModel>();
                var segmentRequet = new GetWebsiteDataByPropertyPath() { SchemaId = requestModel.SchemaId, UserEmail = requestModel.UserEmail, WebsiteId = requestModel.WebsiteId };
                foreach (var segments in requestModel.BulkPropertySegments)
                {
                    segmentRequet.PropertySegments = segments;
                    result.Add(MongoConnector.GetWebsiteDataByPropertyPath(segmentRequet));
                }
                return new CommonActionResult(CommonAPIResponse.OK(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [Route("v1/get-schema")]
        public IActionResult GetPageSchema([FromQuery]string websiteId, [FromQuery]string userId, [FromQuery]string filePath)
        {
            try
            {
                var refObject = MongoConnector.Deserialize(filePath);

                return Ok(refObject);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/{schemaname}/get-file-data")]
        public IActionResult GetPageData([FromRoute]string schemaname, [FromQuery]string websiteId, [FromQuery]string projectId, [FromQuery]string filePath, [FromQuery]string currentPageNumber)
        {
            try
            {
                var requestModel = new GetPageDataRequest
                {
                    ProjectId = projectId,
                    SourcePath = filePath,
                    WebsiteId = websiteId,
                    SchemaName = schemaname,
                    CurrentPageNumber = currentPageNumber
                };

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                else
                    requestModel.UserId = userId;

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                var result = MongoConnector.GetBaseClassData(requestModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/{schemaname}/update-data")]
        public IActionResult UpdateWebsiteData([FromRoute]string schemaname, [FromBody]UpdateWebsiteDataRequestModel requestModel)
        {
            ///TODO : Chheck that mongodb driver support long for skip and limit
            // string userid = Request.Headers.Authorization.ToString();
            // command.UserId = userid;
            try
            {

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                else
                    requestModel.UserId = userId;

                requestModel.SchemaName = schemaname;
                // requestModel.SchemaName = schemaname.ToUpper();
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return new CommonActionResult(MongoConnector.UpdateWebsiteData(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpPost]
        [Route("v1/{schemaid}/update-data/{clientid}")]
        public IActionResult UpdateWebsiteDataById([FromRoute]string schemaid, [FromRoute]string clientid, [FromBody]UpdateWebsiteDataRequestModel requestModel)
        {
            ///TODO : Chheck that mongodb driver support long for skip and limit
            // string userid = Request.Headers.Authorization.ToString();
            // command.UserId = userid;
            try
            {

                if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }

                requestModel.SchemaId = schemaid;
                // requestModel.SchemaName = schemaname.ToUpper();
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return new CommonActionResult(MongoConnector.UpdateWebsiteData(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        [HttpPost]
        [Route("v1/{schemaname}/delete-data")]
        public IActionResult DeleteWebsiteData([FromRoute]string schemaname, [FromBody]DeleteDataObjectRequestModel requestModel)
        {
            try
            {

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                else
                    requestModel.UserId = userId;

                requestModel.SchemaName = schemaname;
                // requestModel.SchemaName = schemaname.ToUpper();
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.DeleteObject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost]
        [Route("v1/MapSchema")]
        public IActionResult MapWebsiteToWebAction([FromBody]MapSchemaToProjectRequestModel requestModel)
        {
            try
            {

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                else
                    requestModel.UserId = userId;

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.MapSchemaToProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/{projectid}/get-intellisense")]
        public IActionResult GetIntellisense([FromRoute]string projectid)
        {
            try
            {
                var requestModel = new GetIntellisenseRequestModel
                {
                    ProjectId = projectid
                };

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                else
                    requestModel.UserId = userId;

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetIntellisense(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/VersionLanguageSchema")]
        public IActionResult VersionLanguageSchema(VersionLanguageSchema requestModel)
        {
            try
            {

                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                else
                    requestModel.UserId = userId;

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.VersionLanguageSchema(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("v1/{websiteid}/upload-file")]
        [ValidateMimeMultipartContentFilter]
        public IActionResult UploadFiles(string websiteid, [FromQuery]string assetFileName, IFormFile file)
        {
            Stream stream = Request.Body;
            var byteArrayStream = Kitsune.API2.EnvConstants.Constants.ReadInputStream(file);

            if (string.IsNullOrEmpty(websiteid))
                return BadRequest("Website id can not be empty");

            if (string.IsNullOrEmpty(assetFileName))
                return BadRequest("File name can not be empty");

            if (byteArrayStream == null || byteArrayStream.Length == 0)
                return BadRequest("File can not be empty");

            websiteid = websiteid.Replace(" ", "");

            //TODO : Validate the websiteid against userid
            var userId = AuthHelper.AuthorizeRequest(Request);
            if (string.IsNullOrEmpty(userId))
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            else
            {
                var website = MongoConnector.GetKitsuneWebsiteDetails(websiteid);
                if (website != null && website.Response != null)
                {
                    if (userId != ((WebsiteDetailsResponseModel)website.Response).DeveloperId)
                        return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                else
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Website id invalid")));
            }


            var bucketName = "kitsune-website-files-cdn";
            var tempName = Kitsune.Helper.KitsuneCommonUtils.FormatResourceFileName(assetFileName).ToLower();
            var lastIndexOfDot = tempName.LastIndexOf('.');

            var fileName = string.Format("{0}-{1}{2}", lastIndexOfDot == -1 ? tempName : tempName.Substring(0, lastIndexOfDot), ObjectId.GenerateNewId().ToString(), lastIndexOfDot == -1 ? tempName : tempName.Substring(lastIndexOfDot));
            var result = AmazonS3Helper.SaveAssetsAndReturnObjectkey($"v1/{websiteid}/{fileName}", byteArrayStream, bucketName);
            if (!string.IsNullOrEmpty(result))
            {
                var domainUrl = EnvConstants.Constants.KitsuneFilesCDNUrl;
                return Ok(String.Format("{0}/{1}", domainUrl, result));
            }

            // do something witht his stream now

            return BadRequest("Invalid website id");
        }

        [HttpPost("v1/{websiteid}/delete-file")]
        public IActionResult DeleteFile(string websiteid, [FromQuery]string assetFileName)
        {

            if (string.IsNullOrEmpty(websiteid))
                return BadRequest("Website id can not be empty");

            if (string.IsNullOrEmpty(assetFileName))
                return BadRequest("File name can not be empty");

            var userId = AuthHelper.AuthorizeRequest(Request);
            if (string.IsNullOrEmpty(userId))
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            //TODO : Validate the userid with websiteid


            var bucketName = "kitsune-website-files-cdn";
            assetFileName = assetFileName.Trim();
            var result = AmazonS3Helper.DeleteAsset($"v1/{websiteid}/{assetFileName}", bucketName);
            if (result)
            {
                return Ok();
            }

            return BadRequest("Invalid website id");
        }

        [HttpPost("v1/{schemaname}/search/{searchtext}")]
        public IActionResult SearchData(string schemaname, string searchtext, [FromBody] SearchDataRequestModel requestModel, [FromQuery]int skip = 0, [FromQuery]int limit = 100)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());

                requestModel.UserId = userId;

                if (requestModel == null)
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Invalid request body")));
                if (string.IsNullOrEmpty(schemaname))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Schema name can not be empty")));

                if (string.IsNullOrEmpty(searchtext))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Search text can not be empty")));

                requestModel.SchemaName = schemaname;
                requestModel.SearchText = searchtext;
                requestModel.Skip = skip;
                requestModel.Limit = limit;
                var validationResult = requestModel.Validate();
                if (validationResult != null && validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.SearchData(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/{schemaname}/search/{searchtext}")]
        public IActionResult SearchGetData(string schemaname,
                                        string searchtext,
                                        [FromQuery]string websiteId,
                                        [FromQuery]string searchProperty,
                                        [FromQuery]string filter = null,
                                        [FromQuery]string include = null,
                                        [FromQuery]string sort = null,
                                        [FromQuery]int skip = 0,
                                        [FromQuery]int limit = 100)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());

                if (string.IsNullOrEmpty(schemaname))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Schema name can not be empty")));
                if (string.IsNullOrEmpty(searchtext))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Search text can not be empty")));
                if (string.IsNullOrEmpty(websiteId))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("WebsiteId can not be empty")));
                if (string.IsNullOrEmpty(searchProperty))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Search Property can not be empty")));

                SearchDataRequestModel requestModel = new SearchDataRequestModel
                {
                    Filter = filter,
                    Include = include?.Split(',').ToList(),
                    Sort = sort,
                    SearchProperty = searchProperty,
                    WebsiteId = websiteId,
                    UserId = userId,
                    SchemaName = schemaname.Trim(' ').ToUpper(),
                    SearchText = searchtext,
                    Skip = skip,
                    Limit = limit
                };
                var validationResult = requestModel.Validate();
                if (validationResult != null && validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.SearchData(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }
        [HttpGet("v1/get-object-link/{websiteid}/{objectid}/{clientid}")]
        public IActionResult GetObjectLink([FromRoute]string websiteid, [FromRoute]string objectid, [FromRoute]string clientid, [FromQuery]string classname, [FromQuery]string schemaid = null)
        {
            if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
            {
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            }
            if (string.IsNullOrEmpty(classname))
            {
                return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Class name can not be empty")));
            }
            try
            {
                return new CommonActionResult(MongoConnector.GenerateObjectUrl(websiteid, classname, objectid, schemaid));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/ksearch/{searchtext}")]
        public IActionResult KGlobalSearch([FromRoute]string searchtext, [FromQuery]string websiteid, [FromQuery]int skip = 0, [FromQuery]int limit = 100)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                GlobalSearchDataRequestModel requestModel = new GlobalSearchDataRequestModel();
                requestModel.UserId = userId;

                if (string.IsNullOrEmpty(searchtext))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Search text can not be empty")));
                else
                {
                    //Replace hyphen with space for the keywords
                    searchtext = searchtext.Replace('-', ' ');
                }

                requestModel.SearchText = searchtext;
                requestModel.Skip = skip;
                requestModel.Limit = limit;
                requestModel.WebsiteId = websiteid;
                var validationResult = requestModel.Validate();
                if (validationResult != null && validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.SearchGlobalData(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost("v1/{schemaname}/find")]
        public IActionResult FindData(string schemaname, [FromBody] SearchDataRequestModel requestModel, [FromQuery]int skip = 0, [FromQuery]int limit = 100)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                requestModel.UserId = userId;
                if (requestModel == null)
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Invalid request body")));
                if (string.IsNullOrEmpty(schemaname))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Schema name can not be empty")));

                if (string.IsNullOrEmpty(requestModel.Filter))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Filter can not be empty")));

                requestModel.SchemaName = schemaname;
                requestModel.Skip = skip;
                requestModel.Limit = limit;
                var validationResult = requestModel.Validate();
                if (validationResult != null && validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.SearchData(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }


        }

        [HttpGet("v1/get-storage-size")]
        public IActionResult GetStorageSize([FromQuery] string clientid, [FromQuery] string userid = null)
        {
            try
            {
                if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                return new CommonActionResult(MongoConnector.GetWebsiteDataStorageSize(userid));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }
    }
}