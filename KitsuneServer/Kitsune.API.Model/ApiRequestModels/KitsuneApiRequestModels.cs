using Kitsune.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class GetWebActionsForProjectResult
    {
        public IEnumerable<WebActionResultItem> WebActions { get; set; }

        public string Token { get; set; }
    }
    public class WebActionResultItem
    {
        public string ActionId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string WebsiteId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<WebActionProperty> Properties { get; set; }
    }
}
