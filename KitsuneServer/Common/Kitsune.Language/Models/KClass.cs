using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Language.Models
{
    
    /// <summary>s
    /// KClass is a class in the kitsune language
    /// KClass can contain the KProperties same like object properties
    /// Name : Class Name
    /// IsCustom : use to customize the language with additional properties
    /// KProperties : Properties of widget
    /// </summary>
    public class KClass  
    {
        public string Name { get; set; }
        public string GroupName { get; set; }//verb
        public Dictionary<string, string> Schemas { get; set; }
        public string Description { get; set; } //verb
        //IsCustom is not required but used for K-Script (verify and remove it)
        public bool IsCustom { get; set; }
        public KClassType ClassType { get; set; }
        public IList<KProperty> PropertyList { get;  set; }
        public KReferenceType ReferenceType { get; set; }

        public void DefineProperty(KProperty property)
        {
            if (PropertyList != null)
                PropertyList.Add(property);
            else
                PropertyList = new List<KProperty> { property };
        }
        public void DefineProperties(IEnumerable<KProperty> properties)
        {
            if (PropertyList != null)
                foreach (var property in properties)
                    PropertyList.Add(property);
            else
                PropertyList = properties.ToList();
        }
    }

    public enum KClassType
    {
        UserDefinedClass, //All the language specific classes (Update, Offer)
        BaseClass, //Base Classes  (Business)
        DefaultClass, //Default predefined classes for the language (Image, Location, Link) (Language/System defined)
        DataTypeClass //Datatype supported in the language (STR, NUMBER, BOOLEAN..)
    }

    public enum KReferenceType
    {
        Value, //Class has it's own value and can be referenced by other classes.
        Reference, //Class can reference other value type classes.
        Hybrid //Class can have it's own value or be a reference of other value type classes.
    }
}
