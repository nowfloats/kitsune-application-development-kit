using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Language.Models
{
    /// <summary>
    ///   Properties for the KClasses 
    /// </summary>
    public class KProperty : ICloneable
    {
        public string Name { get; set; }
        public Dictionary<string, string> Schemas { get; set; }
        public string Description { get; set; } //verb
        public string GroupName { get; set; } //verb
        public bool IsRequired { get; set; }

        public PropertyType Type { get; set; }
        public DataType DataType { get; set; }
        public bool EnableSearch { get; set; }
        public ulong PropertyCode { get; set; }
        public KProperty Clone() { return (KProperty)this.MemberwiseClone(); }
        object ICloneable.Clone() { return Clone(); }
        public Dictionary<string, object> _AdvanceProperties { get; set; }

    }
    public class DataType
    {
        public DataType(string name)
        {
            this.Name = name;
        }
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public enum PropertyType
    {
        str = 0,
        array = 1,
        number = 2,
        boolean = 3,
        date = 4,
        obj = 5,
        function = 6,
        kstring = 7,
        phonenumber = 8
    }
}
