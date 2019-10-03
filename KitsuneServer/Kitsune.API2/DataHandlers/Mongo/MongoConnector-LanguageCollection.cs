using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Utils;
using Kitsune.BasePlugin.Utils;
using Kitsune.Language.Helper;
//using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using Kitsune.Models.Project;
using Kitsune.Models.WebsiteModels;
using Kitsune.SyntaxParser;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using Kitsune.API.CQRS.Queries;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using WebActionPropertyType = Kitsune.Models.WebActionPropertyType;

namespace Kitsune.API2.DataHandlers.Mongo
{
    public static partial class MongoConnector
    {
        /// <summary>
        /// Create or update kitsune language
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static string CreateOrUpdateLanguageEntity(CreateOrUpdateLanguageEntityRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();


                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);

                UpdateLanguageDefaultProperties(requestModel.Entity, string.IsNullOrEmpty(requestModel.LanguageId));

                KLanguageModel existingEntity = null;
                if (string.IsNullOrEmpty(requestModel.LanguageId))
                {
                    existingEntity = LanguageCollection.Find(x => x.Entity.EntityName == requestModel.Entity.EntityName && x.UserId == requestModel.UserId).FirstOrDefault();
                    if (existingEntity != null)
                        throw new Exception("Schema with name : " + requestModel.Entity.EntityName + ", already exist");
                }
                else
                {
                    existingEntity = LanguageCollection.Find(x => x._id == requestModel.LanguageId).FirstOrDefault();
                    if (existingEntity == null)
                        throw new Exception("Schema with id : " + requestModel.LanguageId + ", does not exist");
                }

               


                #region Validate entity doesn't have cyclic reference
                if (requestModel.Entity.Classes != null && requestModel.Entity.Classes.Any())
                {
                    Stack<string> classHierarchy = new Stack<string>();
                    Dictionary<string, KClass> classDetails = new Dictionary<string, KClass>();
                    foreach (KClass kClass in requestModel.Entity.Classes)
                    {
                        classDetails.Add(kClass.Name.ToLower(), kClass);
                        if (kClass.ClassType == KClassType.BaseClass && kClass.ReferenceType != KReferenceType.Value)
                        {
                            throw new Exception("Reference type cannot be set for base class");
                        }
                    }
                    string baseClassName = requestModel.Entity.Classes.Where<KClass>(c => c.ClassType == KClassType.BaseClass).FirstOrDefault().Name;
                    classHierarchy.Push(baseClassName);
                    ValidateCyclicReference(classHierarchy, classDetails, baseClassName);
                    classDetails = null;
                }

                #endregion

