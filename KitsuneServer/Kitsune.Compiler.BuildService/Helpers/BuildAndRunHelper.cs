using HtmlAgilityPack;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Compiler.BuildService.Models;
using Kitsune.Compiler.Core.Helpers;
using Kitsune.Compiler.Helpers;
using Kitsune.Constants;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using Kitsune.Models.BuildAndRunModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Kitsune.Compiler.BuildService
{
	class BuildAndRunHelper
	{
		public List<CompileResult> BuildProject(string userEmail, string projectId, bool _isDev, int currentCompilerVersion)
		{
			try
			{
				var response = new List<CompileResult>();
				var api = new CompilerAPI();
				var oldPage = false;
				var changedKDLOrPartial = false;
				var filesCompiledCount = 0;
				var buildStatusUpdateResult = true;
				var dict = new Dictionary<string, int>();
				System.Diagnostics.Stopwatch compileStopWatch = new System.Diagnostics.Stopwatch();
				Log.Information("Compilation started");
				dict.Add("LINK", 0);
				//List<BuildError> BuildErrors;
				var userId = new UserAPI().GetUserId(userEmail);
				var project = api.GetProjectDetailsApi(userEmail, projectId);
                var forceBuild = false;
                var skipValidation = false;


                if (project == null)
				{
					response.Add(new CompileResult { Success = false, ErrorMessages = new List<CompilerError>() { new CompilerError { Message = $"Project with projectid \"{projectId}\" not found." } } });
					return response;
				}
                else
                {
				    forceBuild = project.CompilerVersion < currentCompilerVersion;
                }

				if (project.Resources.Any(x => Kitsune.Helper.Constants.ConfigFileRegularExpression.IsMatch(x.SourcePath)))
				{
					var settings = api.GetProjectResourceDetailsApi(userEmail, projectId, HttpUtility.UrlEncode("/kitsune-settings.json"));
					if (!string.IsNullOrEmpty(settings.HtmlSourceString))
					{
						try
						{
							var settingsOb = JsonConvert.DeserializeObject<dynamic>(settings.HtmlSourceString);
							if (settingsOb.GetType() == typeof(JObject))
							{
								var cleanBuild = ((JObject)settingsOb).SelectToken("build.cleanBuild");
								if (cleanBuild != null && !string.IsNullOrEmpty((string)cleanBuild))
								{
									if ("true" == ((string)cleanBuild).ToLower())
										forceBuild = true;
								}
                                var syntaxValidation = ((JObject)settingsOb).SelectToken("build.syntax_validation");
                                if (syntaxValidation != null && !string.IsNullOrEmpty((string)syntaxValidation))
                                {
                                    if ("true" == ((string)syntaxValidation).ToLower())
                                    {
                                        skipValidation = false;
                                    }
                                    else
                                    {
                                        skipValidation = true;
                                    }
                                    Console.WriteLine($"skipValidation:{skipValidation}");
                                }
                            }

						}
						catch { }

					}
				}

				project.UserEmail = userEmail;
				project.Resources = project.Resources.Where(x => x.IsStatic == false && x.ResourceType != Kitsune.Models.Project.ResourceType.APPLICATION).ToList();
				var staticFileCount = project.Resources.Count();
				dict.Add("TOTAL", staticFileCount);

				var languageEntity = new KEntity
				{
					Classes = new List<KClass>()
				};
				if (!string.IsNullOrEmpty(project.SchemaId))
					languageEntity = new LanguageAPI().GetLanguage(project.SchemaId, userId);
				languageEntity = new KLanguageBase().GetKitsuneLanguage(userEmail, projectId, project, languageEntity, userId);

				#region Handle Project Modules
				var componentEntities = UpdateModuleReference(project);
				#endregion

				var partialPages = api.GetPartialPagesDetailsApi(userEmail, projectId, null);
				GetAuditProjectAndResourcesDetailsResponseModel auditProject = new GetAuditProjectAndResourcesDetailsResponseModel();
				DateTime latestBuildDate = DateTime.MinValue;
				if (project.Version > 1 && !forceBuild)
				{
					var latestProjectVersion = project.Version - 1;
					auditProject = api.GetAuditProjectAndResourcesDetails(userEmail, projectId, latestProjectVersion);
				}
				var latestBuild = api.GetLastCompletedBuild(userEmail, projectId);
				if (latestBuild != null && latestBuild.CreatedOn != null)
					latestBuildDate = latestBuild.CreatedOn;

				var ToBeUpdatedResources = new List<CreateOrUpdateResourceRequestModel>();
				var CompiledResources = new Dictionary<string, CompileResult>();
				var UncompiledResources = new List<CompileResourceRequest>();
                
				//put partial pages first   
				var partialResources = project.Resources.FindAll(x => x.PageType == Kitsune.Models.Project.KitsunePageType.PARTIAL);
				if (partialResources.Any())
				{
					foreach (var resource in partialResources)
					{
						project.Resources.Remove(resource);
						project.Resources.Insert(0, resource);
					}
				}
				foreach (var page in project.Resources)
				{
					oldPage = false;
					var resource = api.GetProjectResourceDetailsApi(userEmail, projectId, HttpUtility.UrlEncode(page.SourcePath));
					// page.IsStatic = KitsuneCompiler.IsResourceStatic(resource.HtmlSourceString, page.SourcePath);
					if (!page.IsStatic)
					{
						var compiled = false;
						var document = new HtmlDocument();
						Console.WriteLine("Page: " + page.SourcePath);
						var req = new CompileResourceRequest();
						var outsd = new CompileResult();
						if (project.Version > 1 && !forceBuild)
							oldPage = auditProject.Resources.Any(x => x.SourcePath == page.SourcePath);
						GetFileFromS3RequestModel resourceTreeRequest = new GetFileFromS3RequestModel
						{
							SourcePath = string.Format(CompilerConstants.TreeFileName, page.SourcePath),
							ProjectId = page.ProjectId,
							BucketName = project.BucketNames.demo
						};
						var resourceTreePresent = api.GetFileFromS3(resourceTreeRequest);
						if (!changedKDLOrPartial && project.Version > 1 && oldPage && page.UpdatedOn <= latestBuildDate && !string.IsNullOrEmpty(resourceTreePresent))
						{
							var samePage = auditProject.Resources.Find(x => x.SourcePath == page.SourcePath);
							GetFileFromS3RequestModel FileRequest = new GetFileFromS3RequestModel
							{
								SourcePath = samePage.SourcePath,
								ProjectId = samePage.ProjectId,
								BucketName = project.BucketNames.demo,
								Compiled = true,
								Version = samePage.Version
							};
							outsd = new CompileResult()
							{
								CompiledString = api.GetFileFromS3(FileRequest),
								Success = true
							};
							req.PageType = samePage.PageType;
							req.UrlPattern = samePage.UrlPattern;
							req.SourcePath = samePage.SourcePath;
							compiled = false;
						}
						else
						{
							req = new CompileResourceRequest { SourcePath = page.SourcePath, FileContent = resource.HtmlSourceString, UserEmail = project.UserEmail, ProjectId = projectId, IsDev = _isDev, IsPublish = true, IsStatic = page.IsStatic, IsDefault = page.IsDefault, ClassName = page.ClassName, UrlPattern = page.UrlPattern, KObject = page.KObject, PageType = page.PageType, ResourceType = page.ResourceType };
							//Compile config file

							compileStopWatch.Reset();
							compileStopWatch.Start();
                            if (skipValidation)
                            {
                                outsd = new CompileResult();
                                outsd.CompiledString = req.FileContent;
                                outsd.PageName = req.SourcePath;
                                req.UrlPattern = req.SourcePath;
                                page.UrlPattern = req.SourcePath;
                                outsd.Success = true;
                                
                            }
							else if (Kitsune.Helper.Constants.ConfigFileRegularExpression.IsMatch(req.SourcePath))
							{
								outsd = KitsuneCompiler.ValidateJsonConfig(new ValidateConfigRequestModel
								{
									File = new ConfigFile { Content = req.FileContent },
									UserEmail = req.UserEmail
								});
								outsd.CompiledString = req.FileContent;
								outsd.PageName = req.SourcePath;
								req.UrlPattern = req.SourcePath;
								page.UrlPattern = req.SourcePath;
							}
							//Compile dynamic html file
							else
							{
                                Console.WriteLine($"Compiling : {req.SourcePath}");
								outsd = KitsuneCompiler.CompileHTML(req, out document, project, languageEntity, partialPages, componentEntities: componentEntities);

							}

							if (!outsd.Success)
								response.Add(outsd);
							else
							{

								CompileLog(req.SourcePath, compileStopWatch.ElapsedMilliseconds.ToString());

								// Console.WriteLine($"'{req.SourcePath}'-{compileStopWatch.ElapsedMilliseconds.ToString()}");
								ResourceMetaInfo metaInfo = new ResourceMetaInfo();
								try
								{
									var tagsToIgnore = new List<string>();
									if (outsd.CustomVariables != null)
										tagsToIgnore.AddRange(outsd.CustomVariables.Keys);
									metaInfo = ResourceMetaInfoGenerator.MetaInfo(req, outsd.CompiledString, project, languageEntity, tagsToIgnore);
									outsd.MetaClass = metaInfo?.metaClass;
									outsd.MetaProperty = metaInfo?.Property;
								}
								catch (Exception ex)
								{
									metaInfo = new ResourceMetaInfo
									{
										IsError = true,
										Message = string.Format("Excepiton occured while generating metainfo: {0}", ex.Message)
									};
								}
								CompiledResources.Add(string.Format("{0}:{1}", req.ProjectId, req.SourcePath), outsd);
							}

							if (!changedKDLOrPartial && oldPage)
							{
								var samePage = auditProject.Resources.Find(x => x.SourcePath == page.SourcePath);
								if (samePage.UrlPattern != req.UrlPattern)
									changedKDLOrPartial = true;
								if (samePage.PageType == Kitsune.Models.Project.KitsunePageType.PARTIAL)
									changedKDLOrPartial = true;
							}
							compiled = true;
						}

						//Replacing the aws path to the version controlled folder
						//outsd.CompiledString = KitsuneCompiler.AddAuditAssetsPath(projectId, outsd.CompiledString, project.Version);


						//Save compiled resource to kitsune resource collection
						var updatePageRequest = new CreateOrUpdateResourceRequestModel
						{
							Errors = null,
							FileContent = req.FileContent,
							SourcePath = req.SourcePath,
							ClassName = req.ClassName,
							ProjectId = req.ProjectId,
							UserEmail = req.UserEmail,
							UrlPattern = req.UrlPattern,
							IsStatic = req.IsStatic,
							IsDefault = req.IsDefault,
							PageType = req.PageType,
							KObject = req.KObject,
							CustomVariables = outsd.CustomVariables,
							Offset = req.Offset
						};



						if (compiled)
						{
							ToBeUpdatedResources.Add(updatePageRequest);
							filesCompiledCount += 1;

							if (filesCompiledCount % 5 == 0)
							{
								try
								{
									dict["LINK"] = filesCompiledCount;
									buildStatusUpdateResult = api.UpdateBuildStatus(userEmail, new CreateOrUpdateKitsuneStatusRequestModel
									{
										ProjectId = projectId,
										Compiler = dict,
										Stage = BuildStatus.Compiled
										//Error = BuildErrors
									});
								}
								catch (Exception ex)
								{

								}
							}

						}
						else if (!compiled)
							UncompiledResources.Add(new CompileResourceRequest
							{
								FileContent = resource.HtmlSourceString,
								SourcePath = page.SourcePath,
								IsStatic = page.IsStatic,
								IsDefault = page.IsDefault,
								UrlPattern = page.UrlPattern,
								ClassName = page.ClassName,

							});
					}
				}
				//Exception operations on uncompiled Pages
				if (!skipValidation && UncompiledResources.Any())
				{
					if (changedKDLOrPartial)
					{
						foreach (var currpage in UncompiledResources)
						{
							var document = new HtmlDocument();
							var req = new CompileResourceRequest { SourcePath = currpage.SourcePath, FileContent = currpage.FileContent, UserEmail = project.UserEmail, ProjectId = projectId, IsDev = _isDev, IsPublish = true, IsStatic = currpage.IsStatic, IsDefault = currpage.IsDefault, ClassName = currpage.ClassName, UrlPattern = currpage.UrlPattern };

							compileStopWatch.Reset();
							compileStopWatch.Start();

                            Console.WriteLine($"Compiling UncompiledResources : {currpage.SourcePath}");

							var outsd = KitsuneCompiler.CompileHTML(req, out document, project, languageEntity, partialPages, componentEntities: componentEntities);

							if (!outsd.Success)
								response.Add(outsd);
							else
							{
								CompileLog(currpage.SourcePath, compileStopWatch.ElapsedMilliseconds.ToString());

								ResourceMetaInfo metaInfo = new ResourceMetaInfo();
								try
								{
									var tagsToIgnore = new List<string>();
									if (outsd.CustomVariables != null)
										tagsToIgnore.AddRange(outsd.CustomVariables.Keys);
									metaInfo = ResourceMetaInfoGenerator.MetaInfo(req, outsd.CompiledString, project, languageEntity, tagsToIgnore);
									outsd.MetaClass = metaInfo?.metaClass;
									outsd.MetaProperty = metaInfo?.Property;
								}
								catch (Exception ex)
								{
									metaInfo = new ResourceMetaInfo
									{
										IsError = true,
										Message = string.Format("Excepiton occured while generating metainfo: {0}", ex.Message)
									};
								}
								CompiledResources.Add(string.Format("{0}:{1}", req.ProjectId, req.SourcePath), outsd);
							}
							//outsd.CompiledString = KitsuneCompiler.AddAuditAssetsPath(projectId, outsd.CompiledString, project.Version);
							ToBeUpdatedResources.Add(new CreateOrUpdateResourceRequestModel
							{
								Errors = null,
								FileContent = req.FileContent,
								SourcePath = req.SourcePath,
								ClassName = req.ClassName,
								ProjectId = req.ProjectId,
								UserEmail = req.UserEmail,
								UrlPattern = req.UrlPattern,
								IsStatic = req.IsStatic,
								IsDefault = req.IsDefault,
								PageType = req.PageType,
								KObject = req.KObject,
								Offset = req.Offset,
								CustomVariables = outsd.CustomVariables
							});
							filesCompiledCount += 1;
							if (filesCompiledCount % 5 == 0)
							{
								try
								{
									dict["LINK"] = filesCompiledCount;
									buildStatusUpdateResult = api.UpdateBuildStatus(userEmail, new CreateOrUpdateKitsuneStatusRequestModel
									{
										ProjectId = projectId,
										Compiler = dict,
										Stage = BuildStatus.Compiled
										//Error = BuildErrors
									});
								}
								catch (Exception ex)
								{

								}
							}


						}
					}
					else
					{
						foreach (var currpage in UncompiledResources)
						{
							//SubmittedResources.Add(new CreateOrUpdateResourceRequest
							//{
							//    Errors = null,
							//    FileContent = currpage.HtmlSourceString,
							//    SourcePath = currpage.SourcePath.Trim().ToUpper(),
							//    ClassName = currpage.ClassName,
							//    ProjectId = currpage.ProjectId,
							//    UserEmail = currpage.UserEmail,
							//    UrlPattern = currpage.UrlPattern,
							//    IsStatic = currpage.IsStatic,
							//    IsDefault = currpage.IsDefault,
							//    PageType = currpage.PageType,
							//    KObject = currpage.KObject
							//});
							filesCompiledCount += 1;
							if (filesCompiledCount % 5 == 0)
							{
								try
								{
									dict["LINK"] = filesCompiledCount;
									buildStatusUpdateResult = api.UpdateBuildStatus(userEmail, new CreateOrUpdateKitsuneStatusRequestModel
									{
										ProjectId = projectId,
										Compiler = dict,
										Stage = BuildStatus.Compiled
										//Error = BuildErrors
									});
								}
								catch (Exception ex)
								{

								}
							}

						}
					}
				}

				try
				{
					dict["LINK"] = filesCompiledCount;
					buildStatusUpdateResult = api.UpdateBuildStatus(userEmail, new CreateOrUpdateKitsuneStatusRequestModel
					{
						ProjectId = projectId,
						Compiler = dict,
						Stage = BuildStatus.Compiled
						//Error = BuildErrors
					});
				}
				catch (Exception ex)
				{

				}
				if (!response.Any())
				{
					List<UrlPatternRegexDetails> UrlPatternRegexList = new List<UrlPatternRegexDetails>();
					foreach (var resource in project.Resources.Where(x => !string.IsNullOrEmpty(x.UrlPattern)))
					{
						var UrlPatternRegex = KitsuneCommonUtils.GenerateUrlPatternRegex(resource.UrlPattern.Substring(resource.UrlPattern.IndexOf("/") + 1), resource.SourcePath);
						UrlPatternRegexList.Add(new UrlPatternRegexDetails
						{
							SourcePath = resource.SourcePath,
							UrlPattern = resource.UrlPattern,
							UrlPatternRegex = UrlPatternRegex
						});
					}
					var UrlPatternRegexDistinct = UrlPatternRegexList.GroupBy(x => x.UrlPatternRegex).Select(x => new { Regex = x.Key, Count = x.Count() }).Where(x => x.Count > 1).ToList();
					if (UrlPatternRegexDistinct != null && UrlPatternRegexDistinct.Any())
					{
						int regexMatchesCount = 1;
						var message = "Url pattern conflict : \n";
						var pageName = string.Empty;
						foreach (var pattern in UrlPatternRegexDistinct)
						{
							var result = UrlPatternRegexList.FindAll(x => x.UrlPatternRegex == pattern.Regex);
							pageName = result?.First()?.SourcePath;
							var partMessage = string.Join(", ", result.Select(x => x.SourcePath).ToArray());
							message += regexMatchesCount.ToString() + ": " + partMessage + "\n";
							regexMatchesCount += 1;
						}
						return new List<CompileResult> {
							new CompileResult {
								ErrorMessages = new List<CompilerError> {
									new CompilerError {
										Message = message,
										LineNumber = 1,
										LinePosition = 1
									}
								},
								Success = false
							}
						};
					}
					List<ResourcePropertyPosition> AllResourcesMeta = new List<ResourcePropertyPosition>();
					var metaClassCompletion = true;
					foreach (var resource in ToBeUpdatedResources)
					{
						Console.WriteLine($"Uploading : {resource?.SourcePath}");
						resource.UrlPatternRegex = UrlPatternRegexList.Find(x => x.SourcePath == resource.SourcePath).UrlPatternRegex;
						var retryCount = 0;
						do
						{
							var isUploaded = api.CreateOrUpdateResourceApi(resource);
							if (!isUploaded)
							{
								Console.WriteLine($"Upload Failed.. retrying {retryCount}..");
								retryCount++;
								if (retryCount >= 3)
									return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = string.Format("Faild to save to DB for file : '{0}'", resource.SourcePath), LineNumber = 1, LinePosition = 1 } }, Success = false } };
							}
							else
								retryCount = 0;

						} while (retryCount != 0);

						if (CompiledResources.ContainsKey(string.Format("{0}:{1}", resource.ProjectId, resource.SourcePath)))
						{
							var currentCompiledResource = CompiledResources[string.Format("{0}:{1}", resource.ProjectId, resource.SourcePath)];
							SaveFileContentToS3RequestModel resourceContent = new SaveFileContentToS3RequestModel
							{
								ProjectId = resource.ProjectId,
								BucketName = project.BucketNames.demo,
								SourcePath = resource.SourcePath,
								FileContent = currentCompiledResource.CompiledString
							};
							var compiledStringResult = api.SaveFileContentToS3(resourceContent);
							if (compiledStringResult == null)
								return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = string.Format("Faild to save to Demo Bucket for file : '{0}'", resource.SourcePath), LineNumber = 1, LinePosition = 1 } }, Success = false } };
							//Save compiled (.kc) file
							if (currentCompiledResource.KitsunePage != null)
							{
								var jsonSerializeSettings = new JsonSerializerSettings();
								jsonSerializeSettings.TypeNameHandling = TypeNameHandling.Auto;
								string output = JsonConvert.SerializeObject(currentCompiledResource.KitsunePage, Formatting.None, jsonSerializeSettings);

								resourceContent = new SaveFileContentToS3RequestModel
								{
									ProjectId = resource.ProjectId,
									BucketName = project.BucketNames.demo,
									SourcePath = $"{resource.SourcePath}.kc",
									FileContent = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(output)),
									base64 = true
								};
								compiledStringResult = api.SaveFileContentToS3(resourceContent);
								if (compiledStringResult == null)
									return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = string.Format("Faild to save to Demo Bucket for file : '{0}'", resource.SourcePath), LineNumber = 1, LinePosition = 1 } }, Success = false } };
							}

							try
							{
								if (Kitsune.Helper.Constants.DynamicFileExtensionRegularExpression.IsMatch(resource.SourcePath.ToLower()))
								{
									if (currentCompiledResource.MetaClass == null)
									{
										metaClassCompletion = false;
										throw new Exception(string.Format("Meta Class is null for resource: {0}", resource.SourcePath));
									}
									var serializedMetaClass = MetaHelper.ProtoSerializer(CompiledResources[string.Format("{0}:{1}", resource.ProjectId, resource.SourcePath)].MetaClass);
									if (string.IsNullOrEmpty(serializedMetaClass))
										throw new Exception(string.Format("Meta Class serialization failed for resource: {0}", resource.SourcePath));
									//return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = string.Format("Faild to serialize meta Class for : {0}", resource.SourcePath), LineNumber = 1, LinePosition = 1 } }, Success = false } };
									resourceContent = new SaveFileContentToS3RequestModel
									{
										ProjectId = resource.ProjectId,
										BucketName = project.BucketNames.demo,
										SourcePath = string.Format(CompilerConstants.TreeFileName, resource.SourcePath),
										FileContent = serializedMetaClass,
										base64 = true
									};
									var metaClassResult = api.SaveFileContentToS3(resourceContent);
									if (metaClassResult == null)
										throw new Exception(string.Format("Meta Class .k file not saved for resource: {0}", resource.SourcePath));
									//return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = string.Format("Faild to save meta class to Demo Bucket for : '{0}'", resource.SourcePath), LineNumber = 1, LinePosition = 1 } }, Success = false } };
									metaClassResult = null;

									AllResourcesMeta.Add(new ResourcePropertyPosition
									{
										SourcePath = resource.SourcePath,
										UrlPattern = resource.UrlPattern,
										Properties = CompiledResources[string.Format("{0}:{1}", resource.ProjectId, resource.SourcePath)].MetaClass
									});
								}
							}
							catch (Exception ex)
							{
								//ToDo: Log MetaInfo Error
							}

						}
					}
					try
					{
						if (AllResourcesMeta.Any() && metaClassCompletion)
						{
							MetaGraphNode metaGraphResult = new MetaGraphNode();
							GetFileFromS3RequestModel GraphRequest = new GetFileFromS3RequestModel
							{
								SourcePath = string.Format(CompilerConstants.GraphFileName, projectId),
								ProjectId = projectId,
								BucketName = project.BucketNames.demo
							};
							var serializedMetaGraph = api.GetFileFromS3(GraphRequest);
							var metaGraphBuilder = new MetaGraphBuilder();
							if (serializedMetaGraph != null)
							{
								var metaGraph = MetaHelper.ProtoDeserialize<MetaGraphNode>(serializedMetaGraph);
								if (metaGraph == null)
									throw new Exception("Meta Graph Deserialization failed");
								//return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = "Failed to deserialize Reference Graph", LineNumber = 1, LinePosition = 1 } }, Success = false } };
								metaGraphResult = metaGraphBuilder.CreateGraph(AllResourcesMeta, metaGraph);
							}
							else
							{
								metaGraphResult = metaGraphBuilder.CreateGraph(AllResourcesMeta);
							}
							if (metaGraphResult == null)
								throw new Exception("Meta Graph creation failed");
							//return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = "Faild to create reference Graph", LineNumber = 1, LinePosition = 1 } }, Success = false } };
							var newSerializedMetaGraph = MetaHelper.ProtoSerializer(metaGraphResult);
							if (string.IsNullOrEmpty(newSerializedMetaGraph))
								throw new Exception("Meta Graph serialization failed");
							//return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = string.Format("Faild to serialize meta Class for : {0}", projectId), LineNumber = 1, LinePosition = 1 } }, Success = false } };
							SaveFileContentToS3RequestModel graphContent = new SaveFileContentToS3RequestModel
							{
								ProjectId = projectId,
								BucketName = project.BucketNames.demo,
								SourcePath = string.Format(CompilerConstants.GraphFileName, projectId),
								FileContent = newSerializedMetaGraph,
								base64 = true
							};
							var graphSaveResult = api.SaveFileContentToS3(graphContent);
							if (graphSaveResult == null)
								throw new Exception("Meta Graph file not saved");
							//return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = "Faild to save Graph to Demo Bucket", LineNumber = 1, LinePosition = 1 } }, Success = false } };
						}
					}
					catch (Exception ex)
					{
						//ToDo: Log MetaInfo Error
					}

					//Update the project version

					if (project.CompilerVersion != currentCompilerVersion)
					{
						APIHelpers.UpdateProjectCompilerVersion(userEmail, projectId, currentCompilerVersion);
					}
				}
				return response;
			}
			catch (Exception ex)
			{
				Log.Error($"BuildProject : Exception : {ex.Message}, {ex.StackTrace}, {(ex.Data != null ? ex.Data.Count > 0 ? Newtonsoft.Json.JsonConvert.SerializeObject(ex.Data) : "" : "")}");
				return new List<CompileResult> { new CompileResult { ErrorMessages = new List<CompilerError> { new CompilerError { Message = "BuildProject : Exception :  Something went wrong. Please try again.", LineNumber = 1, LinePosition = 1 } }, Success = false } };
			}
		}

		private static List<KLanguageModel> UpdateModuleReference(GetProjectDetailsResponseModel projectDetails)
		{
			//List<Kitsune.Models.Project.KitsuneProject> modulesDetails = null;
			//  var api = new CompilerAPI();

			//if (projectDetails.Modules != null && projectDetails.Modules.Any())
			//{
			//    modulesDetails = new List<Kitsune.Models.Project.KitsuneProject>();
			//    var moduleDetail = new Kitsune.Models.Project.KitsuneProject();
			//    foreach (var module in projectDetails.Modules)
			//    {
			//        moduleDetail = api.GetProjectDetailsByClientIdApi(ClientIdConstants._defaultClientId, module.ProjectId);

			//        if (moduleDetail == null)
			//            return new CompileResult
			//            {
			//                Success = false,
			//                ErrorMessages = new List<CompilerError> { new CompilerError { Message = $"Module {module.ProjectId} not found." } }
			//            };
			//        modulesDetails.Add(moduleDetail);
			//    }
			//}
			var languageEntity = new List<KLanguageModel>();

			if (projectDetails != null && projectDetails.Components != null && projectDetails.Components.Any())
			{

				foreach (var component in projectDetails.Components.Where(x => x.SchemaId != null))
				{

					var moduleEntity = new LanguageAPI().GetLanguageByClientId(component.SchemaId, ClientIdConstants._defaultClientId);

					if (moduleEntity != null)
					{
						languageEntity.Add(new KLanguageModel { _id = component.SchemaId, Entity = moduleEntity });
					}
					else
					{
						///TODO : Could not find module/app entity, Log/Inform user
					}

				}
				//Add appClass as base class which will have reference of all reference app 
				return languageEntity.Any() ? languageEntity : null;
			}
			return null;
		}
		private static void CompileLog(string sourcePath, string time)
		{
			Log.Information($"Compiled '{sourcePath}' {time}");
		}

	}
}
