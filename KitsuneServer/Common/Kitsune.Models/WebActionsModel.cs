using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class WebActionLink
    {
        public string url { get; set; }
        public string description { get; set; }
    }
    public class WebActionMetaData : MongoEntity
    {
        public string WebsiteId { get; set; }
        public string ActionId { get; set; }
        public string ActionDataId { get; set; }
        public string UserId { get; set; }
        public string ActionName { get; set; }
        public string IPAddress { get; set; }
        public bool IsArchived { get; set; }
        public Dictionary<string, string> RequestHeader { get; set; }
    }
    public class WebAction : MongoEntity
    {
        public int ActionId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ProjectId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsArchived { get; set; }
        public List<WebActionProperty> Properties { get; set; }
    }
    public class WebActionProperty
    {
        public string DisplayName { get; set; }
        public string PropertyName { get; set; }
        public WebActionPropertyType PropertyType { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRegex { get; set; }
    }
    public enum WebActionPropertyType
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
    public class WebsiteWebAction : MongoEntity
    {
        public string WebsiteId { get; set; }
        public string ProjectId { get; set; }
        public string UserId { get; set; }
        public string WebActionId { get; set; }
        public bool IsArchived { get; set; }
    }
}
