using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class SearchModel
    {
        public string originalsearchtext { get; set; }
        public string modifidedsearchtext { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string tileimage { get; set; }
        public string actualimage { get; set; }
        public DateTime timestamp { get; set; }
        public string itemtype { get; set; }
        public object data { get; set; }
    }
    public class SearchResult
    {
        public string originalsearchtext { get; set; }
        public string modifidedsearchtext { get; set; }
        public IEnumerable<SearchModel> result { get; set; }
        public SearchPagination extra { get; set; }
    }
    public class SearchPagination
    {
        public int currentindex { get; set; }
        public long totalcount { get; set; }
        public int pagesize { get; set; }
    }

    public class SearchModelDAO
    {
        public string OriginalSearchText { get; set; }
        public string ModifidedSearchText { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string TileImage { get; set; }
        public string ActualImage { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ItemType { get; set; }
        public string Data { get; set; }

    }
    public class SearchResultDAO
    {
        public IEnumerable<SearchModelDAO> Result { get; set; }
        public SearchPaginationDAO Extra { get; set; }
    }
    public class SearchPaginationDAO
    {
        public int CurrentIndex { get; set; }
        public long TotalCount { get; set; }
        public int PageSize { get; set; }
    }
}

