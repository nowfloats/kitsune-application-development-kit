using Kitsune.Language.Models;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using static Kitsune.Language.Models.KEntity;

namespace Kitsune.API2.Utils
{
    public static class JsonHelper
    {
        public static JToken DeserializeWithLowerCasePropertyNames(string json)
        {
            using (TextReader textReader = new StringReader(json))
            using (JsonReader jsonReader = new LowerCasePropertyNameJsonReader(textReader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<JToken>(jsonReader);
            }
        }
    }

    public class LowerCasePropertyNameJsonReader : JsonTextReader
    {
        public LowerCasePropertyNameJsonReader(TextReader textReader)
            : base(textReader)
        {
        }

        public override object Value
        {
            get
            {
                if (TokenType == JsonToken.PropertyName)
                    return ((string)base.Value).ToLower();

                return base.Value;
            }
        }
    }
    public class JsonParserHelper
    {
        internal static KEntity ParseToKEntity(dynamic jsonObject)
        {
            try
            {
                if(jsonObject != null)
                {
                    var completejson = jsonObject;
                    var finalJson = new KEntity();
                    var classes = jsonObject["Classes"];
                    var classesList = new List<KClass>();
                    foreach (var tempClass in classes)
                    {
                        var parentClass = tempClass;

                        if(tempClass["ClassType"].Value.ToString().Equals("1"))
                        {
                            finalJson.EntityName = tempClass["Name"].Value;
                        }
                       
                      
                        var parentKClass = childClasses(tempClass, classesList);
                        classesList.Add(parentKClass);
                        
                    }

                    finalJson.Classes = classesList;

                    return finalJson;
                }
            }
            catch(Exception ex)
            {

            }

            return null;
        }

        internal static KClass childClasses(dynamic jsonObject, List<KClass> subclasses)
        {
            try
            {
                if(jsonObject != null)
                {
                    var propertyList = jsonObject["PropertyList"];
                    var tempClass = new KClass()
                    {
                        ClassType = (KClassType)jsonObject["ClassType"].Value,
                        Name = jsonObject["Name"].Value,
                        Description = jsonObject["Name"].Value,
                        IsCustom = true,
                    };
                    var tempPropertyList = new List<KProperty>();
                    foreach (var tempArray in propertyList)
                    {
                        var property = new KProperty();
                        var typeExists = DynamicFieldExists(tempArray, "Type");
                        if(typeExists)
                        {
                            var propType = tempArray["Type"];
                            property.Type = (PropertyType)tempArray["Type"].Value;
                            property.Name = tempArray["Name"].Value;
                            if(property.Type == PropertyType.array)
                            {
                                var propertyName = tempArray["DataType"];
                                property.DataType = new DataType(propertyName["Name"].Value.ToString());
                                
                            }
                            tempPropertyList.Add(property);
                        }
                        else
                        {
                            property.Name = tempArray["Name"].Value;
                            property.Type = PropertyType.obj;
                            property.DataType = new DataType(tempArray["Name"].Value.ToString());
                            tempPropertyList.Add(property);
                            DeeperClasses(tempArray, subclasses);
                        }
                        

                        var tempObject = tempArray;
                    }

                    tempClass.PropertyList = tempPropertyList;

                    return tempClass;
                }
            }
            catch(Exception ex)
            {

            }

            return null;
        }

        internal static void DeeperClasses(dynamic jsonObject, List<KClass> subclasses)
        {
            try
            {
                if (jsonObject != null)
                {
                    var propertyList = jsonObject["PropertyList"];
                    var tempClass = new KClass()
                    {
                        ClassType = (KClassType)jsonObject["ClassType"].Value,
                        Name = jsonObject["Name"].Value,
                        Description = jsonObject["Name"].Value,
                        IsCustom = true,
                    };
                    var tempPropertyList = new List<KProperty>();
                    foreach (var tempArray in propertyList)
                    {
                        var property = new KProperty();
                        var typeExists = DynamicFieldExists(tempArray, "Type");
                        if (typeExists)
                        {
                            var propType = tempArray["Type"];
                            property.Type = (PropertyType)tempArray["Type"].Value;
                            property.Name = tempArray["Name"].Value;
                            if(property.Type == PropertyType.array)
                            {
                                var propertyName = tempArray["DataType"];
                                property.DataType = new DataType(propertyName["Name"].Value.ToString());
                            }
                            tempPropertyList.Add(property);
                        }
                        else
                        {
                            property.Name = tempArray["Name"].Value;
                            property.Type = PropertyType.obj;
                            property.DataType = new DataType(tempArray["Name"].Value.ToString());
                            tempPropertyList.Add(property);
                            DeeperClasses(tempArray, subclasses);
                        }


                        var tempObject = tempArray;
                    }

                    tempClass.PropertyList = tempPropertyList;

                    subclasses.Add(tempClass);
                }

            }
            catch (Exception ex)
            {

            }

        }

        private static bool DynamicFieldExists(dynamic obj, string field)
        {
            bool retval = false;
            try
            {
                dynamic finalObject = null;
                finalObject = obj;
                var temp = finalObject[field];
                if(temp != null)
                {
                    retval = true;
                }
               
            }
            catch (RuntimeBinderException) { }
            catch (Exception ex)
            {

            }
            return retval;
        }

        private static bool DynamicFieldExist(dynamic obj, int field)
        {
            bool retval = false;
            try
            {
                dynamic finalObject = null;
                finalObject = obj;
                var temp = finalObject[field];
                retval = true;
            }
            catch (RuntimeBinderException) { }
            catch (Exception ex)
            {

            }
            return retval;
        }
    }
}