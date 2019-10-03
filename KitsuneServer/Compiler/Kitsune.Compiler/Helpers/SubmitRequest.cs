using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Kitsune.Models.Theme;
using Kitsune.Models.Project;

namespace Kitsune.Compiler.Helpers
{
    public class SubmitResourceRequest
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string UserEmail { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string FileContent { get; set; }
        public string UrlPattern { get; set; }
        //public bool IsMobileResponsive { get; set; }
        public bool IsDev { get; set; }
        public bool IsStatic { get; set; }
        public bool IsDefault { get; set; }
        public string DefaultProjectId { get;  set; }
        public string SourcePath { get; set; }
        public string ClassName { get; set; }
        public string KObject { get; set; }
        public bool IsPublish { get; internal set; }
        public KitsunePageType PageType { get;  set; }
        public ProjectStatus ProjectStatus { get; set; }
        public ResourceType ResourceType { get; set; }
        public bool IsPreview { get; set; }
    }
}