                //get all collections
                if (!string.IsNullOrEmpty(requestModel.LanguageId))
                {
                    var filterDefinition = new FilterDefinitionBuilder<KLanguageModel>();
                    var fd = filterDefinition.Eq(q => q._id, requestModel.LanguageId);
                    var updateDefinition = new UpdateDefinitionBuilder<KLanguageModel>();

                    #region Prevent changing the entity name and base class name
                    requestModel.Entity.EntityName = existingEntity.Entity.EntityName;
                    if (requestModel.Entity?.Classes != null && existingEntity.Entity?.Classes != null)
                    {
                        var baseClass = requestModel.Entity.Classes.FirstOrDefault(x => x.ClassType == KClassType.BaseClass);
                        var entityBaseClass = existingEntity.Entity.Classes.FirstOrDefault(x => x.ClassType == KClassType.BaseClass);
                        if (baseClass != null)
                            baseClass.Name = entityBaseClass?.Name;
                    }
                    #endregion

                    var result = LanguageCollection.UpdateOne(fd, updateDefinition.Set(x => x.Entity, requestModel.Entity).Set(x => x.UpdatedOn, DateTime.UtcNow));

                    return requestModel.LanguageId;
                }
                else
                {
                    var basePlugin = BasePluginConfigGenerator.GetBasePlugin(requestModel.ClientId);

                    //update clientId if the id is not present
                    requestModel.ClientId = basePlugin.GetClientId();

                    var insertObject = new KLanguageModel
                    {
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                        Entity = requestModel.Entity,
                        UserId = requestModel.UserId,
                        ClientId = requestModel.ClientId
                    };


                    LanguageCollection.InsertOne(insertObject);
                    return insertObject._id;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Validate entity doesn't have cyclic reference of reference type classes.
        /// </summary>
        /// <param name="classHierarchy"></param>
        /// <param name="classDetails"></param>
        /// <param name="className"></param>
        private static void ValidateCyclicReference(Stack<string> classHierarchy, Dictionary<string, KClass> classDetails, string className)
        {
            foreach (KProperty kProperty in classDetails[className].PropertyList)
            {
                if (kProperty.Type == PropertyType.obj)
                {
                    string classType = kProperty.DataType.Name.ToLower();
                    if (classHierarchy.Contains(classType) && classDetails[classType].ReferenceType == KReferenceType.Reference)
                    {
                        throw new Exception("Reference type class in entity cannot have cyclic reference");
                    }
                    classHierarchy.Push(classType);
                    ValidateCyclicReference(classHierarchy, classDetails, classType);
                    classHierarchy.Pop();
                }
            }
        }

        /// <summary>
        /// Create new class for the language
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static string CreateDataClass(CreateDatatypeEntityRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();


                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                if (!string.IsNullOrEmpty(requestModel.UserId))
                {
                    var filterDefinition = new FilterDefinitionBuilder<KLanguageModel>();
                    var fd = filterDefinition.And(filterDefinition.Eq(q => q._id, requestModel.LanguageId), filterDefinition.Eq(q => q.UserId, requestModel.UserId));
                    var updateDefinition = new UpdateDefinitionBuilder<KLanguageModel>();
                    var result = LanguageCollection.Find(fd).FirstOrDefault();

                    // Add default class properties
                    KEntity entity = new KEntity()
                    {
                        Classes = new List<KClass> { requestModel.Datatype }
                    };
                    UpdateLanguageDefaultProperties(entity, false);

                    if (result != null)
                    {
                        if (result.Entity != null && result.Entity.Classes != null)
                        {
                            if (!result.Entity.Classes.Any(x => x.Name.ToLower() == requestModel.Datatype.Name.Trim().ToLower()))
                            {
                                result.Entity.Classes.Add(requestModel.Datatype);
                                var updateResult = LanguageCollection.UpdateOne(fd, new UpdateDefinitionBuilder<KLanguageModel>().Set(x => x.Entity, result.Entity));
                                if (updateResult.IsAcknowledged && updateResult.IsModifiedCountAvailable && updateResult.ModifiedCount == 1)
                                {
                                    return string.Format("Data class added.");
                                }
                                throw new Exception(string.Format("Something went wrong"));
                            }
                            else
                            {
                                throw new Exception(string.Format("Class with name : {0} already exist in the language.", requestModel.Datatype.Name));
                            }
                        }
                    }
                    else
                        throw new Exception("Language can not be found");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        /// <summary>
        /// Add new property to the existing langauge custom class
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static string AddPropertyToClass(AddPropertyRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();


                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                if (!string.IsNullOrEmpty(requestModel.UserId))
                {
                    var filterDefinition = new FilterDefinitionBuilder<KLanguageModel>();
                    var fd = filterDefinition.And(filterDefinition.Eq(q => q._id, requestModel.LanguageId), filterDefinition.Eq(q => q.UserId, requestModel.UserId));
                    var updateDefinition = new UpdateDefinitionBuilder<KLanguageModel>();
                    var result = LanguageCollection.Find(fd).FirstOrDefault();
                    if (result != null)
                    {
                        if (result.Entity != null && result.Entity.Classes != null)
                        {
                            if (result.Entity.Classes.Any(x => x.Name.ToLower() == requestModel.ClassName.Trim().ToLower()))
                            {
                                var tempClass = result.Entity.Classes.First(x => x.Name.ToLower() == requestModel.ClassName.Trim().ToLower());
                                if (!tempClass.PropertyList.Any(x => x.Name.ToLower() == requestModel.ClassName.ToLower()))
                                {
                                    tempClass.PropertyList.Add(requestModel.Property);
                                }
                                else
                                {
                                    throw new Exception(string.Format("Property with name : {0} already exist in the class.", requestModel.Property.Name));
                                }
                                var updateResult = LanguageCollection.UpdateOne(fd, new UpdateDefinitionBuilder<KLanguageModel>().Set(x => x.Entity, result.Entity));
                                if (updateResult.IsAcknowledged && updateResult.IsModifiedCountAvailable && updateResult.ModifiedCount == 1)
                                {
                                    return string.Format("Property added.");
                                }
                                throw new Exception(string.Format("Something went wrong"));
                            }
                            else
                            {
                                throw new Exception(string.Format("Class with name : {0} does not exist in the language.", requestModel.ClassName));
                            }
                        }
                    }
                    else
                        throw new Exception("Language can not be found");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        /// <summary>
        /// Get the all langauge schemas for the user id
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static GetLanguageSchemaResponseResult GetLanguageSchemaForUser(GetLanguageSchemaRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

                GetLanguageSchemaResponseResult response = null;
            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();
                var projectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                if (!string.IsNullOrEmpty(requestModel.UserId))
                {
                    var filterDefinition = new FilterDefinitionBuilder<KLanguageModel>();
                    var fd = filterDefinition.Eq(q => q.UserId, requestModel.UserId);
                    var projectDefinition = new ProjectionDefinitionBuilder<KLanguageModel>().Include(x => x._id).Include(x => x.Entity.EntityName).Include(x => x.Entity.EntityDescription);
                    var result = LanguageCollection.Find(fd).Project<KLanguageModel>(projectDefinition);
                    if (result != null)
                    {
                        response = new GetLanguageSchemaResponseResult
                        {
                            Schemas = result.ToList()?.Select(x => new SchemaItem
                            {
                                EntityDescription = x.Entity.EntityDescription,
                                EntityName = x.Entity.EntityName,
                                SchemaId = x._id
                            })?.ToList()
                        };
                    }
                }
                if (!string.IsNullOrEmpty(requestModel.ProjectId))
                {
                    var projectSchemaId = projectCollection.Find(x => x.ProjectId == requestModel.ProjectId).Project(x => x.SchemaId).FirstOrDefault();
                    if (!string.IsNullOrEmpty(projectSchemaId))
                    {
                        if(response == null || response.Schemas == null || !response.Schemas.Any(x => x.SchemaId == projectSchemaId))
                        {
                            var fd = new FilterDefinitionBuilder<KLanguageModel>().Eq(q => q._id, projectSchemaId);
                            var projectDefinition = new ProjectionDefinitionBuilder<KLanguageModel>().Include(x => x._id).Include(x => x.Entity.EntityName).Include(x => x.Entity.EntityDescription);
                            var result = LanguageCollection.Find(fd).Project<KLanguageModel>(projectDefinition).FirstOrDefault();
                            if (response == null)
                                response = new GetLanguageSchemaResponseResult();
                            response.Schemas.Add(new SchemaItem
                            {
                                EntityDescription = result.Entity.EntityDescription,
                                EntityName = result.Entity.EntityName,
                                SchemaId = result._id
                            });
                        }
                      
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;

        }

        /// <summary>
        /// Get datatyp class list
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<KClass> GetDataTypeClasses()
        {
            //Removing the function class for now
            return LanguageDefaults.GetDataTypeClasses().Where(x => x.ClassType == KClassType.DataTypeClass && x.Name.ToLower() != "function");
        }


        /// <summary>
        /// Get langauge schema by languageid and version (optional)
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static GetLanguageByPropertyGroupResponse GetLanguageEntityByPropertyGroup(GetLanguageEntityRequestModel requestModel)
        {
            var result = new GetLanguageByPropertyGroupResponse();
            var entity = GetLanguageEntity(requestModel);
            if (entity != null)
            {
                result.EntityDescription = entity.EntityDescription;
                result.EntityName = entity.EntityName;
                if (entity.Classes != null)
                    result.Classes = entity.Classes.Select(x => new KClassGroupProperties
                    {
                        ClassType = x.ClassType,
                        Description = x.Description,
                        GroupName = x.Description,
                        IsCustom = x.IsCustom,
                        Name = x.Name,
                        PropertyList = x.PropertyList,
                        ReferenceType = x.ReferenceType,
                        Schemas = x.Schemas
                    }).ToList();
                return result;
            }
            else
            {
                throw new Exception("language not found");
            }
        }

        /// <summary>
        /// Get langauge schema by languageid and version (optional)
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static KEntity GetLanguageEntity(GetLanguageEntityRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();


                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                var ProdLanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModelProd>(KitsuneProdLanguageCollectionName);
                if (!string.IsNullOrEmpty(requestModel.EntityId))
                {
                    var filterDefinition = new FilterDefinitionBuilder<KLanguageModel>();
                    var fd = filterDefinition.Eq(q => q._id, requestModel.EntityId);

                    //To support clientid and userid both
                    //if (!string.IsNullOrEmpty(requestModel.UserId))
                    //    fd = filterDefinition.And(filterDefinition.Eq(q => q._id, requestModel.EntityId), filterDefinition.Eq(q => q.UserId, requestModel.UserId));

                    var result = LanguageCollection.Find(fd).FirstOrDefault();
                    if (requestModel.Version <= 0)
                    {

                        if (result?.Entity != null)
                        {
                            List<KClass> totalClasses = result.Entity.Classes.ToList();
                            //totalClasses.AddRange(LanguageDefaults.GetDataTypeClasses().Where(x => x.ClassType == KClassType.DataTypeClass));
                            result.Entity.Classes = totalClasses;
                            return result.Entity;
                        }
                    }
                    else if (!string.IsNullOrEmpty(requestModel.EntityId) && requestModel.Version > 0)
                    {
                        var filterDefinition1 = new FilterDefinitionBuilder<KLanguageModelProd>();
                        var fd1 = filterDefinition1.And(filterDefinition1.Eq(q => q._id, requestModel.EntityId), filterDefinition1.Eq(q => q.Version, requestModel.Version));
                        var updateDefinition = new UpdateDefinitionBuilder<KLanguageModelProd>();
                        var result1 = ProdLanguageCollection.Find(fd1).FirstOrDefault();
                        if (result1 != null)
                        {
                            var newLanguage = new KLanguageModel()
                            {
                                CreatedOn = result1.CreatedOn,
                                Entity = result1.Entity,
                                _id = result1._id,
                                UpdatedOn = result1.UpdatedOn,
                                UserId = result1.UserId,
                            };

                            return newLanguage.Entity;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return null;
        }

        /// <summary>
        /// Get langauge schema by languageid with properties of given class name
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static List<string> GetSimilarPropertiesInLanguageEntity(GetSimilarPropertiesInLanguageEntityRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();


                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                if (!string.IsNullOrEmpty(requestModel.EntityId))
                {
                    var filterDefinition = new FilterDefinitionBuilder<KLanguageModel>();
                    var fd = filterDefinition.Eq(q => q._id, requestModel.EntityId);
                    var entityresult = LanguageCollection.Find(fd).FirstOrDefault();
                    if (entityresult?.Entity?.Classes != null)
                    {
                        Queue<KeyValuePair<string, string>> classListToProcess = new Queue<KeyValuePair<string, string>>();
                        List<string> fullQualifiedNames = new List<string>();
                        foreach (KClass kClass in entityresult.Entity.Classes)
                        {
                            List<KProperty> propertyList = kClass.PropertyList.Where(p => p.Type == PropertyType.array && p.DataType.Name == requestModel.ClassName).ToList();
                            foreach (KProperty property in propertyList)
                            {
                                if (kClass.ClassType == KClassType.BaseClass)
                                {
                                    fullQualifiedNames.Add(kClass.Name + "." + property.Name);
                                }
                                else
                                {
                                    classListToProcess.Enqueue(KeyValuePair.Create(kClass.Name, kClass.Name + "." + property.Name));
                                }
                            }
                        }
                        while (classListToProcess.Count() > 0)
                        {
                            KeyValuePair<string, string> classValuePair = classListToProcess.Dequeue();
                            string className = classValuePair.Key;
                            string[] classNameValues = classValuePair.Value.Split('.');
                            foreach (KClass kClass in entityresult.Entity.Classes)
                            {
                                List<KProperty> propertyList = kClass.PropertyList.Where(p => p.DataType.Name == className).ToList();
                                if (propertyList.Count() > 0)
                                {
                                    if (kClass.ClassType != KClassType.BaseClass)
                                    {
                                        foreach (KProperty property in propertyList)
                                        {
                                            if (property.Type == PropertyType.array)
                                            {
                                                classNameValues[0] = property.Name + ":" + property.DataType.Name;
                                            }
                                            else
                                            {
                                                classNameValues[0] = property.Name + ":" + property.DataType.Name;
                                            }
                                            classListToProcess.Enqueue(KeyValuePair.Create(kClass.Name, kClass.Name + "." + string.Join(".", classNameValues)));
                                        }
                                    }
                                    else
                                    {
                                        foreach (KProperty property in propertyList)
                                        {
                                            if (property.Type == PropertyType.array)
                                            {
                                                classNameValues[0] = property.Name + ":" + property.DataType.Name;
                                            }
                                            else
                                            {
                                                classNameValues[0] = property.Name + ":" + property.DataType.Name;
                                            }
                                            fullQualifiedNames.Add(kClass.Name + "." + string.Join(".", classNameValues));
                                        }
                                    }
                                }
                            }
                        }

                        return fullQualifiedNames;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }
        internal static List<List<PropertyPathSegment>> GetSimilarPropertyPathSegments(GetSimilarPropertiesInLanguageEntityRequestModel requestModel)
        {
            var fullQualifiedNames = GetSimilarPropertiesInLanguageEntity(requestModel);
            var response = new List<List<PropertyPathSegment>>();
            var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
            var languageModel = LanguageCollection.Find(x => x._id == requestModel.EntityId).FirstOrDefault();
            var classes = languageModel?.Entity?.Classes;
            if (classes != null && fullQualifiedNames != null && fullQualifiedNames.Any())
            {
                foreach (var path in fullQualifiedNames)
                {
                    List<PropertyPathSegment> pathSegments = new List<PropertyPathSegment>();
                    var segments = path.Split(':');

                    PropertyType nextPropertyType;
                    pathSegments.Add(new PropertyPathSegment
                    {
                        PropertyName = segments[0].Split('.')[0],
                        PropertyDataType = segments[0].Split('.')[0],
                        Type = PropertyType.obj
                    });

                    nextPropertyType = classes.FirstOrDefault(x => x.Name == pathSegments[0].PropertyDataType).PropertyList.FirstOrDefault(x => x.Name == segments[0].Split('.')[1]).Type;
                    pathSegments.Add(new PropertyPathSegment
                    {
                        PropertyName = segments[0].Split('.')[1],
                        PropertyDataType = classes.FirstOrDefault(x => x.Name == pathSegments[0].PropertyDataType).PropertyList.FirstOrDefault(x => x.Name == segments[0].Split('.')[1])?.DataType?.Name,
                        Type = nextPropertyType
                    });


                    if (segments.Length > 1)
                    {
                        for (var i = 1; i < segments.Length; i++)
                        {
                            nextPropertyType = classes.FirstOrDefault(x => x.Name == pathSegments[i].PropertyDataType).PropertyList.FirstOrDefault(x => x.Name == segments[i].Split('.')[1]).Type;

                            pathSegments.Add(new PropertyPathSegment
                            {
                                PropertyName = segments[i].Split('.')[1],
                                PropertyDataType = classes.FirstOrDefault(x => x.Name == segments[i].Split('.')[0]).PropertyList.FirstOrDefault(x => x.Name == segments[i].Split('.')[1])?.DataType?.Name,
                                Type = nextPropertyType
                            });
                        }
                    }
                    response.Add(pathSegments);
                }
            }
            return response;
        }

        /// <summary>
        /// Add data for the website first time 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static string AddDataForWebsite(AddOrUpdateWebsiteRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();

                List<string> Errors = new List<string>();
                var _schemaName = string.Empty;
                var _userId = string.Empty;
                KLanguageModel _schema = new KLanguageModel();
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                KLanguageModel language = null;
                if (!string.IsNullOrEmpty(requestModel.SchemaId))
                    language = LanguageCollection.Find(x => x._id == requestModel.SchemaId).Limit(1).FirstOrDefault();
                else
                    language = LanguageCollection.Find(x => x.UserId == requestModel.UserId && x.Entity.EntityName == requestModel.SchemaName).Limit(1).FirstOrDefault();
                BsonDocument dataObject = new BsonDocument();

                #region Get website root url from the websiteid
                var rootUrl = string.Empty;
                var website = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName).Find(x => x._id == requestModel.WebsiteId)
                    .Project<KitsuneWebsiteCollection>(new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>().Include(y => y.WebsiteUrl).Include(x => x.ProjectId)).FirstOrDefault();
                if (website != null)
                {
                    rootUrl = website.WebsiteUrl;
                }
                else
                {
                    Errors.Add($"Website not found with website id : {requestModel.WebsiteId}");

                }
                #endregion

                if (language != null)
                {
                    _schema = language;
                    _schemaName = _schema.Entity.EntityName;
                    _userId = _schema.UserId;
                    var kClass = _schema.Entity.Classes.First(x => x.ClassType == KClassType.BaseClass && x.Name == _schemaName);
                    if (kClass != null)
                    {

                        foreach (var ac in kClass.PropertyList)
                        {
                            if (ac.IsRequired && requestModel.Data.ContainsKey(ac.Name) && requestModel.Data[ac.Name] == null)
                                Errors.Add(ac.Name + "  is required property.");
                        }
                    }
                    else
                    {
                        Errors.Add(_schemaName + " base class not found in entity.");
                    }
                    var newObj = new BsonDocument();
                    JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat
                    };
                    var bsonDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(JsonConvert.SerializeObject(requestModel.Data, microsoftDateFormatSettings));

                    //Update the reference id

                    var timeStamp = DateTime.UtcNow;
                    var defaultProperties = new List<string> { "userid", "schemaid", COLLECTION_KEY_WEBISTE_ID, "rootaliasurl", COLLECTION_KEY_CREATED_ON, COLLECTION_KEY_UPDATED_ON, COLLECTION_KEY_IS_ARCHIVED, COLLECTION_KEY_ID, COLLECTION_KEY_KID, COLLECTION_KEY_PARENT_CLASS_ID, COLLECTION_KEY_PARENT_CLASS_NAME, COLLECTION_KEY_PROPERTY_NAME };

                    foreach (var prop in defaultProperties)
                    {
                        bsonDoc.Remove(prop);
                    }


                    var error = GenerateDataObject(bsonDoc, _schema.Entity, _schemaName, newObj);

                    if (error.Any())
                        foreach (var er in error)
                            Errors.Add(er);
                    else
                        dataObject = newObj;
                }
                else
                    Errors.Add(string.Format("Schema '{0}' does not exist", requestModel.SchemaName));

                if (Errors.Any())
                {
                    Exception ex = new Exception("Checkout the <ErroList> field in Data Property for list of errors");
                    ex.Data["ErrorList"] = Errors;
                    throw ex;
                }



                //var _id = ObjectId.GenerateNewId();
                dataObject.Add("userid", _userId);
                // dataObject.Add(COLLECTION_KEY_ID, _id);
                dataObject.Add("schemaid", _schema._id);

                #region Validate if the website data with same website id exist

                var websiteDataExist = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(EnvConstants.Constants.GenerateSchemaName(_schemaName)).Find($"{{websiteid :'{requestModel.WebsiteId}', isarchived : false }}").FirstOrDefault();
                if (websiteDataExist != null && websiteDataExist.Any())
                {
                    Errors.Add($"Website data already exist, Can not add the data for the website id : {requestModel.WebsiteId}");
                }

                #endregion

                if (Errors.Any())
                {
                    Exception ex = new Exception("Checkout the <ErroList> field in Data Property for list of errors");
                    ex.Data["ErrorList"] = Errors;
                    throw ex;
                }

                #region callTracker

                Dictionary<string, bool> componentStatus = new Dictionary<string, bool> {
                    { EnvConstants.Constants.ComponentId.callTracker, false }
                };
                List<phoneNumber> activeCallTrackerList = new List<phoneNumber>();
                bool callTrackerComponentEnabled = false;
                callTrackerComponentEnabled = MongoConnector.IsAppEnabled(website.ProjectId, EnvConstants.Constants.ComponentId.callTracker).IsActive;
                if (callTrackerComponentEnabled)
                {
                    var callTrackerEnabledDomains = GetCallTrackerDomainsFromConfig(requestModel.WebsiteId, website.ProjectId);
                    if (callTrackerEnabledDomains.Contains(EnvConstants.Constants.callTrackerAllWebsitesIdentifier) || callTrackerEnabledDomains.Contains(rootUrl))
                        componentStatus[EnvConstants.Constants.ComponentId.callTracker] = true;
                }

                #endregion

                dataObject.Add("rootaliasurl", new BsonDocument("url", rootUrl ?? requestModel.WebsiteId));
                dataObject.Add(COLLECTION_KEY_WEBISTE_ID, requestModel.WebsiteId);
                dataObject.Add(COLLECTION_KEY_IS_ARCHIVED, false);


                //var jsonDoc = Newtonsoft.Json.JsonConvert.SerializeObject(JsonHelper.DeserializeWithLowerCasePropertyNames(Newtonsoft.Json.JsonConvert.SerializeObject(dataObject)));
                //var bsonDoc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jsonDoc);


                InsertObject(dataObject,
                    _schema.Entity,
                    _schema.Entity.Classes.FirstOrDefault(x => x.ClassType == KClassType.BaseClass),
                    EnvConstants.Constants.GenerateSchemaName(_schemaName),
                    requestModel.UserId,
                    requestModel.WebsiteId,
                    null,
                    null,
                    null,
                    null,
                    componentStatus);


                //#region deacitvate extra VMNs assigned
                //if (componentStatus[EnvConstants.Constants.ComponentId.callTracker])
                //{
                //    activeCallTrackerList = GetListOfVMNs(requestModel.WebsiteId);
                //    var InUseCallTrackers = GetInUseVMNs(requestModel.WebsiteId, requestModel.UserId, requestModel.SchemaName);
                //    var vmnsRemoved = activeCallTrackerList.RemoveAll(x => InUseCallTrackers.Any(y => y.contactNumber == x.contactNumber));
                //    if (activeCallTrackerList.Any())
                //        MongoConnector.DisableVMNs(activeCallTrackerList.Select(x => x.contactNumber).ToList());
                //}
                //#endregion

                if (!string.IsNullOrEmpty(requestModel.WebsiteId))
                    new CacheServices().InvalidateDataCache(requestModel.WebsiteId);

                return dataObject[COLLECTION_KEY_ID].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get data for specific website by websiteid(optional) 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static GetWebsiteDataResponseModel GetWebsiteData(GetWebsiteDataRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();


                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                KLanguageModel entity = null;

                #region Validate Schema Agains User exist

                if (string.IsNullOrEmpty(requestModel.SchemaName))
                {
                    var ProductinProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                    var website = (WebsiteDetailsResponseModel)GetKitsuneWebsiteDetails(requestModel.WebsiteId)?.Response;
                    if (requestModel.WebsiteId != null)
                    {
                        if (string.IsNullOrEmpty(requestModel.SchemaId))
                        {
                            var project = ProductinProjectCollection.Find(x => x.ProjectId == website.ProjectId).Project<ProductionKitsuneProject>(new ProjectionDefinitionBuilder<ProductionKitsuneProject>().Include(x => x.SchemaId)).FirstOrDefault();
                            if (project != null)
                            {
                                requestModel.SchemaId = project.SchemaId;
                            }
                        }
                        entity = LanguageCollection.Find(x => x._id == requestModel.SchemaId).FirstOrDefault();


                        if (entity == null)
                            throw new Exception($"Could not find the schema for website '{requestModel.WebsiteId}'");

                        requestModel.SchemaName = entity.Entity.EntityName;
                        requestModel.UserId = entity.UserId;
                    }
                }
                else
                {
                    entity = LanguageCollection.Find(x => x.UserId == requestModel.UserId && x.Entity.EntityName.ToLower() == requestModel.SchemaName.Trim().ToLower()).FirstOrDefault();
                    if (entity == null || entity.Entity == null)
                        throw new Exception(string.Format("Schema not found with the name : '{0}'.", requestModel.SchemaName));

                }
                #endregion


                var _schemaName = requestModel.SchemaName.Trim().ToLower();

                var queryDoc = new BsonDocument();
                if (!string.IsNullOrEmpty(requestModel.Query))
                {
                    queryDoc = BsonSerializer.Deserialize<BsonDocument>(requestModel.Query);
                }

                #region Handle get data with custom query with _parentClassName, _parentClassId, _propertyName and Return the result

                if (queryDoc != null && queryDoc.Contains(COLLECTION_KEY_PARENT_CLASS_NAME) && queryDoc[COLLECTION_KEY_PARENT_CLASS_NAME] != null && queryDoc.Contains(COLLECTION_KEY_PROPERTY_NAME) && queryDoc[COLLECTION_KEY_PROPERTY_NAME] != null && !string.IsNullOrEmpty(queryDoc[COLLECTION_KEY_PARENT_CLASS_NAME].AsString) && !string.IsNullOrEmpty(queryDoc[COLLECTION_KEY_PROPERTY_NAME].AsString))
                {
                    var referenceCollectionName = GetKClassFromReferenceId(queryDoc[COLLECTION_KEY_PARENT_CLASS_NAME].AsString, queryDoc[COLLECTION_KEY_PROPERTY_NAME].AsString, entity.Entity)?.Name?.ToLower();
                    if (referenceCollectionName != null)
                    {
                        var q = new BsonDocument(COLLECTION_KEY_PARENT_CLASS_NAME, queryDoc[COLLECTION_KEY_PARENT_CLASS_NAME].AsString);
                        q.Add(COLLECTION_KEY_PROPERTY_NAME, queryDoc[COLLECTION_KEY_PROPERTY_NAME].AsString);
                        q.Add(COLLECTION_KEY_PARENT_CLASS_ID, queryDoc[COLLECTION_KEY_PARENT_CLASS_ID].AsString);
                        q.Add(COLLECTION_KEY_IS_ARCHIVED, false);
                        var actualCollectionName = $"{EnvConstants.Constants.GenerateSchemaName(_schemaName)}_{referenceCollectionName}";
                        var referenceObjs = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(actualCollectionName)
                            .Find(q).Sort(new SortDefinitionBuilder<BsonDocument>().Descending(COLLECTION_KEY_ID)).ToList();
                        if (referenceObjs != null && referenceObjs.Any())
                        {
                            var data = new List<object>();
                            foreach (var d in referenceObjs.ToList())
                            {
                                d.Remove(COLLECTION_KEY_ID);
                                data.Add(BsonSerializer.Deserialize<object>(d));
                            }

                            return new GetWebsiteDataResponseModel
                            {
                                Data = data,
                                Extra = new Pagination
                                {
                                    CurrentIndex = 0,
                                    PageSize = 0,
                                    TotalCount = referenceObjs.Count()
                                }
                            };
                        }

                    }

                }
                #endregion


                queryDoc.Add(COLLECTION_KEY_IS_ARCHIVED, false);


                //if (!string.IsNullOrEmpty(requestModel.UserId))
                //    queryDoc.Add("userid", requestModel.UserId);


                if (!string.IsNullOrEmpty(requestModel.WebsiteId))
                    queryDoc.Add(COLLECTION_KEY_WEBISTE_ID, requestModel.WebsiteId);

                var actions = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(EnvConstants.Constants.GenerateSchemaName(_schemaName)).Find(queryDoc);

                var totalCount = actions.Count();

                actions = actions.Limit(requestModel.Limit);
                if (!string.IsNullOrEmpty(requestModel.Sort))
                {
                    SortDefinition<BsonDocument> sd = BsonSerializer.Deserialize<BsonDocument>(requestModel.Sort);
                    if (sd != null)
                        actions = actions.Sort(sd);
                }
                else //Default sort by updated on
                {
                    actions = actions.Sort(new SortDefinitionBuilder<BsonDocument>().Descending(COLLECTION_KEY_UPDATED_ON));
                }

                if (!string.IsNullOrEmpty(requestModel.Include))
                {
                    var projectDefinition = BsonSerializer.Deserialize<BsonDocument>(requestModel.Include);

                    actions = actions.Project<BsonDocument>(projectDefinition);
                }
                if (requestModel.Skip != 0)
                    actions = actions.Skip(requestModel.Skip);


                var result = actions.ToList();

                if (result != null)
                {
                    var baseClass = entity.Entity.Classes.First(x => x.ClassType == KClassType.BaseClass);
                    var dataTypeObjects = new string[] { "str", "number", "datetime", "boolean" };
                    var baseClassProperties = baseClass.PropertyList.Where(x => (x.Type == PropertyType.array && !dataTypeObjects.Contains(x.DataType?.Name?.ToLower())) || x.Type == PropertyType.obj || x.Type == PropertyType.kstring || x.Type == PropertyType.phonenumber);
                    var dataResult = new List<object>();
                    var _schemaCollectionName = EnvConstants.Constants.GenerateSchemaName(_schemaName);
                    var inMemoryCollectionos = new Dictionary<string, List<BsonDocument>>();
                    foreach (var item in result)
                    {

                        inMemoryCollectionos = GetInMemoryCollection(entity.Entity, requestModel.WebsiteId);
                        foreach (var prop in baseClassProperties)
                        {
                            var ob = GetObjectFromInMemoryCollections(entity.Entity, prop.DataType.Name.Trim('[', ']'), baseClass.Name, prop.Name, _schemaCollectionName, item[COLLECTION_KEY_KID].AsString, inMemoryCollectionos);
                            if (prop.Type == PropertyType.array)
                            {
                                var arr = new BsonArray();
                                foreach (var arritem in ob)
                                {
                                    arr.Add(arritem.AsBsonValue);
                                }
                                item.Remove(prop.Name.ToLower());
                                item.Add(prop.Name.ToLower(), arr);
                            }
                            else
                            {
                                if (ob != null && ob.Any())
                                {
                                    item.Remove(prop.Name.ToLower());
                                    item.Add(prop.Name.ToLower(), ob.FirstOrDefault());
                                }
                            }
                        }
                        item.Remove(COLLECTION_KEY_ID);

                        dataResult.Add(BsonSerializer.Deserialize<object>(item));
                    }

                    var response = new GetWebsiteDataResponseModel
                    {
                        Data = dataResult,
                        Extra = new Pagination
                        {
                            CurrentIndex = requestModel.Skip,
                            PageSize = requestModel.Limit,
                            TotalCount = totalCount
                        }
                    };
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }


        internal static GetWebsiteDataByPropertyResponseModel GetWebsiteDataByPropertyPath(GetWebsiteDataByPropertyPath requestModel)
        {
            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                {
                    InitializeConnection();
                }
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                var entity = LanguageCollection.Find(x => x._id == requestModel.SchemaId).FirstOrDefault();
                if (entity == null)
                {
                    throw new Exception($"Language not found with id \"{requestModel.SchemaId}\".");
                }

                var end = requestModel.PropertySegments.Count - 1;
                GetWebsiteDataByPropertyResponseModel responseModel = new GetWebsiteDataByPropertyResponseModel() { };

                //Get first parent _kid

                var filterDoc = new BsonDocument();
                filterDoc.Add(COLLECTION_KEY_IS_ARCHIVED, false);
                filterDoc.Add(COLLECTION_KEY_WEBISTE_ID, requestModel.WebsiteId);
                var projectDoc = new BsonDocument();
                if (end == 0 && requestModel.PropertySegments[0].ObjectKeys != null && requestModel.PropertySegments[0].ObjectKeys.Any())
                {

                    foreach (var key in requestModel.PropertySegments[0].ObjectKeys)
                    {
                        projectDoc.Add(key.Key, key.Value);
                    }
                    if (!projectDoc.Contains(COLLECTION_KEY_KID) && !projectDoc.Any(x => x.Value == false))
                    {
                        projectDoc.Add(COLLECTION_KEY_KID, true);
                    }
                }
                else
                {
                    projectDoc.Add(COLLECTION_KEY_KID, 1);
                }

                var baseCollectionName = EnvConstants.Constants.GenerateSchemaName(entity.Entity.EntityName);
                var baseObject = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(baseCollectionName)
                    .Find(filterDoc).Project(projectDoc).FirstOrDefault();

                if (baseObject == null)
                    throw new Exception($"No data found for website : \"{requestModel.WebsiteId}\"");

                if (end == 0)
                {
                    responseModel.Data = BsonSerializer.Deserialize<dynamic>(baseObject);
                    return responseModel;
                }

                var parentId = baseObject[COLLECTION_KEY_KID].AsString;
                bool returnLength = false;
                //foreach property in propertysegments
                if (requestModel.PropertySegments.Count > 2 && requestModel.PropertySegments[requestModel.PropertySegments.Count - 1].Type == PropertyType.function && requestModel.PropertySegments[requestModel.PropertySegments.Count - 2].Type == PropertyType.str)
                {
                    requestModel.PropertySegments.RemoveAt(requestModel.PropertySegments.Count - 1);
                    returnLength = true;
                }

                PropertyPathSegment previousProp = null;
                PropertyPathSegment currentProp = null;
                PropertyPathSegment nextProp = null;
                int? startIndex = null;
                int? limit = null;
                var dataTypeObjects = new string[] { "str", "number", "datetime", "boolean" };


                for (var i = 1; i <= end; i++)
                {
                    currentProp = requestModel.PropertySegments[i];
                    nextProp = i < end ? requestModel.PropertySegments[i + 1] : null;
                    previousProp = requestModel.PropertySegments[i - 1];
                    startIndex = currentProp.Index;
                    limit = currentProp.Limit ?? 1;
                    if (currentProp.Type == PropertyType.obj || currentProp.Type == PropertyType.kstring || currentProp.Type == PropertyType.phonenumber)
                    {
                        if (i < end)
                        {
                            if ((nextProp.Type == PropertyType.array && nextProp.Type == PropertyType.obj) || nextProp.Type == PropertyType.obj)
                            {
                                var projection = new Dictionary<string, bool>();
                                projection.Add(COLLECTION_KEY_KID, true);
                                var kid = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, currentProp.Sort, currentProp.Filter, startIndex, limit);
                                if (kid != null && kid.Any() && kid[0].Contains(COLLECTION_KEY_KID))
                                    parentId = kid.First()[COLLECTION_KEY_KID].AsString;
                            }
                            else if (i == (end - 1) && (nextProp.Type != PropertyType.array || (nextProp.Type == PropertyType.array && dataTypeObjects.Contains(nextProp.PropertyDataType)))
                                && nextProp.Type != PropertyType.obj
                                && nextProp.Type != PropertyType.phonenumber
                                && nextProp.Type != PropertyType.kstring
                                && nextProp.Type != PropertyType.function)//check if its second last property
                            {
                                //get the next property from database
                                var projection = new Dictionary<string, bool>();
                                projection.Add(nextProp.PropertyName, true);
                                var propertyResult = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, currentProp.Sort, currentProp.Filter, startIndex, limit);
                                if (propertyResult != null && propertyResult.Any() && propertyResult[0].Contains(nextProp.PropertyName.ToLower()))
                                {

                                    switch (nextProp.Type)
                                    {
                                        case PropertyType.array:
                                            {
                                                if (nextProp.Sort != null)
                                                {
                                                    if (nextProp.Sort.Values.First() == 1)
                                                        propertyResult[0][nextProp.PropertyName.ToLower()] = new BsonArray((propertyResult[0][nextProp.PropertyName.ToLower()].AsBsonArray).OrderBy(x => x));
                                                    else if (nextProp.Sort.Values.First() == -1)
                                                        propertyResult[0][nextProp.PropertyName.ToLower()] = new BsonArray((propertyResult[0][nextProp.PropertyName.ToLower()].AsBsonArray).OrderByDescending(x => x));

                                                }
                                                if (nextProp.Index == null)
                                                {
                                                    var arrayResult = new List<dynamic>();
                                                    foreach (var propRes in propertyResult[0][nextProp.PropertyName.ToLower()].AsBsonArray)
                                                    {
                                                        arrayResult.Add(ExtractDataFromBsonValue(propRes));
                                                    }
                                                    responseModel.Data = arrayResult;
                                                }
                                                else
                                                {
                                                    var propRes = propertyResult[0][nextProp.PropertyName.ToLower()].AsBsonArray;
                                                    if (propRes.Count >= nextProp.Index)
                                                    {
                                                        responseModel.Data = ExtractDataFromBsonValue(propRes[nextProp.Index ?? 0]);
                                                    }
                                                }

                                            }; break;
                                        case PropertyType.boolean: { responseModel.Data = propertyResult[0][nextProp.PropertyName.ToLower()].AsBoolean; } break;
                                        case PropertyType.number: { responseModel.Data = propertyResult[0][nextProp.PropertyName.ToLower()].AsDouble; } break;
                                        case PropertyType.date: { responseModel.Data = propertyResult[0][nextProp.PropertyName.ToLower()].ToNullableUniversalTime(); } break;
                                        case PropertyType.str: { responseModel.Data = propertyResult[0][nextProp.PropertyName.ToLower()].AsString; } break;
                                    }

                                }
                                if (returnLength)
                                    responseModel.Data = responseModel.Data != null ? ((string)responseModel.Data).Length : 0;

                                return responseModel;
                            }
                            else
                            {
                                var projection = new Dictionary<string, bool>();
                                projection.Add(COLLECTION_KEY_KID, true);
                                var propertyResult = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, currentProp.Sort, currentProp.Filter, currentProp.Index, 1);

                                if (propertyResult != null && propertyResult.Any() && propertyResult[0].Contains(COLLECTION_KEY_KID))
                                {
                                    parentId = propertyResult[0][COLLECTION_KEY_KID].AsString;
                                }
                            }
                        }
                        else if (i == end)
                        {
                            //Get entire object
                            var propertyResult = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, currentProp.ObjectKeys, currentProp.Sort, currentProp.Filter, startIndex, limit);
                            if (propertyResult != null && propertyResult.Any())
                            {
                                responseModel.Data = BsonSerializer.Deserialize<dynamic>(propertyResult[0]);
                            }
                            return responseModel;
                        }

                    }
                    else if (currentProp.Type == PropertyType.array)
                    {
                        if (i < end)
                        {
                            if (currentProp.Index == null && (nextProp.Type != PropertyType.array || (nextProp.Type == PropertyType.array && dataTypeObjects.Contains(nextProp.PropertyDataType)))
                                && nextProp.Type != PropertyType.obj
                                && nextProp.Type != PropertyType.phonenumber
                                && nextProp.Type != PropertyType.kstring
                                && nextProp.Type != PropertyType.function)
                            {
                                var projection = new Dictionary<string, bool>();
                                projection.Add(nextProp.PropertyName, true);
                                var propertyResult = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, currentProp.Sort, currentProp.Filter, startIndex, limit);

                                if (propertyResult != null && propertyResult.Any())
                                {
                                    if (currentProp.Index != null)
                                    {
                                        responseModel.Data = BsonSerializer.Deserialize<dynamic>(propertyResult[0]);
                                    }
                                    else
                                    {
                                        var arrayResult = new List<dynamic>();
                                        foreach (var propRes in propertyResult)
                                        {
                                            arrayResult.Add(BsonSerializer.Deserialize<dynamic>(propRes));
                                        }
                                        responseModel.Data = arrayResult;
                                    }
                                    return responseModel;
                                }
                            }
                            else if ((nextProp.Type == PropertyType.array && nextProp.Type == PropertyType.obj) || nextProp.Type == PropertyType.obj || nextProp.Type == PropertyType.kstring)
                            {
                                var projection = new Dictionary<string, bool>();
                                projection.Add(COLLECTION_KEY_KID, true);

                                var kid = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, currentProp.Sort, currentProp.Filter, startIndex, limit);
                                if (kid != null && kid.Any() && kid[0].Contains(COLLECTION_KEY_KID))
                                    parentId = kid.First()[COLLECTION_KEY_KID].AsString;
                            }
                            else if (i == (end - 1)
                                && (nextProp.Type != PropertyType.array || (nextProp.Type == PropertyType.array && dataTypeObjects.Contains(nextProp.PropertyDataType)))
                                && nextProp.Type != PropertyType.obj
                                && nextProp.Type != PropertyType.phonenumber
                                && nextProp.Type != PropertyType.kstring
                                && nextProp.Type != PropertyType.function)//check if its second last property
                            {
                                //get the next property from database
                                var projection = new Dictionary<string, bool>();
                                projection.Add(nextProp.PropertyName, true);

                                var propertyResult = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, currentProp.Sort, currentProp.Filter, startIndex, limit);
                                if (propertyResult != null && propertyResult.Any() && propertyResult[0].Contains(nextProp.PropertyName.ToLower()))
                                {

                                    switch (nextProp.Type)
                                    {
                                        case PropertyType.array:
                                            {
                                                if (nextProp.Sort != null)
                                                {
                                                    if (nextProp.Sort.Values.First() == 1)
                                                        propertyResult[0][nextProp.PropertyName.ToLower()] = new BsonArray((propertyResult[0][nextProp.PropertyName.ToLower()].AsBsonArray).OrderBy(x => x));
                                                    else if (nextProp.Sort.Values.First() == -1)
                                                        propertyResult[0][nextProp.PropertyName.ToLower()] = new BsonArray((propertyResult[0][nextProp.PropertyName.ToLower()].AsBsonArray).OrderByDescending(x => x));

                                                }
                                                if (nextProp.Index == null)
                                                {
                                                    var arrayResult = new List<dynamic>();
                                                    foreach (var propRes in propertyResult[0][nextProp.PropertyName.ToLower()].AsBsonArray)
                                                    {
                                                        arrayResult.Add(ExtractDataFromBsonValue(propRes));
                                                    }
                                                    responseModel.Data = arrayResult;
                                                }
                                                else
                                                {
                                                    var propRes = propertyResult[0][nextProp.PropertyName.ToLower()].AsBsonArray;
                                                    if (propRes.Count >= nextProp.Index)
                                                    {
                                                        responseModel.Data = ExtractDataFromBsonValue(propRes[nextProp.Index ?? 0]);
                                                    }
                                                }

                                            }; break;
                                        case PropertyType.boolean: { responseModel.Data = propertyResult[0][nextProp.PropertyName.ToLower()].AsBoolean; } break;
                                        case PropertyType.number: { responseModel.Data = propertyResult[0][nextProp.PropertyName.ToLower()].AsDouble; } break;
                                        case PropertyType.date: { responseModel.Data = propertyResult[0][nextProp.PropertyName.ToLower()].ToNullableUniversalTime(); } break;
                                        case PropertyType.str: { responseModel.Data = propertyResult[0][nextProp.PropertyName.ToLower()].AsString; } break;
                                    }
                                }
                                if (returnLength)
                                    responseModel.Data = responseModel.Data != null ? ((string)responseModel.Data).Length : 0;
                                return responseModel;
                            }
                            else if (nextProp.Type != PropertyType.function)
                            {
                                var projection = new Dictionary<string, bool>();
                                projection.Add(COLLECTION_KEY_KID, true);

                                var propertyResult = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, currentProp.Sort, currentProp.Filter, currentProp.Index, 1);

                                if (propertyResult != null && propertyResult.Any() && propertyResult[0].Contains(COLLECTION_KEY_KID))
                                {
                                    parentId = propertyResult[0][COLLECTION_KEY_KID].AsString;
                                }
                            }
                        }
                        else if (end == 1 && dataTypeObjects.Contains(currentProp.PropertyDataType))
                        {
                            var projection = new Dictionary<string, bool>();
                            projection.Add(currentProp.PropertyName, true);
                            var propertyResult = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, null, currentProp.Filter, startIndex, limit);
                            if (propertyResult != null && propertyResult.Any() && propertyResult[0].Contains(currentProp.PropertyName.ToLower()))
                            {
                                if (currentProp.Sort != null)
                                {
                                    if (currentProp.Sort.Values.First() == 1)
                                        propertyResult[0][currentProp.PropertyName.ToLower()] = new BsonArray((propertyResult[0][currentProp.PropertyName.ToLower()].AsBsonArray).OrderBy(x => x));
                                    else if (currentProp.Sort.Values.First() == -1)
                                        propertyResult[0][currentProp.PropertyName.ToLower()] = new BsonArray((propertyResult[0][currentProp.PropertyName.ToLower()].AsBsonArray).OrderByDescending(x => x));

                                }
                                if (currentProp.Index == null)
                                {
                                    var arrayResult = new List<dynamic>();
                                    foreach (var propRes in propertyResult[0][currentProp.PropertyName.ToLower()].AsBsonArray)
                                    {
                                        arrayResult.Add(ExtractDataFromBsonValue(propRes));
                                    }
                                    responseModel.Data = arrayResult;
                                }
                                else
                                {
                                    var propRes = propertyResult[0][currentProp.PropertyName.ToLower()].AsBsonArray;
                                    if (propRes.Count >= currentProp.Index)
                                    {
                                        responseModel.Data = ExtractDataFromBsonValue(propRes[currentProp.Index ?? 0]);
                                    }
                                }

                            }
                        }
                        else if (i == end)
                        {
                            var propertyResult = GetObject(entity.Entity, currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, currentProp.ObjectKeys, currentProp.Sort, currentProp.Filter, startIndex, limit);

                            if (propertyResult != null && propertyResult.Any())
                            {
                                if (currentProp.Limit == 1)
                                {
                                    responseModel.Data = BsonSerializer.Deserialize<dynamic>(propertyResult[0]);
                                }
                                else
                                {
                                    var arrayResult = new List<dynamic>();
                                    foreach (var propRes in propertyResult)
                                    {
                                        arrayResult.Add(BsonSerializer.Deserialize<dynamic>(propRes));
                                    }
                                    responseModel.Data = arrayResult;
                                }

                            }
                            //Get entire list of array
                        }
                    }
                    else if (currentProp.Type == PropertyType.function)
                    {
                        if (currentProp.PropertyName.ToLower() == FUNCTION_NAME_LENGTH && previousProp.Type == PropertyType.array)
                        {
                            if (i > 1 && previousProp.PropertyDataType != null && dataTypeObjects.Contains(previousProp.PropertyDataType))
                            {
                                var objectCollectionProp = requestModel.PropertySegments[i - 2];
                                var length = GetObjectArrayLength(entity.Entity, baseCollectionName, objectCollectionProp.PropertyDataType, previousProp.PropertyName, parentId, isCustomDatatype: false, filter: previousProp.Filter);
                                responseModel.Data = length;
                            }
                            else
                            {
                                var length = GetObjectArrayLength(entity.Entity, baseCollectionName, previousProp.PropertyDataType, previousProp.PropertyName, parentId, isCustomDatatype: true, filter: previousProp.Filter);
                                responseModel.Data = length;
                            }
                        }
                    }
                    else if (nextProp == null)
                    {
                        var projection = new Dictionary<string, bool>();
                        projection.Add(currentProp.PropertyName, true);
                        var propertyResult = GetObject(entity.Entity, end == 1 ? previousProp.PropertyDataType : currentProp.PropertyDataType, previousProp.PropertyDataType, currentProp.PropertyName, baseCollectionName, parentId, projection, currentProp.Sort, currentProp.Filter, startIndex, limit);
                        if (propertyResult != null && propertyResult.Any() && propertyResult[0].Contains(currentProp.PropertyName))
                        {
                            responseModel.Data = ExtractDataFromBsonValue(propertyResult[0][currentProp.PropertyName]);
                        }
                        if (returnLength)
                            responseModel.Data = responseModel.Data != null ? ((string)responseModel.Data).Length : 0;
                        return responseModel;
                    }

                }


                //add default query
                //add additional custom query
                //add sort
                //add skip/limit for arrya
                //add include if the next segment is default property 
                //if next segment is object or array just get the _kid
                //

                return responseModel;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static dynamic ExtractDataFromBsonValue(BsonValue bsonValue)
        {
            dynamic data = null;
            switch (bsonValue.BsonType)
            {
                case BsonType.Array:
                    {
                        var arrayResult = new List<dynamic>();
                        foreach (var propRes in bsonValue.AsBsonArray)
                        {
                            arrayResult.Add(BsonSerializer.Deserialize<dynamic>((BsonDocument)propRes));
                        }
                        data = arrayResult;
                    }; break;
                case BsonType.Boolean: { data = bsonValue.AsBoolean; } break;
                case BsonType.Int32: case BsonType.Int64: case BsonType.Double: { data = bsonValue.AsDouble; } break;
                case BsonType.DateTime: { data = bsonValue.ToNullableUniversalTime(); } break;
                case BsonType.String: { data = bsonValue.AsString; } break;
            }
            return data;
        }

        private static long GetObjectArrayLength(KEntity entity, string baseCollectionName, string kClassName, string propertyName, string parentId, bool isCustomDatatype, Dictionary<string, object> filter = null)
        {
            if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                InitializeConnection();
            var kClass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == kClassName.ToLower());

            long documentsCount = 0;
            if (kClass != null)
            {
                var queryDoc = new BsonDocument();

                if(filter != null)
                {
                    try
                    {
                        queryDoc = BsonDocument.Parse(JsonConvert.SerializeObject(filter));
                    }
                    catch (Exception ex)
                    {
                        if (ex.Data != null)
                            ex.Data.Add("Invalid query", JsonConvert.SerializeObject(filter));
                        throw ex;
                    }
                }

                if (!isCustomDatatype)
                {
                    queryDoc.Add(COLLECTION_KEY_KID, parentId);
                    var projectDoc = new BsonDocument();
                    projectDoc.Add(propertyName, 1);
                    var collectionName = kClass.ClassType == KClassType.BaseClass ? baseCollectionName : $"{baseCollectionName}_{kClassName}";
                    var propertyValue = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(collectionName).Find(queryDoc).Project(projectDoc).FirstOrDefault();
                    if (propertyValue != null && propertyValue.Contains(propertyName) && propertyValue[propertyName].GetType() == typeof(BsonArray))
                        documentsCount = ((BsonArray)propertyValue[propertyName]).Count;
                    else
                        documentsCount = 0;
                }
                //if the base class then it will be always one document per website
                else if (kClass.ClassType == KClassType.BaseClass)
                    documentsCount = 1;
                else
                {
                    queryDoc.Add(COLLECTION_KEY_PARENT_CLASS_ID, parentId);
                    queryDoc.Add(COLLECTION_KEY_PROPERTY_NAME, propertyName);
                    queryDoc.Add(COLLECTION_KEY_IS_ARCHIVED, false);
                    documentsCount = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{baseCollectionName}_{kClassName}").CountDocuments(queryDoc);
                }

            }
            return documentsCount;
        }

        private static Dictionary<string, List<BsonDocument>> GetInMemoryCollection(KEntity entity, string websiteId)
        {
            if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                InitializeConnection();
            var _schemaCollectionName = EnvConstants.Constants.GenerateSchemaName(entity.EntityName);
            var queryDoc = new BsonDocument();
            queryDoc.Add("isarchived", false);
            var result = new Dictionary<string, List<BsonDocument>>();
            queryDoc.Add(COLLECTION_KEY_WEBISTE_ID, websiteId);

            Parallel.ForEach(entity.Classes.Where(x => x.ClassType != KClassType.BaseClass), new ParallelOptions { MaxDegreeOfParallelism = 10 }, kclass =>
            {
                var tmpCollName = $"{_schemaCollectionName}_{kclass.Name.ToLower()}";
                if (_kitsuneSchemaDatabase.GetCollection<BsonDocument>(tmpCollName).Count(queryDoc) > 0)
                {
                    if (!result.Keys.Contains(tmpCollName))
                        result.Add(tmpCollName, _kitsuneSchemaDatabase.GetCollection<BsonDocument>(tmpCollName).Find(queryDoc).SortByDescending(x => x["updatedon"]).ToList());
                }
            });
            return result;
        }

        /// <summary>
        /// Get data for specific website by websiteid and classType 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static GetWebsiteDataByTypeResponseModel GetWebsiteDataByType(GetWebsiteDataByTypeRequestModel requestModel)
        {
            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();

                var _schemaName = requestModel.SchemaName.Trim().ToLower();
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                var entity = LanguageCollection.Find(x => x.UserId == requestModel.UserId && x.Entity.EntityName.ToLower() == _schemaName).FirstOrDefault();
                if (entity == null || entity.Entity == null)
                    throw new Exception(string.Format("Schema not found with the name : '{0}'.", _schemaName));
                var classCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(string.Format("k_{0}_{1}", _schemaName, requestModel.ClassName.ToLower()));
                var baseCollectionResult = classCollection.Find($"{{{COLLECTION_KEY_WEBISTE_ID} : '{requestModel.WebsiteId}', {COLLECTION_KEY_IS_ARCHIVED} : false, {COLLECTION_KEY_REFLECTION_ID} : null}}")
                                            .Sort($"{{ {COLLECTION_KEY_PARENT_CLASS_NAME} : -1, {COLLECTION_KEY_PROPERTY_NAME}: -1, {COLLECTION_KEY_CREATED_ON}: -1}}").ToList();
                List<object> dataResult = new List<object>();
                List<GroupCount> parentCounts = new List<GroupCount>();
                GroupCount count = new GroupCount();
                int parentCount = 0, propertyCount = 0;
                string parent = "", property = "";

                if (baseCollectionResult != null && baseCollectionResult.Any())
                {
                    var parentClassNames = new Dictionary<string, List<string>>();
                    foreach (var cls in entity.Entity.Classes)
                    {
                        var prpList = new List<string>();
                        foreach (var prp in cls.PropertyList)
                        {
                            if (prp.DataType != null && prp.DataType.Name == requestModel.ClassName.ToLower())
                            {
                                prpList.Add(prp.Name);
                            }
                        }
                        if (prpList.Any())
                            parentClassNames.Add(cls.Name, prpList);
                    }
                    //foreach (var baseRecord in baseCollectionResult.ToList())
                    //{
                    //    var data = GetObject(entity.Entity, requestModel.ClassName.ToLower(), baseRecord[COLLECTION_KEY_PARENT_CLASS_NAME].AsString, baseRecord[COLLECTION_KEY_PROPERTY_NAME].AsString, EnvConstants.Constants.GenerateSchemaName(_schemaName), baseRecord[COLLECTION_KEY_PARENT_CLASS_ID].AsString)?.FirstOrDefault();
                    //}
                    var dataCollection = new List<BsonDocument>();
                    foreach (var parentClass in parentClassNames)
                    {
                        foreach (var prop in parentClass.Value)
                        {
                            var parentIds = baseCollectionResult.ToList().Where(x => x[COLLECTION_KEY_PARENT_CLASS_NAME].AsString == parentClass.Key && x[COLLECTION_KEY_PROPERTY_NAME].AsString == prop);
                            foreach (var parentId in parentIds.Select(x => x[COLLECTION_KEY_PARENT_CLASS_ID]?.AsString).Distinct())
                            {
                                if (!string.IsNullOrEmpty(parentId))
                                    dataCollection.AddRange(GetObject(entity.Entity, requestModel.ClassName.ToLower(), parentClass.Key, prop, EnvConstants.Constants.GenerateSchemaName(_schemaName), parentId));
                            }

                        }

                    }
                    foreach (var data in dataCollection)
                    {
                        //var data = GetObject(entity.Entity, requestModel.ClassName.ToLower(), baseRecord[COLLECTION_KEY_PARENT_CLASS_NAME].AsString, baseRecord[COLLECTION_KEY_PROPERTY_NAME].AsString, EnvConstants.Constants.GenerateSchemaName(_schemaName), baseRecord[COLLECTION_KEY_PARENT_CLASS_ID].AsString)?.FirstOrDefault();

                        if (data[COLLECTION_KEY_PARENT_CLASS_NAME].ToString() == parent)
                        {
                            parentCount++;
                            if (data[COLLECTION_KEY_PROPERTY_NAME].ToString() == property)
                            {
                                propertyCount++;
                            }
                            else
                            {
                                if (property != "")
                                {
                                    count.SubGroupCounts.Add(new GroupCount
                                    {
                                        Name = property,
                                        Count = propertyCount
                                    });
                                }
                                propertyCount = 1;
                                property = data[COLLECTION_KEY_PROPERTY_NAME].ToString();
                            }
                        }
                        else
                        {
                            if (property != "")
                            {
                                count.SubGroupCounts.Add(new GroupCount
                                {
                                    Name = property,
                                    Count = propertyCount
                                });
                            }
                            propertyCount = 1;
                            property = data[COLLECTION_KEY_PROPERTY_NAME].ToString();
                            if (parent != "")
                            {
                                count.Name = parent;
                                count.Count = parentCount;
                                parentCounts.Add(count);
                                count = new GroupCount();
                            }
                            parentCount = 1;
                            parent = data[COLLECTION_KEY_PARENT_CLASS_NAME].ToString();
                        }
                        data.Remove("_id");
                        dataResult.Add(BsonSerializer.Deserialize<object>(data));
                    }

                    if (property != "")
                    {
                        count.SubGroupCounts.Add(new GroupCount
                        {
                            Name = property,
                            Count = propertyCount
                        });
                        count.Name = parent;
                        count.Count = parentCount;
                        parentCounts.Add(count);
                    }
                }

                var response = new GetWebsiteDataByTypeResponseModel
                {
                    Data = dataResult,
                    GroupCount = parentCounts
                };
                return response;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        /// <summary>
        /// Update data object for specific website, single / bulk object with query
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static CommonAPIResponse UpdateWebsiteData(UpdateWebsiteDataRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();
                var _schemaName = string.Empty;
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                IFindFluent<KLanguageModel, KLanguageModel> result = null;
                if (!string.IsNullOrEmpty(requestModel.SchemaId))
                    result = LanguageCollection.Find(x => x._id == requestModel.SchemaId).Limit(1);
                else
                    result = LanguageCollection.Find(x => x.Entity.EntityName == requestModel.SchemaName.Trim().ToLower() && x.UserId == requestModel.UserId).Limit(1);
                KEntity kEntity = null;
                if (result != null && result.Any())
                {
                    kEntity = result.First().Entity;
                    _schemaName = kEntity.EntityName;
                }
                else
                    return CommonAPIResponse.BadRequest(new ValidationResult($"Schema does not exist with name : {requestModel.SchemaName}"));


                List<ValidationResult> ErrorList = new List<ValidationResult>();


                var filterdefinitionbuilder = new FilterDefinitionBuilder<BsonDocument>();
                var fd = filterdefinitionbuilder.Eq(COLLECTION_KEY_IS_ARCHIVED, false);
                var queryDoc = new BsonDocument();
                var updateDoc = new BsonDocument();

                if (requestModel.BulkUpdates == null)
                    requestModel.BulkUpdates = new List<UpdateDataItem> { new UpdateDataItem { Query = requestModel.Query, UpdateValue = requestModel.UpdateValue } };
                UpdateResult updateResultTemp;
                List<InsertUpdateResult> BulkUpdateResult = new List<InsertUpdateResult>();


                #region callTracker

                var projectId = string.Empty;
                var website = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName).Find(x => x._id == requestModel.WebsiteId)
                    .Project<KitsuneWebsiteCollection>(new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>().Include(x => x.ProjectId).Include(x => x.WebsiteUrl)).FirstOrDefault();
                if (website != null)
                {
                    projectId = website.ProjectId;
                }

                Dictionary<string, bool> componentStatus = new Dictionary<string, bool> {
                    { EnvConstants.Constants.ComponentId.callTracker, false }
                };
                List<phoneNumber> activeCallTrackerList = new List<phoneNumber>();
                bool callTrackerComponentEnabled = false;
                callTrackerComponentEnabled = MongoConnector.IsAppEnabled(projectId, EnvConstants.Constants.ComponentId.callTracker).IsActive;
                if (callTrackerComponentEnabled)
                {
                    var callTrackerEnabledDomains = GetCallTrackerDomainsFromConfig(requestModel.WebsiteId, projectId);
                    if (callTrackerEnabledDomains.Contains(EnvConstants.Constants.callTrackerAllWebsitesIdentifier) || callTrackerEnabledDomains.Contains(website.WebsiteUrl))
                        componentStatus[EnvConstants.Constants.ComponentId.callTracker] = true;
                }


                #endregion


                foreach (var update in requestModel.BulkUpdates)
                {
                    #region Update single Object

                    if (update.UpdateValue != null)
                    {
                        try
                        {
                            if (update.Query != null)
                            {
                                try
                                {
                                    BsonDocument.TryParse(update.Query.ToString(), out queryDoc);
                                }
                                catch (Exception ex)
                                {
                                    return CommonAPIResponse.BadRequest(new ValidationResult($"Invalid mongo query : {update.Query.ToString()}"));
                                }
                            }
                            else
                                return CommonAPIResponse.BadRequest(new ValidationResult($"Query can no be empty"));

                            var parentClassName = queryDoc.Contains(COLLECTION_KEY_PARENT_CLASS_NAME) ? queryDoc[COLLECTION_KEY_PARENT_CLASS_NAME].AsString : null;
                            var parentClassId = queryDoc.Contains(COLLECTION_KEY_PARENT_CLASS_ID) ? queryDoc[COLLECTION_KEY_PARENT_CLASS_ID].AsString : null;
                            var propertyName = queryDoc.Contains(COLLECTION_KEY_PROPERTY_NAME) ? queryDoc[COLLECTION_KEY_PROPERTY_NAME].AsString : null;
                            var _kid = queryDoc.Contains(COLLECTION_KEY_KID) && queryDoc[COLLECTION_KEY_KID] != BsonNull.Value ? queryDoc[COLLECTION_KEY_KID].AsString : null;

                            KClass kClass = GetKClassFromReferenceId(parentClassName, propertyName, kEntity);
                            var collectionName = EnvConstants.Constants.GenerateSchemaName(kEntity.EntityName) + (kClass.ClassType != KClassType.BaseClass ? string.Format("_{0}", kClass.Name.ToLower()) : "");

                            var updateQuery = new BsonDocument();
                            if (_kid != null)
                                updateQuery = new BsonDocument(COLLECTION_KEY_KID, _kid);
                            else
                                updateQuery = new BsonDocument(COLLECTION_KEY_KID, BsonNull.Value);

                            JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
                            {
                                DateFormatHandling = DateFormatHandling.IsoDateFormat
                            };
                            updateDoc = BsonSerializer.Deserialize<BsonDocument>(JsonConvert.SerializeObject(update.UpdateValue, microsoftDateFormatSettings));

                            //Insert/Update the object in db
                            BulkUpdateResult.Add(InsertOrUpdateObject(kEntity, kClass, updateDoc, updateQuery, collectionName, parentClassName, parentClassId, requestModel.WebsiteId, propertyName, requestModel.SchemaName.Trim().ToLower(), requestModel.UserId, requestModel.ReferenceList, componentStatus, requestModel.IgnoreNested));

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    #endregion
                }

                #region deacitvate extra VMNs assigned
                if (componentStatus[EnvConstants.Constants.ComponentId.callTracker])
                {
                    activeCallTrackerList = GetListOfVMNs(requestModel.WebsiteId);
                    var InUseCallTrackers = GetInUseVMNs(requestModel.WebsiteId, requestModel.UserId, requestModel.SchemaName);
                    var vmnsRemoved = activeCallTrackerList.RemoveAll(x => InUseCallTrackers.Any(y => y.contactNumber == x.contactNumber));
                    if (activeCallTrackerList.Any())
                        MongoConnector.DisableVMNs(activeCallTrackerList.Select(x => x.contactNumber).ToList());
                }
                #endregion

                if (BulkUpdateResult != null &&
                    BulkUpdateResult.Any(x => x.Status == InsertUpdateStatus.Inserted || x.Status == InsertUpdateStatus.Inserted) &&
                    !string.IsNullOrEmpty(requestModel.WebsiteId))
                {
                    new CacheServices().InvalidateDataCache(requestModel.WebsiteId);
                }
                return CommonAPIResponse.OK(BulkUpdateResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Delete the data object for specific website
        /// Archives the object based on referenceid and _kid
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static string DeleteObject(DeleteDataObjectRequestModel requestModel)
        {

            if (requestModel.BulkDelete == null)
                requestModel.BulkDelete = new List<object> { requestModel.Query };

            long deleteCount = 0;
            if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                InitializeConnection();

            var _schemaName = string.Empty;
            var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
            var result = LanguageCollection.Find(x => x.Entity.EntityName == requestModel.SchemaName.Trim().ToLower() && x.UserId == requestModel.UserId).Limit(1);

            KEntity kEntity = null;
            if (result != null && result.Any())
            {
                kEntity = result.First().Entity;
                _schemaName = kEntity.EntityName;
            }
            else
                throw new Exception("Schema does not exist with name : " + requestModel.SchemaName);

            foreach (var deleteQueryTemp in requestModel.BulkDelete)
            {
                BsonDocument deleteQuery = new BsonDocument();
                if (deleteQueryTemp != null)
                {
                    try
                    {
                        BsonDocument.TryParse(deleteQueryTemp.ToString(), out deleteQuery);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Invalid mongo query");
                    }
                }

                var _kid = deleteQuery.Contains(COLLECTION_KEY_KID) && deleteQuery[COLLECTION_KEY_KID] != BsonNull.Value ? deleteQuery[COLLECTION_KEY_KID].AsString : null;
                if (_kid == null)
                    throw new Exception("_kid can not be null");

                var parentClassName = deleteQuery.Contains(COLLECTION_KEY_PARENT_CLASS_NAME) ? deleteQuery[COLLECTION_KEY_PARENT_CLASS_NAME].AsString : null;
                var parentClassId = deleteQuery.Contains(COLLECTION_KEY_PARENT_CLASS_ID) ? deleteQuery[COLLECTION_KEY_PARENT_CLASS_ID].AsString : null;
                var propertyName = deleteQuery.Contains(COLLECTION_KEY_PROPERTY_NAME) ? deleteQuery[COLLECTION_KEY_PROPERTY_NAME].AsString : null;
                if (parentClassName == null || propertyName == null)
                    throw new Exception("Reference properties can not be null");

                KClass kClass = GetKClassFromReferenceId(parentClassName, propertyName, kEntity);

                if (kClass != null)
                {
                    var collectionName = EnvConstants.Constants.GenerateSchemaName(kEntity.EntityName) + (kClass.ClassType != KClassType.BaseClass ? string.Format("_{0}", kClass.Name.ToLower()) : "");
                    var updateDeleteDocument = new BsonDocument("$set", new BsonDocument(COLLECTION_KEY_IS_ARCHIVED, true));
                    var collection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(collectionName);
                    var deleteResult = collection.UpdateOne(new BsonDocument(COLLECTION_KEY_KID, _kid), updateDeleteDocument);
                    if (deleteResult != null && deleteResult.IsAcknowledged && deleteResult.IsModifiedCountAvailable && deleteResult.ModifiedCount > 0)
                        deleteCount += deleteResult.ModifiedCount;
                }
                else
                {
                    throw new Exception("Invalid reference id");
                }

                deleteQuery = null;
            }

            if (deleteCount > 0)
            {
                if (!string.IsNullOrEmpty(requestModel.WebsiteId))
                    new CacheServices().InvalidateDataCache(requestModel.WebsiteId);

                return string.Format("{0} record deleted", deleteCount);
            }
            else
                return "No records found to delete";
        }

        /// <summary>
        /// Search the data based on the schema array property with kstring data class only
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static CommonAPIResponse SearchData(SearchDataRequestModel requestModel)
        {
            if (requestModel == null)
            {
                return CommonAPIResponse.InternalServerError(new ArgumentNullException(nameof(requestModel)));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var resultOb = new GetWebsiteDataResponseModel()
                {
                    Data = new List<object>(),
                    Extra = new Pagination()
                    {
                        CurrentIndex = requestModel.Skip,
                        PageSize = requestModel.Limit
                    }
                };
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);

                if (!string.IsNullOrEmpty(requestModel.UserId))
                {
                    var entity = LanguageCollection.Find(x => x.UserId == requestModel.UserId && x.IsArchived == false && x.Entity.EntityName.ToLower() == requestModel.SchemaName.Trim().ToLower()).FirstOrDefault();
                    if (entity == null)
                        return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Could not find the schema"));

                    KClass obClass;
                    if (requestModel.SearchProperty != null && Compiler.Helpers.KitsuneCompiler.IsSearchableProperty(requestModel.SearchProperty, entity.Entity, out obClass))
                    {

                        var websiteDoc = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(EnvConstants.Constants.GenerateSchemaName(requestModel.SchemaName))
                            .Find(new BsonDocument(COLLECTION_KEY_WEBISTE_ID, requestModel.WebsiteId)).Limit(1).Project(new BsonDocument(COLLECTION_KEY_KID, 1))?.ToList();

                        IFindFluent<BsonDocument, BsonDocument> result = null;

                        #region TextSearch KString
                        if (!string.IsNullOrEmpty(requestModel.SearchText))
                        {
                            long totalCount = 0;
                            if (websiteDoc != null && websiteDoc.Any())
                            {
                                var segments = Kitsune.API2.Utils.Helpers.ExtractPropertiesFromPath(requestModel.SearchProperty + "._kid", entity.Entity);
                                //TODO: Review
                                segments.ForEach(x => x.Limit = 10);

                                var idResults = GetWebsiteDataByPropertyPath(new API.Model.ApiRequestModels.GetWebsiteDataByPropertyPath { PropertySegments = segments, SchemaId = entity._id, WebsiteId = requestModel.WebsiteId });
                                if (idResults != null && idResults.Data != null)
                                {
                                    var parentIds = new List<string>();
                                    if (idResults.Data.GetType() == typeof(string))
                                    {
                                        parentIds = new List<string> { (string)idResults.Data };
                                    }
                                    else if (idResults.Data.GetType() == typeof(List<object>))
                                    {
                                        parentIds = ((List<dynamic>)idResults.Data).Select(x => (string)x._kid).ToList();
                                    }
                                    if (parentIds != null && parentIds.Any())
                                        result = KStringSearch(obClass, requestModel.SchemaName, requestModel.WebsiteId, requestModel.SearchText, ref totalCount, null,
                                                        null, null, parentIds).FirstOrDefault()?.ResultDocs;
                                }
                            }
                        }
                        #endregion

                        #region FilterSearchKString
                        if (!string.IsNullOrEmpty(requestModel.Filter))
                        {
                            try
                            {
                                var filterDoc = BsonSerializer.Deserialize<BsonDocument>(requestModel.Filter);

                                if (filterDoc.Contains(COLLECTION_KEY_IS_ARCHIVED))
                                    filterDoc.Remove(COLLECTION_KEY_IS_ARCHIVED);
                                filterDoc.Add(COLLECTION_KEY_IS_ARCHIVED, false);
                                if (filterDoc.Contains(COLLECTION_KEY_WEBISTE_ID))
                                    filterDoc.Remove(COLLECTION_KEY_WEBISTE_ID);
                                filterDoc.Add(COLLECTION_KEY_WEBISTE_ID, requestModel.WebsiteId);

                                if (!filterDoc.Contains(COLLECTION_KEY_PROPERTY_NAME))
                                    filterDoc.Add(COLLECTION_KEY_PROPERTY_NAME, requestModel.SearchProperty.Substring(requestModel.SearchProperty.LastIndexOf('.') + 1));

                                FilterDefinition<BsonDocument> fd = filterDoc;


                                var count = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{EnvConstants.Constants.GenerateSchemaName(requestModel.SchemaName)}_{obClass.Name.ToLower()}").Count(fd);

                                resultOb.Extra.TotalCount = count;

                                result = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{EnvConstants.Constants.GenerateSchemaName(requestModel.SchemaName)}_{obClass.Name.ToLower()}").Find(fd);

                            }
                            catch
                            {

                            }
                        }
                        #endregion

                        if (result == null)
                            return CommonAPIResponse.OK(resultOb);

                        if (!string.IsNullOrEmpty(requestModel.Sort))
                        {
                            try
                            {
                                SortDefinition<BsonDocument> sd = BsonSerializer.Deserialize<BsonDocument>(requestModel.Sort);
                                if (sd != null)
                                    result = result.Sort(sd);
                            }
                            catch
                            {

                            }
                        }
                        else //Default sort by updated on
                        {
                            result = result.Sort(new SortDefinitionBuilder<BsonDocument>().Descending(COLLECTION_KEY_UPDATED_ON));
                        }
                        BsonDocument projectDefinition = null;
                        if (requestModel.Include != null)
                        {
                            try
                            {
                                projectDefinition = new BsonDocument();
                                foreach (var prop in requestModel.Include)
                                {
                                    projectDefinition.Add(prop, 1);
                                }
                                result = result.Project<BsonDocument>(projectDefinition);
                            }
                            catch
                            {

                            }

                        }
                        resultOb.Extra.TotalCount = result.Count();

                        //
                        result = result.Skip(requestModel.Skip).Limit(requestModel.Limit);


                        var dataTypeObjects = new string[] { "str", "number", "datetime", "boolean" };
                        var baseClassProperties = obClass.PropertyList.Where(x => (x.Type == PropertyType.array && !dataTypeObjects.Contains(x.DataType?.Name?.ToLower())) || x.Type == PropertyType.obj || x.Type == PropertyType.kstring || x.Type == PropertyType.phonenumber);
                        var dataResult = new List<object>();
                        var _schemaCollectionName = EnvConstants.Constants.GenerateSchemaName(requestModel.SchemaName);

                        foreach (var item in result.ToList())
                        {
                            foreach (var prop in baseClassProperties)
                            {
                                if (requestModel.Include == null || requestModel.Include.Any(x => x.Contains(prop.Name.ToLower())))
                                {
                                    var parentClassName = obClass.Name;
                                    var propertyName = prop.Name;
                                    var parentId = item[COLLECTION_KEY_KID].AsString;
                                    IEnumerable<string> includeob = null;
                                    if (requestModel.Include != null)
                                    {
                                        includeob = requestModel.Include.Where(x => x.ToLower().StartsWith($"{propertyName}"));

                                        //if include contains entire object
                                        if (includeob != null && includeob.Any(x => x.IndexOf('.') < 0))
                                            includeob = null;
                                        else if (includeob != null)
                                            includeob = includeob.Select(x => x.Substring(x.LastIndexOf('.') + 1));
                                    }
                                    var projection = new Dictionary<string, bool>();
                                    if (includeob != null)
                                    {
                                        foreach (var proj in includeob)
                                        {
                                            projection.Add(proj, true);
                                        }
                                    }


                                    var ob = GetObject(entity.Entity, prop.DataType.Name.Trim('[', ']'), parentClassName, propertyName, _schemaCollectionName, parentId, projection);
                                    if (prop.Type == PropertyType.array)
                                    {
                                        var arr = new BsonArray();
                                        foreach (var arritem in ob)
                                        {
                                            arr.Add(arritem.AsBsonValue);
                                        }
                                        item.Add(prop.Name.ToLower(), arr);
                                    }
                                    else
                                    {
                                        if (ob != null && ob.Any())
                                            item.Add(prop.Name.ToLower(), ob?.FirstOrDefault());
                                    }
                                }
                            }
                            item.Remove(COLLECTION_KEY_ID);
                            item.Remove(COLLECTION_KEY_IS_ARCHIVED);

                            dataResult.Add(BsonSerializer.Deserialize<object>(item));
                        }

                        resultOb.Data = dataResult;

                        return CommonAPIResponse.OK(resultOb);
                    }
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Invalid search property"));
                }
                return CommonAPIResponse.UnAuthorized();

            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse SearchGlobalData(GlobalSearchDataRequestModel requestModel)
        {
            if (requestModel == null)
            {
                return CommonAPIResponse.InternalServerError(new ArgumentNullException(nameof(requestModel)));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var resultOb = new GetWebsiteDataResponseModel()
                {
                    Data = new List<object>(),
                    Extra = new Pagination()
                    {
                        CurrentIndex = requestModel.Skip,
                        PageSize = requestModel.Limit
                    }
                };
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);

                if (!string.IsNullOrEmpty(requestModel.UserId))
                {

                    var website = (WebsiteDetailsResponseModel)GetKitsuneWebsiteDetails(requestModel.WebsiteId)?.Response;
                    if (website != null)
                    {
                        if (string.IsNullOrEmpty(requestModel.SchemaId))
                        {
                            var project = ProductionProjectCollection.Find(x => x.ProjectId == website.ProjectId).Project<ProductionKitsuneProject>(new ProjectionDefinitionBuilder<ProductionKitsuneProject>().Include(x => x.SchemaId)).FirstOrDefault();
                            if (project != null)
                            {
                                requestModel.SchemaId = project.SchemaId;
                            }
                        }
                        var entity = LanguageCollection.Find(x => x._id == requestModel.SchemaId).FirstOrDefault();


                        if (entity == null)
                            return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Could not find the schema"));
                        var schemaName = entity.Entity.EntityName.ToLower();
                        var websiteDoc = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(EnvConstants.Constants.GenerateSchemaName(schemaName))
                             .Find(new BsonDocument(COLLECTION_KEY_WEBISTE_ID, requestModel.WebsiteId)).Limit(1).Project(new BsonDocument(COLLECTION_KEY_KID, 1))?.ToList();



                        if (website != null)
                        {
                            var detailsPages = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName)
                                .Find(x => x.ProjectId == website.ProjectId && x.PageType == KitsunePageType.DETAILS && x.KObject != null)
                                .Project<ProductionKitsuneResource>(new ProjectionDefinitionBuilder<ProductionKitsuneResource>().Include(x => x.KObject).Include(x => x.UrlPattern)).ToList();
                            GetWebsiteDataResponseModel responseOb = new GetWebsiteDataResponseModel()
                            {
                                Extra = new Pagination
                                {
                                    CurrentIndex = requestModel.Skip,
                                    PageSize = requestModel.Limit,
                                    TotalCount = 0
                                }
                            };
                            if (detailsPages != null && detailsPages.Any())
                            {
                                KClass obClass = null;
                                long totalCount = 0;
                                string ksearchPropertyPath = null;

                                SearchDocumentResult searchResult;
                                var _schemaCollectionName = EnvConstants.Constants.GenerateSchemaName(schemaName);

                                foreach (var page in detailsPages.Where(x => x.KObject != null && x.KObject.IndexOf(":") > 0))
                                {
                                    totalCount = 0;
                                    ksearchPropertyPath = page.KObject.Split(':')[1].ToLower();
                                    //obClass = entity.Entity.Classes.FirstOrDefault(x => x.Name.ToLower() == page.KObject.Split(':')[1].ToLower());
                                    Compiler.Helpers.KitsuneCompiler.IsSearchableProperty(ksearchPropertyPath, entity.Entity, out obClass);

                                    if (obClass != null)
                                    {
                                        // obClass = entity.Entity.Classes.FirstOrDefault(x => x.Name.ToLower() == page.KObject.Split(':')[1].ToLower());

                                        var result = KStringSearch(obClass, schemaName, requestModel.WebsiteId, requestModel.SearchText, ref totalCount, parentPropertyName: ksearchPropertyPath.Substring(ksearchPropertyPath.LastIndexOf('.') + 1));

                                        searchResult = result.FirstOrDefault(x => x.ResultClass.ToLower() == obClass.Name.ToLower());
                                        if (searchResult != null)
                                        {
                                            try
                                            {
                                                SortDefinition<BsonDocument> sd = BsonSerializer.Deserialize<BsonDocument>("{_id : -1}");
                                                if (sd != null)
                                                    searchResult.ResultDocs = searchResult.ResultDocs.Sort(sd);
                                            }
                                            catch
                                            {

                                            }

                                            var dataResult = new List<GlobalSearchResult>();
                                            var tempDataResult = new List<GlobalSearchResult>();
                                            foreach (var item in searchResult.ResultDocs.ToList())
                                            {
                                                //GenerateLink
                                                //foreach(var prop in obClass.PropertyList.Where(x => x.Type == PropertyType.kstring))
                                                //{
                                                //    item[prop.Name] = 
                                                //}
                                                tempDataResult = new List<GlobalSearchResult>();
                                                item.Remove(COLLECTION_KEY_ID);
                                                item.Remove(COLLECTION_KEY_IS_ARCHIVED);
                                                foreach (var kstringItem in searchResult.KStringDocs.Where(x => x[COLLECTION_KEY_PARENT_CLASS_ID].AsString == item[COLLECTION_KEY_KID].AsString))
                                                {
                                                    //TODO : handle array of kstring
                                                    item.Remove(kstringItem[COLLECTION_KEY_PROPERTY_NAME].AsString);
                                                    item.Add(kstringItem[COLLECTION_KEY_PROPERTY_NAME].AsString, BsonValue.Create(kstringItem));
                                                    tempDataResult.Add(new GlobalSearchResult
                                                    {
                                                        Keywords = kstringItem.Contains("keywords") ? (kstringItem["keywords"].AsBsonArray)?.Select(x => x.AsString)?.ToList() : new List<string>(),
                                                        Text = kstringItem["text"].AsString,
                                                        _kid = item[COLLECTION_KEY_KID].AsString
                                                    });
                                                }
                                                var url = GenerateUrl(page.UrlPattern, item, page.KObject.Split(':')[0], website.WebsiteUrl);
                                                foreach (var res in tempDataResult)
                                                {
                                                    res.Url = url;
                                                }
                                                dataResult.AddRange(tempDataResult);
                                            }
                                            resultOb.Data.AddRange(dataResult.Select(x => (object)x).ToList());

                                            resultOb.Extra.TotalCount += totalCount;

                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                return CommonAPIResponse.OK(resultOb);
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }
        internal static CommonAPIResponse GenerateObjectUrl(string websiteid, string className, string _kid, string schemaId = null)
        {
            if (websiteid == null)
            {
                return CommonAPIResponse.InternalServerError(new ArgumentNullException(nameof(websiteid)));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var WebsiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);


                var website = WebsiteCollection.Find(x => x._id == websiteid).Project<KitsuneWebsiteCollection>(new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>()
                    .Include(x => x.ProjectId).Include(x => x.WebsiteUrl)).FirstOrDefault();
                if (website != null)
                {
                    if (string.IsNullOrEmpty(schemaId))
                    {
                        var project = ProductionProjectCollection.Find(x => x.ProjectId == website.ProjectId).Project<ProductionKitsuneProject>(new ProjectionDefinitionBuilder<ProductionKitsuneProject>().Include(x => x.SchemaId)).FirstOrDefault();
                        if (project != null)
                        {
                            schemaId = project.SchemaId;
                        }
                    }
                    var entity = LanguageCollection.Find(x => x._id == schemaId).FirstOrDefault();


                    if (entity == null)
                        return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Could not find the schema"));
                    var schemaName = entity.Entity.EntityName.ToLower();
                    var projectDoc = new BsonDocument(COLLECTION_KEY_KID, 1);
                    projectDoc.Add(COLLECTION_KEY_PARENT_CLASS_NAME, true);
                    projectDoc.Add(COLLECTION_KEY_PARENT_CLASS_ID, true);
                    projectDoc.Add(COLLECTION_KEY_PROPERTY_NAME, true);
                    projectDoc.Add("index", true);
                    projectDoc.Add("title", true);
                    projectDoc.Add("name", true);
                    var websiteDoc = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{EnvConstants.Constants.GenerateSchemaName(schemaName)}_{className.Trim().ToLower()}")
                             .Find(new BsonDocument(COLLECTION_KEY_ID, ObjectId.Parse(_kid))).Limit(1).Project(projectDoc)?.FirstOrDefault();
                    if (websiteDoc != null)
                    {
                        //var projectDefinition = new Dictionary<string, bool>();
                        //projectDefinition.Add("index", true);
                        //projectDefinition.Add("title", true);
                        //projectDefinition.Add("id", true);
                        //projectDefinition.Add("name", true);
                        //var websiteObject = GetObject(entity.Entity,
                        //    className,
                        //    websiteDoc[COLLECTION_KEY_PARENT_CLASS_NAME].AsString,
                        //    websiteDoc[COLLECTION_KEY_PROPERTY_NAME].AsString,
                        //    EnvConstants.Constants.GenerateSchemaName(schemaName),
                        //    websiteDoc[COLLECTION_KEY_PARENT_CLASS_ID].AsString,
                        //    projectDefinition
                        //    ).FirstOrDefault();


                        var detailsPages = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName)
                            .Find(x => x.ProjectId == website.ProjectId && x.PageType == KitsunePageType.DETAILS)
                            .Project<ProductionKitsuneResource>(new ProjectionDefinitionBuilder<ProductionKitsuneResource>().Include(x => x.KObject).Include(x => x.UrlPattern)).ToList();
                        if (detailsPages != null && detailsPages.Any())
                        {
                            KClass obClass = null;
                            string ksearchPropertyPath = null;

                            var _schemaCollectionName = EnvConstants.Constants.GenerateSchemaName(schemaName);

                            foreach (var page in detailsPages.Where(x => x.KObject != null && x.KObject.IndexOf(":") > 0))
                            {
                                ksearchPropertyPath = page.KObject.Split(':')[1].ToLower();
                                //obClass = entity.Entity.Classes.FirstOrDefault(x => x.Name.ToLower() == page.KObject.Split(':')[1].ToLower());
                                Compiler.Helpers.KitsuneCompiler.IsSearchableProperty(ksearchPropertyPath, entity.Entity, out obClass);

                                if (obClass != null && obClass.Name.ToLower() == className.Trim().ToLower())
                                {
                                    var projectDef = new ProjectionDefinitionBuilder<WebsiteDNSInfo>().Include(x => x.IsSSLEnabled).Include(x => x.DomainName);
                                    var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
                                    List<WebsiteDNSInfo> dns = websiteDNSCollection.Find(x => x.WebsiteId == website._id)
                                        .Project<WebsiteDNSInfo>(projectDef).ToList();
                                    if (dns != null && dns.Any(x => x.DomainName == website.WebsiteUrl))
                                    {

                                        website.WebsiteUrl = $"{(dns.First(x => x.DomainName == website.WebsiteUrl).IsSSLEnabled ? "https://" : "http://")}{website.WebsiteUrl}";
                                    }
                                    else
                                    {
                                        website.WebsiteUrl = $"http://{website.WebsiteUrl}";
                                    }

                                    var url = GenerateUrl(page.UrlPattern, websiteDoc, page.KObject.Split(':')[0], website.WebsiteUrl);
                                    return CommonAPIResponse.OK(url);
                                }
                            }
                        }
                    }
                }
                return CommonAPIResponse.NotFound();
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        private static string GenerateUrl(string urlPattern, BsonDocument item, string kobjectVar, string rootUrl)
        {
            var jsonWriterSettings = new MongoDB.Bson.IO.JsonWriterSettings { OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict };
            var jsonDoc = JObject.Parse($"{{ {kobjectVar} :  {{}} }}");

            jsonDoc[kobjectVar] = JObject.Parse(item.ToJson<MongoDB.Bson.BsonDocument>(jsonWriterSettings));

            var patternmatches = Kitsune.Helper.Constants.WidgetRegulerExpression.Matches(urlPattern);

            List<string> matchPattern = new List<string>();
            foreach (Match match in patternmatches)
            {
                matchPattern.Add(match.Value);
            }
            if (matchPattern != null && matchPattern.Any())
            {
                foreach (var attr in matchPattern)
                {
                    var expression = string.Empty;
                    var tempVal = attr;
                    var tempMatch = attr.Trim('[', ']');
                    var matches = Parser.GetObjects(tempMatch);


                    for (int i = 0; i < matches.Count; i++)
                    {
                        var mat = matches[i];

                        expression = mat.ToString().Replace("[[", "").Replace("]]", "").Trim();

                        var expressionValue = mat.ToLower().EndsWith("rootaliasurl.url") ? $"'{rootUrl}'" : $"'{jsonDoc.SelectToken(mat.ToLower())?.ToString()}'";



                        tempVal = ReplaceFirstOccurrence(tempVal, expression, expressionValue);
                    }

                    var expressionValue2 = !string.IsNullOrEmpty(tempVal.Trim('[', ']')) ? Parser.Execute(tempVal.Trim('[', ']')) : "";
                    urlPattern = ReplaceFirstOccurrence(urlPattern, attr, WebUtility.HtmlDecode(expressionValue2?.ToString()));
                }
            }
            return urlPattern;
        }

        public static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int place = source.IndexOf(find);
            if (place > 0)
            {
                return source.Remove(place, find.Length).Insert(place, replace);
            }

            return source.Replace(find, replace);
        }

        internal static CommonAPIResponse GetWebsiteDataStorageSize(string userid = null)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var result = new LanguageStorageDetailsResponse()
                {
                    Schemas = new List<LanguageStorage>()
                };
                List<KLanguageModel> languages = null;
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);

                if (!string.IsNullOrEmpty(userid))
                    languages = LanguageCollection.Find(x => x.UserId == userid && x.IsArchived == false).ToList();
                else
                    languages = LanguageCollection.Find(x => x.IsArchived == false).ToList();

                if (languages != null && languages.Any())
                {
                    foreach (var language in languages.Where(x => x.Entity != null && x.Entity.Classes != null && x.Entity.Classes.Any(y => y != null && y.ClassType == KClassType.UserDefinedClass) && !string.IsNullOrEmpty(x.Entity.EntityName)))
                    {
                        try
                        {
                            var baseCollectionName = EnvConstants.Constants.GenerateSchemaName(language.Entity.EntityName);
                            var schemaCollections = new List<string>() { baseCollectionName };
                            foreach (var cl in language.Entity.Classes.Where(x => x.ClassType == KClassType.UserDefinedClass))
                            {
                                schemaCollections.Add($"{baseCollectionName}_{cl.Name.ToLower()}");
                            }
                            List<LanguageStorage> schemaWebsites = new List<LanguageStorage>();
                            var websiteIds = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(baseCollectionName).Find($"{{ isarchived : false, userid : '{language.UserId}' }}").Project("{websiteid : 1}").ToList();

                            foreach (var schemaCol in schemaCollections)
                            {
                                var dataCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(schemaCol);
                                if (dataCollection.Count("{}") > 0)
                                {
                                    var collection = _kitsuneSchemaDatabase.RunCommand<MongoCollectionStatus>($"{{collstats: '{schemaCol}'}}");

                                    var avgSize = collection.AvgObjSize;

                                    var match = new BsonDocument { { "isarchived", false }, { "websiteid", new BsonDocument { { "$in", new BsonArray(websiteIds.Select(x => x["websiteid"].AsString)) } } } };
                                    var group = new BsonDocument { { "_id", new BsonDocument{ {"WebsiteId" , "$websiteid" } } } ,
                                                        { "count" , new BsonDocument { { "$sum", 1 } } } };

                                    var groupResult = dataCollection.Aggregate().Match(match).Group(group).ToList();

                                    if (groupResult != null && groupResult.Any())
                                    {
                                        foreach (var grp in groupResult)
                                        {
                                            schemaWebsites.Add(new LanguageStorage
                                            {
                                                Size = (long)(long.Parse(grp["count"]?.ToString() ?? "0") * avgSize),
                                                UserId = language.UserId,
                                                SchemaName = language.Entity.EntityName,
                                                WebsiteId = grp["_id"]["WebsiteId"].ToString(),
                                                SchemaId = language._id
                                            });
                                        }
                                    }
                                }
                            }
                            result.Schemas.AddRange(schemaWebsites.GroupBy(x => x.WebsiteId).Select(group => new LanguageStorage
                            {
                                Size = group.Sum(y => y.Size),
                                SchemaId = group.First().SchemaId,
                                SchemaName = group.First().SchemaName,
                                UserId = group.First().UserId,
                                WebsiteId = group.Key
                            }));
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    return CommonAPIResponse.OK(result);
                }
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
            return CommonAPIResponse.BadRequest(new ValidationResult("Data not found"));
        }

        public class SearchDocumentResult
        {

            public IFindFluent<BsonDocument, BsonDocument> ResultDocs;
            public string ResultClass { get; set; }
            public List<BsonDocument> KStringDocs { get; set; }
        }

        /// <summary>
        /// Search the kstring property withing the kobject 
        /// </summary>
        /// <param name="obClass"></param>
        /// <param name="schemaName"></param>
        /// <param name="searchText"></param>
        /// <param name="resultOb"></param>
        /// <param name="skip"></param>
        /// <param name="limit"></param>
        /// <returns>Returns all the objects with kstring text or keywords matches</returns>
        internal static List<SearchDocumentResult> KStringSearch(KClass obClass, string schemaName, string websiteId, string searchText, ref long totalCount, int? skip = null, int? limit = null, string parentPropertyName = null, List<string> parentIds = null)
        {
            var tempSearchText = searchText.Trim().Replace(" ", "|").Replace("-", "|");
            IFindFluent<BsonDocument, BsonDocument> kStringDocs;
            List<SearchDocumentResult> searchResults = new List<SearchDocumentResult>();
            var queryDoc = new BsonDocument();

            queryDoc.Add(COLLECTION_KEY_WEBISTE_ID, websiteId);
            queryDoc.Add(COLLECTION_KEY_IS_ARCHIVED, false);

            queryDoc.Add("$or", new BsonArray()
           {
               new BsonDocument($"keywords", new BsonDocument("$eq", $"/{tempSearchText}/i")),
               new BsonDocument($"text", new BsonDocument("$regex", new BsonRegularExpression($"{tempSearchText}", "i")))
           });
            //TODO : check the regex pattern
            //queryDoc.Add("$or", new BsonArray()
            //{
            //    new BsonDocument($"keywords", new BsonDocument("$regex", new BsonRegularExpression($"{searchText.Trim()}", "i"))),
            //    new BsonDocument($"text", new BsonDocument("$regex", new BsonRegularExpression($"{searchText.Trim()}", "i")))
            //});
            if (obClass != null)
            {
                queryDoc.Add(COLLECTION_KEY_PARENT_CLASS_NAME, obClass.Name.ToLower());
            }
            if (parentIds != null && parentIds.Any())
            {
                queryDoc.Add(COLLECTION_KEY_PARENT_CLASS_ID, new BsonDocument("$in", new BsonArray(parentIds)));
            }
            kStringDocs = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{EnvConstants.Constants.GenerateSchemaName(schemaName)}_kstring").Find(queryDoc)
               .Project<BsonDocument>(new ProjectionDefinitionBuilder<BsonDocument>().Include(COLLECTION_KEY_PARENT_CLASS_ID).Include(COLLECTION_KEY_PARENT_CLASS_NAME).Include(COLLECTION_KEY_PROPERTY_NAME).Include("text").Include("keywords"));

            totalCount = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{EnvConstants.Constants.GenerateSchemaName(schemaName)}_kstring").Count(queryDoc);

            if (kStringDocs != null)
            {
                // Get all the data if the skip and limit is not provided
                if (skip != null && limit != null)
                {
                    kStringDocs = kStringDocs.Skip(skip).Limit(limit);
                }
                var resultDocs = kStringDocs.ToList();

                if (obClass != null)
                {
                    var _kids = resultDocs.Select(x => x[COLLECTION_KEY_PARENT_CLASS_ID].AsString);

                    var filterDoc = new BsonDocument(COLLECTION_KEY_KID, new BsonDocument("$in", new BsonArray(_kids)));
                    //Include only the parent property
                    if (!string.IsNullOrEmpty(parentPropertyName))
                    {
                        filterDoc.Add(COLLECTION_KEY_PROPERTY_NAME, parentPropertyName);
                    }
                    searchResults.Add(new SearchDocumentResult
                    {
                        ResultDocs = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{EnvConstants.Constants.GenerateSchemaName(schemaName)}_{obClass.Name.ToLower()}")
                                                        .Find(filterDoc),
                        ResultClass = obClass.Name,
                        KStringDocs = resultDocs
                    });
                    return searchResults;
                }
                else
                {
                    var parentClasses = resultDocs.Select(x => x[COLLECTION_KEY_PARENT_CLASS_NAME].AsString).Distinct();
                    var _kids = new List<string>();
                    var filterDoc = new BsonDocument();

                    foreach (var parentClass in parentClasses)
                    {
                        var resultClassDocs = resultDocs.Where(x => x[COLLECTION_KEY_PARENT_CLASS_NAME].AsString == parentClass).ToList();
                        _kids = resultClassDocs.Select(x => x[COLLECTION_KEY_PARENT_CLASS_ID].AsString).ToList();
                        filterDoc = new BsonDocument(COLLECTION_KEY_KID, new BsonDocument("$in", new BsonArray(_kids)));

                        //Include only the parent property
                        if (!string.IsNullOrEmpty(parentPropertyName))
                        {
                            filterDoc.Add(COLLECTION_KEY_PROPERTY_NAME, parentPropertyName);
                        }

                        searchResults.Add(new SearchDocumentResult
                        {
                            ResultDocs = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{EnvConstants.Constants.GenerateSchemaName(schemaName)}_{parentClass.ToLower()}")
                                        .Find(filterDoc),
                            ResultClass = parentClass,
                            KStringDocs = resultClassDocs
                        });
                    }
                    return searchResults;
                }
            }
            return null;
        }


        //TODO:MIGRATION
        /// <summary>
        /// Map the language schema to existing project
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static bool MapSchemaToProject(MapSchemaToProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();

                var UserCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                var WebsiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var WebsiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);

                #region Validate Schema Present or not

                var validationResult = LanguageCollection.Find(x => x._id == requestModel.SchemaId);
                if (validationResult == null || !validationResult.Any())
                    throw new Exception("Schema not found for SchemaId : " + requestModel.SchemaId);

                #endregion

                #region Update Project Schema

                var Update = new UpdateDefinitionBuilder<KitsuneProject>();
                var updateDef = Update.Set(x => x.SchemaId, requestModel.SchemaId)
                                      .Set(x => x.UpdatedOn, DateTime.Now);
                var result2 = ProjectCollection.UpdateOne(x => x.ProjectId == requestModel.ProjectId, updateDef, new UpdateOptions { IsUpsert = true });

                #endregion

                if (result2.IsAcknowledged)
                {
                    //Given : ProjectId

                    //1)Get ProjectName from ProjectCollection
                    //2)Get WebsiteId from KitsuneWebsiteCollection
                    //3)Get first Website User from KitsuneWebsiteUserCollection (UserName,Password,DeveloperId)
                    //4)Get Developer Email From UserCollection

                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get intellisense for the IDE
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static GetIntellisenseResponseModel GetIntellisense(GetIntellisenseRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                    InitializeConnection();


                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                var UserCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
                var intellisenseOb = new Dictionary<string, object>();
                var tempOb = new Dictionary<string, object>();
                var user = UserCollection.Find(x => x._id == requestModel.UserId).Project<UserModel>(new ProjectionDefinitionBuilder<UserModel>().Include(x => x.Email)).FirstOrDefault();
                if (user != null)
                {
                    var projectDetails = GetProjectDetails(new GetProjectDetailsRequestModel { ProjectId = requestModel.ProjectId, UserEmail = user.Email });
                    if (projectDetails != null && !string.IsNullOrEmpty(projectDetails.SchemaId))
                    {
                        var schema = LanguageCollection.Find(x => x._id == projectDetails.SchemaId).FirstOrDefault();
                        if (schema != null)
                        {
                            #region ViewClass
                            UpdateClassViews(user.Email, requestModel.ProjectId, schema.Entity, projectDetails);
                            #endregion

                            #region WebAction
                            UpdateWebaactionClasses(user.Email, schema.Entity);
                            #endregion

                            foreach (var cls in schema.Entity.Classes.Where(x => x.ClassType == KClassType.BaseClass && x.Name != null))
                            {
                                tempOb = new Dictionary<string, object>();
                                if (cls.PropertyList != null)
                                    foreach (var prop in cls.PropertyList.Where(x => x.Name != null))
                                    {
                                        if (!tempOb.ContainsKey(prop.Name))
                                        {
                                            if (prop.Type != PropertyType.obj && prop.Type != PropertyType.array)
                                            {
                                                tempOb.Add(prop.Name, prop.Type);
                                            }
                                            else
                                            {
                                                if (prop.DataType != null && prop.DataType.Name != null)
                                                {
                                                    tempOb.Add(prop.Name, GenerateClassOb(schema.Entity.Classes.FirstOrDefault(x => (x.Name ?? "").ToLower() == prop.DataType.Name.Trim('[').Trim(']').ToLower()), schema.Entity.Classes, 0));
                                                }
                                            }
                                        }
                                    }
                                intellisenseOb.Add(cls.Name, tempOb);
                            }
                            return new GetIntellisenseResponseModel { Intellisense = intellisenseOb };
                        }
                    }

                }

                return null;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Handle versioning of the language schema
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static string VersionLanguageSchema(VersionLanguageSchema requestModel)
        {

            if (_kitsuneSchemaServer == null || _kitsuneServer == null)
                InitializeConnection();

            var newLanguage = new KLanguageModelProd();
            var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
            var prodLanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModelProd>(KitsuneProdLanguageCollectionName);
            var oldLanguage = LanguageCollection.Find(x => x._id == requestModel.LanguageId && x.UserId == requestModel.UserId).FirstOrDefault();
            if (newLanguage != null)
            {
                newLanguage.UserId = requestModel.UserId;
                newLanguage.CreatedOn = oldLanguage.CreatedOn;
                newLanguage.UpdatedOn = oldLanguage.UpdatedOn;
                newLanguage.Version = requestModel.Version;
                newLanguage.LanguageId = oldLanguage._id;
                newLanguage.Entity = oldLanguage.Entity;

                prodLanguageCollection.InsertOne(newLanguage);
                return newLanguage._id;
            }
            else
            {
                throw new Exception("Language with id : " + requestModel.LanguageId + ", does not exist");
            }
        }

        #region Helper Functions

        /// <summary>
        /// To Generate Class object for intellisence
        /// </summary>
        /// <param name="kclass"></param>
        /// <param name="classes"></param>
        /// <returns></returns>
        internal static Dictionary<string, object> GenerateClassOb(KClass kclass, IList<KClass> classes, int depth)
        {
            var tempOb = new Dictionary<string, object>();
            //prevent infinite depth 
            if (depth > 10)
                return tempOb;
            depth++;
            if (kclass != null)
            {
                if (kclass.PropertyList != null)
                    foreach (var prop in kclass.PropertyList.Where(x => x.Name != null))
                    {
                        if (!tempOb.ContainsKey(prop.Name))
                        {
                            if (prop.Type != PropertyType.obj && prop.Type != PropertyType.array)
                            {
                                tempOb.Add(prop.Name, prop.Type);
                            }
                            else
                            {
                                if (prop.DataType != null && prop.DataType.Name != null)
                                    tempOb.Add(prop.Name, GenerateClassOb(classes.FirstOrDefault(x => (x.Name ?? "").ToLower() == prop.DataType.Name.Trim('[').Trim(']').ToLower()), classes, depth));

                            }
                        }
                    }
            }

            return tempOb;
        }

        /// <summary>
        /// Update the language entity to handle the default classes and properties and validation
        /// </summary>
        /// <param name="entity"></param>
        private static void UpdateLanguageDefaultProperties(KEntity entity, bool iscreate = true)
        {

            var defaultProperties = new List<string> { "userid", "schemaid", COLLECTION_KEY_WEBISTE_ID, "rootaliasurl", COLLECTION_KEY_CREATED_ON, COLLECTION_KEY_UPDATED_ON, COLLECTION_KEY_IS_ARCHIVED, COLLECTION_KEY_ID, COLLECTION_KEY_KID, COLLECTION_KEY_PARENT_CLASS_NAME, COLLECTION_KEY_PROPERTY_NAME, COLLECTION_KEY_PARENT_CLASS_ID, COLLECTION_KEY_REFLECTION_ID };
            var dataTypeClassList = new List<string> { "str", "function", "number", "array", "datetime", "boolean", "kstring", "phonenumber" };

            if (iscreate)
            {
                if (string.IsNullOrEmpty(entity?.EntityName) || entity.EntityName.Length < 4)
                    throw new Exception("language name must be more than 3 characters");

                if (EnvConstants.Constants.LanguageClassNameValidator.IsMatch(entity.EntityName))
                    throw new Exception($"invalid language name : {entity.EntityName}, language name can only contain alphabets, numbers and underscore and must start with alphabet or underscore");

                entity.EntityName = entity.EntityName.ToLower();
            }


            //Validation for entity without base class
            if (entity.Classes == null || !entity.Classes.Any(x => x.ClassType == KClassType.BaseClass))
            {
                throw new Exception("Can not create language without base class");
            }
            else if (entity.Classes.Count(x => x.ClassType == KClassType.BaseClass) > 1)
            {
                throw new Exception("language must have only one base class");
            }

            var classesToRemove = entity.Classes.Where(x => dataTypeClassList.Contains(x.Name.ToLower())).ToList();
            if (classesToRemove.Any())
            {
                foreach (var cl in classesToRemove)
                {
                    entity.Classes.Remove(cl);
                }
            }

            //Add default classes
            var defaultClasses = LanguageDefaults.GetDataTypeClasses().Where(x => x.ClassType == KClassType.DataTypeClass);

            foreach (var dfc in defaultClasses)
            {
                entity.Classes.Add(dfc);
            }
            //Add image and link class if does not exist by default
            if (!entity.Classes.Any(x => x.Name?.ToLower() == "link"))
            {
                entity.Classes.Add(new KClass
                {
                    Name = "link",
                    ClassType = KClassType.UserDefinedClass,
                    PropertyList = new List<KProperty> {
                    new KProperty
                        {
                            IsRequired = true,
                            Name = "url",
                            DataType = new DataType("str"),
                            Description = "Absolute url",
                            Type = PropertyType.str,
                            Schemas = new Dictionary<string, string> { { "itemprop", "url"} }
                        },
                    new KProperty
                        {
                            Name = "description",
                            DataType = new DataType("str"),
                            Description = "Url description",
                            Type = PropertyType.str
                        },
                    }
                });
            }
            if (!entity.Classes.Any(x => x.Name?.ToLower() == "image"))
            {
                entity.Classes.Add(new KClass
                {
                    Name = "image",
                    ClassType = KClassType.UserDefinedClass,
                    PropertyList = new List<KProperty> {
                    new KProperty
                        {
                            IsRequired = true,
                            Name = "url",
                            DataType = new DataType("str"),
                            Description = "Image url",
                            Type = PropertyType.str,
                            Schemas = new Dictionary<string, string> { { "itemprop", "url"} }
                        },
                    new KProperty
                        {
                            Name = "description",
                            DataType = new DataType("str"),
                            Description = "Image description",
                            Type = PropertyType.str
                        },
                    }
                });
            }


            //Invalid class name validation
            var invalidClasses = entity.Classes.Where(x => string.IsNullOrEmpty(x.Name) || EnvConstants.Constants.LanguageClassNameValidator.IsMatch(x.Name)).Select(x => x.Name ?? "");
            if (invalidClasses != null && invalidClasses.Any())
            {
                throw new Exception($"Invalid class names : {string.Join(", ", invalidClasses)}");
            }

            //Validation for duplicate class name
            var groupClasses = entity.Classes.Where(x => !string.IsNullOrEmpty(x.Name)).GroupBy(x => x.Name.ToLower()).ToList();
            if (groupClasses != null && groupClasses.Any())
            {
                var duplicateClasses = groupClasses.Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                if (duplicateClasses.Count > 0)
                    throw new Exception($"Duplicate class names found : {string.Join(", ", duplicateClasses)}");
            }



            foreach (var cl in entity.Classes)
            {
                cl.Name = cl.Name.ToLower();

                if (cl.ClassType == KClassType.BaseClass && cl.Name != entity.EntityName)
                    throw new Exception("Base class name and language name not match");

                if (cl.PropertyList == null)
                    cl.PropertyList = new List<KProperty>();
                else
                {
                    var propertiesToRemove = cl.PropertyList.Where(x => defaultProperties.Contains(x.Name.ToLower())).ToList();
                    if (propertiesToRemove.Any())
                    {
                        foreach (var prop in propertiesToRemove)
                        {
                            cl.PropertyList.Remove(prop);
                        }
                    }
                }

                //Update the default property datatype with its name
                if ((cl.ClassType == KClassType.UserDefinedClass || cl.ClassType == KClassType.BaseClass) && (cl.PropertyList != null || cl.PropertyList.Any()))
                {
                    //Invalid properties validation
                    var invalidProperties = cl.PropertyList.Where(x => string.IsNullOrEmpty(x.Name) || EnvConstants.Constants.LanguageClassNameValidator.IsMatch(x.Name)).Select(x => x.Name ?? "");
                    if (invalidProperties != null && invalidProperties.Any())
                    {
                        throw new Exception($"Invalid property names in class {cl.Name.ToLower()} : {string.Join(", ", invalidProperties)}");
                    }
                    //foreach (var prop in cl.PropertyList.Where(x => x._AdvanceProperties != null))
                    //{
                    //    var tempAdvance = new Dictionary<string, object>();
                    //    foreach (var ap in prop._AdvanceProperties)
                    //    {
                    //        if (ap.Value.GetType() == typeof(JObject) || ap.Value.GetType() == typeof(JArray))
                    //        {

                    //            tempAdvance.Add(ap.Key, BsonDocument.Parse(ap.Value.ToString()));
                    //        }
                    //        else
                    //        { tempAdvance.Add(ap.Key, ap.Value); }
                    //    }
                    //    prop._AdvanceProperties = tempAdvance;

                    //}


                    //Duplicate properties validation
                    var groupProperties = cl.PropertyList.Where(x => !string.IsNullOrEmpty(x.Name)).GroupBy(x => x.Name.ToLower()).ToList();
                    if (groupProperties != null && groupProperties.Any())
                    {
                        var duplicateProperties = groupProperties.Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                        if (duplicateProperties.Count > 0)
                            throw new Exception($"Duplicate properties found in class {cl.Name.ToLower()} : {string.Join(", ", duplicateProperties)}");
                    }


                    foreach (var pr in cl.PropertyList)
                    {
                        pr.Name = pr.Name.ToLower();

                        pr.DataType = (pr.Type == PropertyType.array || pr.Type == PropertyType.obj) ? pr.DataType : new DataType(pr.Type.ToString().ToUpper());
                    }
                }


                #region Default Properties
                if (cl.ClassType == KClassType.BaseClass)
                {
                    cl.PropertyList.Add(new KProperty
                    {
                        Name = "userid",
                        DataType = new DataType("str"),
                        Description = "User Id",
                        IsRequired = true,
                        Type = PropertyType.str
                    });

                    cl.PropertyList.Add(new KProperty
                    {
                        Name = "schemaid",
                        DataType = new DataType("str"),
                        Description = "Schema Id",
                        IsRequired = true,
                        Type = PropertyType.str
                    });

                    cl.PropertyList.Add(new KProperty
                    {
                        Name = COLLECTION_KEY_WEBISTE_ID,
                        DataType = new DataType("str"),
                        Description = "Website Id (domain name)",
                        IsRequired = true,
                        Type = PropertyType.str
                    });
                    cl.PropertyList.Add(new KProperty
                    {
                        Name = "rootaliasurl",
                        DataType = new DataType("link"),
                        Description = "Rootalias Url",
                        IsRequired = true,
                        Type = PropertyType.obj
                    });
                }


                cl.PropertyList.Add(new KProperty
                {
                    Name = COLLECTION_KEY_CREATED_ON,
                    DataType = new DataType("datetime"),
                    Description = "Created On date",
                    IsRequired = true,
                    Type = PropertyType.date
                });
                cl.PropertyList.Add(new KProperty
                {
                    Name = COLLECTION_KEY_UPDATED_ON,
                    DataType = new DataType("datetime"),
                    Description = "Updated On date",
                    IsRequired = true,
                    Type = PropertyType.date
                });
                cl.PropertyList.Add(new KProperty
                {
                    Name = COLLECTION_KEY_IS_ARCHIVED,
                    DataType = new DataType("boolean"),
                    Description = "User Id",
                    IsRequired = true,
                    Type = PropertyType.boolean
                });
                cl.PropertyList.Add(new KProperty
                {
                    Name = COLLECTION_KEY_KID,
                    DataType = new DataType("str"),
                    Description = "Kitsune unique id",
                    IsRequired = true,
                    Type = PropertyType.str
                });

                #endregion



            }

        }

        /// <summary>
        /// Generate data object for language class
        /// </summary>
        /// <param name="actionData"></param>
        /// <param name="entity"></param>
        /// <param name="className"></param>
        /// <param name="userId"></param>
        /// <param name="newObj"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GenerateDataObject(BsonDocument actionData, KEntity entity, string className, BsonDocument newObj)
        {
            try
            {
                List<string> ErrorList = new List<string>();
                if (newObj == null)
                    newObj = new BsonDocument();
                var entityClass = entity.Classes.First(x => x.Name.ToLower() == className.ToLower());
                var key = string.Empty;

                var defaultProperties = new List<string> { "userid", "schemaid", COLLECTION_KEY_WEBISTE_ID, "rootaliasurl", COLLECTION_KEY_CREATED_ON, COLLECTION_KEY_UPDATED_ON, COLLECTION_KEY_IS_ARCHIVED, COLLECTION_KEY_ID, COLLECTION_KEY_KID, COLLECTION_KEY_PARENT_CLASS_ID, COLLECTION_KEY_PARENT_CLASS_NAME, COLLECTION_KEY_PROPERTY_NAME };

                foreach (var prop in defaultProperties)
                {
                    actionData.Remove(prop);
                }

                foreach (var prop in actionData)
                {
                    if (prop.Name == COLLECTION_KEY_REFLECTION_ID)
                    {
                        newObj.Add(prop);
                        continue;
                    }
                    key = prop.Name?.Trim()?.ToLower();
                    if (!entityClass.PropertyList.Any(x => x.Name.ToLower() == key))
                        ErrorList.Add(key + " is not valid property. It does not belong to " + className);
                    else
                    {
                        var actionType = entityClass.PropertyList.First(x => x.Name.ToLower() == key);
                        switch (actionType.Type)
                        {
                            case PropertyType.boolean:
                                {
                                    var temp = false;
                                    if (prop.Value != null && !bool.TryParse(prop.Value.ToString(), out temp))
                                        ErrorList.Add(prop.Value + " is not valid value for the property " + key + ".");
                                    else
                                        newObj.Add(key, temp);
                                }
                                break;
                            case PropertyType.date:
                                {
                                    DateTime temp = new DateTime();
                                    if (prop.Value != null && !DateTime.TryParse(prop.Value.ToString(), out temp))
                                        ErrorList.Add(prop.Value + " is not valid value for the property " + key + ".");
                                    else
                                        newObj.Add(key, temp);
                                }
                                break;
                            case PropertyType.number:
                                {
                                    double temp = new double();
                                    if (prop.Value != null && !double.TryParse(prop.Value.ToString(), out temp))
                                        ErrorList.Add(prop.Value + " is not valid value for the property " + key + ".");
                                    else
                                        newObj.Add(key, temp);
                                }
                                break;
                            case PropertyType.str:
                                {
                                    if (prop.Value != null && prop.Value != BsonNull.Value)
                                        newObj.Add(key, prop.Value.ToString());
                                }
                                break;
                            case PropertyType.array:
                                {
                                    if (prop.Value.GetType() == typeof(BsonArray))
                                    {
                                        var defaultClassList = new List<string> { "str", "datetime", "boolean", "number" };
                                        var bsonArray = prop.Value.AsBsonArray;
                                        var dataClassType = entityClass.PropertyList.First(x => x.Name.ToLower() == key.ToLower()).DataType;
                                        IEnumerable<string> validateObject = new List<string>();
                                        BsonArray newArray = new BsonArray();

                                        //Handle the default type (str, boolean, date, number) for array
                                        if (defaultClassList.Contains(dataClassType.Name.ToLower()))
                                        {
                                            ErrorList.AddRange(ValidateDataTypeArray(dataClassType.Name.ToLower(), key, bsonArray, out newArray));
                                        }
                                        //handle the object array
                                        else
                                        {
                                            //newArray = jarray;
                                            foreach (var item in bsonArray)
                                            {
                                                var objclass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == dataClassType.Name.ToLower());
                                                if (objclass == null)
                                                    ErrorList.Add($"Invalid datatype '{dataClassType.Name}' of the Property '{prop.Name}'");
                                                else
                                                {
                                                    BsonDocument newArrayItemOb = null;
                                                    try
                                                    {
                                                        newArrayItemOb = item.AsBsonDocument;
                                                    }
                                                    catch { }
                                                    if (newArrayItemOb != null)
                                                    {
                                                        var newObjTemp = new BsonDocument();
                                                        validateObject = GenerateDataObject(newArrayItemOb, entity, objclass.Name, newObjTemp);
                                                        newArray.Add(newObjTemp);
                                                        if (validateObject.Any())
                                                            ErrorList.AddRange(validateObject);
                                                    }
                                                    else
                                                    {
                                                        ErrorList.Add($"{item.ToString()} is not valid object of type '{objclass.Name}'");
                                                    }
                                                }

                                            }
                                        }

                                        newObj.Add(new BsonElement(key, newArray));
                                    }
                                    else if (!prop.Value.IsBsonNull)
                                    {
                                        ErrorList.Add("Provide array value for the property " + key + ".");
                                    }
                                }
                                break;
                            case PropertyType.phonenumber:
                            case PropertyType.kstring:
                            case PropertyType.obj:
                                {
                                    var dataClassType = entityClass.PropertyList.First(x => x.Name.ToLower() == key).DataType;
                                    BsonDocument newOb = null;
                                    try
                                    {
                                        newOb = prop.Value.AsBsonDocument;
                                    }
                                    catch { }
                                    if (!prop.Value.IsBsonNull && newOb == null)
                                        ErrorList.Add($"{prop.Name} is not valid object'");
                                    else if (!prop.Value.IsBsonNull)
                                    {
                                        var objclass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == dataClassType.Name.ToLower());
                                        if (objclass == null)
                                            ErrorList.Add($"Invalid datatype '{dataClassType.Name}' of the Property '{prop.Name}'");
                                        else
                                        {
                                            var newObjTemp = new BsonDocument();
                                            var validateObject = GenerateDataObject(newOb, entity, objclass.Name, newObjTemp);
                                            if (validateObject.Any())
                                                ErrorList.AddRange(validateObject);
                                            else
                                            {
                                                newObj.Add(key, newObjTemp);
                                            }
                                        }

                                    }


                                }
                                break;

                                //case Models.WebActionPropertyType.reference: break;
                        }
                    }
                }
                return ErrorList;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Validate the Array of system datatype
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="propertyName"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        private static List<string> ValidateDataTypeArray(string dataType, string propertyName, BsonArray array, out BsonArray outarray)
        {
            List<string> ErrorList = new List<string>();
            outarray = new BsonArray();
            foreach (var item in array)
            {
                switch (dataType.ToLower())
                {
                    case "boolean":
                        {
                            var temp = false;
                            if (item.ToString() != null && !bool.TryParse(item.ToString(), out temp))
                                ErrorList.Add(item.ToString() + " is not valid value for the property " + propertyName.ToLower() + ".");
                            else
                                outarray.Add(temp);
                        }
                        break;
                    case "date"://TODO : Remove date once all schema migration done 
                    case "datetime":
                        {
                            DateTime temp = new DateTime();
                            if (item.ToString() != null && !DateTime.TryParse(item.ToString(), out temp))
                                ErrorList.Add(item.ToString() + " is not valid value for the property " + propertyName.ToLower() + ".");
                            else
                                outarray.Add(new BsonDateTime(temp));
                        }
                        break;
                    case "number":
                        {
                            double temp = new double();
                            if (item.ToString() != null && !double.TryParse(item.ToString(), out temp))
                                ErrorList.Add(item.ToString() + " is not valid value for the property " + propertyName.ToLower() + ".");
                            else
                                outarray.Add(temp);
                        }
                        break;
                    case "str":
                        {
                            if (item.GetType() != typeof(BsonString))
                                ErrorList.Add(item.ToString() + " is not valid value for the property " + propertyName.ToLower() + ".");
                            else
                                outarray.Add(item?.ToString());
                        }
                        break;
                }
            }
            return ErrorList;

        }

        /// <summary>
        /// Insert individual dataobject to collection of type
        /// </summary>
        /// <param name="dataObject"></param>
        /// <param name="entity"></param>
        /// <param name="kClass"></param>
        /// <param name="referenceId"></param>
        /// <param name="baseCollection"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal static IEnumerable<string> InsertObject(BsonDocument dataObject,
            KEntity entity,
            KClass kClass,
            string baseCollection,
            string userId,
            string websiteId,
            string reflectionId,
            string parentId,
            string parentClassName,
            string propertyName,
            Dictionary<string, bool> componentStatus)
        {
            phoneNumber assignedNumber = new phoneNumber();
            List<string> ErrorList = new List<string>();
            var collectionName = baseCollection;
            //if (kClass == null)
            //    return new List<string> { "KClass can not be empty" };
            if (kClass.ClassType != KClassType.BaseClass)
                collectionName = string.Format("{0}_{1}", collectionName, kClass.Name.ToLower());


            var dataCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(collectionName);
            var id = ObjectId.GenerateNewId();
            dataObject.Remove(COLLECTION_KEY_ID);
            dataObject.Remove(COLLECTION_KEY_KID);
            dataObject.Remove(COLLECTION_KEY_CREATED_ON);
            dataObject.Remove(COLLECTION_KEY_UPDATED_ON);
            dataObject.Remove(COLLECTION_KEY_IS_ARCHIVED);
            dataObject.Remove(COLLECTION_KEY_WEBISTE_ID);
            dataObject.Remove(COLLECTION_KEY_REFLECTION_ID);
            dataObject.Remove(COLLECTION_KEY_PARENT_CLASS_ID);
            dataObject.Remove(COLLECTION_KEY_PARENT_CLASS_NAME);
            dataObject.Remove(COLLECTION_KEY_PROPERTY_NAME);


            dataObject.Add(COLLECTION_KEY_ID, id);
            dataObject.Add(COLLECTION_KEY_KID, id.ToString());
            var timeStamp = DateTime.UtcNow;

            dataObject.Add(COLLECTION_KEY_CREATED_ON, timeStamp);
            dataObject.Add(COLLECTION_KEY_UPDATED_ON, timeStamp);
            dataObject.Add(COLLECTION_KEY_IS_ARCHIVED, false);
            dataObject.Add(COLLECTION_KEY_WEBISTE_ID, websiteId);


            if (!string.IsNullOrEmpty(reflectionId))
                dataObject.Add(COLLECTION_KEY_REFLECTION_ID, reflectionId);
            if (!string.IsNullOrEmpty(parentId))
                dataObject.Add(COLLECTION_KEY_PARENT_CLASS_ID, parentId);
            if (!string.IsNullOrEmpty(parentClassName))
                dataObject.Add(COLLECTION_KEY_PARENT_CLASS_NAME, parentClassName);
            if (!string.IsNullOrEmpty(propertyName))
                dataObject.Add(COLLECTION_KEY_PROPERTY_NAME, propertyName);

            #region KString API Queue
            //TODO : handle the reference id in keyword extraction 
            SendToKStringKeywordExtractors(kClass, dataObject, id.ToString(), $"{parentId}-{parentClassName}-{propertyName}", entity.EntityName, userId);
            #endregion

            #region Call Tracker 
            //TODO : handle the reference id in keyword extraction 
            if (componentStatus[EnvConstants.Constants.ComponentId.callTracker])
                assignedNumber = CreateCallTracker(kClass, dataObject, websiteId);
            #endregion



            var objects = kClass.PropertyList.Where(x => x.Type == PropertyType.obj || x.Type == PropertyType.kstring || x.Type == PropertyType.phonenumber);
            var dataTypeObjects = new string[] { "str", "number", "datetime", "boolean" };
            var arrays = kClass.PropertyList.Where(x => x.Type == PropertyType.array && x.DataType != null && x.DataType.Name != null && !dataTypeObjects.Contains(x.DataType.Name.Trim('[', ']').ToLower()));
            var propertiesToRemove = objects.Any() ? objects.ToList() : new List<KProperty>();
            if (arrays.Any())
                propertiesToRemove.AddRange(arrays);
            var tempBsonDoc = new BsonDocument();
            var tempBsonArray = new BsonArray();
            foreach (var prop in propertiesToRemove)
            {
                if (prop.Type == PropertyType.array)
                {
                    var dataOb = dataObject.FirstOrDefault(x => x.Name.ToLower() == prop.Name.ToLower());
                    if (dataOb != null)
                        tempBsonArray = dataOb.Value?.AsBsonArray;

                    // tempBsonArray = dataObject[prop.Name]?.AsBsonArray;
                    if (tempBsonArray != null && tempBsonArray.Count > 0)
                    {
                        foreach (var arrItem in tempBsonArray)
                        {
                            tempBsonDoc = arrItem?.AsBsonDocument;

                            if (tempBsonDoc != null && tempBsonDoc != new BsonDocument())
                            {
                                var newClass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == prop.DataType.Name.Trim('[', ']').ToLower());
                                InsertObject(tempBsonDoc,
                                            entity,
                                            newClass,
                                            baseCollection,
                                            userId,
                                            websiteId,
                                            null,
                                            id.ToString(),
                                            kClass.Name.ToLower(),
                                            prop.Name.ToLower(),
                                            componentStatus
                                            );
                            }

                        }
                    }
                }
                else
                {
                    tempBsonDoc = new BsonDocument();
                    var dataOb = dataObject.FirstOrDefault(x => x.Name.ToLower() == prop.Name.ToLower());
                    if (dataOb != null && dataOb.Value != null)
                        tempBsonDoc = dataOb.Value?.AsBsonDocument;
                    //tempBsonDoc = dataObject[prop.Name]?.AsBsonDocument;
                    if (tempBsonDoc != null && tempBsonDoc != new BsonDocument())
                    {
                        var newClass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == prop.DataType.Name.Trim('[', ']').ToLower());
                        InsertObject(tempBsonDoc,
                                      entity,
                                      newClass,
                                      baseCollection,
                                      userId,
                                      websiteId,
                                      null,
                                      id.ToString(),
                                      kClass.Name.ToLower(),
                                      prop.Name.ToLower(),
                                      componentStatus);
                    }

                }
                var dobj = dataObject.Where(x => x.Name.ToLower() == prop.Name.ToLower()).FirstOrDefault();
                if (dobj != null && dobj.Value != null)
                    dataObject.Remove(dobj.Name);
            }
            dataCollection.InsertOne(dataObject);


            return ErrorList;
        }

        /// <summary>
        ///   Get recursive object of the schema from the different collection
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="kClassName"></param>
        /// <param name="propertyName"></param>
        /// <param name="isArray"></param>
        /// <returns></returns>
        private static List<BsonDocument> GetObject(KEntity entity,
            string kClassName,
            string parentClassName,
            string propertyName,
            string baseCollectionName,
            string parentId,
            Dictionary<string, bool> projectDefinition = null,
            Dictionary<string, int> sort = null,
            Dictionary<string, object> query = null,
            int? skip = null,
            int? limit = null)
        {
            var dataTypeObjects = new string[] { "str", "number", "datetime", "boolean" };
            if(dataTypeObjects.Contains( kClassName ))
            {
                kClassName = parentClassName;
            }
            parentClassName = parentClassName?.ToLower();
            kClassName = kClassName?.ToLower();
            propertyName = propertyName?.ToLower();
            var kClass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == kClassName.ToLower());
            var kParentClass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == parentClassName.ToLower());
            if (kClass != null)
            {
                var isBase = kClass.ClassType == KClassType.BaseClass;
                IMongoCollection<BsonDocument> classCollection = null;
                if (kClass.ClassType == KClassType.DataTypeClass && kClass.Name.ToLower() != "kstring" && kClass.Name.ToLower() != "phonenumber")
                {
                    classCollection = isBase
                        ? _kitsuneSchemaDatabase.GetCollection<BsonDocument>(baseCollectionName)
                        : _kitsuneSchemaDatabase.GetCollection<BsonDocument>(string.Format("{0}_{1}", baseCollectionName, parentClassName.ToLower()));
                }
                else
                {
                    classCollection = isBase
                    ? _kitsuneSchemaDatabase.GetCollection<BsonDocument>(baseCollectionName)
                    : _kitsuneSchemaDatabase.GetCollection<BsonDocument>(string.Format("{0}_{1}", baseCollectionName, kClassName.ToLower()));
                }


                var findOb = new BsonDocument();
                var defaultQueryParams = new Dictionary<string, object>();



                //defaultQueryParams.Add(COLLECTION_KEY_PARENT_CLASS_NAME, parentClassName);
                defaultQueryParams.Add(COLLECTION_KEY_PARENT_CLASS_ID, parentId);

                if (!string.IsNullOrEmpty(propertyName))
                    defaultQueryParams.Add(COLLECTION_KEY_PROPERTY_NAME, propertyName);
                defaultQueryParams.Add("isarchived", false);
                if (query != null)
                {
                    try
                    {
                        findOb = BsonDocument.Parse(JsonConvert.SerializeObject(query));
                    }
                    catch (Exception ex)
                    {
                        if (ex.Data != null)
                            ex.Data.Add("Invalid query", JsonConvert.SerializeObject(query));
                        throw ex;
                    }
                }
                if (isBase)
                    findOb.Add(COLLECTION_KEY_KID, parentId);
                else
                    findOb.AddRange(defaultQueryParams);

                IFindFluent<BsonDocument, BsonDocument> ob = null;
                if (EnvironmentConstants.ApplicationConfiguration != null && EnvironmentConstants.ApplicationConfiguration.MongoQueryMaxtimeOut > 0)
                    ob = classCollection
                       .Find(findOb, new FindOptions { MaxTime = TimeSpan.FromMilliseconds(EnvironmentConstants.ApplicationConfiguration.MongoQueryMaxtimeOut) });
                else
                    ob = classCollection
                       .Find(findOb);

                //Handle sorting 
                if (sort != null && sort.Any())
                {
                    ob = ob.Sort(new BsonDocument(sort));
                }
                else
                {
                    ob = ob.Sort(new SortDefinitionBuilder<BsonDocument>().Descending(COLLECTION_KEY_ID));
                }
                //Handle query

                if (skip != null)
                    ob.Skip(skip);
                if (limit != null)
                    ob.Limit(limit);
                if (projectDefinition != null && projectDefinition.Any())
                {
                    var projDoc = new BsonDocument();
                    foreach (var proj in projectDefinition)
                    {
                        projDoc.Add(proj.Key, proj.Value);
                    }

                    if (!projectDefinition.Any(x => x.Value == false) && !projDoc.Contains(COLLECTION_KEY_KID))
                        projDoc.Add(COLLECTION_KEY_KID, true);
                    ob = ob.Project(projDoc);
                }

                var finalResult = new List<BsonDocument>();
                var listDocs = ob.ToList();
                //if (ob != null && ob.Any())
                if (listDocs?.Count > 0)
                {

                    var classProperties = kClass.PropertyList.Where(x => (x.Type == PropertyType.array && !dataTypeObjects.Contains(x.DataType?.Name?.ToLower())) || x.Type == PropertyType.obj || x.Type == PropertyType.kstring || x.Type == PropertyType.phonenumber);

                    foreach (var item in listDocs)
                    {
                        foreach (var prop in classProperties)
                        {
                            if (projectDefinition == null || projectDefinition.Any(x => x.Value == true && x.Key.Contains($"{propertyName}.{prop.Name.ToLower()}")) || projectDefinition.Any(x => x.Value == true && x.Key.Contains($"{prop.Name.ToLower()}")))
                            {
                                var resob = GetObject(entity, prop.DataType.Name.Trim('[', ']'), kClassName, prop.Name, baseCollectionName, item[COLLECTION_KEY_KID].AsString);
                                if (prop.Type == PropertyType.array)
                                {
                                    var arr = new BsonArray();
                                    foreach (var arritem in resob)
                                    {
                                        arr.Add(arritem.AsBsonValue);
                                    }
                                    item.Add(prop.Name.ToLower(), arr);
                                }
                                else if (resob.Any())
                                    item.Add(prop.Name.ToLower(), resob.FirstOrDefault());
                            }
                        }
                        item.Remove(COLLECTION_KEY_ID);
                        finalResult.Add(item);

                    }
                }
                return finalResult;
            }
            throw new Exception($"Class : {kClassName} does not found");
        }
        /// <summary>
        ///   Get recursive object of the schema from the different collection
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="kClassName"></param>
        /// <param name="propertyName"></param>
        /// <param name="isArray"></param>
        /// <returns></returns>
        private static List<BsonDocument> GetObjectFromInMemoryCollections(KEntity entity, string kClassName, string parentClassName, string propertyName, string baseCollectionName, string parentId, Dictionary<string, List<BsonDocument>> inMemoryCollections, List<string> projectDefinition = null)
        {
            parentClassName = parentClassName?.ToLower();
            kClassName = kClassName?.ToLower();
            propertyName = propertyName?.ToLower();
            var kClass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == kClassName.ToLower());

            if (kClass != null && inMemoryCollections.ContainsKey($"{baseCollectionName}_{kClassName.ToLower()}"))
            {
                var classCollection = inMemoryCollections[string.Format("{0}_{1}", baseCollectionName, kClassName.ToLower())];
                var ob = classCollection.Where(x => x[COLLECTION_KEY_PARENT_CLASS_NAME].AsString == parentClassName && x[COLLECTION_KEY_PROPERTY_NAME].AsString == propertyName && x[COLLECTION_KEY_PARENT_CLASS_ID].AsString == parentId && x["isarchived"].AsBoolean == false).ToList();

                var finalResult = new List<BsonDocument>();
                if (ob != null && ob.Any())
                {
                    var dataTypeObjects = new string[] { "str", "number", "datetime", "boolean" };

                    var classProperties = kClass.PropertyList.Where(x => (x.Type == PropertyType.array && !dataTypeObjects.Contains(x.DataType?.Name?.ToLower())) || x.Type == PropertyType.obj || x.Type == PropertyType.kstring || x.Type == PropertyType.phonenumber);
                    foreach (var item in ob.ToList())
                    {
                        foreach (var prop in classProperties)
                        {
                            if (projectDefinition == null || projectDefinition.Any(x => x.Contains($"{propertyName}.{prop.Name.ToLower()}")))
                            {
                                var resob = GetObjectFromInMemoryCollections(entity, prop.DataType.Name.Trim('[', ']'), kClassName, prop.Name, baseCollectionName, item[COLLECTION_KEY_KID].AsString, inMemoryCollections);
                                if (prop.Type == PropertyType.array)
                                {
                                    var arr = new BsonArray();
                                    foreach (var arritem in resob)
                                    {
                                        arr.Add(arritem.AsBsonValue);
                                    }
                                    item.Remove(prop.Name.ToLower());
                                    item.Add(prop.Name.ToLower(), arr);
                                }
                                else if (resob.Any())
                                {
                                    item.Remove(prop.Name.ToLower());
                                    item.Add(prop.Name.ToLower(), resob.FirstOrDefault());
                                }
                            }
                        }
                        item.Remove(COLLECTION_KEY_ID);
                        finalResult.Add(item);

                    }
                }
                return finalResult;
            }
            else if (kClass == null)
                throw new Exception($"Class : {kClassName} does not found");
            else
                return new List<BsonDocument>();
        }
        /// <summary>
        /// Update the individual class object in the collection
        /// </summary>
        /// <param name="updateObject">Contains fields to be updated</param>
        /// <param name=COLLECTION_KEY_KID>Unique id of the updated</param>
        /// <param name="collectionName"></param>
        private static InsertUpdateResult InsertOrUpdateObject(KEntity entity, KClass kClass, BsonDocument updateDoc, BsonDocument queryDoc, string collectionName, string parentClassName, string parentId, string websiteId, string propertyName, string schemaName, string userId, List<ReferenceItem> referenceList, Dictionary<string, bool> componentStatus, bool ignoreNested = false)
        {
            phoneNumber assignedNumber = new phoneNumber();
            var collection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(collectionName);
            parentClassName = parentClassName?.ToLower();
            var identityProperties = new List<string> { COLLECTION_KEY_ID, COLLECTION_KEY_KID, COLLECTION_KEY_PARENT_CLASS_ID, COLLECTION_KEY_PARENT_CLASS_NAME, COLLECTION_KEY_PROPERTY_NAME, COLLECTION_KEY_REFLECTION_ID, COLLECTION_KEY_CREATED_ON, COLLECTION_KEY_UPDATED_ON, COLLECTION_KEY_IS_ARCHIVED, COLLECTION_KEY_WEBISTE_ID };
            var defaultProperties = new List<string> { "userid", "schemaid", COLLECTION_KEY_WEBISTE_ID, "rootaliasurl", COLLECTION_KEY_CREATED_ON, COLLECTION_KEY_UPDATED_ON, COLLECTION_KEY_IS_ARCHIVED, COLLECTION_KEY_ID, COLLECTION_KEY_KID, COLLECTION_KEY_PARENT_CLASS_ID, COLLECTION_KEY_PARENT_CLASS_NAME, COLLECTION_KEY_PROPERTY_NAME };
            var propertiesToRemove = defaultProperties;
            var nestedProperties = new Dictionary<KProperty, List<BsonDocument>>();
            var resultKid = string.Empty;
            foreach (var field in updateDoc)
            {
                //field.Name = field.Name.Trim().ToLower();
                //Remove all the extra properties which is not in schema and also nested properties without array of default datatype
                var kProp = kClass.PropertyList.FirstOrDefault(x => x.Name?.ToLower() == field.Name.ToLower());
                var defaultClassList = new string[] { "str", "number", "datetime", "boolean" };

                if ((field.Name != COLLECTION_KEY_REFLECTION_ID) &&
                      (kProp == null ||
                      (kProp.Type == PropertyType.array && kProp.DataType != null && !defaultClassList.Contains(kProp.DataType.Name?.ToLower())) ||
                      kProp.Type == PropertyType.obj ||
                      kProp.Type == PropertyType.kstring ||
                      kProp.Type == PropertyType.phonenumber))
                {
                    propertiesToRemove.Add(field.Name);
                    if (updateDoc[field.Name].GetType() == typeof(BsonDocument))
                        nestedProperties.Add(kProp, new List<BsonDocument> { updateDoc[field.Name].AsBsonDocument });
                    else if (updateDoc[field.Name].GetType() == typeof(BsonArray))
                    {
                        nestedProperties.Add(kProp, updateDoc[field.Name].AsBsonArray.Select(x => (BsonDocument)x).ToList());
                    }
                }
            }

            foreach (var prop in propertiesToRemove)
            {
                updateDoc.Remove(prop);
            }
            BsonDocument bsonDoc = new BsonDocument();

            var Errors = GenerateDataObject(updateDoc, entity, kClass.Name, bsonDoc);
            if (Errors != null && Errors.Any())
            {
                Exception ex = new Exception("Checkout the <ErroList> field in Data Property for list of errors");
                ex.Data["ErrorList"] = Errors;
                throw ex;
            }

            var dateTime = DateTime.UtcNow;
            InsertUpdateResult result = null;
            //Update object if _kid is present
            if (queryDoc.Contains(COLLECTION_KEY_KID) && queryDoc[COLLECTION_KEY_KID] != BsonNull.Value)
            {

                #region KString API Queue
                SendToKStringKeywordExtractors(kClass, bsonDoc, queryDoc[COLLECTION_KEY_KID].AsString, $"{parentId}-{parentClassName}-{propertyName}", schemaName, userId);
                #endregion

                #region Call Tracker 
                if (componentStatus[EnvConstants.Constants.ComponentId.callTracker])
                    assignedNumber = UpdateCallTracker(kClass, bsonDoc, websiteId, parentClassName, parentId, propertyName, queryDoc[COLLECTION_KEY_KID].AsString, userId, schemaName);
                #endregion

                var keyId = queryDoc[COLLECTION_KEY_KID];
                resultKid = keyId.AsString;
                if (kClass.ReferenceType == KReferenceType.Reference || kClass.ReferenceType == KReferenceType.Hybrid)
                {
                    BsonDocument parentDocument = collection.Find(queryDoc).FirstOrDefault();

                    if (kClass.ReferenceType == KReferenceType.Reference && parentDocument.Contains(COLLECTION_KEY_KID) && updateDoc.Contains(COLLECTION_KEY_REFLECTION_ID) && parentDocument[COLLECTION_KEY_KID] != BsonNull.Value && updateDoc[COLLECTION_KEY_REFLECTION_ID].AsString == parentDocument[COLLECTION_KEY_KID].AsString)
                        throw new Exception("Self reflection detected");


                    if (kClass.ReferenceType == KReferenceType.Reference && parentDocument.Contains(COLLECTION_KEY_REFLECTION_ID) && parentDocument[COLLECTION_KEY_REFLECTION_ID] != BsonNull.Value && updateDoc[COLLECTION_KEY_REFLECTION_ID] == parentDocument[COLLECTION_KEY_REFLECTION_ID])
                    {
                        string reflectionId = parentDocument[COLLECTION_KEY_REFLECTION_ID].AsString;
                        queryDoc.Remove(COLLECTION_KEY_KID);
                        queryDoc.Add(COLLECTION_KEY_KID, reflectionId);
                        updateDoc.Remove(COLLECTION_KEY_REFLECTION_ID);
                        result = InsertOrUpdateObject(entity, kClass, updateDoc, queryDoc, collectionName, null, null, websiteId, null, schemaName, userId, referenceList, componentStatus);
                    }
                    else
                    {
                        var updateSetDoc = new BsonDocument();
                        updateSetDoc.Add("$set", bsonDoc);

                        if (kClass.ReferenceType == KReferenceType.Hybrid)
                        {
                            updateSetDoc["$set"].AsBsonDocument.Remove(COLLECTION_KEY_REFLECTION_ID);
                            updateSetDoc.Add("$unset", COLLECTION_KEY_REFLECTION_ID);
                        }
                        var updateres = collection.UpdateOne(queryDoc, updateSetDoc);
                        result = new InsertUpdateResult
                        {
                            Kid = keyId.AsString,
                            ParentClassName = kClass.Name?.ToLower(),
                            PropertyName = propertyName,
                            ParentClassId = parentId,
                            Status = updateres.IsAcknowledged ? InsertUpdateStatus.Updated : InsertUpdateStatus.Error
                        };

                        #region Reference Objects
                        parentDocument = collection.Find(queryDoc).FirstOrDefault();

                        List<BsonDocument> childDocuments = collection.Find<BsonDocument>(x => x[COLLECTION_KEY_REFLECTION_ID] == keyId).ToList();
                        foreach (BsonDocument childDocument in childDocuments)
                        {
                            foreach (var field in parentDocument)
                            {
                                if (!identityProperties.Contains(field.Name))
                                {
                                    if (childDocument.Contains(field.Name))
                                    {
                                        childDocument.Remove(field.Name);
                                    }
                                    childDocument.Add(field);
                                }
                            }
                            queryDoc.Remove(COLLECTION_KEY_KID);
                            childDocument.Remove(COLLECTION_KEY_ID);
                            queryDoc.Add(COLLECTION_KEY_KID, childDocument[COLLECTION_KEY_KID]);
                            childDocument[COLLECTION_KEY_UPDATED_ON] = dateTime;

                            updateSetDoc = new BsonDocument();
                            updateSetDoc.Add("$set", childDocument);


                            collection.UpdateOne(queryDoc, updateSetDoc);
                        }
                        #endregion
                    }
                }
                else if (kClass.ReferenceType == KReferenceType.Value)
                {
                    bsonDoc.Remove(COLLECTION_KEY_REFLECTION_ID);
                    bsonDoc.Add(COLLECTION_KEY_UPDATED_ON, dateTime);

                    var updateSetDoc = new BsonDocument();
                    updateSetDoc.Add("$set", bsonDoc);

                    var updateres = collection.UpdateOne(queryDoc, updateSetDoc);
                    result = new InsertUpdateResult
                    {
                        Kid = keyId.AsString,
                        ParentClassName = kClass.Name?.ToLower(),
                        PropertyName = propertyName,
                        ParentClassId = parentId,
                        Status = updateres.IsAcknowledged ? InsertUpdateStatus.Updated : InsertUpdateStatus.Error
                    };


                    #region Update the reference documents
                    List<BsonDocument> childDocuments = collection.Find<BsonDocument>(x => x[COLLECTION_KEY_REFLECTION_ID] == keyId).ToList();
                    foreach (BsonDocument childDocument in childDocuments)
                    {
                        childDocument.Remove(COLLECTION_KEY_REFLECTION_ID);
                        childDocument.Remove(COLLECTION_KEY_ID);
                        childDocument[COLLECTION_KEY_UPDATED_ON] = dateTime;
                        queryDoc.Remove(COLLECTION_KEY_KID);
                        queryDoc.Add(COLLECTION_KEY_KID, childDocument[COLLECTION_KEY_KID]);


                        updateSetDoc = new BsonDocument();
                        updateSetDoc.Add("$set", childDocument);

                        collection.UpdateOne(queryDoc, updateSetDoc);
                    }
                    #endregion
                }
            }
            //Insert object if _kid is not present
            else
            {
                var id = ObjectId.GenerateNewId();
                resultKid = id.ToString();

                bsonDoc.Add(COLLECTION_KEY_ID, id);
                bsonDoc.Add(COLLECTION_KEY_CREATED_ON, dateTime);
                bsonDoc.Add(COLLECTION_KEY_UPDATED_ON, dateTime);
                bsonDoc.Add(COLLECTION_KEY_PARENT_CLASS_ID, parentId);
                bsonDoc.Add(COLLECTION_KEY_PARENT_CLASS_NAME, parentClassName);
                bsonDoc.Add(COLLECTION_KEY_PROPERTY_NAME, propertyName);
                bsonDoc.Add(COLLECTION_KEY_KID, id.ToString());
                bsonDoc.Add(COLLECTION_KEY_WEBISTE_ID, websiteId);
                bsonDoc.Add(COLLECTION_KEY_IS_ARCHIVED, false);

                #region KString API Queue
                SendToKStringKeywordExtractors(kClass, bsonDoc, id.ToString(), $"{parentId}-{parentClassName}-{propertyName}", schemaName, userId);
                #endregion

                #region Call Tracker 
                if (componentStatus[EnvConstants.Constants.ComponentId.callTracker])
                    assignedNumber = CreateCallTracker(kClass, bsonDoc, websiteId);
                #endregion

                //Referencing
                if (updateDoc.Contains(COLLECTION_KEY_REFLECTION_ID))
                {
                    var parentData = collection.Find<BsonDocument>(x => x[COLLECTION_KEY_KID] == updateDoc[COLLECTION_KEY_REFLECTION_ID]).FirstOrDefault();
                    foreach (var field in parentData)
                    {
                        if (!identityProperties.Contains(field.Name))
                        {
                            if (bsonDoc.Contains(field.Name))
                            {
                                bsonDoc.Remove(field.Name);
                            }
                            bsonDoc.Add(field);
                        }
                    }
                }
                //Insert main object
                collection.InsertOne(bsonDoc);
                result = new InsertUpdateResult
                {
                    Kid = id.ToString(),
                    ParentClassId = parentId,
                    ParentClassName = parentClassName,
                    PropertyName = propertyName,
                    Status = InsertUpdateStatus.Inserted
                };
                var pId = bsonDoc[COLLECTION_KEY_KID];
                List<string> childClassNames = kClass.PropertyList.Where(p => p.Type == PropertyType.obj || p.Type == PropertyType.kstring || p.Type == PropertyType.phonenumber).Select(p => p.DataType.Name.ToLower()).ToList();
                List<KClass> childClasses = entity.Classes.Where(c => childClassNames.Contains(c.Name)).ToList();
                foreach (KClass childClass in childClasses)
                {
                    if (childClass.ReferenceType == KReferenceType.Value)
                    {
                        continue;
                    }
                    var childCollectionName = EnvConstants.Constants.GenerateSchemaName(entity.EntityName) + (childClass.ClassType != KClassType.BaseClass ? string.Format("_{0}", childClass.Name.ToLower()) : "");
                    List<BsonDocument> children = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(childCollectionName)
                                            .Find(x => x[COLLECTION_KEY_PARENT_CLASS_ID] == pId).ToList();
                    foreach (BsonDocument child in children)
                    {
                        BsonDocument newQueryDoc = new BsonDocument();
                        newQueryDoc[COLLECTION_KEY_REFLECTION_ID] = child[COLLECTION_KEY_KID];
                        child.Remove(COLLECTION_KEY_KID);
                        child[COLLECTION_KEY_REFLECTION_ID] = child[COLLECTION_KEY_KID];
                        child[COLLECTION_KEY_PARENT_CLASS_ID] = id;
                        InsertOrUpdateObject(entity, childClass, child, newQueryDoc, childCollectionName, kClass.Name, id.ToString(), websiteId, child[COLLECTION_KEY_PROPERTY_NAME].AsString, schemaName, userId, null, componentStatus);
                    }
                }

                //Reverse referencing
                if (referenceList != null)
                {
                    var reflectionId = id;
                    foreach (ReferenceItem referenceItem in referenceList)
                    {
                        foreach (var prop in identityProperties)
                        {
                            bsonDoc.Remove(prop);
                        }
                        id = ObjectId.GenerateNewId();
                        bsonDoc.Add(COLLECTION_KEY_ID, id);
                        bsonDoc.Add(COLLECTION_KEY_PARENT_CLASS_ID, referenceItem.ParentId);
                        bsonDoc.Add(COLLECTION_KEY_PARENT_CLASS_NAME, referenceItem.ParentClassName);
                        bsonDoc.Add(COLLECTION_KEY_PROPERTY_NAME, referenceItem.PropertyName);
                        bsonDoc.Add(COLLECTION_KEY_KID, id.ToString());
                        bsonDoc.Add(COLLECTION_KEY_REFLECTION_ID, reflectionId.ToString());
                        collection.InsertOne(bsonDoc);
                    }
                }
            }
            #region Nested Object Update
            if (!ignoreNested && !string.IsNullOrEmpty(resultKid) && nestedProperties != null && nestedProperties.Any())
                result.NestedObjects = NestedObjectUpdate(nestedProperties, entity, kClass.Name.ToLower(), schemaName, resultKid, websiteId, userId, referenceList, componentStatus);

            #endregion

            return result;

        }
        private static List<InsertUpdateResult> NestedObjectUpdate(Dictionary<KProperty, List<BsonDocument>> nestedProperties, KEntity entity, string parentClassName, string schemaName, string parentId, string websiteId, string userId, List<ReferenceItem> referenceList, Dictionary<string, bool> componentStatus)
        {
            try
            {
                var result = new List<InsertUpdateResult>();
                if (nestedProperties != null && nestedProperties.Any())
                {
                    foreach (var nestedProperty in nestedProperties)
                    {
                        var nestedQuery = new BsonDocument();
                        KClass nestedKlass = entity.Classes.FirstOrDefault(x => x.Name.ToLower() == nestedProperty.Key.DataType?.Name?.ToLower());
                        if (nestedKlass != null)
                        {
                            foreach (var nestedOb in nestedProperty.Value)
                            {
                                nestedQuery = new BsonDocument();
                                if (nestedOb.Contains(COLLECTION_KEY_KID))
                                    nestedQuery.Add(COLLECTION_KEY_KID, nestedOb[COLLECTION_KEY_KID]);

                                result.Add(InsertOrUpdateObject(entity,
                                     nestedKlass,
                                     nestedOb,
                                     nestedQuery,
                                     $"{EnvConstants.Constants.GenerateSchemaName(schemaName)}_{nestedKlass.Name.ToLower()}",
                                     parentClassName,
                                     parentId,
                                     websiteId,
                                     nestedProperty.Key.Name.ToLower(),
                                     schemaName, userId, referenceList, componentStatus));
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// Get langauge class from the reference id of schema
        /// </summary>
        /// <param name="referenceid"></param>
        /// <param name="kEntity"></param>
        /// <returns></returns>
        private static KClass GetKClassFromReferenceId(string parentClassName, string propertyName, KEntity kEntity)
        {

            KClass kClass = null;
            KClass parentClass = null;
            KProperty property = null;
            if (parentClassName == null || propertyName == null)
            {
                kClass = kEntity.Classes.FirstOrDefault(x => x.ClassType == KClassType.BaseClass);
            }
            else
            {
                parentClass = kEntity.Classes.FirstOrDefault(x => x.Name.ToLower() == parentClassName.ToLower());
                if (parentClass != null)
                    property = parentClass.PropertyList?.FirstOrDefault(p => p.Name.ToLower() == propertyName.ToLower());
                if (parentClass != null && property != null)
                {
                    kClass = kEntity.Classes.FirstOrDefault(x => x.Name.ToLower() == property?.DataType.Name.ToLower());
                }
                else
                {
                    kClass = kEntity.Classes.FirstOrDefault(x => x.ClassType == KClassType.BaseClass);
                }
            }
            return kClass;

        }

        /// <summary>
        /// Send the kstring properties to keyword extraction kinesis stream to process
        /// </summary>
        /// <param name="kClass"></param>
        /// <param name="bsonDoc"></param>
        /// <param name=COLLECTION_KEY_KID></param>
        /// <param name="referenceid"></param>
        /// <param name="schemaName"></param>
        /// <param name="userId"></param>
        private static void SendToKStringKeywordExtractors(KClass kClass, BsonDocument bsonDoc, string _kid, string referenceid, string schemaName, string userId)
        {
            try
            {
                if (string.Compare("kstring", kClass?.Name?.ToLower()) == 0)
                {
                    //var kstringProperties = kClass.PropertyList.Where(x => x.Type == PropertyType.kstring).Select(x => x.Name);
                    //foreach (var kstrProp in kstringProperties)
                    //{
                    //    if (bsonDoc.Contains(kstrProp))
                    //    {
                    if (bsonDoc.Contains("text") && !string.IsNullOrEmpty(bsonDoc["text"].AsString))
                    {
                        var normalTextNode = HtmlAgilityPack.HtmlNode.CreateNode($"<div>{bsonDoc["text"].AsString}</div>");
                        var result = AmazonKinesisHelper.LogRecord(new KStringQueueModel
                        {
                            KID = _kid,
                            KString = normalTextNode != null ? normalTextNode.InnerText : string.Empty,
                            ReferenceId = referenceid,
                            SchemaName = schemaName,
                            UserId = userId
                        }, "KitsuneKStringStream");
                    }
                    //else if (bsonDoc[kstrProp].AsBsonDocument.Contains("keywords"))
                    //{
                    //    bsonDoc.Add($"{kstrProp}.keywords", bsonDoc[kstrProp]["keywords"]);
                    //    bsonDoc.Remove(kstrProp);
                    //}
                    //TODO : Send error message with data to update manual kstring
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                //Log Exception

            }
        }

        /// <summary>
        /// Send the phonenumber properties to assign vmn
        /// </summary>
        /// <param name="kClass"></param>
        /// <param name="bsonDoc"></param>
        /// <param name="websiteId"></param>
        private static phoneNumber CreateCallTracker(KClass kClass, BsonDocument bsonDoc, string websiteId)
        {
            try
            {
                if (string.Compare("phonenumber", kClass?.Name?.ToLower()) == 0)
                {
                    if (bsonDoc.Contains("contactnumber") && bsonDoc.Contains("isactive"))
                    {
                        if (bsonDoc["isactive"].AsBoolean)
                        {
                            var phone = bsonDoc["contactnumber"].AsString;
                            HttpClient client = new HttpClient();
                            client.BaseAddress = new Uri(EnvConstants.Constants.VMNBaseUrl);
                            HttpResponseMessage response = client.GetAsync(string.Format(EnvConstants.Constants.AssignVMNApiParam, websiteId, phone)).Result;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var dataObjects = response.Content.ReadAsAsync<List<VMNAssignResponse>>().Result;
                                bsonDoc.Add("calltrackernumber", dataObjects[0].virtualNumber);
                                return new phoneNumber
                                {
                                    contactNumber = dataObjects[0].primaryNumber,
                                    callTrackerNumber = dataObjects[0].virtualNumber
                                };
                            }
                            else
                            {
                                var responseString = response.Content.ReadAsStringAsync().Result;
                                if (responseString == "primary number has vmn assigned")
                                {
                                    var VMNs = GetListOfVMNs(websiteId);
                                    var callTrackerNumberAssigned = VMNs.Where(x => x.contactNumber == phone).FirstOrDefault()?.callTrackerNumber;
                                    if (callTrackerNumberAssigned == null)
                                        throw new Exception("Please use another contact number, as this number is already in use in another Project");
                                    bsonDoc["calltrackernumber"] = callTrackerNumberAssigned;
                                    return new phoneNumber
                                    {
                                        contactNumber = phone
                                    };
                                }
                                else
                                {
                                    //Log VMN assign failed
                                    bsonDoc["calltrackernumber"] = string.Empty;
                                    bsonDoc["isactive"] = false;
                                }
                            }
                        }
                        else
                        {
                            bsonDoc["calltrackernumber"] = string.Empty;
                            return null;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                //Log Exception
                throw ex;
            }
        }
        private static phoneNumber UpdateCallTracker(KClass kClass, BsonDocument bsonDoc, string websiteId, string parentClassName, string parentClassId, string propertyName, string kid, string userId, string schemaName)
        {
            try
            {
                if (string.Compare("phonenumber", kClass?.Name?.ToLower()) == 0)
                {
                    var setBsonDoc = bsonDoc;
                    if (setBsonDoc.Contains("contactnumber") && setBsonDoc.Contains("isactive"))
                    {
                        if (setBsonDoc["isactive"].AsBoolean)
                        {
                            var phone = setBsonDoc["contactnumber"].AsString;
                            var query = new PhoneNumberDatatTypeDetailsResponse
                            {
                                _kid = kid,
                                _parentClassId = parentClassId,
                                _parentClassName = parentClassName,
                                _propertyName = propertyName
                            };
                            var serializeQuery = JsonConvert.SerializeObject(query);
                            var phoneNumberListDetails = MongoConnector.GetWebsiteData(new GetWebsiteDataRequestModel
                            {
                                WebsiteId = websiteId,
                                Query = serializeQuery,
                                SchemaName = schemaName,
                                UserId = userId
                            }).Data;
                            var phoneNumberDetail = (IDictionary<string, object>)phoneNumberListDetails[0];
                            string oldCallTrackerNumber = string.Empty;
                            if (phoneNumberDetail.Keys.Contains("calltrackernumber") && phoneNumberDetail["calltrackernumber"] != null)
                                oldCallTrackerNumber = phoneNumberDetail["calltrackernumber"].ToString();
                            if (!string.IsNullOrEmpty(oldCallTrackerNumber))
                            {
                                return UpdateVMN(websiteId, oldCallTrackerNumber, phone);
                            }
                            //if (setBsonDoc.Contains("calltrackernumber") && !string.IsNullOrEmpty(setBsonDoc["calltrackernumber"].AsString))
                            //{
                            //    return UpdateVMN(websiteId, setBsonDoc["calltrackernumber"].AsString, phone);
                            //}
                            else
                            {
                                HttpClient client = new HttpClient();
                                client.BaseAddress = new Uri(EnvConstants.Constants.VMNBaseUrl);
                                HttpResponseMessage response = client.GetAsync(string.Format(EnvConstants.Constants.AssignVMNApiParam, websiteId, phone)).Result;
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    var dataObjects = response.Content.ReadAsAsync<List<VMNAssignResponse>>().Result;
                                    setBsonDoc["calltrackernumber"] = dataObjects[0].virtualNumber;
                                    return new phoneNumber
                                    {
                                        contactNumber = dataObjects[0].primaryNumber,
                                        callTrackerNumber = dataObjects[0].virtualNumber
                                    };
                                }
                                else
                                {
                                    var responseString = response.Content.ReadAsStringAsync().Result;
                                    if (responseString == "primary number has vmn assigned")
                                    {
                                        var VMNs = GetListOfVMNs(websiteId);
                                        var callTrackerNumberAssigned = VMNs.Where(x => x.contactNumber == phone).FirstOrDefault()?.callTrackerNumber;
                                        if (callTrackerNumberAssigned == null)
                                            throw new Exception("Please use another contact number, as this number is already in use in another Project");
                                        setBsonDoc["calltrackernumber"] = callTrackerNumberAssigned;
                                        return new phoneNumber
                                        {
                                            contactNumber = phone
                                        };
                                    }
                                    else
                                    {
                                        //Log VMN assign failed
                                        bsonDoc["calltrackernumber"] = string.Empty;
                                        bsonDoc["isactive"] = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            setBsonDoc["calltrackernumber"] = string.Empty;
                            return null;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                //Log Exception
                throw ex;
            }
        }

        /// <summary>
        /// Update the view classes in the intellisense
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="projectDetails"></param>
        internal static void UpdateClassViews(string userEmail, string projectId, KEntity entity, GetProjectDetailsResponseModel projectDetails)
        {
            if (projectDetails != null)
            {

                projectDetails.ProjectId = projectId;

                foreach (var resource in projectDetails.Resources.Where(x => x.IsStatic == false))
                {

                    entity.Classes.Add(new KClass
                    {
                        ClassType = KClassType.BaseClass,
                        Description = resource.SourcePath,
                        Name = string.Format("View('{0}')", resource.SourcePath),
                        PropertyList = new List<KProperty>()
                        {
                            new KProperty
                            {
                                DataType = new DataType("function"),
                                Description = "Page link",
                                Name = "geturl",
                                Type = PropertyType.function
                            },
                             new KProperty
                            {
                                DataType = new DataType("function"),
                                Description = "Set details page object",
                                Name = "setobject",
                                Type = PropertyType.function
                            },
                            new KProperty
                            {
                                DataType = new DataType("link"),
                                Description = "First page link",
                                Name = "firstpage",
                                Type = PropertyType.obj
                            },
                            new KProperty
                            {
                                DataType = new DataType("link"),
                                Description = "Last page link",
                                Name = "lastpage",
                                Type = PropertyType.obj
                            },
                            new KProperty
                            {
                                DataType = new DataType("str"),
                                Description = "Current page number",
                                Name = "currentpagenumber",
                                Type = PropertyType.str
                            },
                            new KProperty
                            {
                                DataType = new DataType("link"),
                                Description = "Next page link",
                                Name = "nextpage",
                                Type = PropertyType.obj
                            },
                            new KProperty
                            {
                                DataType = new DataType("str"),
                                Description = "Offset of list page",
                                Name = "offset",
                                Type = PropertyType.str
                            },
                            new KProperty
                            {
                                DataType = new DataType("link"),
                                Description = "Previous page link",
                                Name = "previouspage",
                                Type = PropertyType.obj
                            },
                            new KProperty
                            {
                                DataType = new DataType("str"),
                                Description = "Set object for the details page",
                                Name = "setobject",
                                Type = PropertyType.function
                            },

                    }
                    });
                }
            }
        }

        /// <summary>
        /// Update the webaction classes in the intellisense
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="entity"></param>
        internal static void UpdateWebaactionClasses(string userId, KEntity entity)
        {
            IList<KProperty> PropertyList = new List<KProperty>();
            IList<KClass> webactionClassList = new List<KClass>();

            KClass webactionClass;
            var webActions = APIHelper.GetWebAction(userId);
            var webActionBaseClass = new KClass
            {
                ClassType = KClassType.BaseClass,
                Description = "Webaction",
                Name = "webactions",
            };
            if (webActions != null && webActions.WebActions != null)
                foreach (var webaction in webActions.WebActions)
                {
                    PropertyList.Add(new KProperty
                    {
                        DataType = new DataType("WA" + webaction.Name.ToUpper()),
                        Description = webaction.Name,
                        Name = webaction.Name.ToLower(),
                        Type = PropertyType.array
                    });
                    webactionClass = new KClass
                    {
                        ClassType = KClassType.UserDefinedClass,
                        Description = "Webaction products",
                        Name = "WA" + webaction.Name.ToUpper(),
                        PropertyList = webaction.Properties.Select(x => new KProperty
                        {
                            DataType = new DataType(x.DataType),
                            Description = x.DisplayName,
                            Name = x.PropertyName.ToLower(),
                            Type = ConvertProperty(x.PropertyType),
                            IsRequired = x.IsRequired
                        }).ToList()
                    };
                    webactionClass.PropertyList.Add(new KProperty
                    {
                        DataType = new DataType("str"),
                        Description = "Id",
                        Name = COLLECTION_KEY_ID,
                        Type = PropertyType.str

                    });
                    webactionClass.PropertyList.Add(new KProperty
                    {
                        DataType = new DataType("datetime"),
                        Description = "Created on",
                        Name = COLLECTION_KEY_CREATED_ON,
                        Type = PropertyType.date
                    });
                    webactionClass.PropertyList.Add(new KProperty
                    {
                        DataType = new DataType("datetime"),
                        Description = "Updated on",
                        Name = COLLECTION_KEY_UPDATED_ON,
                        Type = PropertyType.date
                    });
                    entity.Classes.Add(webactionClass);
                }
            webActionBaseClass.PropertyList = PropertyList;
            entity.Classes.Add(webActionBaseClass);
        }
        /// <summary>
        /// Convert webaction datatype to language datatype
        /// </summary>
        /// <param name="waproperty"></param>
        /// <returns></returns>
        internal static PropertyType ConvertProperty(WebActionPropertyType waproperty)
        {
            switch (waproperty)
            {
                case WebActionPropertyType.array: return PropertyType.array;
                case WebActionPropertyType.boolean: return PropertyType.boolean;
                case WebActionPropertyType.date: return PropertyType.date;
                case WebActionPropertyType.image: return PropertyType.obj;
                case WebActionPropertyType.link: return PropertyType.obj;
                case WebActionPropertyType.number: return PropertyType.number;
                case WebActionPropertyType.reference: return PropertyType.obj;
                case WebActionPropertyType.str: return PropertyType.str;
                case WebActionPropertyType.webaction: return PropertyType.obj;
                default: return PropertyType.str;
            }
        }
        #endregion
        internal static List<phoneNumber> GetListOfVMNs(string websiteId)
        {
            try
            {
                List<phoneNumber> activeCallTrackerList = new List<phoneNumber>();
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(EnvConstants.Constants.VMNBaseUrl);
                HttpResponseMessage response = client.GetAsync(string.Format(EnvConstants.Constants.FetchVMNApiParam, websiteId)).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var listedVMNs = response.Content.ReadAsAsync<List<VMNFetchResponse>>().Result;
                    foreach (var vmn in listedVMNs)
                        activeCallTrackerList.Add(new phoneNumber
                        {
                            contactNumber = vmn.primaryNumber,
                            callTrackerNumber = vmn.virtualNumber
                        });
                }
                return activeCallTrackerList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static void DisableVMNs(List<string> contactNumbers = null, List<string> websiteIds = null)
        {
            try
            {
                if (contactNumbers == null && websiteIds == null)
                    throw new Exception("Pass atleast one parameter: contactNumbers or websiteIds");
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                DisableVMNRequestModel requestObject = new DisableVMNRequestModel();
                if (websiteIds != null)
                    requestObject.customerIds = websiteIds;
                else if (contactNumbers != null)
                    requestObject.primaryNumbers = contactNumbers;
                var jsonData = JsonConvert.SerializeObject(requestObject);
                client.BaseAddress = new Uri(EnvConstants.Constants.VMNBaseUrl);
                var response = client.PostAsync(EnvConstants.Constants.DisableVMNApiParam, new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (!response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    //Log VMN not disabled
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static List<phoneNumber> GetInUseVMNs(string websiteId, string userId, string schemaName)
        {
            try
            {
                List<phoneNumber> result = new List<phoneNumber>();
                var phoneNumberDataTypeData = MongoConnector.GetWebsiteDataByType(new GetWebsiteDataByTypeRequestModel
                {
                    UserId = userId,
                    ClassName = PropertyType.phonenumber.ToString(),
                    SchemaName = schemaName,
                    WebsiteId = websiteId
                }).Data;
                if (phoneNumberDataTypeData.Any())
                {
                    foreach (var phoneNumber in phoneNumberDataTypeData)
                    {
                        var number = (IDictionary<string, object>)phoneNumber;
                        if (number.Keys.Contains("isactive") && number.Keys.Contains("contactnumber") && number.Keys.Contains("calltrackernumber") && (bool)number["isactive"])
                        {
                            result.Add(new phoneNumber
                            {
                                contactNumber = number["contactnumber"].ToString(),
                                callTrackerNumber = number["calltrackernumber"].ToString()
                            });
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static phoneNumber UpdateVMN(string websiteId, string callTrackerNumber, string newContactNumber)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(EnvConstants.Constants.VMNBaseUrl);
                var response = client.PutAsync(string.Format(EnvConstants.Constants.UpdateVMNApiParam, websiteId, callTrackerNumber, newContactNumber), null).Result;
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    var dataObjects = response.Content.ReadAsAsync<List<VMNAssignResponse>>().Result;
                    return new phoneNumber
                    {
                        contactNumber = dataObjects[0].primaryNumber,
                        callTrackerNumber = dataObjects[0].virtualNumber
                    };
                }
                else if (!response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    //Log VMN not disabled
                }
                return new phoneNumber
                {
                    contactNumber = newContactNumber
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal static List<string> GetCallTrackerDomainsFromConfig(string websiteId, string projectId)
        {
            try
            {
                callTrackerConfigModel callTrackerEnabledDomain = new callTrackerConfigModel
                {
                    domain = new List<string>()
                };
                string config = string.Empty;
                try
                {
                    config = MongoConnector.GetProjectConfig(new GetProjectConfigRequestModel
                    {
                        ProjectId = projectId,
                        Level = FileLevel.PROD
                    })?.File?.Content;
                }
                catch (Exception ex)
                {
                    return callTrackerEnabledDomain.domain;
                }
                var callTrackerConfig = JsonConvert.DeserializeObject<JObject>(config)?.GetValue("call_tracker")?.ToString();
                if (!string.IsNullOrEmpty(callTrackerConfig))
                {
                    callTrackerEnabledDomain = JsonConvert.DeserializeObject<callTrackerConfigModel>(callTrackerConfig);
                }
                callTrackerEnabledDomain.domain = callTrackerEnabledDomain.domain.Select(x => x.ToUpper()).ToList();
                return callTrackerEnabledDomain.domain;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static List<PhoneNumberDatatTypeDetailsResponse> PhoneNumberDatatTypeDetails(string websiteId, string userId, string schemaName)
        {
            try
            {
                List<PhoneNumberDatatTypeDetailsResponse> result = new List<PhoneNumberDatatTypeDetailsResponse>();
                var phoneNumberDataTypeData = MongoConnector.GetWebsiteDataByType(new GetWebsiteDataByTypeRequestModel
                {
                    UserId = userId,
                    ClassName = PropertyType.phonenumber.ToString(),
                    SchemaName = schemaName,
                    WebsiteId = websiteId
                }).Data;
                if (phoneNumberDataTypeData.Any())
                {
                    foreach (var phoneNumber in phoneNumberDataTypeData)
                    {
                        var number = (IDictionary<string, object>)phoneNumber;
                        if ((bool)number["isactive"])
                        {
                            result.Add(new PhoneNumberDatatTypeDetailsResponse
                            {
                                _kid = number["_kid"].ToString(),
                                _parentClassId = number["_parentClassId"].ToString(),
                                _parentClassName = number["_parentClassName"].ToString(),
                                _propertyName = number["_propertyName"].ToString()
                            });
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}