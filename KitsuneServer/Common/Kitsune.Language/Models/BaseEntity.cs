using Kitsune.Language.Helper;
using Kitsune.Language.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Language.Models
{
    public partial class KEntity 
    {
        public string EntityName { get; set; }
        public string EntityDescription { get; set; }
        public IList<KClass> Classes { get; set; }
        public void SetEntityType()
        {
            this.Classes = LanguageDefaults.GetDefaultClassList();
        }
        internal IEnumerable<KClass> GetAllAvailableClasses()
        {
            return Classes;
        }
       
    }
    public enum LanguageAttributes
    {
        [Description("K-Entity")]
        KEntity,
        [Description("K-Show")]
        KShow,
        [Description("K-Repeat")]
        KRepeat,
        [Description("k-script")]
        KScript,
        [Description("K-Hide")]
        KHide,
        [Description("K-Val")]
        KVal,
        [Description("K-Dl")]
        KDL,
        [Description("K-List")]
        KLIST,
        [Description("K-Object")]
        KObject,
        [Description("k-partial")]
        KPartial,
        [Description("k-pay-amount")]
        KPayAmount,
        [Description("k-pay-purpose")]
        KPayPurpose,
        [Description("k-search")]
        KSearch,
        [Description("k")]
        KTag,
        [Description("k-norepeat")]
        KNoRepeat,
    }
}
