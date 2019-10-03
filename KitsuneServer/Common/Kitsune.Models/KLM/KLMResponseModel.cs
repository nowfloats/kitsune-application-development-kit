using System.Collections.Generic;

namespace Kitsune.Models.KLM
{
    public class KLMResponseModel
    {
        public string HtmlCode;
        public bool CacheableResult;
        public Dictionary<string, long> PerfLog;
        public string ResponseSource;
    }
}
