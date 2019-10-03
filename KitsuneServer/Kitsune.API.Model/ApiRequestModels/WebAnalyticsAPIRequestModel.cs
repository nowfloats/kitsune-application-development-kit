using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.API.Model.ApiRequestModels
{
    public enum VistorsFilterType
    {
        HOUR,
        DAY,
        CITY,
        COUNTRY
    }

    public class AnalyticsData
    {
        public string Key1;
        public string Key2;
        public double DataCount;
    }

    public class GWTSearchAnalyticsRequestModel
    {
        public List<string> WebsiteIds;
        public int Limit = 10;
        public int Offset = 0;
    }
}
