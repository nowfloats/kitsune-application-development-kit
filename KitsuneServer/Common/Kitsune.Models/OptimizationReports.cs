using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class OptimizationReports: MongoEntity
    {
        public string ProjectId { get; set; }
        public OptimizationMetrics Link { get; set; }
        public OptimizationMetrics Style { get; set; }
        public OptimizationMetrics Script { get; set; }
        public OptimizationMetrics Asset { get; set; }
        public OptimizationMetrics Total { get; set; }
        public int BrokenLinks { get; set; }
        public bool MailSent { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
    public class OptimizationMetrics
    {
        public string BeforeOptimization { get; set; }
        public string AfterOptimization { get; set; }
        public double? Improvement { get; set; }
    }

}
