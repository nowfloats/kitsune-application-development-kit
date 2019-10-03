using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.PublishModels
{
    public class PublishSQS
    {
        public string CustomerId { get; set; }
        public string ProjectId { get; set; }
    }

    public class PublishProjectSQSModel
    {
        public string PublishId { get; set; }
    }

}
