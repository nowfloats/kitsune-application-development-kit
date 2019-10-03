using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper
{
    public class CompilerConstants
    {
        public static string GraphParentNode { get { return "project"; } }
        public static string GraphResourceNode { get { return "resource"; } }
        public static string GraphFileName { get { return "/referenceGraph-{0}.k"; } }
        public static string TreeFileName { get { return "{0}.k"; } }
        public static string SiteMapFileName { get { return "/websiteresources/{0}/sitemap.xml"; } }
        public static string KObjectReference { get { return "[k_obj_ind]"; } }
        public static string GraphSchemaNode { get { return "schema"; } }
        public static string PartialNode { get { return "partial"; } }
    }   
}
