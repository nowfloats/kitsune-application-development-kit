using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.API.Model.ApiRequestModels
{
    public enum EventTypeWebEngage { Optimization, Publish ,CrawlerAnalysed, Transpilation, TranspilationFailed }

    public class CreateEventRequest : BaseModel
    {
        public string ProjectId { get; set; }
        public EventTypeWebEngage Event { get; set; }
    }

    public class CreateEventResponse : ErrorApiResponseModel
    {

    }
}
