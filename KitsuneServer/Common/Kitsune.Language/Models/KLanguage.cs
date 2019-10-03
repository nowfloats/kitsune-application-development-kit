using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kitsune.Language.Models.KEntity;

namespace Kitsune.Language.Models
{
    //public delegate void LoadThemeData(string id, string widget,  Func<string, string, dynamic> GetWidgetData);
    //public class KLanguage
    //{
    //    public LoadThemeData LoadThemeDataDelegate = null;
    //    public IList<KEntity> Entities { get; private set; }
    //    public IList<KTheme> Themes { get; private set; }
    //    public KLanguage()
    //    {

    //    }
    //    public KLanguage(EntityType entityType)
    //    {
    //        var entity = new KEntity();
    //        entity.SetEntityType(EntityType.Business);
    //        this.Entities = new List<KEntity> { entity };
    //        Themes = new List<KTheme> { new KTheme(ThemeType.Default, "Default Theme") };
    //    }
    //    public KLanguage(IList<EntityType> entityTypes)
    //    {
    //        this.Entities = new List<KEntity>();
    //        var entity = new KEntity();
    //        entity.SetEntityType(EntityType.Business);
    //        foreach (var entityType in entityTypes)
    //            Entities.Add(entity);
    //        Themes = new List<KTheme> { new KTheme(ThemeType.Default, "Default Theme") };
    //    }
    //    internal void DefineEntity(KEntity entity)
    //    {
    //        if (this.Entities == null)
    //            this.Entities = new List<KEntity>();
    //        if (this.Entities.Any(x => x.EntityName == entity.EntityName))
    //        {
    //            var oldEntity = Entities.First(x => x.EntityName == entity.EntityName);
    //            oldEntity = entity;
    //        }
    //        else
    //        {
    //            this.Entities.Add(entity);
    //        }
                 
    //    }
    //    internal void DefineTheme(KTheme theme)
    //    {
    //        if (this.Themes == null)
    //            this.Themes = new List<KTheme>();
    //        if (this.Themes.Any(x => x.ThemeName == theme.ThemeName))
    //        {
    //            var oldTheme = Themes.First(x => x.ThemeName == theme.ThemeName);
    //            oldTheme = theme;
    //        }
    //        else
    //        {
    //            this.Themes.Add(theme);
    //        }

    //    }

    //}

}
