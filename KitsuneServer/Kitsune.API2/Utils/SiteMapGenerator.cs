using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Kitsune.API2.Utils
{
    public class SiteMapGenerator
    {
        public class SiteMapModel
        {
            public string SourcePath { get; set; }
            public DateTime UpdatedOn { get; set; }
        }
        public static string Create(Uri uri,List<SiteMapModel> links,List<string> extensionToIgnore=null)
        {
            try
            {
                var ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                var sci = "http://www.w3.org/2001/XMLSchema-instance";
                var scl = "http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd";

                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement("urlset", ns);
                xmlDoc.AppendChild(rootNode);
                xmlDoc.DocumentElement.SetAttribute("xmlns", ns);
                xmlDoc.DocumentElement.SetAttribute("xmlns:xsi", sci);
                xmlDoc.DocumentElement.SetAttribute("xmlns:schemaLocation", scl);

                HashSet<string> Urls = new HashSet<string>();
                foreach (var link in links)
                {
                    try
                    {
                        if (extensionToIgnore!=null && extensionToIgnore.Any(x => link.SourcePath.EndsWith(x,StringComparison.InvariantCultureIgnoreCase)))
                        {
                            continue;
                        }

                        #region creating child nodes

                        XmlNode urlNode = xmlDoc.CreateElement("url", xmlDoc.DocumentElement.NamespaceURI);
                        XmlNode loc = xmlDoc.CreateElement("loc", xmlDoc.DocumentElement.NamespaceURI);
                        XmlNode priority = xmlDoc.CreateElement("priority", xmlDoc.DocumentElement.NamespaceURI);
                        XmlNode lastModified = xmlDoc.CreateElement("lastmod", xmlDoc.DocumentElement.NamespaceURI);

                        Uri url;
                        string linkval = String.Empty;
                        if (Uri.TryCreate(uri, link.SourcePath, out url))
                        {
                            linkval = url.AbsoluteUri;
                        }
                        else
                            continue;

                        //if already added then skip it
                        if (Urls.Contains(linkval))
                            continue;
                        Urls.Add(linkval);

                        // Calculate the priority of the Url by 
                        var priorityValue = linkval.Count(x => x.Equals('/'));
                        if (priorityValue == 0)
                        {
                            priorityValue = 0;
                        }
                        else if (priorityValue > 0 && priorityValue <= 3)
                        {
                            priorityValue -= 1;
                        }
                        else
                        {
                            priorityValue = 3;
                        }

                        // Add Url
                        loc.InnerText = linkval;
                        urlNode.AppendChild(loc);

                        // Add Priority
                        priority.InnerText = (1.0 - ((double)priorityValue * 0.1)).ToString("0.0");
                        urlNode.AppendChild(priority);
                        
                        // Add lastModified
                        lastModified.InnerText = String.Format("{0:yyyy-MM-dd}", link.UpdatedOn);
                        urlNode.AppendChild(lastModified);
                        
                        // Add all child node to root node
                        rootNode.AppendChild(urlNode);

                        #endregion
                    }
                    catch { }
                }
                return xmlDoc.InnerXml;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while creating Sitemap",ex);
            }
        }
    }
}
