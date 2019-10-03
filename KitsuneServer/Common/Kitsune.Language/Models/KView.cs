using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Language.Models
{
    public class KView 
    {
        public string Name { get; set; }
        /// <summary>
        /// View type should be provided at the time of the view created
        /// </summary>
        public KViewType ViewType { get; set; }
        /// <summary>
        /// IF View Type is Details or List then KClass according to that
        /// </summary>
        #region Pagination details
        public string ViewObject { get; set; }
        public string UrlPattern { get; set; }
        #endregion

        public Dictionary<string, string> Schemas { get; set; }
        public string Description { get; set; } //verb
        public bool IsRequired { get; set; }
        public bool IsPaginationEnabled { get; set; }
        public int PageSize { get; set; }
        public IList<string> CompulsoryPropertyList { get; private set; }
        public IList<KEntity> Entities { get; private set; }

        public void AddCompulsoryProperty(string property)
        {
            if (CompulsoryPropertyList == null)
                CompulsoryPropertyList = new List<string>();
            CompulsoryPropertyList.Add(property);
        }
        public void AddCompulsoryWidgets(IList<string> widgets)
        {
            if (CompulsoryPropertyList == null)
                CompulsoryPropertyList = new List<string>();
            foreach (var widget in widgets)
                CompulsoryPropertyList.Add(widget);
        }
        public void AddEntity(KEntity entity)
        {
            if (Entities == null)
                Entities = new List<KEntity>();
            Entities.Add(entity);
        }
        public void AddEntity(IList<KEntity> entities)
        {
            if (Entities == null)
                Entities = new List<KEntity>();
            foreach (var entity in entities)
                Entities.Add(entity);
        }
    }
    public enum KViewType
    {
        Custom = 0,
        List = 1,
        Details = 2,
    }
}
