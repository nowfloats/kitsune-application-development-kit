using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace KitsuneAdminDashboard.Web.Models
{
    public class Schema
    {
        public List<Fields> fields { get; set; }
    }

    public class Fields
    {
        public string type { get; set; }
        public string inputType { get; set; }
        public string label { get; set; }
        public string model { get; set; }
        public string placeholder { get; set; }
        public bool featured { get; set; }
        public bool required { get; set; }
        public bool disabled { get; set; }
        public bool multiSelect{ get; set; }
}


    public class Entity
    {
        public String EntityName { get; set; }
        public String EntityDescription { get; set; }
        public int Type { get; set; }
        public Classes[] Classes { get; set; }
    }

    public class Classes
    {
        public string Name { get; set; }
        public object Schemas { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public bool IsCustom { get; set; }
        public int ClassType { get; set; }
        public Propertylist[] PropertyList { get; set; }
    }

    public class Propertylist
    {
        public string Name { get; set; }
        public object Schemas { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public int Type { get; set; }
        public Datatype DataType { get; set; }
        public bool EnableSearch { get; set; }
        public int PropertyCode { get; set; }
    }

    public class Datatype
    {
        public object Id { get; set; }
        public string Name { get; set; }
    }

    public static class DataTypeInputMap
    {
        public static Dictionary<string, string> dataTypeInput = new Dictionary<string, string>
        {
            {"STR","text" },
            {"[STR]", "text" },
            {"BOOLEAN", "checkbox" }
        };
    }

    public class ImageUpload
    {
        public string WebactionName { get; set; }
        public IFormFile File { get; set; }
        public string AuthId { get; set; }
        public string FileName { get; set; }
    }

}
