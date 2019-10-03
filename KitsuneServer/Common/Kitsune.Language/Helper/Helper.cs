using Kitsune.Language.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Kitsune.Language.Models.KEntity;

namespace Kitsune.Language.Helper
{
    public static class Helper
    {
        public static string GetDescription(this LanguageAttributes value)
        {
            FieldInfo field = typeof(LanguageAttributes).GetField(value.ToString());
            return field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .Cast<DescriptionAttribute>()
                        .Select(x => x.Description)
                        .FirstOrDefault();
        }
        public static object GetClassFromJson(KEntity entity)
        {
            try
            {
                var allClasses = entity.GetAllAvailableClasses();
                var finalList = new List<object>();
                var masterClassList = new List<KClass>();
                var objectClasses = new List<KClass>();
                if (allClasses.Any())
                {
                    foreach (var widgetClass in allClasses)
                    {
                        if (widgetClass.ClassType == KClassType.BaseClass)
                        {
                            masterClassList.Add(widgetClass);
                        }
                        else if (widgetClass.ClassType == KClassType.UserDefinedClass)
                        {
                            var classWithObjReference = widgetClass.PropertyList.Where(s => s.Type == PropertyType.obj).ToList();
                            if (!classWithObjReference.Any())
                            {
                                var userClass = new ClassBuilder(widgetClass.Name.ToLower());
                                var userDefinedClass = GetClassFromProperties(widgetClass.PropertyList.ToList(), userClass, finalList);
                                if (userDefinedClass != null)
                                {
                                    finalList.Add(userDefinedClass);
                                }
                            }
                            else
                            {
                                objectClasses.Add(widgetClass);
                            }

                        }
                        else if (widgetClass.ClassType == KClassType.DataTypeClass)
                        {
                            var userClass = new ClassBuilder(widgetClass.Name.ToLower());
                            var userDefinedClass = GetClassFromProperties(widgetClass.PropertyList.ToList(), userClass, finalList);
                            if (userDefinedClass != null)
                            {
                                finalList.Add(userDefinedClass);
                            }
                        }
                    }

                    if (objectClasses.Any())
                    {
                        foreach (var objectClass in objectClasses)
                        {
                            var userClass = new ClassBuilder(objectClass.Name.ToLower());
                            var userDefinedClass = GetClassFromProperties(objectClass.PropertyList.ToList(), userClass, finalList);
                            if (userDefinedClass != null)
                            {
                                finalList.Add(userDefinedClass);
                            }
                        }
                    }


                    if (masterClassList != null && masterClassList.Any())
                    {
                        foreach (var masterClass in masterClassList)
                        {
                            var userClass = new ClassBuilder(masterClass.Name.ToLower());
                            var finalMasterClass = GetClassFromProperties(masterClass.PropertyList.ToList(), userClass, finalList);
                            if (finalMasterClass != null)
                            {
                                return finalMasterClass;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }
        internal static object GetClassFromProperties(List<KProperty> propertyList, ClassBuilder parentClass, List<object> existingClasses)
        {
            try
            {
                if (propertyList != null && propertyList.Count() > 0)
                {
                    var propertyNames = new List<string>();
                    var propertyTypes = new List<Type>();
                    foreach (var prop in propertyList)
                    {

                        if (prop.Type == PropertyType.str)
                        {
                            propertyNames.Add(prop.Name.ToLower());
                            propertyTypes.Add(typeof(string));
                        }
                        else if (prop.Type == PropertyType.date)
                        {
                            propertyNames.Add(prop.Name.ToLower());
                            propertyTypes.Add(typeof(DateTime));
                        }
                        else if (prop.Type == PropertyType.boolean)
                        {
                            propertyNames.Add(prop.Name.ToLower());
                            propertyTypes.Add(typeof(bool));
                        }
                        else if (prop.Type == PropertyType.number)
                        {
                            propertyNames.Add(prop.Name.ToLower());
                            propertyTypes.Add(typeof(int));
                        }
                        else if (prop.Type == PropertyType.array)
                        {
                            if (existingClasses != null && existingClasses.Any())
                            {
                                var objectClass = existingClasses.Where(s => s.GetType().Name.Equals(prop.DataType.Name.ToLower().Replace("[", "").Replace("]", ""))).FirstOrDefault();
                                if (objectClass != null)
                                {
                                    propertyNames.Add(prop.Name.ToLower());
                                    propertyTypes.Add(objectClass.GetType());
                                }

                            }

                        }
                        else if (prop.Type == PropertyType.obj)
                        {
                            if (existingClasses != null && existingClasses.Any())
                            {
                                var objectClass = existingClasses.Where(s => s.GetType().Name.Equals(prop.DataType.Name.ToLower())).FirstOrDefault();
                                if (objectClass != null)
                                {
                                    propertyNames.Add(prop.Name.ToLower());
                                    propertyTypes.Add(objectClass.GetType());
                                }

                            }

                        }
                    }
                    var myclass = parentClass.CreateObject(propertyNames.ToArray(), propertyTypes.ToArray());
                    return myclass;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public static dynamic ToDynamic<T>(this T obj)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var propertyExpression = Expression.Property(Expression.Constant(obj), propertyInfo);
                var currentValue = Expression.Lambda<Func<string>>(propertyExpression).Compile().Invoke();
                expando.Add(propertyInfo.Name.ToLower(), currentValue);
            }
            return expando as ExpandoObject;
        }
    }
}
