using Kitsune.Language.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Compiler.Helpers
{
    public class ProjectPreview
    {
        public string HtmlString { get; set; }
        public string CompilerTime { get; set; }
        public long PreviewTime { get; set; }
    }
    public class KitsunePreviewModel
    {
        public string FileContent { get; set; }
        public string ProjectId { get; set; }
        public string View { get; set; }
        public string ViewType { get; set; }
        public string WebsiteTag { get; set; }
        public string DeveloperId { get; set; }
        public string[] UrlParams { get; set; }
        public string NoCacheQueryParam { get; set; }
    }
}
