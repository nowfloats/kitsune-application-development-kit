using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Models
{
    /// TODO: Deprecate rest of the types after Akamai Migration
    /// Both in withfloats and kitsune
    public enum CacheType
    {
        FLM,
        KLM,
        IDENTIFIER,
        STAGING,
        PROJECT,
        FLM_API,
        THEME,
        UPDATE_THEME,

        AKAMAI_CACHE_TAG
    }

    public class CacheInvalidationTaskModel
    {
        public string _id;
        public string CacheKey;
        public bool Invalidated;
        public string Error;
        public bool Failed;
        public CacheType type;
        public DateTime CreatedOn;
    }
}