using Kitsune.Compiler.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Configuration;
using System.Diagnostics;
using Kitsune.Models;
using Kitsune.Compiler.Model;
using System.Web;
using System.Text.RegularExpressions;
using Kitsune.Helper;
using Kitsune.SyntaxParser;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Language.Models;

namespace Kitsune.Compiler
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class CompilerService
    {
        public static HtmlDocument DocumentObject()
        {
            var document = new HtmlDocument
            {
                OptionAutoCloseOnEnd = true
            };
            return document;
        }




        public async Task<ProjectPreview> GetPreviewAsync(string userEmail,string projectId, GetResourceDetailsResponseModel resourceDetails,
            GetProjectDetailsResponseModel projectDetails,
            KEntity languageEntity,
            GetPartialPagesDetailsResponseModel partialPages, 
            string fpTag, string userId)
        {
            var watch = new Stopwatch();
            string compilerTime = "";
            watch.Start();

            ProjectPreview result = null;
            if (projectId != null && resourceDetails != null)
            {
                var doc = new HtmlAgilityPack.HtmlDocument();

                var compileResult = KitsuneCompiler.CompileHTML(new CompileResourceRequest
                {
                    UserEmail = userEmail,
                    FileContent = resourceDetails.HtmlSourceString,
                    IsDev = true,
                    IsPublish = true,
                    ProjectId = projectId,
                    SourcePath = resourceDetails.SourcePath,
                    ClassName = resourceDetails.ClassName,
                    IsPreview = true

                }, out doc, projectDetails, languageEntity, partialPages);
                compilerTime += ", CompileHTML : " + watch.ElapsedMilliseconds.ToString();

                watch.Restart();
                var previewRes = "";
                if (!compileResult.Success)
                {
                    previewRes = ErrorCodeConstants.CompilationError;
                }
                else if (resourceDetails.IsStatic)
                    previewRes = compileResult.CompiledString;
                else
                {
                    try
                    {
                        var client = new HttpClient();
                        client.BaseAddress = new Uri("http://kitsune-demo-identifier-952747768.ap-south-1.elb.amazonaws.com");
                        byte[] bytes = Encoding.UTF8.GetBytes(compileResult.CompiledString);
                        string base64 = Convert.ToBase64String(bytes);
                        var obData = new KitsunePreviewModel
                        {
                            DeveloperId = userId,
                            FileContent = base64,
                            NoCacheQueryParam = null,
                            ProjectId = projectId,
                            View = resourceDetails.SourcePath,
                            WebsiteTag = fpTag
                        };
                        var jsonData = JsonConvert.SerializeObject(obData);

                        var response = await client.PostAsync("/home/kpreview", new StringContent(jsonData, Encoding.UTF8, "application/json"));
                        if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                        {
                            // Log(response?.Content?.ReadAsStringAsync()?.Result);
                            throw new Exception(ErrorCodeConstants.UnableToGetPreview);
                        }
                        previewRes = response.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }


                }
                var previewTime = watch.ElapsedMilliseconds;
                result = new ProjectPreview
                {
                    HtmlString = previewRes,
                    CompilerTime = compilerTime,
                    PreviewTime = previewTime
                };
            }
            return result;
        }

        #region ThemeAssets

        public AssetChildren ProjectResourceTree(List<string> resources, string projectName)
        {
            var final = new AssetChildren
            {
                children = new List<AssetChildren>(),
                name = projectName,
                toggled = true,
                IsKitsune = true
            };
            string data, filePath;

            for (var i = 0; i < resources.Count; i++)
            {
                data = resources[i];
                filePath = resources[i];
                GenerateAssetTree(final.children, filePath, filePath, true);
            }
            final.children = OrderLeaves(final.children);
            // RemoveEmptyChildren(final);
            return final;
        }
        public List<AssetChildren> OrderLeaves(List<AssetChildren> children)
        {
            if (children != null)
            {
                children = children.OrderByDescending(x => x.children.Count > 0 ? 1 : 0).ThenBy(x => x.name).ToList();

                foreach (var child in children)
                {
                    if (child.children != null && child.children.Any())
                        child.children = OrderLeaves(child.children);
                    else
                        child.children = null;
                }
            }
            return children;
        }
        private AssetChildren GenerateAssetTree(List<AssetChildren> children, string remainingPath, string filePath, bool isKitsune)
        {
            var search = remainingPath.TrimStart('/').IndexOf('/');
            if (remainingPath.StartsWith("/") && search != -1)
                search += 1;
            List<AssetChildren> firstCheck = null;
            string current, remaining;
            AssetChildren addObj;
            if (search != -1)
            {
                current = remainingPath.Substring(0, search);
                remaining = remainingPath.Substring(search + 1, (remainingPath.Length - search - 1));
            }
            else
            {
                current = remainingPath;
                remaining = "";
            }

            if (current != "")
            {
                firstCheck = children.Where(x => x.name == current.TrimStart('/'))?.ToList();
                if (firstCheck != null && firstCheck.Any())
                {
                    return GenerateAssetTree(firstCheck[0].children, remaining, filePath, isKitsune);
                }
                else
                {
                    addObj = new AssetChildren
                    {
                        name = current.TrimStart('/'),
                        Path = filePath.Substring(0, (filePath.IndexOf(current) + current.Length)),
                        children = new List<AssetChildren>(),
                        IsKitsune = isKitsune
                    };
                    children.Insert(0, addObj);
                    if (remaining != "")
                    {
                        return GenerateAssetTree(children[0].children, remaining, filePath, isKitsune);
                    }
                }
            }
            return null;
        }
        private void RemoveEmptyChildren(AssetChildren obj)
        {
            if (obj.children == null || obj.children.Count == 0)
            {
                obj.children = null;
                obj.toggled = null;
            }
            else
            {
                foreach (var child in obj.children)
                {
                    RemoveEmptyChildren(child);
                }
            }
        }
        #endregion
    }
}
