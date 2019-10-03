using Kitsune.Language.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class CreateOrUpdateLanguageEntityRequestModel : BaseModel
    {
        //TO-DO (Duplicate var) remove schemaId or remove languageId
        public string SchemaId { get; set; }

        public string LanguageId { get; set; }
        public string UserId { get; set; }
        public string ClientId { get; set; }
        public KEntity Entity { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (this.Entity == null)
                validationResultList.Add(new ValidationResult("Entity can not be null"));
            if (this.Entity != null && string.IsNullOrEmpty(this.Entity.EntityName))
                validationResultList.Add(new ValidationResult("Entity name can not be empty"));
            else if (string.IsNullOrEmpty(this.UserId))
                validationResultList.Add(new ValidationResult("User id can not be empty"));

            return validationResultList;
        }

    }
    public class CreateDatatypeEntityRequestModel : BaseModel
    {
        public string LanguageId { get; set; }
        public string UserId { get; set; }
        public KClass Datatype { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (this.Datatype != null && !string.IsNullOrWhiteSpace(this.Datatype.Name))
                validationResultList.Add(new ValidationResult("Class name can not be empty"));
            else if (string.IsNullOrEmpty(this.UserId))
                validationResultList.Add(new ValidationResult("User id can not be empty"));

            return validationResultList;
        }

    }
    public class AddPropertyRequestModel : BaseModel
    {
        public string LanguageId { get; set; }
        public string UserId { get; set; }
        public string ClassName { get; set; }
        public KProperty Property { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (this.Property != null && !string.IsNullOrEmpty(this.Property.Name))
                validationResultList.Add(new ValidationResult("Property name can not be empty"));
            if (this.ClassName != null && !string.IsNullOrEmpty(this.ClassName))
                validationResultList.Add(new ValidationResult("Class name can not be empty"));
            else if (string.IsNullOrEmpty(this.UserId))
                validationResultList.Add(new ValidationResult("User id can not be empty"));

            return validationResultList;
        }

    }

    public class SchemaItem
    {
        public string SchemaId { get; set; }
        public string EntityDescription { get; set; }
        public string EntityName { get; set; }
    }

    public class GetLanguageSchemaRequestModel : BaseModel
    {
        public string UserId { get; set; }
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.UserId))
                validationResultList.Add(new ValidationResult("User id can not be null"));
            return validationResultList;
        }
    }
    public class GetLanguageSchemaResponseResult
    {
        public List<SchemaItem> Schemas { get; set; }
    }

    //public class SchemaToWebSite : BaseModel
    //{
    //    public string SchemaId { get; set; }
    //    public string WebsiteId { get; set; }
    //    public string ProjectId { get; set; }
    //    public string UserId { get; set; }
    //}
    public class MapSchemaToProjectRequestModel : BaseModel
    {
        public string SchemaId { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.SchemaId))
                validationResultList.Add(new ValidationResult("Schema id can not be null"));
            return validationResultList;
        }
    }
    //public class UnMapSchemaToWebSiteRequestModel : SchemaToWebSite
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        return validationResultList;
    //    }

    //}

    public class UpdateWebsiteDataRequestModel : BaseModel
    {
        public bool IgnoreNested { get; set; }
        public object Query { get; set; }
        public bool Multi { get; set; }
        public Dictionary<string, object> UpdateValue { get; set; }
        public string SchemaName { get; set; }
        public string SchemaId { get; set; }
        public string WebsiteId { get; set; }
        public List<UpdateDataItem> BulkUpdates { get; set; }
        public string UserId { get; set; }
        public List<ReferenceItem> ReferenceList { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.SchemaName) && string.IsNullOrEmpty(this.SchemaId))
                validationResultList.Add(new ValidationResult("Language name can not be empty"));
            if (!string.IsNullOrEmpty(this.SchemaName))
                this.SchemaName = this.SchemaName.Replace(" ", "");
            return validationResultList;
        }
    }
    public class UpdateDataItem
    {
        public object Query { get; set; }

        public Dictionary<string, object> UpdateValue { get; set; }

    }
    public class ReferenceItem
    {
        public string ParentId { get; set; }
        public string ParentClassName { get; set; }
        public string PropertyName { get; set; }
    }

    public class DeleteDataObjectRequestModel : BaseModel
    {
        public object Query { get; set; }
        public string SchemaName { get; set; }
        public List<object> BulkDelete { get; set; }

        public string WebsiteId { get; set; }
        public string UserId { get; set; }

    }
    public class GlobalSearchDataRequestModel : BaseModel
    {
        public string SearchText { get; set; }
        public string SchemaId { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
        public string WebsiteId { get; set; }
        public string UserId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("WebsiteId can not be empty"));
            if (string.IsNullOrEmpty(this.SearchText))
                validationResultList.Add(new ValidationResult("Search text can not be empty"));
            //if (string.IsNullOrEmpty(this.SchemaId))
            //    validationResultList.Add(new ValidationResult("Schema Id can not be empty"));
            //else
            //{
            //    this.SchemaName = this.SchemaName.Trim().ToUpper();
            //    this.SchemaName = this.SchemaName.Replace(" ", "");
            //}
            if (string.IsNullOrEmpty(this.UserId))
                validationResultList.Add(new ValidationResult("User Id can not be empty"));
            //this.SchemaName = this.SchemaName.Replace(" ", "");
            return validationResultList;
        }
    }
    public class SearchDataRequestModel : BaseModel
    {
        public string SearchText { get; set; }
        public string SchemaName { get; set; }
        public string SearchProperty { get; set; }
        public string Sort { get; set; }
        public string Filter { get; set; }
        public List<string> Include { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
        public string WebsiteId { get; set; }
        public string UserId { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("WebsiteId can not be empty"));
            //if (string.IsNullOrEmpty(this.SearchText))
            //    validationResultList.Add(new ValidationResult("Search text can not be empty"));
            if (string.IsNullOrEmpty(this.SchemaName))
                validationResultList.Add(new ValidationResult("Schema name can not be empty"));
            else
            {
                this.SchemaName = this.SchemaName.Trim().ToUpper();
                this.SchemaName = this.SchemaName.Replace(" ", "");
            }
            if (string.IsNullOrEmpty(this.SearchProperty))
                validationResultList.Add(new ValidationResult("Search property can not be empty"));

            if (string.IsNullOrEmpty(this.UserId))
                validationResultList.Add(new ValidationResult("User Id can not be empty"));
            this.SchemaName = this.SchemaName.Replace(" ", "");
            return validationResultList;
        }
    }
    public class GetWebsiteDataRequestModel : BaseModel
    {
        public string WebsiteId { get; set; }
        public string UserId { get; set; }
        public string SchemaName { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
        public string Query { get; set; }
        public string Include { get; set; }
        public string Sort { get; set; }
        public string Aggrigate { get; set; }
        public string ClientId { get; set; }
        public string SchemaId { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            //if (string.IsNullOrEmpty(this.SchemaName))
            //    validationResultList.Add(new ValidationResult("Schema name can not be empty"));
            //else
            //{
            //    this.SchemaName = this.SchemaName.Trim().ToUpper();
            //    this.SchemaName = this.SchemaName.Replace(" ", "");

            //}
            //if (string.IsNullOrEmpty(command.UserId))
            //    yield return new ValidationResult("CreateOrUpdateWebAction", "User Id can not be empty");

            return validationResultList;
        }
    }
    public class Pagination
    {
        public int CurrentIndex { get; set; }
        public long TotalCount { get; set; }
        public int PageSize { get; set; }
    }
    public class GetWebsiteDataResponseModel
    {
        public List<object> Data { get; set; }
        public Pagination Extra { get; set; }

    }

    public class GlobalSearchResult
    {
        public string _kid { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public List<string> Keywords { get; set; }
    }
    public class GetWebsiteDataByTypeResponseModel
    {
        public List<GroupCount> GroupCount { get; set; }
        public List<object> Data { get; set; }
    }

    public class GetWebsiteDataByTypeRequestModel : BaseModel
    {
        public string ClassName { get; set; }
        public string UserId { get; set; }
        public string WebsiteId { get; set; }
        public string SchemaName { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.SchemaName))
                validationResultList.Add(new ValidationResult("Schema name can not be empty"));
            else
            {
                this.SchemaName = this.SchemaName.Trim().ToUpper();
                this.SchemaName = this.SchemaName.Replace(" ", "");

            }
            //if (string.IsNullOrEmpty(command.UserId))
            //    yield return new ValidationResult("CreateOrUpdateWebAction", "User Id can not be empty");

            return validationResultList;
        }
    }

    public class GroupCount
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public List<GroupCount> SubGroupCounts { get => subGroupCounts; set => subGroupCounts = value; }

        private List<GroupCount> subGroupCounts;
        public GroupCount()
        {
            SubGroupCounts = new List<GroupCount>();
        }
    }

    public class WebsiteCommand : BaseModel
    {
        public string WebsiteId { get; set; }
        public string UserId { get; set; }
        public string SchemaName { get; set; }
        public string SchemaId { get; set; }
        public string IPAddress { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
    public class AddOrUpdateWebsiteRequestModel : WebsiteCommand
    {
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.SchemaName) && string.IsNullOrEmpty(this.SchemaId))
                validationResultList.Add(new ValidationResult("Schema name can not be empty"));
            else if (!string.IsNullOrEmpty(this.SchemaName))
            {
                this.SchemaName = this.SchemaName.Trim();
                this.SchemaName = this.SchemaName.Replace(" ", "").ToLower();
            };
            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("Website Id can not be empty"));
            if (this.Data == null)
                validationResultList.Add(new ValidationResult("Data can not be empty"));

            return validationResultList;
        }
    }

    public class GetLanguageEntityRequestModel : BaseModel
    {
        public string EntityId { get; set; }
        public int Version { get; set; }
        public string UserId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.EntityId))
                validationResultList.Add(new ValidationResult("Entity id can not be Empty"));

            return validationResultList;
        }
    }

    public class GetSimilarPropertiesInLanguageEntityRequestModel : BaseModel
    {
        public string EntityId { get; set; }
        public string UserId { get; set; }
        public string ClassName { get; set; }
        public string WebsiteId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.EntityId))
                validationResultList.Add(new ValidationResult("Entity id can not be Empty"));

            if (string.IsNullOrEmpty(this.ClassName))
                validationResultList.Add(new ValidationResult("Class name can not be Empty"));

            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("Website id can not be Empty"));

            return validationResultList;

        }
    }

    public class GetIntellisenseRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

            return validationResultList;
        }
    }
    public class GetIntellisenseResponseModel
    {
        public Dictionary<string, object> Intellisense { get; set; }
    }
    public class VersionLanguageSchema : BaseModel
    {
        public string LanguageId { get; set; }
        public int Version { get; set; }
        public string UserId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.LanguageId))
                validationResultList.Add(new ValidationResult("LanguageId can not be Empty"));
            if (string.IsNullOrEmpty(this.UserId))
                validationResultList.Add(new ValidationResult("UserId can not be Empty"));
            if (this.Version <= 0)
                validationResultList.Add(new ValidationResult("Version is not valid"));

            return validationResultList;
        }
    }
    public class LanguageStorageDetailsResponse
    {
        public List<LanguageStorage> Schemas { get; set; }
    }
    public class LanguageStorage
    {
        public string WebsiteId { get; set; }
        public string UserId { get; set; }
        //Size in bytes
        public long Size { get; set; }
        public string SchemaId { get; set; }
        public string SchemaName { get; set; }
    }

    #region GetWebsiteDataBySinglePropertyPath
    public class GetWebsiteDataByPropertyPath : BaseModel
    {
        public string WebsiteId { get; set; }
        public string SchemaId { get; set; }
        public List<PropertyPathSegment> PropertySegments { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("Website id can not be Empty"));
            if (string.IsNullOrEmpty(this.SchemaId))
                validationResultList.Add(new ValidationResult("SchemaId can not be Empty"));
            if (PropertySegments == null || PropertySegments.Count == 0)
                validationResultList.Add(new ValidationResult("Property segments can not be empty"));

            if (PropertySegments != null && PropertySegments.Count > 0)
            {
                int index = 0;
                foreach (var ps in PropertySegments)
                {
                    if (string.IsNullOrEmpty(ps.PropertyName))
                        validationResultList.Add(new ValidationResult($"PropertySegments[{index}] : Property name can not be empty"));
                    if (string.IsNullOrEmpty(ps.PropertyDataType))
                        validationResultList.Add(new ValidationResult($"PropertySegments[{index}] : Property datatype can not be empty"));
                    index++;
                }
            }

            return validationResultList;
        }

    }
    public class GetWebsiteDataByPropertyPathV2 : BaseModel
    {
        public string WebsiteId { get; set; }
        public string SchemaId { get; set; }
        public List<List<PropertyPathSegment>> BulkPropertySegments { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("Website id can not be Empty"));
            if (string.IsNullOrEmpty(this.SchemaId))
                validationResultList.Add(new ValidationResult("SchemaId can not be Empty"));
            if (BulkPropertySegments == null || BulkPropertySegments.Count == 0)
                validationResultList.Add(new ValidationResult("Property segments can not be empty"));

            if (BulkPropertySegments != null && BulkPropertySegments.Count > 0)
            {
                foreach (var segment in BulkPropertySegments)
                {
                    int index = 0;
                    foreach (var ps in segment)
                    {
                        if (string.IsNullOrEmpty(ps.PropertyName))
                            validationResultList.Add(new ValidationResult($"PropertySegments[{index}] : Property name can not be empty"));
                        if (string.IsNullOrEmpty(ps.PropertyDataType))
                            validationResultList.Add(new ValidationResult($"PropertySegments[{index}] : Property datatype can not be empty"));
                        index++;
                    }

                }
            }

            return validationResultList;
        }

    }


    public class GetWebsiteDataByPropertyResponseModel
    {
        public object Data { get; set; }
    }
    #endregion
    public class phoneNumber
    {
        public string countryCode { get; set; }
        public string contactNumber { get; set; }
        public string callTrackerNumber { get; set; }
    }
    public class DisableVMNRequestModel
    {
        public List<string> customerIds { get; set; }
        public List<string> primaryNumbers { get; set; }
        public List<string> virtualNumbers { get; set; }
    }

    public class CallTrackerDataByTypeResponseModel
    {
        public string contactnumber { get; set; }
        public string activate { get; set; }
        public string countrycode { get; set; }
        public string _kid { get; set; }
        public string createdon { get; set; }
        public string updatedon { get; set; }
        public string isarchived { get; set; }
        public string websiteid { get; set; }
        public string _parentClassId { get; set; }
        public string _parentClassName { get; set; }
        public string _propertyName { get; set; }
        public string calltrackernumber { get; set; }
    }

    public class PhoneNumberDatatTypeDetailsResponse
    {
        public string _kid { get; set; }
        public string _parentClassId { get; set; }
        public string _parentClassName { get; set; }
        public string _propertyName { get; set; }
    }

    public class GetLanguageByPropertyGroupResponse
    {
        public string EntityName { get; set; }
        public string EntityDescription { get; set; }
        public IList<KClassGroupProperties> Classes { get; set; }
    }
    public class KClassGroupProperties : KClass
    {
        private static string[] datatypes = new string[] { "str", "number", "boolean", "date" };
        private static string[] propertiesToIgnore = new string[] { "_kid", "userid", "schemaid", "websiteid", "createdon", "updatedon", "isarchived" };
        public Dictionary<string, int> PropertyGroups
        {
            get
            {
                var propertyGroups = new Dictionary<string, int>();
                propertyGroups.Add("DataTypeProperties", DataTypeProperties);
                propertyGroups.Add("ImageProperties", ImageProperties);
                propertyGroups.Add("KStringProperties", KStringProperties);
                propertyGroups.Add("DataTypeArrayProperties", DataTypeArrayProperties);
                propertyGroups.Add("CustomArrayProperties", CustomArrayProperties);
                propertyGroups.Add("PhoneNumberProperties", PhoneNumberProperties);
                propertyGroups.Add("CustomObjectProperties", CustomObjectProperties);
                return propertyGroups;
            }
        }
        private List<KProperty> GetProperties(IList<KProperty> kProperties)
        {
            return kProperties != null ? kProperties.Where(x => x.Name != null && !propertiesToIgnore.Contains(x.Name.ToLower())).ToList() : new List<KProperty>();
        }

        private int DataTypeProperties
        {
            get
            {
                return GetProperties(this.PropertyList).Count(x => x.Type == PropertyType.boolean || x.Type == PropertyType.str || x.Type == PropertyType.number || x.Type == PropertyType.date);
            }
        }
        private int ImageProperties
        {
            get
            {
                return GetProperties(this.PropertyList).Count(x => x.Type == PropertyType.obj && x.DataType != null && x.DataType.Name != null && x.DataType.Name.ToLower() == "image");
            }
        }
        private int KStringProperties
        {
            get
            {
                return GetProperties(this.PropertyList).Count(x => x.Type == PropertyType.kstring);
            }
        }
        private int DataTypeArrayProperties
        {
            get
            {
                return GetProperties(this.PropertyList).Count(x => x.Type == PropertyType.array && x.DataType != null && x.DataType.Name != null && datatypes.Contains(x.DataType.Name.ToLower()));
            }
        }
        private int CustomArrayProperties
        {
            get
            {
                return GetProperties(this.PropertyList).Count(x => x.Type == PropertyType.array && x.DataType != null && x.DataType.Name != null && !datatypes.Contains(x.DataType.Name.ToLower()));
            }
        }
        private int PhoneNumberProperties
        {
            get
            {
                return GetProperties(this.PropertyList).Count(x => x.Type == PropertyType.phonenumber);
            }
        }
        private int CustomObjectProperties
        {
            get
            {
                return GetProperties(this.PropertyList).Count(x => x.Type == PropertyType.obj && x.DataType != null && x.DataType.Name != null && x.DataType.Name.ToLower() != "image");
            }
        }
    }
    public class InsertUpdateResult
    {
        public string Kid { get; set; }
        public string ParentClassId { get; set; }
        public string ParentClassName { get; set; }
        public string PropertyName { get; set; }
        public InsertUpdateStatus Status { get; set; }
        public List<InsertUpdateResult> NestedObjects { get; set; }
    }
    public enum InsertUpdateStatus
    {
        Inserted = 0,
        Updated = 1,
        Error = -1
    }

}