using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Language.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Compiler.Helpers
{
    public class KLanguageBase
    {
        public KEntity GetKitsuneLanguage(string userEmail, string projectId, GetProjectDetailsResponseModel projectDetails, KEntity language, string userId)
        {
            if (language == null)
                language = new KEntity() { Classes = new List<KClass>(), EntityName = "_system" };
            if (projectDetails != null && language != null)
            {
                var entityTemp = language;

                UpdateClassViews(userEmail, projectDetails, entityTemp);

                #region WebAction
                var webActions = new WebActionAPI().GetWebActionList(userId).Result;
                var webActionBaseClass = new KClass
                {
                    ClassType = KClassType.BaseClass,
                    Description = "Webaction",
                    Name = "webactions",
                };
                IList<KProperty> PropertyList = new List<KProperty>();
                IList<KClass> webactionClassList = new List<KClass>();

                KClass webactionClass;
                if (webActions != null && webActions.WebActions != null)
                    foreach (var webaction in webActions.WebActions)
                    {
                        //Webactions.webactionname
                        PropertyList.Add(new KProperty
                        {
                            DataType = new DataType("wa" + webaction.Name.ToLower()),
                            Description = webaction.Name,
                            Name = webaction.Name.ToLower(),
                            Type = PropertyType.array
                        });

                        //Datatype (class) of the specific webaction
                        webactionClass = new KClass
                        {
                            ClassType = KClassType.UserDefinedClass,
                            Description = "Webaction products",
                            Name = "wa" + webaction.Name.ToLower(),
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
                            DataType = new DataType("STR"),
                            Description = "Id",
                            Name = "_id",
                            Type = PropertyType.str

                        });
                        webactionClass.PropertyList.Add(new KProperty
                        {
                            DataType = new DataType("DATE"),
                            Description = "Created on",
                            Name = "createdon",
                            Type = PropertyType.date
                        });
                        webactionClass.PropertyList.Add(new KProperty
                        {
                            DataType = new DataType("DATE"),
                            Description = "Updated on",
                            Name = "updatedon",
                            Type = PropertyType.date
                        });
                        entityTemp.Classes.Add(webactionClass);
                    }
                webActionBaseClass.PropertyList = PropertyList;
                entityTemp.Classes.Add(webActionBaseClass);
                #endregion

                if (projectId == projectDetails.ProjectId)
                    language = entityTemp;
            }
            return language;
        }
        public PropertyType ConvertProperty(WebActionAPIPropertyType waproperty)
        {
            switch (waproperty)
            {
                case WebActionAPIPropertyType.array: return PropertyType.array;
                case WebActionAPIPropertyType.boolean: return PropertyType.boolean;
                case WebActionAPIPropertyType.date: return PropertyType.date;
                case WebActionAPIPropertyType.image: return PropertyType.obj;
                case WebActionAPIPropertyType.link: return PropertyType.obj;
                case WebActionAPIPropertyType.number: return PropertyType.number;
                case WebActionAPIPropertyType.reference: return PropertyType.obj;
                case WebActionAPIPropertyType.str: return PropertyType.str;
                case WebActionAPIPropertyType.webaction: return PropertyType.obj;
                default: return PropertyType.str;
            }
        }

        static void UpdateClassViews(string userEmail, GetProjectDetailsResponseModel projectDetails, KEntity entity)
        {

            if (projectDetails != null)
            {
                foreach (var resource in projectDetails.Resources.Where(x => x.IsStatic == false && !string.IsNullOrEmpty(x.ClassName)))
                {
                    entity.Classes.Add(new KClass
                    {
                        ClassType = KClassType.BaseClass,
                        Description = resource.SourcePath.ToLower(),
                        Name = resource.ClassName,
                        PropertyList = new List<KProperty>()
                        {
                            new KProperty
                            {
                                DataType = new DataType("FUNCTION"),
                                Description = "Page link",
                                Name = "GETURL",
                                Type = PropertyType.function
                            },
                             new KProperty
                            {
                                DataType = new DataType("FUNCTION"),
                                Description = "Set details page object",
                                Name = "SETOBJECT",
                                Type = PropertyType.function
                            },
                            new KProperty
                            {
                                DataType = new DataType("LINK"),
                                Description = "First page link",
                                Name = "FIRSTPAGE",
                                Type = PropertyType.obj
                            },
                            new KProperty
                            {
                                DataType = new DataType("LINK"),
                                Description = "Last page link",
                                Name = "LASTPAGE",
                                Type = PropertyType.obj
                            },
                            new KProperty
                            {
                                DataType = new DataType("STR"),
                                Description = "Current page number",
                                Name = "CURRENTPAGENUMBER",
                                Type = PropertyType.str
                            },
                            new KProperty
                            {
                                DataType = new DataType("LINK"),
                                Description = "Next page link",
                                Name = "NEXTPAGE",
                                Type = PropertyType.obj
                            },
                            new KProperty
                            {
                                DataType = new DataType("STR"),
                                Description = "Offset of list page",
                                Name = "OFFSET",
                                Type = PropertyType.str
                            },
                            new KProperty
                            {
                                DataType = new DataType("LINK"),
                                Description = "Previous page link",
                                Name = "PREVIOUSPAGE",
                                Type = PropertyType.obj
                            },
                            new KProperty
                            {
                                DataType = new DataType("STR"),
                                Description = "Set object for the details page",
                                Name = "SETOBJECT",
                                Type = PropertyType.function
                            },

                    }
                    });
                }
            }
        }
    }

    #region WebactionAPI
    public class WebActionAPI
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<GetWebActionsAPIResult> GetWebActionList(string userId)
        {
            var webactionBaseUrl = "https://webactions.kitsune.tools/api/v1/List";
            try
            {
                if (userId != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(userId);
                    var response = client.GetAsync(new Uri(webactionBaseUrl)).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        throw new Exception("UnAuthorized");
                    var rawResult = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<GetWebActionsAPIResult>(rawResult);
                    return result;
                }
                throw new Exception("UnAuthorized");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    public class GetWebActionsAPIResult
    {
        public IEnumerable<WebActionAPIResultItem> WebActions { get; set; }
    }
    public class WebActionAPIResultItem
    {
        public string ActionId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<WebActionAPIProperty> Properties { get; set; }
    }
    public class WebActionAPIProperty
    {
        public string DisplayName { get; set; }
        public string PropertyName { get; set; }
        public WebActionAPIPropertyType PropertyType { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRegex { get; set; }
    }
    public enum WebActionAPIPropertyType
    {
        str,
        array,
        number,
        boolean,
        date,
        image,
        link,
        webaction,
        reference
    }
    #endregion
}
