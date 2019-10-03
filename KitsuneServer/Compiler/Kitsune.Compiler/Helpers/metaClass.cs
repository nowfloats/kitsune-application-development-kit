using Kitsune.Compiler.Model;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Compiler.Core.Helpers
{
    public class MetaClassBuilder
    {
        public static MetaPropertyType metaPropertyList = new MetaPropertyType();
        public static List<ObjectReference> metaClass = new List<ObjectReference>();
        //public bool IsObjectEqual(object from, object to)
        //{
        //    var haveSameData = false;
        //    foreach (PropertyInfo prop in from.GetType().GetProperties())
        //    {
        //        haveSameData = object.Equals(prop.GetValue(from, null), prop.GetValue(to, null));
        //        if (!haveSameData)
        //            break;
        //    }
        //    return haveSameData;
        //}
        public bool IsObjectEqual(Filter from, Filter to)
        {
            if (object.Equals(from.startIndex, to.startIndex) && object.Equals(from.endIndex, to.endIndex))
                return true;
            return false;
        }

        public bool CreatePartMetaClass(string[] expr, int ind, List<ObjectReference> metaClass)
        {
            if (expr.Length > ind)
            {
                var Property = expr[ind];
                var propertyTypes = metaPropertyList.Type.ElementAtOrDefault(ind);
                // checking for a array property
                if (propertyTypes.Type == metaPropertyType.array)
                {
                    var split = Property.Split(new char[] { '[', ':', ']' }, StringSplitOptions.RemoveEmptyEntries);

                    //TODO: remove default sort query after introduciton of sort option in language
                    var filter = new Filter {
                        sort = new List<SortQuery> {
                            new SortQuery{
                                property = "UpdatedOn",
                                direction = -1
                            }
                        }
                    };
                    int indexResult = new int();
                    bool parseResult = false;
                    switch (split.Length)
                    {
                        case 1:
                            filter.startIndex = 0;
                            filter.endIndex = -1;
                            break;
                        case 2:
                            if (split[1] == CompilerConstants.KObjectReference.Trim('[', ']'))
                            {
                                filter.startIndex = -1;
                                filter.endIndex = -1;
                            }
                            else
                            {
                                parseResult = Int32.TryParse(split[1], out indexResult);
                                if (!parseResult)
                                    return true;
                                    //throw new Exception(string.Format("Cannot convert array indexes to integer for: {0}", expr.Aggregate((current, next) => current + "." + next)));
                                filter.startIndex = -1;
                                filter.endIndex = indexResult;
                            }
                            break;
                        case 3:
                            parseResult = Int32.TryParse(split[1], out indexResult);
                            if (!parseResult)
                                throw new Exception(string.Format("Cannot convert array indexes to integer for: {0}", expr.Aggregate((current, next) => current + "." + next)));
                            filter.startIndex = indexResult;
                            parseResult = Int32.TryParse(split[2], out indexResult);
                            if (!parseResult)
                                throw new Exception(string.Format("Cannot convert array indexes to integer for: {0}", expr.Aggregate((current, next) => current + "." + next)));
                            filter.endIndex = indexResult;
                            break;
                        default:
                            throw new Exception("Unknown splitting in handling of array type objects");
                    }
                    var propertyExist = metaClass.FirstOrDefault(x => x.name == split[0]);
                    if (propertyExist == null)
                    {
                        metaClass.Add(new ObjectReference
                        {
                            name = split[0],
                            type = propertyTypes.Type,
                            dataType = propertyTypes.DataType,
                            arrayRanges = new List<ArrayRange> { new ArrayRange {
                                filter = filter,
                                properties = new List<ObjectReference>()
                            }}
                        });
                        var newResult = CreatePartMetaClass(expr, ++ind, metaClass[metaClass.Count - 1].arrayRanges[0].properties);
                        return newResult;
                    }
                    var rangeExist = propertyExist.arrayRanges.FirstOrDefault(x => IsObjectEqual(x.filter, filter));
                    if (rangeExist == null)
                    {
                        propertyExist.arrayRanges.Add(new ArrayRange
                        {
                            filter = filter,
                            properties = new List<ObjectReference>()
                        });
                        var newResult = CreatePartMetaClass(expr, ++ind, propertyExist.arrayRanges[propertyExist.arrayRanges.Count - 1].properties);
                        return newResult;
                    }
                    var result = CreatePartMetaClass(expr, ++ind, rangeExist.properties);
                    return result;
                }
                else
                {
                    var propertyExist = metaClass.FirstOrDefault(x => x.name == Property);
                    if (propertyExist == null)
                    {
                        metaClass.Add(new ObjectReference
                        {
                            name = Property,
                            dataType = propertyTypes.DataType,
                            type = propertyTypes.Type,
                            properties = new List<ObjectReference>()
                        });
                        var newResult = CreatePartMetaClass(expr, ++ind, metaClass[metaClass.Count - 1].properties);
                        return newResult;
                    }
                    var result = CreatePartMetaClass(expr, ++ind, propertyExist.properties);
                    return result;
                }
            }
            return true;
        }
        
        public List<ObjectReference> BuildMetaClass(KEntity entity, List<MultiplePositionProperty> PropertyNames, string SourcePath)
        {
            try
            {
                metaClass = new List<ObjectReference>();
                metaPropertyList = new MetaPropertyType();
                var baseClasses = entity.Classes.Where(x => x.ClassType == KClassType.BaseClass);
                //var userDefinedClasses = entity.Classes.Where(x => x.ClassType == KClassType.UserDefinedClass);
                //var defaultClassses = entity.Classes.Where(x => x.ClassType == KClassType.DefaultClass);
                //var dataTypeClassses = entity.Classes.Where(x => x.ClassType == KClassType.DefaultClass);
                var prop = PropertyNames.Select(x => x.Property.ToLower().Replace("-1", "")).ToList(); //changing all porperties to lower case and removing -1 thats been signifying whole array to fetch
                var jsonObject = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)jsonObject;

                foreach (var property in prop)
                {

                    var objectPathArray = property.ToLower().Split('.');
                    if (!metaClass.Any(x => x.name == objectPathArray[0]))
                    {
                        metaClass.Add(new ObjectReference
                        {
                            name = objectPathArray[0],
                            type = metaPropertyType.obj,
                            properties = new List<ObjectReference>()
                        });
                    }
                    if (objectPathArray.Length > 1)
                    {
                        var ind = 0;
                        var basePropertyName = objectPathArray[ind];
                        var baseClass = baseClasses.FirstOrDefault(x => x.Name?.ToLower() == basePropertyName);

                        if (baseClass != null)
                        {
                            metaPropertyList = new MetaPropertyType
                            {
                                Property = property,
                                Type = new List<MetaPartPropertyType>(){new MetaPartPropertyType {
                                    Type = metaPropertyType.obj
                                }}
                            };
                            var objectPathArrayCopy = new string[objectPathArray.Length];
                            objectPathArray.CopyTo(objectPathArrayCopy, 0);
                            FindDataType(objectPathArrayCopy, 1, basePropertyName, PropertyType.obj, entity);
                            var result = CreatePartMetaClass(objectPathArray, ++ind, metaClass.FirstOrDefault(x => x.name == objectPathArray[0]).properties);
                        }

                    }

                }
                return metaClass;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to Generate meta Class for {0}: {1}", SourcePath,  ex.Message));
            }
        }

        public IEnumerable<CompilerError> FindDataType(string[] expr, int ind, string className, PropertyType type, KEntity entity)
        {

            if (expr.Length > ind)
            {
                var kDataClass = entity.Classes.Where(x => x.ClassType == KClassType.DataTypeClass);
                var len = expr[ind].Length;
                expr[ind] = Kitsune.Helper.Constants.PropertyArrayExpression.Replace(expr[ind], "");
                var kClass = entity.Classes.FirstOrDefault(x => x.Name?.ToLower() == className);
                if (kClass == null)
                    return new List<CompilerError> { new CompilerError { Message = "'" + className + "' Class does not exist in the schema" } };

                var kProperty = kClass.PropertyList.FirstOrDefault(x => x.Name?.ToLower() == expr[ind]);
                if (kProperty != null)
                {

                    var dataType = (kProperty.Type != PropertyType.array ? kProperty.DataType.Name : kProperty.DataType.Name.Trim('[', ']'))?.ToLower();
                    if (kProperty.Type == PropertyType.function)
                    {
                        if (!Kitsune.Helper.Constants.FunctionRegularExpression.IsMatch(expr[ind]))
                            return new List<CompilerError> { new CompilerError { Message = expr[ind] + " is function but used like property" } };

                    }
                    else if (kProperty.Type == PropertyType.obj && "dynamic" == kProperty.DataType?.Name?.ToLower())
                    {
                        metaPropertyList.Type.Add(new MetaPartPropertyType
                        {
                            DataType = kProperty.DataType.Name,
                            Type = PropertyToMetaProperty(kProperty.Type)
                        });
                        ind++;
                        while (expr.Length > ind)
                        {
                            metaPropertyList.Type.Add(new MetaPartPropertyType
                            {
                                DataType = kProperty.DataType.Name,
                                Type = PropertyToMetaProperty(kProperty.Type)
                            });
                            ind++;
                        }
                        return new List<CompilerError>();
                    }


                    var newClassName = ((kProperty.Type == PropertyType.array && len == expr[ind].Length) ? "array" : dataType.ToLower());
                    metaPropertyList.Type.Add(new MetaPartPropertyType
                    {
                        DataType = kProperty.DataType.Name,
                        Type = PropertyToMetaProperty(kProperty.Type)
                    });
                    return FindDataType(expr, ++ind, newClassName, kProperty.Type, entity);

                }
                else
                {
                    return new List<CompilerError> { new CompilerError { Message = "Property " + expr[ind] + " does not exist in the class " + className } };
                }

            }
            return new List<CompilerError>();
        }

        public metaPropertyType PropertyToMetaProperty(PropertyType input)
        {
            switch ((int)input)
            {
                case 0:
                    return metaPropertyType.str;
                case 1:
                    return metaPropertyType.array;
                case 2:
                    return metaPropertyType.number;
                case 3:
                    return metaPropertyType.boolean;
                case 4:
                    return metaPropertyType.date;
                case 5:
                    return metaPropertyType.obj;
                case 6:
                    return metaPropertyType.function;
                case 7:
                    return metaPropertyType.kstring;
                case 8:
                    return metaPropertyType.phonenumber;
                default:
                    return metaPropertyType.others;
            }
        }

    }
}
