using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Models
{
    public class FPRequestModels
    {
        
    }

    public class FloatingPointProductFinal
    {
        public string _id;
        public long ProductIndex;
        public string Name;
    }
    public class FloatingPointBizMessageFinal
    {
        public string _id;
        public long MessageIndex;
        public string Title;
    }


    public class FloatingPointFinal
    {
        public string Tag, RootAliasUri, WebTemplateId, _id;
        public double WebTemplateType;
    }
}
