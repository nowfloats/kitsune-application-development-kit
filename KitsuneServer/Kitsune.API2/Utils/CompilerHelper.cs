using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.IO.Compression;
using Kitsune.Language.Models;
using static Kitsune.Language.Models.KEntity;
using Kitsune.Language.Helper;
using System.Text.RegularExpressions;
using Kitsune.Compiler.Model;
using Kitsune.Models;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Helper;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Kitsune.API2.Validators;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.Compiler.Helpers;

namespace Kitsune.API2.Utils
{
    public class CompilerHelper
    {
        public static ResourceCompilationResult CompileProjectResource(CompileResourceRequest req,
            GetPartialPagesDetailsResponseModel partialPages = null)
        {
            //Source path should always start with / (to make sure its from root folder)
            if (!string.IsNullOrEmpty(req.SourcePath))
                req.SourcePath = string.Format("/{0}", req.SourcePath.Trim('/'));
            else
                return new ResourceCompilationResult { Success = false, ErrorMessages = new List<CompilerError> { new CompilerError { LineNumber = 1, LinePosition = 1, Message = "Source path can not be empty." } } };

            if (!string.IsNullOrEmpty(req.FileContent))
            {
                var document = new HtmlDocument();
                CompileResult compileResult = null;
                try
                {
                    //If the file is html extension then only compile 
                    if (Kitsune.Helper.Constants.DynamicFileExtensionRegularExpression.IsMatch(req.SourcePath.ToLower()))
                    {
                        try
                        {
                            var byteStream = Convert.FromBase64String(req.FileContent);
                            req.FileContent = System.Text.Encoding.UTF8.GetString(byteStream);
                        }
                        catch
                        {
                            return new ResourceCompilationResult { Success = false, ErrorMessages = new List<CompilerError> { new CompilerError { Message = "Input string is not valid base64 string" } } };
                        }

                        var projectDetailsRequestModel = new GetProjectDetailsRequestModel
                        {
                            ProjectId = req.ProjectId,
                            ExcludeResources = false,
                            UserEmail = req.UserEmail
                        };
                        var projectDetails = MongoConnector.GetProjectDetails(projectDetailsRequestModel);
                        if (projectDetails == null)
                            return new ResourceCompilationResult
                            {
                                Success = false,
                                ErrorMessages = new List<CompilerError> { new CompilerError { Message = "Project not found." } }
                            };



                        var user = MongoConnector.GetUserIdFromUserEmail(new GetUserIdRequestModel
                        {
                            UserEmail = req.UserEmail
                        });
                        var languageEntity = MongoConnector.GetLanguageEntity(new GetLanguageEntityRequestModel
                        {
                            EntityId = projectDetails.SchemaId,
                        });
                        languageEntity = new KLanguageBase().GetKitsuneLanguage(req.UserEmail, req.ProjectId, projectDetails, languageEntity, user.Id);

                        #region Handle Project Components
                        var componentEntities = GetComponentReference(projectDetails.Components);
                        #endregion

                        //Compile only if the file type is supported
                        compileResult = KitsuneCompiler.CompileHTML(req, out document, projectDetails, languageEntity, partialPages, componentEntities: componentEntities);
                    }
                    //Validate the config file 
                    else if (Helper.Constants.ConfigFileRegularExpression.IsMatch(req.SourcePath))
                    {
                        var byteStream = Convert.FromBase64String(req.FileContent);
                        req.FileContent = System.Text.Encoding.UTF8.GetString(byteStream);
                        compileResult = KitsuneCompiler.ValidateJsonConfig(new ValidateConfigRequestModel
                        {
                            File = new ConfigFile { Content = req.FileContent },
                            UserEmail = req.UserEmail
                        });
                        req.UrlPattern = req.SourcePath;
                        req.IsStatic = false;
                    }
                }
                catch (Exception ex)
                {

                }

                if (compileResult != null && compileResult.ErrorMessages?.Count() > 0)
                {
                    return new ResourceCompilationResult { Success = false, ErrorMessages = compileResult.ErrorMessages };
                }
                else
                    return new ResourceCompilationResult { Success = true };
            }

            return new ResourceCompilationResult { Success = false, ErrorMessages = new List<CompilerError> { new CompilerError { Message = "Input should not be empty" } } };

        }

        private static List<KLanguageModel> GetComponentReference(List<Kitsune.Models.Project.ProjectComponent> components)
        {
            //List<Kitsune.Models.Project.KitsuneProject> modulesDetails = null;
            //if (modules != null && modules.Any())
            //{
            //    modulesDetails = new List<Kitsune.Models.Project.KitsuneProject>();
            //    var moduleDetail = new Kitsune.Models.Project.KitsuneProject();
            //    foreach (var module in modules)
            //    {
            //        moduleDetail = MongoConnector.GetProjectDetails(module.ProjectId);
            //        if (moduleDetail == null)
            //        {
            //            return (null, new ResourceCompilationResult
            //            {
            //                Success = false,
            //                ErrorMessages = new List<CompilerError> { new CompilerError { Message = $"Module {module.ProjectId} not found." } }
            //            });
            //        }

            //        modulesDetails.Add(moduleDetail);
            //    }
            //}

            // if (modulesDetails != null && modulesDetails.Any())
            //{
            var languageEntity = new List<KLanguageModel>();
            if (components != null && components.Any())
            {
                foreach (var component in components)
                {

                    var componentEntity = MongoConnector.GetLanguageEntity(new GetLanguageEntityRequestModel
                    {
                        EntityId = component.SchemaId,
                    });


                    if (componentEntity != null)
                    {
                        languageEntity.Add(new KLanguageModel { _id = component.SchemaId, Entity = componentEntity });
                    }
                    else
                    {
                        ///TODO : Could not find component/app entity, Log/Inform user
                    }
                    return languageEntity.Any() ? languageEntity : null;
                }
            }

            return null;
        }
    }
}
