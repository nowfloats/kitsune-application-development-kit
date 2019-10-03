using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Kitsune.Models.Theme
{
    public class BaseThemeModel : MongoEntity
    {
        public string UserEmail { get; set; }
        public string ThemeName { get; set; }
        public List<string> Category { get; set; }
        public bool IsMobileResponsive { get; set; }
        public int Version { get; set; }
        
    }
    public enum Status { Pending = 0, InVerification = 1, Verified = 2, Accepted = 3, Live = 4, Rejected = 5, CompiledLiveTheme = 6, Unassigned = 7, Error = -1 };

}
