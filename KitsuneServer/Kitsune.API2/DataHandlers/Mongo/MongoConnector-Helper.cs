
using AWS.Services.S3Helper;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Utils;
using Kitsune.BasePlugin.Utils;
using Kitsune.Models;
using Kitsune.Models.WebsiteModels;
using Kitsune.Models.Project;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kitsune.Language.Models;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization;

namespace Kitsune.API2.DataHandlers.Mongo
{
	public static partial class MongoConnector
	{
		internal static string CreateNewProject(string projectName, string userEmail, string clientId, int compilerVersion, ProjectType projectType = ProjectType.NEWPROJECT, ProjectStatus projectStatus = ProjectStatus.IDLE)
		{
			try
			{
				if (_kitsuneServer == null)
					InitializeConnection();
				var kitsuneProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

				var projectId = ObjectId.GenerateNewId().ToString();
				DateTime datetime = DateTime.Now;

				#region Create Kitsune Project

				BucketNames bucketNames = new BucketNames
				{
					source = AmazonAWSConstants.SourceBucketName,
					demo = AmazonAWSConstants.DemoBucketName,
					placeholder = AmazonAWSConstants.PlaceHolderBucketName,
					production = AmazonAWSConstants.ProductionBucketName
				};

				KitsuneProject newProject = new KitsuneProject()
				{
					CreatedOn = datetime,
					UpdatedOn = datetime,
					IsArchived = false,
					ProjectId = projectId,
					ProjectName = projectName,
					UserEmail = userEmail,
					BucketNames = bucketNames,
					ProjectType = projectType,
					ProjectStatus = projectStatus,
					PublishedVersion = 0,
					Version = 1,
					CompilerVersion = compilerVersion
				};

				kitsuneProjectCollection.InsertOne(newProject);

				#endregion

				#region Create Default Customer

				try
				{
					CreateDefaultWebsite(projectId, projectName, userEmail, clientId);
				}
				catch (Exception ex)
				{
					//Log Error creating New Customer
				}

				#endregion

				#region Create Settings File

				if (!CreateDefaultSettingsFile(projectId))
				{
					//TODO Log error creating default settings file
				}

				#endregion

				#region Create Default Resources

				var projectResourceCreated = CreateDefaultStaticIndexResource(projectId);

				#endregion

				#region SendDeveloperEmail
				if (!string.IsNullOrEmpty(projectName))
				{
					var UserCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
					var WebsiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
					var WebsiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);

					var websiteDetail = WebsiteCollection.Find(x => x.ProjectId == projectId).FirstOrDefault();
					if (websiteDetail != null)
					{
						string websiteId = websiteDetail._id;
						KitsuneWebsiteUserCollection websiteUser = WebsiteUserCollection.Find(x => x.WebsiteId.Equals(websiteId)).SortBy(x => x.CreatedOn).FirstOrDefault();
						if (websiteUser != null)
						{
							string username = websiteUser.UserName;
							string password = websiteUser.Password;
							string developerId = websiteUser.DeveloperId;
							if (!String.IsNullOrEmpty(developerId))
							{
								var userDetails = UserCollection.Find(x => x._id.Equals(developerId)).FirstOrDefault();
								if (userDetails != null)
								{
									Dictionary<string, string> optionalParameters = new Dictionary<string, string>
										{
											{ EnvConstants.Constants.EmailParam_KAdminUserName, websiteUser.UserName},
											{ EnvConstants.Constants.EmailParam_KAdminPassword, websiteUser.Password}
										};
									optionalParameters[EnvConstants.Constants.EmailParam_KAdminUrl] = string.Format(EnvConstants.Constants.KAdminBaseUrl, websiteDetail.WebsiteUrl).ToLower();
									optionalParameters[EnvConstants.Constants.EmailParam_ProjectName] = projectName;
									///TODO : Put first name once its added on user creation
									optionalParameters[EnvConstants.Constants.EmailParam_DeveloperName] = userDetails.DisplayName;
									EmailHelper emailHelper = new EmailHelper();
									emailHelper.SendGetKitsuneEmail(string.Empty, userDetails.UserName, MailType.DEFAULT_CUSTOMER_KADMIN_CREDENTIALS, null, optionalParameters);
								}
							}
						}
					}
				}
				#endregion

				return projectId;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Kitsune website creation with default website user
		/// </summary>
		/// <param name="projectId"></param>
		/// <param name="websiteTag"></param>
		/// <param name="customerMail"></param>
		/// <param name="customerPhoneNumber"></param>
		/// <param name="developerEmail"></param>
		/// <param name="websiteUrl"></param>
		/// <param name="developerId"></param>
		/// <returns></returns>
		internal static KitsuneWebsiteCollection CreateNewWebsite(string projectId, string websiteTag, string customerMail, string customerPhoneNumber, string developerEmail, string websiteUrl, string developerId, string clientId, string customerName = null, string domain = null, string customWebsiteId = null, bool? isSSLEnabled = true)
		{
			try
			{
				if (_kitsuneServer == null)
					InitializeConnection();
				var kitsuneWebsiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
				var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
				var userCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);

				DateTime datetime = DateTime.Now;
				ObjectId websiteId = ObjectId.GenerateNewId();

				//if the custom website id is present and is valid objectid then assign it

				if (!string.IsNullOrEmpty(customWebsiteId))
				{
					if (!ObjectId.TryParse(customWebsiteId, out websiteId))
						websiteId = ObjectId.GenerateNewId();
				}



				#region GENERATE USERNAME AND PASSWORD

				PasswordGenerator passwordGenerator = new PasswordGenerator();
				var password = passwordGenerator.GeneratePassword(true, true, true, 7);
				var userName = passwordGenerator.GenerateUserName(websiteTag);

				if (String.IsNullOrEmpty(password))
					throw new Exception($"Error generating password");
				if (String.IsNullOrEmpty(userName))
					throw new Exception($"Error generating userName");

				#endregion

				#region VERIFY IF WEBSITETAG ALREADY PRESENT OR NOT

				var count = kitsuneWebsiteCollection.Count(x => x.WebsiteTag.Equals(websiteTag) && x.ClientId == clientId);
				if (count > 0)
				{
					throw new Exception("Unable to get the KitsuneTag");

					////String kitsuneTag = kitsuneUrl.Split('.').First();
					//if (string.IsNullOrEmpty(websiteTag))
					//    throw new Exception("Unable to get the KitsuneTag");

					//Random random = new Random();
					//var basePlugin = BasePluginConfigGenerator.GetBasePlugin(clientId);

					//kitsuneUrl = kitsuneTag + random.Next(100, 999) + basePlugin.GetSubDomain();
				}

				#endregion

				#region CREATE NEW WEBSITE IN WEBSITECOLLECTION (WITH SUBDOMAIN AS WEBSITEURL)

				KitsuneWebsiteCollection websiteObject = new KitsuneWebsiteCollection
				{
					_id = websiteId.ToString(),
					UpdatedOn = datetime,
					CreatedOn = datetime,
					DeveloperId = developerId,
					WebsiteTag = websiteTag,
					ProjectId = projectId,
					IsArchived = false,
					IsActive = false,
					WebsiteUrl = websiteUrl,
					ClientId = clientId
				};
				kitsuneWebsiteCollection.InsertOne(websiteObject);

				#endregion

				#region CREATE NEW RECORD IN WEBSITEDNSCOLLECTION (FOR SUBDOMAIN AS ACTIVE STATUS AND ALSO FOR REQUESTED DOMAIN IF PROVIDED WITH PENDING STATUS)

				var websiteDNSObj = new WebsiteDNSInfo
				{
					CreatedOn = datetime,
					DNSStatus = DNSStatus.Active,
					DomainName = websiteUrl.ToUpper(),
					IsSSLEnabled = isSSLEnabled ?? true,
					RootPath = websiteUrl.ToUpper(),
					WebsiteId = websiteObject._id
				};
				websiteDNSCollection.InsertOne(websiteDNSObj);

				if (!string.IsNullOrEmpty(domain))
				{
					websiteDNSObj = new WebsiteDNSInfo
					{
						CreatedOn = datetime,
						DNSStatus = DNSStatus.Pending,
						DomainName = domain.ToUpper(),
						IsSSLEnabled = isSSLEnabled ?? false, //default is false for custom domain
						RootPath = domain.ToUpper(),
						WebsiteId = websiteObject._id
					};
					websiteDNSCollection.InsertOne(websiteDNSObj);
				}

				#endregion

				#region CREATE NEW WEBISITE USER FOR THE ABOVE CREATED WEBSITE (WITH OWNER ACCESSTYPE)

				var websiteUser = new KitsuneWebsiteUserCollection
				{
					DeveloperId = developerId,
					WebsiteId = websiteObject._id,
					AccessType = KitsuneWebsiteAccessType.Owner,
					UserName = websiteTag,
					CreatedOn = DateTime.UtcNow,
					IsActive = true,
					Password = password,
					Contact = new ContactDetails { Email = customerMail, PhoneNumber = customerPhoneNumber, FullName = customerName ?? websiteTag },
				};
				userCollection.InsertOne(websiteUser);

				#endregion

				return websiteObject;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		internal static bool IsValidWebsiteId(string websiteId)
		{
			try
			{
				if (_kitsuneServer == null)
					InitializeConnection();

				websiteId = websiteId.Trim().ToLower();

				if (String.IsNullOrWhiteSpace(websiteId))
					return false;

				var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

				if (websiteCollection.Count(x => x._id == websiteId && x.IsActive == true) > 0)
					return true;
				else
					return false;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		internal static string GetWebsiteUrlFromWebsiteId(string websiteId)
		{
			try
			{
				if (!String.IsNullOrEmpty(websiteId))
				{
					if (_kitsuneServer == null)
						InitializeConnection();

					websiteId = websiteId.Trim().ToLower();
					var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

					var websiteDNSHost = websiteCollection.Find(x => x._id == websiteId && x.IsActive == true && x.IsArchived == false)
						.SortByDescending(x => x.CreatedOn).Limit(1).FirstOrDefault().WebsiteUrl;

					//support ssl and ssl config in websites or change all places to websiteDNS
					return $"http://{websiteDNSHost}";
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return null;
		}

		internal static string GetProjectIdFromWebsiteId(string websiteId, bool isActiveMatters = true)
		{
			try
			{
				if (_kitsuneServer == null)
					InitializeConnection();

				websiteId = websiteId.Trim().ToLower();

				if (String.IsNullOrEmpty(websiteId) || String.IsNullOrWhiteSpace(websiteId))
					return null;

				var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
				if (isActiveMatters)
					return websiteCollection.Find(x => (x._id == websiteId && x.IsActive == true)).Project(x => x.ProjectId).FirstOrDefault();
				else
					return websiteCollection.Find(x => (x._id == websiteId)).Project(x => x.ProjectId).FirstOrDefault();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return null;
		}

		internal static KitsuneWebsiteCollection CreateDefaultWebsite(string projectId, string projectName, string developerEmail, string clientId)
		{
			try
			{
				if (_kitsuneServer == null)
					InitializeConnection();

				var userCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);
				var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

				UserModel userDetails = userCollection.Find(x => x.UserName.Equals(developerEmail)).Limit(1).FirstOrDefault();

				#region GENERATE WEBSITETAG AND WEBSITEURL
				bool? isSSLEnabled = null;
				// Removes all special characters
				if (projectName.ToLower().Trim().StartsWith("http://"))
					isSSLEnabled = false;
				string websiteTag = projectName.Replace("http://", "", StringComparison.InvariantCultureIgnoreCase)
											   .Replace("https://", "", StringComparison.InvariantCultureIgnoreCase)
											   .Replace("www.", "", StringComparison.InvariantCultureIgnoreCase);
				websiteTag = Regex.Replace(websiteTag, @"[^0-9a-zA-Z]+", "").ToUpper();

				websiteTag = $"{websiteTag}DEFAULT";

				//If the same name website tag exist use {projectname_developerfirstname_01}
				var existingWebsites = websiteCollection.Find(x => x.WebsiteTag == websiteTag).Count();
				if (existingWebsites > 0)
				{
					Random random = new Random();
					websiteTag = websiteTag + random.Next(100, 999);
				}

				var basePlugin = BasePluginConfigGenerator.GetBasePlugin(clientId);

				//update clientId if the id is not present
				clientId = basePlugin.GetClientId();
				string websiteDomain = websiteTag.ToUpper() + basePlugin.GetSubDomain();

				#endregion

				//Create new customer
				KitsuneWebsiteCollection customerDetails = CreateNewWebsite(projectId, websiteTag, developerEmail, userDetails.PhoneNumber, developerEmail, websiteDomain, userDetails._id, clientId, isSSLEnabled: isSSLEnabled);
				return customerDetails;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		internal static bool CreateDefaultDynamicResources(string ProjectId, string UserEmail, string DefaultProjectId)
		{
			try
			{
				if (_kitsuneServer == null)
					InitializeConnection();
				var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
				var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

				//if theme not present in database or it is deleted then return false
				if (!ProjectCollection.Find(x => x.ProjectId == ProjectId && x.IsArchived == false && x.UserEmail == UserEmail).Any())
					return false;//Error:Theme doesnot exist


				//bring all the views from the language
				//var Views = NFLanguage.Themes.Where(x => x.ThemeType == Language.Models.ThemeType.Default).First().Views;
				var resources = ResourceCollection.Find(x => x.ProjectId == DefaultProjectId).ToList();
				var dateTime = DateTime.Now;
				foreach (var resource in resources)
				{
					resource.ProjectId = ProjectId;
					resource.Errors = null;
					resource._id = null;
					resource.CreatedOn = dateTime;
					resource.UpdatedOn = dateTime;
					resource.SourcePath = AmazonS3FileProcessor.SaveFileContentToS3(ProjectId, AmazonAWSConstants.SourceBucketName, resource.SourcePath, AmazonS3FileProcessor.getFileFromS3(resource.SourcePath, DefaultProjectId, AmazonAWSConstants.SourceBucketName));

					ResourceCollection.InsertOne(resource);
				}
				return true;
			}
			catch (Exception ex)
			{
				return false;//error:ex
			}
		}
		internal static ResourceType GetResourceType(string filename)
		{
			ResourceType resourceType = ResourceType.LINK;
			var extension = filename.Split(new char[] { '.' }).LastOrDefault().Split(new char[] { '?', '#' }).FirstOrDefault().ToLower();
			switch (extension)
			{
				case "html":
				case "htm":
				case "dl":
					resourceType = ResourceType.LINK;
					break;
				case "css":
					resourceType = ResourceType.STYLE;
					break;
				case "js":
					resourceType = ResourceType.SCRIPT;
					break;
				default:
					resourceType = ResourceType.FILE;
					break;
			}
			return resourceType;
		}
		internal static bool CreateDefaultStaticIndexResource(string projectId)
		{
			try
			{
				if (_kitsuneServer == null)
					InitializeConnection();
				var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
				var dateTime = DateTime.Now;

				#region 
				foreach (var file in Kitsune.API2.EnvConstants.Constants.ProjectDefaultFiles)
				{
					KitsuneResource resourceCollection = new KitsuneResource
					{
						ProjectId = projectId,
						SourcePath = file,
						CreatedOn = dateTime,
						UpdatedOn = dateTime,
						ResourceType = GetResourceType(file),
						IsStatic = true,
						Version = 1,
						IsDefault = file.Contains("/index.html")
					};
					ResourceCollection.InsertOne(resourceCollection);

					//upload the object to S3
					var result = S3UploadHelper.GetAssetFromS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey,
															EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey,
															AmazonAWSConstants.ResourceBucketName,
															$"static/defaults_new{file}");

					string defaultHtmlData = String.Empty;
					if (result != null && result.IsSuccess)
					{
						defaultHtmlData = result.File.Content;


						Byte[] byteHtmlData = Encoding.ASCII.GetBytes(defaultHtmlData);
						var saveResult = S3UploadHelper.SaveAssetsAndReturnObjectkey(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, projectId + file, byteHtmlData, AmazonAWSConstants.SourceBucketName, result.File.ContentType);
						var demosaveResult = S3UploadHelper.SaveAssetsAndReturnObjectkey(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey, projectId + "/cwd" + file, byteHtmlData, AmazonAWSConstants.DemoBucketName, result.File.ContentType);
					}
				}

				#endregion


				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		internal static bool CreateDefaultSettingsFile(string projectId)
		{
			try
			{
				var result = S3UploadHelper.GetAssetFromS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey,
														EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey,
														AmazonAWSConstants.ResourceBucketName,
														string.Format(Kitsune.API2.EnvConstants.Constants.ProjectDefaultSettingsFile, projectId));

				string fileContent = String.Empty;
				if (result != null && result.IsSuccess)
					fileContent = result.File != null ? result.File.Content : String.Empty;
				CreateOrUpdateProjectConfigRequestModel request = new CreateOrUpdateProjectConfigRequestModel()
				{
					File = new ConfigFile
					{
						Content = fileContent
					},
					ProjectId = projectId
				};
				CreateOrUpdateProjectConfig(request);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}


		//Data Creator Classes

		public static GetWebsiteDataResponseModel GetBaseClassData(GetPageDataRequest pageDataRequest)
		{
			string websiteId = pageDataRequest.WebsiteId;
			string userId = pageDataRequest.UserId;

			string filePath = pageDataRequest.SourcePath.Split('?')[0];
			ObjectReference refObject = GetMetadataReferenceObject(pageDataRequest.ProjectId, filePath, pageDataRequest.SchemaName, websiteId, userId);

			if (refObject == null)
				throw new ArgumentNullException(nameof(refObject));

			if (_kitsuneSchemaServer == null || _kitsuneServer == null)
				InitializeConnection();

			var dataResult = new List<object>();
			try
			{
				string baseClassName = refObject.name.ToLower();
				var collectionName = GenerateBaseClassName(baseClassName);
				var collection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(collectionName);
				string filter = String.Format("{{userid : '{0}', websiteid : '{1}', isarchived : false}}", userId, websiteId);
				var result = collection.Find(filter).FirstOrDefault();
				BsonDocument resultDocument = new BsonDocument();
				if (result != null)
				{
					GetResultDocument(baseClassName, websiteId, null, result, refObject.properties, resultDocument, pageDataRequest.CurrentPageNumber);
					resultDocument.Remove(COLLECTION_KEY_ID);
					dataResult.Add(BsonSerializer.Deserialize<object>(resultDocument));
				}
				var response = new GetWebsiteDataResponseModel
				{
					Data = dataResult
				};
				return response;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Extract meta reference object from list of reference objects.
		/// </summary>
		/// <param name="pageDataRequest"></param>
		/// <param name="websiteId"></param>
		/// <param name="userId"></param>
		/// <returns></returns>
		private static ObjectReference GetMetadataReferenceObject(string projectId, string sourcePath, string schemaName, string websiteId, string userId)
		{
			GetResourceMetaInfoRequest request = new GetResourceMetaInfoRequest
			{
				ProjectId = projectId,
				SourcePath = sourcePath
			};

			var refObjectList = MongoConnector.GetMetaInfo(request);
			if (String.IsNullOrEmpty(websiteId))
				throw new ArgumentNullException(nameof(websiteId));
			if (String.IsNullOrEmpty(userId))
				throw new ArgumentNullException(nameof(userId));
			ObjectReference refObject = null;
			foreach (var obj in refObjectList)
			{
				if (obj.name.ToLower() == schemaName)
				{
					refObject = obj;
				}
			}

			return refObject;
		}

		public static BsonDocument GetObjectData(ObjectReference refObject, string baseClassName, string websiteId, string parentId, Filter dataFilter, string parentClassName, string currentPageNumber)
		{
			if (_kitsuneSchemaServer == null || _kitsuneServer == null)
				InitializeConnection();

			if (refObject == null)
				throw new ArgumentNullException(nameof(refObject));
			if (String.IsNullOrEmpty(baseClassName))
				throw new ArgumentNullException(nameof(baseClassName));
			try
			{
				BsonDocument result = new BsonDocument();
				string className = refObject.dataType.ToLower();
				string propertyName = refObject.name.ToLower();

				var collectionName = GenerateClassName(baseClassName, className);
				//var referenceId = GenerateReferenceId(parentId, className, propertyName);

				#region COLLECTION INITIALISATION
				var collection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(collectionName);
				#endregion

				#region FILTER DEFINITION
				var filter = new BsonDocument(COLLECTION_KEY_WEBISTE_ID, websiteId);
				if (parentClassName != null)
				{
					filter.Add(COLLECTION_KEY_PARENT_CLASS_NAME, parentClassName);
					filter.Add(COLLECTION_KEY_PROPERTY_NAME, propertyName);
					filter.Add(COLLECTION_KEY_PARENT_CLASS_ID, parentId);
				}
				filter.Add(COLLECTION_KEY_IS_ARCHIVED, false);
				#endregion

				if (refObject.type.Equals(metaPropertyType.array))
				{
					int startIndex, endIndex;
					GetStartEndIndexOfArray(refObject, out startIndex, out endIndex);

					long arrayLength = collection.Find(filter).Count();
					result.Add("_total", arrayLength);
					List<BsonDocument> arrayResult = null;
					if (startIndex == 0 && endIndex == -1)
					{
						arrayResult = collection.Find(filter).ToList();
					}
					else
					{
						if (startIndex == -1 && endIndex == -1)
						{
							filter.Add(COLLECTION_KEY_ID, currentPageNumber);
						}
						else if (startIndex == -1)
						{
							int.TryParse(currentPageNumber, out int pageNumber);
							if (pageNumber > 0 && pageNumber.ToString().Equals(currentPageNumber))
							{
								startIndex = endIndex * (pageNumber - 1);
								endIndex += startIndex;
							}
						}
						arrayResult = collection.Find(filter).Skip(startIndex).Limit(endIndex - startIndex).ToList();
					}
					if (arrayResult != null)
					{
						int index = startIndex;
						/*for (index = 0; index < startIndex; index++)
                        {
                            result.Add(new BsonDocument());
                        }*/
						foreach (BsonDocument arrayResultItem in arrayResult)
						{
							BsonDocument resultDocument = new BsonDocument();
							List<ObjectReference> propertyList = new List<ObjectReference>();
							foreach (var range in refObject.arrayRanges)
							{
								if ((range.filter.startIndex == 0 && range.filter.endIndex == -1) ||
									(range.filter.startIndex <= index && index <= range.filter.endIndex))
								{
									propertyList.AddRange(range.properties);
								}
							}
							GetResultDocument(baseClassName, websiteId, className, arrayResultItem, propertyList, resultDocument, currentPageNumber);
							result.Add("_" + index.ToString(), resultDocument);
							index++;
						}
					}
				}
				else
				{
					BsonDocument singleResult = collection.Find(filter).FirstOrDefault();
					if (singleResult != null)
					{
						GetResultDocument(baseClassName, websiteId, className, singleResult, refObject.properties, result, currentPageNumber);
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private static void GetStartEndIndexOfArray(ObjectReference refObject, out int startIndex, out int endIndex)
		{
			startIndex = 0;
			endIndex = -1;
			foreach (var arrayObject in refObject.arrayRanges)
			{
				if (arrayObject.filter.startIndex == 0 && arrayObject.filter.endIndex == -1)
				{
					startIndex = 0;
					endIndex = -1;
					break;
				}
				if (arrayObject.filter.startIndex < startIndex)
				{
					startIndex = arrayObject.filter.startIndex;
				}
				if (arrayObject.filter.endIndex > endIndex)
				{
					endIndex = arrayObject.filter.endIndex;
				}
			}
		}

		private static void GetResultDocument(string baseClassName, string websiteId, string className, BsonDocument arrayResultItem, List<ObjectReference> properties, BsonDocument resultDocument, string currentPageNumber)
		{
			var primaryDataTypeObjects = new string[] { "str", "number", "datetime", "boolean" };
			foreach (var project in properties)
			{
				if ((project.type.Equals(metaPropertyType.array) &&
					!primaryDataTypeObjects.Contains(project.dataType?.ToLower())) ||
					project.type == metaPropertyType.obj ||
					project.type == metaPropertyType.kstring || project.type == metaPropertyType.phonenumber)
				{
					var arrayList = GetObjectData(project, baseClassName, websiteId, arrayResultItem["_id"].ToString(), null, className, currentPageNumber);
					if (arrayList.Any())
					{
						resultDocument.Add(project.name.ToLower(), arrayList);
					}
				}
				else
				{
					if (arrayResultItem.Contains(project.name.ToLower()))
					{
						resultDocument.Add(project.name.ToLower(), arrayResultItem[project.name]);
					}
				}
			}
		}

		//Helpers Classes

		internal static string GenerateBaseClassName(string className)
		{
			if (String.IsNullOrEmpty(className))
				throw new ArgumentNullException(nameof(className));
			return $"k_{className}";
		}

		internal static string GenerateClassName(string baseClassName, string className)
		{
			if (String.IsNullOrEmpty(baseClassName))
				throw new ArgumentNullException(nameof(baseClassName));
			if (String.IsNullOrEmpty(className))
				throw new ArgumentNullException(nameof(className));
			return $"k_{baseClassName}_{className}";
		}

		public static ObjectReference Deserialize(string filePath)
		{
			FileStream fs = new FileStream(filePath, FileMode.Open);
			string fileContents;
			using (StreamReader reader = new StreamReader(fs))
			{
				fileContents = reader.ReadToEnd();
			}

			try
			{
				byte[] bytes = Convert.FromBase64String(fileContents);
				//byte[] bytes = Encoding.UTF8.GetBytes(serializedContent);
				BinaryFormatter formatter = new BinaryFormatter();

				using (MemoryStream stream = new MemoryStream(bytes))
				{
					var result = (List<ObjectReference>)formatter.Deserialize(stream);
					return result.FirstOrDefault();
				}
			}
			catch (SerializationException e)
			{
				return default(ObjectReference);
			}
			catch (Exception ex)
			{
				return null;
			}

		}

	}
}
