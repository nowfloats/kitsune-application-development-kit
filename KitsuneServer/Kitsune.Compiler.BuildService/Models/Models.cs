using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.Compiler.BuildService.Models
{
    public class UrlPatternRegexDetails
    {
        public string SourcePath { get; set; }
        public string UrlPattern { get; set; }
        public string UrlPatternRegex { get; set; }
    }
}
