using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crawler.HtmlParser
{
    public class KHtmlDocument : HtmlDocument
    {
        /// <summary>
        ///     Stores the base tag properties once IdentifyBaseTagAndGetValue(uri) is called.
        ///     If IdentifyBaseTagAndGetValue(uri) is not called it remains null.
        /// </summary>
        public BaseTag BaseTag { get; set; } = new BaseTag() { Exists=false,Href=null};





        /// <summary>
        ///     Get all the link(html files) Nodes
        ///     Ex: <a href="value"></a>
        /// </summary>
        /// <returns>Collection of link Nodes</returns>
        public HtmlNodeCollection GetAnchorsNodeCollection()
        {
            try
            {
                HtmlNode documentNode = this.DocumentNode;
                var nodes = documentNode.SelectNodes("//a[@href] | //form");
                return nodes;
            }
            catch (Exception ex)
            {
                //TODO: throw exception or return null
                return null;
            }
        }
        /// <summary>
        ///     Get all the scripts(js files) Nodes
        /// </summary>
        /// <returns>Collection of all script files Nodes</returns>
        public HtmlNodeCollection GetScriptsNodeCollection()
        {
            try
            {
                HtmlNode documentNode = this.DocumentNode;
                var nodes = documentNode.SelectNodes("//script[@src]");
                return nodes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        ///     Get all the styles(css files) Nodes
        /// </summary>
        /// <returns>Collection of all styles files Nodes</returns>
        public HtmlNodeCollection GetExternalStylesNodeCollection()
        {
            try
            {
                HtmlNode documentNode = this.DocumentNode;
                var nodes = documentNode.SelectNodes("//link[@href]");
                return nodes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        ///     Get all the Embedded style Nodes
        ///     Ex: <script></script>
        /// </summary>
        /// <returns>Collection of all Embedded style tag Nodes</returns>
        public HtmlNodeCollection GetEmbeddedStylesNodeCollection()
        {
            try
            {
                HtmlNode documentNode = this.DocumentNode;
                var nodes = documentNode.SelectNodes("//style");
                return nodes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        ///     Get all the Inner style Nodes
        ///     Ex: <a style="css styling"></a> 
        /// </summary>
        /// <returns>Collection of all Internal style tag Nodes</returns>
        public HtmlNodeCollection GetInnerStylesNodeCollection()
        {
            try
            {
                HtmlNode documentNode = this.DocumentNode;
                var nodes = documentNode.SelectNodes("//*[@style]");
                return nodes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        ///     Get all other Assets except css, js and html files
        /// </summary>
        /// <returns>Collection of all Assest Nodes</returns>
        public HtmlNodeCollection GetAssetsNodeCollection()
        {
            try
            {
                HtmlNode documentNode = this.DocumentNode;
                var nodes = documentNode.SelectNodes("//*[@src] | //*[@srcset]");
                return nodes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        /// <summary>
        ///     finds the base tag if present and initialise the BaseTag variable
        /// </summary>
        /// <param name="uri">create the absolute Uri from the base Url if given as parameter(default null)</param>
        /// <returns></returns>
        public void IdentifyBaseTagAndSetValue(Uri uri = null)
        {
            BaseTag baseTag = new BaseTag() { Exists = false, Href = null };
            try
            {
                var listOfBaseTags = this.DocumentNode.SelectNodes("//base[@href]");
                if (listOfBaseTags != null)
                {
                    var tag = listOfBaseTags.FirstOrDefault();
                    var href = tag.GetAttributeValue("href", String.Empty);
                    if (!String.IsNullOrEmpty(href))
                    {
                        Uri baseUri;
                        var isAbsolute = Uri.TryCreate(href, UriKind.Absolute, out baseUri);
                        if (uri != null && !isAbsolute)
                        {
                            Uri.TryCreate(uri, href, out baseUri);
                        }

                        baseTag.Exists = true;
                        baseTag.Href = baseUri;
                    }
                }
                BaseTag = BaseTag;
            }
            catch (Exception ex)
            {
                BaseTag = BaseTag;
            }
        }
        /// <summary>
        ///     Remove baseTag from the Html Document
        ///     TODO: set BaseUrl=null if baseUrl is Removed
        /// </summary>
        public void RemoveBaseTags()
        {
            try
            {
                var listOfBaseTags = this.DocumentNode.SelectNodes("//base[@href]");
                if (listOfBaseTags != null & listOfBaseTags.Count != 0)
                {
                    foreach (var tag in listOfBaseTags)
                    {
                        tag.Remove();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
        public Uri GetFaviconIcon(Uri uri)
        {
            try
            {
                if (uri == null) throw new Exception("uri cannot be null");

                HtmlNode documentNode = this.DocumentNode;

                HtmlNode faviconNode;
                string hrefValue = String.Empty;

                var faviconList = documentNode.SelectNodes("//link[translate(@rel,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='icon'] | //link[translate(@rel,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='shortcut icon']");
                if (faviconList != null)
                {
                    faviconNode = faviconList.First();
                    hrefValue = faviconNode.GetAttributeValue("href", "");
                    Uri faviconUri;
                    Uri.TryCreate(uri, hrefValue, out faviconUri);
                    return faviconUri;
                }
                else
                {
                    throw new Exception("favicon icon couldn't be found");
                }
            }
            catch (Exception ex)
            {
                //TODO: throw exception or return null
                throw ex;
            }
        }

    }
}
