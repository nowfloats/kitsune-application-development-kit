using Kitsune.Language.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kitsune.Language.Models.KEntity;
namespace Kitsune.Language.Helper
{
    public static class LanguageDefaults
    {
        public static IList<KView> GetDefaultViews()
        {
            List<KView> Views = new List<KView>();
            return Views;
        }

        public static IList<KClass> GetDefaultClassList()
        {

            #region Initialize Classes
            var allClasses = new List<KClass>
            {
                #region BusinessBaseClass
                new KClass
                             {
                                Name = "BUSINESS",
                                ClassType = KClassType.BaseClass,
                                PropertyList = new List<KProperty> {
                                new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "NAME",
                                        Description = "Business name",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },
                                new KProperty
                                    {
                                        Name = "DESCRIPTION",
                                        Description = "Business description",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")

                                    },
                                 new KProperty
                                    {
                                        Name = "FBPAGENAME",
                                        Description = "Facebook page name",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")

                                    },
                                  new KProperty
                                    {
                                        Name = "TAG",
                                        Description = "Floating point tag",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")

                                    },
                                   new KProperty
                                    {
                                        Name = "EMAIL",
                                        Description = "Email",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")

                                    },
                                    new KProperty
                                    {
                                        Name = "Id",
                                        Description = "Business Id",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")

                                    },
                                    new KProperty
                                    {
                                        Name = "TOTALVISITS",
                                        Description = "Total visits of business website",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")

                                    },
                                    new KProperty
                                    {
                                        Name = "TOTALSUBSCRIBERS",
                                        Description = "Total subscribers of business website",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")

                                    },
                                    new KProperty
                                    {
                                        Name = "ISPAID",
                                        Description = "Is customer paid?",
                                        Type = PropertyType.boolean,
                                        DataType = new Models.DataType("BOOLEAN")

                                    },
                                       new KProperty
                                    {
                                        Name = "ISARCHIVED",
                                        Description = "Is customer archived?",
                                        Type = PropertyType.boolean,
                                        DataType = new Models.DataType("BOOLEAN")

                                    },
                                   new KProperty
                                    {
                                        Name = "ADDRESS",
                                        Description = "Business address",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("ADDRESS")
                                    },
                                    new KProperty
                                    {
                                        Name = "LOCATION",
                                        Description = "Location details (Lattitude, Longitude)",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("LOCATION")
                                    },
                                    new KProperty
                                    {
                                        Name = "EXTERNALLINKS",
                                        Description = "External busiess links",
                                        Type = PropertyType.array,
                                        DataType = new DataType("LINK")
                                    },
                                    new KProperty
                                    {
                                        Name = "ROOTALIASURL",
                                        Description = "Base url of the website",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("LINK")
                                    },
                                    new KProperty
                                    {
                                        Name = "SITEMAP",
                                        Description = "Site map url",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("LINK")
                                    },
                                      new KProperty
                                    {
                                        Name = "BIZHOURS",
                                        Description = "Business timings",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("BIZHOURS")
                                    },
                                       new KProperty
                                    {
                                        Name = "GALLERYIMAGES",
                                        Description = "Gallery images",
                                        Type = PropertyType.array,
                                        DataType = new DataType("IMAGE")
                                    },
                                   new KProperty
                                    {
                                        Name = "LOGO",
                                        Description = "Business logo",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("IMAGE")
                                    },
                                   new KProperty
                                    {
                                        Name = "FEATUREDIMAGE",
                                        Description = "Featured image",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("IMAGE")
                                    },
                                   new KProperty
                                    {
                                        Name = "BACKGROUNDIMAGE",
                                        Description = "Background image",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("IMAGE")
                                    },
                                    new KProperty
                                    {
                                        Name = "BACKGROUNDIMAGES",
                                        Description = "Background images list",
                                        Type = PropertyType.array,
                                        DataType = new DataType("IMAGE")
                                    },
                                    new KProperty
                                    {
                                        Name = "FAVICON",
                                        Description = "Fav icon",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("IMAGE")
                                    },
                                     new KProperty
                                    {
                                        Name = "CATEGORIES",
                                        Description = "Business categories",
                                        Type = PropertyType.array,
                                        DataType = new DataType("STR")
                                    },
                                     new KProperty
                                    {
                                        Name = "KEYWORDS",
                                        Description = "Business Keywords",
                                        Type = PropertyType.array,
                                        DataType = new DataType("LINK")
                                    },
                                       new KProperty
                                    {
                                        Name = "FPWEBWIDGETS",
                                        Description = "Floating point web widgets",
                                        Type = PropertyType.array,
                                        DataType = new DataType("STR")
                                    },
                                     new KProperty
                                    {
                                        Name = "EXTERNALIDS",
                                        Description = "External ids for third party integration",
                                        Type = PropertyType.array,
                                        DataType = new DataType("STR")
                                    },
                                    new KProperty
                                    {
                                        Name = "TESTIMONIALS",
                                        Description = "User testimonials",
                                        Type = PropertyType.array,
                                        DataType = new DataType("TESTIMONIAL")
                                    },

                                     new KProperty
                                    {
                                        Name = "UPDATES",
                                        Description = "Updates",
                                        Type = PropertyType.array,
                                        DataType = new DataType("UPDATE")
                                    },
                                      new KProperty
                                    {
                                        Name = "POPULARUPDATES",
                                        Description = "Popular updates",
                                        Type = PropertyType.array,
                                        DataType = new DataType("UPDATE")
                                    },

                                    new KProperty
                                    {
                                        Name = "OFFERS",
                                        Description = "Business offers array",
                                        Type = PropertyType.array,
                                        DataType = new DataType("OFFER")
                                    },
                                     new KProperty
                                    {
                                        Name = "POPULAROFFERS",
                                        Description = "Business popular offers array",
                                        Type = PropertyType.array,
                                        DataType = new DataType("OFFER")
                                    },
                                    new KProperty
                                    {
                                        Name = "PRODUCTS",
                                        Description = "Business products",
                                        Type = PropertyType.array,
                                        DataType = new DataType("PRODUCT")
                                    },
                                    new KProperty
                                    {
                                        Name = "POPULARPRODUCTS",
                                        Description = "Business popular products",
                                        Type = PropertyType.array,
                                        DataType = new DataType("PRODUCT")
                                    },
                                    new KProperty
                                    {
                                        Name = "CONTACTS",
                                        Description = "Business contacts",
                                        Type = PropertyType.array,
                                        DataType = new DataType("CONTACT")
                                    },
                                    new KProperty
                                    {
                                        Name = "CUSTOMPAGES",
                                        Description = "Custom pages",
                                        Type = PropertyType.array,
                                        DataType = new DataType("CUSTOMPAGE")
                                    },
                                     new KProperty
                                    {
                                        Name = "CUSTOMPAGELABEL",
                                        Description = "Custom page label",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR")
                                    },
                                    new KProperty
                                    {
                                        Name = "COUNTRYCODE",
                                        Description = "Country code",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR")
                                    },
                                     new KProperty
                                    {
                                        Name = "GATOKEN",
                                        Description = "Google analytics token",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR")
                                    },
                                      new KProperty
                                    {
                                        Name = "GADOMAIN",
                                        Description = "Google analytics domain",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR")
                                    },
                                       new KProperty
                                    {
                                        Name = "BOOKINGENGINEURL",
                                        Description = "Hotel booking engine url",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR")
                                    },
                                }

                             },
                #endregion
          
                #region SearchBaseClass
                   new KClass
                             {
                                Name = "SEARCH",
                                ClassType = KClassType.BaseClass,
                                PropertyList = new List<KProperty> {
                                new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "RESULT",
                                        Description = "Search result",
                                        Type = PropertyType.array,
                                        DataType = new Models.DataType("SEARCHRESULT")
                                    },
                                 new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "EXTRA",
                                        Description = "Search result extra metadata",
                                        Type = PropertyType.obj,
                                        DataType = new Models.DataType("SEARCHPAGINATION")
                                    },
                                  new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "ORIGINALSEARCHTEXT",
                                        Description = "Original search text",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },
                                 new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "MODIFIDEDSEARCHTEXT",
                                        Description = "Modifided search text",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    }
                                }
                   },
                    new KClass
                             {
                                Name = "SEARCHRESULT",
                                ClassType = KClassType.DefaultClass,
                                PropertyList = new List<KProperty> {
                                new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "ORIGINALSEARCHTEXT",
                                        Description = "Original search text",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },
                                 new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "MODIFIDEDSEARCHTEXT",
                                        Description = "Modifided search text",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },
                                   new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "TITLE",
                                        Description = "Title",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "DESCRIPTION",
                                        Description = "Description",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "TILEIMAGE",
                                        Description = "Tile image",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "ACTUALIMAGE",
                                        Description = "Actual image",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },
                                     new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "ITEMTYPE",
                                        Description = "Item type",
                                        Type = PropertyType.str,
                                        DataType = new Models.DataType("STR")
                                    },
                                      new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "TIMESTAMP",
                                        Description = "Time stamp",
                                        Type = PropertyType.date,
                                        DataType = new Models.DataType("DATE")
                                    },
                                 new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "DATA",
                                        Description = "Search details object",
                                        Type = PropertyType.obj,
                                        DataType = new Models.DataType("DYNAMIC")
                                    }
                                }
                   },
                     new KClass
                             {
                                Name = "SEARCHPAGINATION",
                                ClassType = KClassType.DefaultClass,
                                PropertyList = new List<KProperty> {
                                new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "CURRENTINDEX",
                                        Description = "Current index",
                                        Type = PropertyType.number,
                                        DataType = new Models.DataType("NUMBER")
                                    },
                                 new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "TOTALCOUNT",
                                        Description = "Total count",
                                        Type = PropertyType.number,
                                        DataType = new Models.DataType("NUMBER")
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "PAGESIZE",
                                        Description = "Page size",
                                        Type = PropertyType.number,
                                        DataType = new Models.DataType("NUMBER")
                                    },
                                }
                   },
                #endregion

               
                #region LanguageClass
                new KClass
                             {
                                Name = "ADDRESS",
                                ClassType = KClassType.UserDefinedClass,
                                Schemas = new Dictionary<string, string> { {"itemscope", null }, { "itemtype", "http://schema.org/PostalAddress" }, { "itemprop", "address" }  },
                                PropertyList = new List<KProperty> {
                                new KProperty
                                    {
                                        Name = "COUNTRY",
                                        Description = "Country",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                    },
                                 new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "FULLADDRESS",
                                        Description = "Address",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "streetAddress" }, }

                                    },
                                new KProperty
                                    {
                                        Name = "CITY",
                                        Description = "City",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR"),
                                        Schemas = new Dictionary<string, string> { { "itemprop" , "addressRegion" } }
                                    },
                                new KProperty
                                    {
                                        Name = "PINCODE",
                                        Description = "PIN-CODE",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number
                                    }
                                }
                             },
                             new KClass
                             {
                                Name = "CONTACT",
                                ClassType = KClassType.UserDefinedClass,
                                Description = "Contact itme",
                                PropertyList = new List<KProperty> {
                                new KProperty
                                    {
                                        Name = "CONTACTTYPE",
                                        Description = "Contact type",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR")
                                    },
                                new KProperty
                                    {
                                        Name = "CONTACTNUMBER",
                                        Description = "Phone Number",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR"),
                                        Schemas = new Dictionary<string, string> { { "itemprop", "telephone" } }
                                    },
                                }
                             },
                             new KClass
                             {
                                Name = "CUSTOMPAGE",
                                IsCustom = true,
                                ClassType = KClassType.UserDefinedClass,
                                Description = "Custom page class",
                                PropertyList = new List<KProperty>
                                {
                                      new KProperty
                                    {
                                        Name = "ID",
                                        Description = "Unique id",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                    },
                                       new KProperty
                                    {
                                        Name = "HTML",
                                        Description = "Html string",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                    },
                                    new KProperty
                                    {
                                        Name = "NAME",
                                        Description = "Page Title",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                    },
                                     new KProperty
                                    {
                                        Name = "URL",
                                        Description = "Page Url",
                                        DataType = new DataType("LINK"),
                                        Type = PropertyType.obj,
                                    },
                                     new KProperty
                                    {
                                        Name = "TOTALVIEWS",
                                        Description = "Total Views",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number
                                    },
                                      new KProperty
                                    {
                                        Name = "CREATEDON",
                                        DataType = new DataType("DATETIME"),
                                        Description = "Created on datetime",
                                        Type = PropertyType.date,
                                    },
                                       new KProperty
                                    {
                                        Name = "UPDATEDON",
                                        DataType = new DataType("DATETIME"),
                                        Description = "Last modified on datetim",
                                        Type = PropertyType.date,
                                    },
                                }
                             },
                               new KClass
                               {
                                   Name = "UPDATE",
                                   ClassType = KClassType.UserDefinedClass,
                                   Description = "Update class",
                                   Schemas = new Dictionary<string, string> { {"itemscope", null }, {"itemtype", "http://schema.org/Offer" } },
                                   PropertyList = new List<KProperty>
                                   {
                                        new KProperty
                                    {
                                        Name = "ID",
                                        Description = "Update Id",
                                        DataType = new DataType("STR"),
                                        IsRequired = true,
                                        Type = PropertyType.str
                                    },
                                     new KProperty
                                    {
                                        Name = "INDEX",
                                        Description = "Update index",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number,
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "DESCRIPTION",
                                        Description = "Update description",
                                        DataType = new DataType("STR"),
                                        Type  = PropertyType.str,
                                        Schemas = new Dictionary<string, string> { {"itemprop", "description" } }
                                    },
                                      new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "TITLE",
                                        Description = "Update title",
                                        DataType = new DataType("STR"),
                                        Type  = PropertyType.str,
                                        Schemas = new Dictionary<string, string> { {"itemprop", "title" } }
                                    },
                                       new KProperty
                                    {
                                        Name = "HTMLSTRING",
                                        Description = "Update htmlstring",
                                        DataType = new DataType("STR"),
                                        Type  = PropertyType.str,
                                    },
                                    new KProperty
                                    {
                                        Name = "TOTALVIEWS",
                                        Description = "Update total view count",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number,
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "CREATEDON",
                                        Description = "Update created on timestamp",
                                        DataType = new DataType("DATETIME"),
                                        Type = PropertyType.date,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "validFrom" } }

                                    },
                                    new KProperty
                                    {
                                        Name = "KEYWORDS",
                                        Description = "Update keywords",
                                        DataType = new DataType("[LINK]"),
                                        IsRequired = true,
                                        Type = PropertyType.array,
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "URL",
                                        Description = "Update url",
                                        DataType = new DataType("LINK"),
                                        Type = PropertyType.obj,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "url" } }

                                    },
                                    new KProperty
                                    {
                                        Name = "IMAGE",
                                        Description = "Update image url",
                                        DataType= new DataType("IMAGE"),
                                        Type = PropertyType.obj,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "image" } }
                                    },
                                     new KProperty
                                    {
                                        Name = "TYPE",
                                        Description = "Update type (Popular/Latest)",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                    },
                                   }
                               },
                            new KClass
                             {
                                Name = "OFFER",
                                Schemas = new Dictionary<string, string> { {"itemscope", null }, {"itemtype", "http://schema.org/Offer" } },
                                ClassType = KClassType.UserDefinedClass,
                                Description = "Business offer",
                                PropertyList = new List<KProperty>
                                {
                                        new KProperty
                                    {
                                        Name = "ID",
                                        Description = "Update Id",
                                        DataType = new DataType("STR"),
                                        IsRequired = true,
                                        Type = PropertyType.str
                                    },
                                     new KProperty
                                    {
                                        Name = "INDEX",
                                        Description = "Update index",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number,
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "DESCRIPTION",
                                        Description = "Update description",
                                        DataType = new DataType("STR"),
                                        Type  = PropertyType.str,
                                        Schemas = new Dictionary<string, string> { {"itemprop", "description" } }
                                    },
                                      new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "TITLE",
                                        Description = "Update title",
                                        DataType = new DataType("STR"),
                                        Type  = PropertyType.str,
                                        Schemas = new Dictionary<string, string> { {"itemprop", "title" } }
                                    },
                                    new KProperty
                                    {
                                        Name = "TOTALVIEWS",
                                        Description = "Update total view count",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number,
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "CREATEDON",
                                        Description = "Update created on timestamp",
                                        DataType = new DataType("DATETIME"),
                                        Type = PropertyType.date,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "validFrom" } }

                                    },
                                    new KProperty
                                    {
                                        Name = "KEYWORDS",
                                        Description = "Update keywords",
                                        DataType = new DataType("[LINK]"),
                                        IsRequired = true,
                                        Type = PropertyType.array,
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "URL",
                                        Description = "Update url",
                                        DataType = new DataType("LINK"),
                                        Type = PropertyType.obj,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "url" } }

                                    },
                                    new KProperty
                                    {
                                        Name = "IMAGE",
                                        Description = "Update image url",
                                        DataType= new DataType("IMAGE"),
                                        Type = PropertyType.obj,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "image" } }
                                    },
                                     new KProperty
                                    {
                                        Name = "TYPE",
                                        Description = "Update type (Popular/Latest)",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                    },
                                       new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "STARTDATE",
                                        Description = "Offer start date timestamp",
                                        DataType = new DataType("DATETIME"),
                                        Type = PropertyType.date,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "validFrom" } }

                                    },
                                        new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "EndDate",
                                        Description = "Offer end date timestamp",
                                        DataType = new DataType("DATETIME"),
                                        Type = PropertyType.date,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "validThrough" } }

                                    },
                                    new KProperty
                                    {
                                        Name = "CATEGORY",
                                        Description = "Offer category",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                    },
                                    new KProperty
                                    {
                                        Name = "PROMOCODE",
                                        Description = "Offer category",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                    },

                                },
                             },
                             new KClass
                             {
                                Name = "PRODUCT",
                                Schemas = new Dictionary<string, string> { {"itemscope", null }, {"itemtype", "http://schema.org/Offer" } },
                                ClassType = KClassType.UserDefinedClass,
                                Description = "Business offer",
                                PropertyList = new List<KProperty>
                                {
                                     new KProperty
                                    {
                                        Name = "ID",
                                        Description = "Product id",
                                        DataType = new Models.DataType("STR"),
                                        Type = PropertyType.str
                                    },
                                     new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "CREATEDON",
                                        Description = "Created on date",
                                        DataType = new Models.DataType("DATETIME"),
                                        Type = PropertyType.date,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "validFrom" } }

                                    },
                                    new KProperty
                                    {
                                        Name = "INDEX",
                                        Description = "Product index",
                                        DataType = new Models.DataType("NUMBER"),
                                        Type = PropertyType.number,
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "NAME",
                                        Description = "Product name",
                                        DataType = new Models.DataType("STR"),
                                        Type = PropertyType.str,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "name" } }
                                    },
                                    new KProperty
                                    {
                                        Name = "KEYWORDS",
                                        Description = "Product Keywords",
                                        DataType = new Models.DataType("LINK"),
                                        Type = PropertyType.array,
                                    },
                                    new KProperty
                                    {
                                        Name = "TOTALVIEWS",
                                        Description = "Total views",
                                        Type = PropertyType.number,
                                        DataType = new DataType("NUMBER")
                                    },
                                      new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "DESCRIPTION",
                                        Description = "Product description",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "description" } }

                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "PRICE",
                                        Description = "Product price",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "price" } }
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "URL",
                                        Description = "Product url",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("LINK"),
                                        Schemas = new Dictionary<string, string> { { "itemprop", "url" } }
                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "CURRENCY",
                                        Description = "Price currency",
                                        DataType = new DataType("STR"),
                                        Type = PropertyType.str,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "priceCurrency" } }

                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "ISAVAILABLE",
                                        Description = "Product available flag",
                                        Type = PropertyType.boolean,
                                        DataType = new DataType("BOOLEAN"),
                                        Schemas = new Dictionary<string, string> { { "itemprop", "availability" }, { "itemtype" , "http://schema.org/InStock" } }
                                    },
                                     new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "IMAGE",
                                        Description = "Main image link of the product",
                                        DataType = new DataType("IMAGE"),
                                        Type = PropertyType.obj,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "image" } }
                                    },
                                    new KProperty
                                    {
                                        Name = "SECONDARYIMAGES",
                                        Description = "Product secondary images",
                                        Type = PropertyType.array,
                                        DataType = new DataType("IMAGE"),
                                        Schemas = new Dictionary<string, string> { { "itemprop", "image" } }
                                    },
                                     new KProperty
                                    {
                                        Name = "DISCOUNTAMOUNT",
                                        Description = "Product discount amount",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number,
                                    },
                                      new KProperty
                                    {
                                        Name = "DISCOUNTEDAMOUNT",
                                        Description = "Product price after discount",
                                        Type = PropertyType.number,
                                        DataType = new DataType("NUMBER"),
                                    },
                                       new KProperty
                                    {
                                        Name = "DISCOUNTPERCENT",
                                        Description = "Discount percentage",
                                        DataType = new DataType("NUMBER"),
                                        Type = PropertyType.number
                                    },
                                     new KProperty
                                    {
                                        Name = "BUYONLINELINK",
                                        Description = "Buy online link of product",
                                        Type = PropertyType.obj,
                                        DataType = new DataType("LINK"),
                                        Schemas = new Dictionary<string, string> { { "itemprop", "url" } }
                                    },
                                     new KProperty
                                    {
                                        Name = "SHIPMENTDURATION",
                                        Description = "Product shipment duration",
                                        Type = PropertyType.number,
                                        DataType = new DataType("NUMBER")
                                    },
                                    new KProperty
                                    {
                                        Name = "ISCODAVAILABLE",
                                        Description = "Is cash on delievery available for the product?",
                                        DataType = new DataType("BOOLEAN"),
                                        Type = PropertyType.boolean,
                                    },
                                    new KProperty
                                    {
                                        Name = "IS-FREE-SHIPMENT-AVAILABLE",
                                        Description = "Is free shipment available for the product",
                                        DataType= new DataType("BOOLEAN"),
                                        Type = PropertyType.boolean
                                    },
                                    new KProperty
                                    {
                                        Name = "PRODUCTLABEL",
                                        Description = "Product lable",
                                        Type = PropertyType.str,
                                        DataType = new DataType("STR")
                                    }
                                }
                             },
                             new KClass
                             {
                                 Name= "BIZHOURS",
                                 ClassType = KClassType.UserDefinedClass,
                                 Description = "Biz Hours class",
                                 PropertyList = new List<KProperty>
                                 {
                                     new KProperty
                                     {
                                         Name = "ISOPEN",
                                         DataType = new DataType("BOOLEAN"),
                                         Description = "Is business open",
                                         Type = PropertyType.boolean,
                                     },
                                     new KProperty
                                     {
                                         Name = "NOSERVICESLOT",
                                         DataType = new DataType("BOOLEAN"),
                                         Description = "Does service not have a time slot",
                                         Type = PropertyType.boolean,
                                     },
                                     new KProperty
                                     {
                                         Name = "sameServiceSlot",
                                         DataType = new DataType("BOOLEAN"),
                                         Description = "Does service has same timings as business hour",
                                         Type = PropertyType.boolean,
                                     },
                                     new KProperty
                                     {
                                         Name = "TIMINGS",
                                         DataType = new DataType("TIMING"),
                                         Type = PropertyType.array,
                                         Description= "Business timings, opening hours",
                                     }
                                 }
                             },
                             new KClass
                             {
                                 Name= "TIMING",
                                 ClassType=  KClassType.UserDefinedClass,
                                 Description= "Business timing",
                                 PropertyList = new List<KProperty>
                                 {
                                     new KProperty
                                     {
                                         Name = "FROM",
                                         DataType = new DataType("STR"),
                                         Description = "From time",
                                         Type = PropertyType.str
                                     },
                                     new KProperty
                                     {
                                         Name = "TO",
                                         DataType = new DataType("STR"),
                                         Description = "To time",
                                         Type = PropertyType.str
                                     },
                                       new KProperty
                                     {
                                         Name = "DAY",
                                         DataType = new DataType("STR"),
                                         Description = "Day of the week",
                                         Type = PropertyType.str
                                     }
                                 }
                             },
                             new KClass
                             {
                                 Name= "TESTIMONIAL",
                                 ClassType=  KClassType.UserDefinedClass,
                                 Description= "Testimonial",
                                 PropertyList = new List<KProperty>
                                 {
                                     new KProperty
                                     {
                                         Name = "CREATEDON",
                                         DataType = new DataType("DATE"),
                                         Description = "Createdon time",
                                         Type = PropertyType.date
                                     },
                                     new KProperty
                                     {
                                         Name = "TITLE",
                                         DataType = new DataType("STR"),
                                         Description = "Title",
                                         Type = PropertyType.str
                                     },
                                       new KProperty
                                     {
                                         Name = "DESCRIPTION",
                                         DataType = new DataType("STR"),
                                         Description = "Description",
                                         Type = PropertyType.str
                                     },
                                       new KProperty
                                     {
                                         Name = "USERNAME",
                                         DataType = new DataType("STR"),
                                         Description = "Username",
                                         Type = PropertyType.str
                                     },
                                       new KProperty
                                     {
                                         Name = "CONTACT",
                                         DataType = new DataType("CONTACT"),
                                         Description = "Contact",
                                         Type = PropertyType.obj
                                     },
                                       new KProperty
                                     {
                                         Name = "PROFILEIMAGE",
                                         DataType = new DataType("IMAGE"),
                                         Description = "Profile image",
                                         Type = PropertyType.obj
                                     },
                                 }
                             },
                            
            #endregion
        };
            #endregion

            #region DataTypeClasses
            allClasses.AddRange(GetDataTypeClasses());
            #endregion

            return allClasses;
        }
        public static List<KClass> GetDataTypeClasses()
        {
            return new List<KClass>
            {
                 #region DataTypeClasses
                new KClass
                {
                    ClassType = KClassType.DataTypeClass,
                    Description = "String data type",
                    Name = "str",
                    PropertyList = new List<KProperty>
                    {
                        new KProperty
                        {
                            DataType = new DataType("number"),
                            Description = "String length",
                            Name = "length",
                            Type = PropertyType.number
                        },
                        new KProperty
                        {
                            DataType = new DataType("number"),
                            Description = "String length",
                            Name = "length()",
                            Type = PropertyType.number
                        },
                        new KProperty
                        {
                            DataType = new DataType("function"),
                            Description = "Sub string",
                            Name = "substr",
                            Type = PropertyType.function
                        },
                         new KProperty
                        {
                            DataType = new DataType("function"),
                            Description = "String replace",
                            Name = "replace",
                            Type = PropertyType.function
                        },
                         new KProperty
                        {
                            DataType = new DataType("function"),
                            Description = "Html decode",
                            Name = "htmlencode",
                            Type = PropertyType.function
                        },
                         new KProperty
                        {
                            DataType = new DataType("function"),
                            Description = "Html encode",
                            Name = "htmldecode",
                            Type = PropertyType.function
                        },
                         new KProperty
                        {
                            DataType = new DataType("function"),
                            Description = "String to lower case",
                            Name = "tolower",
                            Type = PropertyType.function
                        },
                         new KProperty
                        {
                            DataType = new DataType("function"),
                            Description = "String to upper case",
                            Name = "toupper",
                            Type = PropertyType.function
                        }
                    }
                },
                new KClass
                {
                    ClassType = KClassType.DataTypeClass,
                    Description = "Function data type",
                    Name = "function",
                    PropertyList = new List<KProperty>
                    {

                    }
                },
                new KClass
                {
                    ClassType = KClassType.DataTypeClass,
                    Description = "Number data type",
                    Name = "number",
                    PropertyList = new List<KProperty>
                    {

                    }
                },
                new KClass
                {
                    ClassType = KClassType.DataTypeClass,
                    Description = "Array data type",
                    Name = "array",
                    PropertyList = new List<KProperty>
                    {
                        new KProperty
                        {
                            DataType = new DataType("number"),
                            Description = "Array length",
                            Name = "length",
                            Type = PropertyType.number
                        },
                        new KProperty
                        {
                            DataType = new DataType("number"),
                            Description = "Array length",
                            Name = "length()",
                            Type = PropertyType.number
                        },
                        new KProperty
                        {
                            DataType = new DataType("function"),
                            Description = "Distinct values",
                            Name = "distinct()",
                            Type = PropertyType.function
                        }

                    }
                },
                new KClass
                {
                    ClassType = KClassType.DataTypeClass,
                    Description = "Date time data type",
                    Name = "datetime",
                    PropertyList = new List<KProperty>
                    {

                    }
                },
                new KClass
                {
                    ClassType = KClassType.DataTypeClass,
                    Description = "Boolean time data type",
                    Name = "boolean",
                    PropertyList = new List<KProperty>
                    {

                    }
                },
                new KClass
                {
                    Name = "link",
                    ClassType = KClassType.DefaultClass,
                    PropertyList = new List<KProperty> {
                    new KProperty
                        {
                            IsRequired = true,
                            Name = "url",
                            DataType = new DataType("str"),
                            Description = "Absolute url",
                            Type = PropertyType.str,
                            Schemas = new Dictionary<string, string> { { "itemprop", "url"} }
                        },
                    new KProperty
                        {
                            Name = "description",
                            DataType = new DataType("str"),
                            Description = "Url description",
                            Type = PropertyType.str
                        },
                    }
                },
                new KClass
                             {
                                Name = "location",
                                ClassType = KClassType.DefaultClass,
                                Schemas = new Dictionary<string, string> { {"itemscope", null }, { "itemtype" , "http://schema.org/GeoCoordinates" }, { "itemprop", "geo" } },
                                PropertyList = new List<KProperty>
                                {
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "latitude",
                                        Description = "Latitude",
                                        DataType = new DataType("number"),
                                        Type = PropertyType.number,
                                        Schemas = new Dictionary<string, string> { { "itemprop", "latitude" } },

                                    },
                                    new KProperty
                                    {
                                        IsRequired = true,
                                        Name = "longitude",
                                        Description = "Longitude",
                                        Type = PropertyType.number,
                                        DataType = new DataType("number"),
                                        Schemas = new Dictionary<string, string> { { "itemprop", "longitude" } },

                                    },
                                }
                             },
                new KClass
                    {
                        ClassType = KClassType.DataTypeClass,
                        Description = "KString data type",
                        Name = "kstring",
                        PropertyList = new List<KProperty>
                        {
                                new KProperty
                                {
                                    IsRequired = true,
                                    Name = "text",
                                    Description = "String value",
                                    DataType = new DataType("str"),
                                    Type = PropertyType.str,

                                },
                                new KProperty
                                {
                                    IsRequired = true,
                                    Name = "keywords",
                                    Description = "Keywords list",
                                    Type = PropertyType.array,
                                    DataType = new DataType("str"),

                                },
                        }
                },
                new KClass
                {
                    Name = "phonenumber",
                    ClassType = KClassType.DataTypeClass,
                    Description = "call tracker component data type",
                    PropertyList = new List<KProperty>
                    {
                        new KProperty
                            {
                                IsRequired = true,
                                Name = "countrycode",
                                DataType = new DataType("str"),
                                Description = "Country code",
                                Type = PropertyType.str
                            },
                        new KProperty
                            {
                                IsRequired = true,
                                Name = "contactnumber",
                                DataType = new DataType("str"),
                                Description = "Contact Number",
                                Type = PropertyType.str
                            },
                        new KProperty
                            {
                                Name = "calltrackernumber",
                                DataType = new DataType("str"),
                                Description = "Assigned Call Trcker Number",
                                Type = PropertyType.str
                            },
                        new KProperty
                            {
                                IsRequired = true,
                                Name = "isactive",
                                DataType = new DataType("boolean"),
                                Description = "Call Tracker Number activation status",
                                Type = PropertyType.boolean
                            },
                    }
                },

                      #endregion
            };
        }
    }
}
