using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.WebAPI
{
    public class QueryResultModel:QueryModelBase
    {
        public string guid { get; set; }
        public DateTime timestamp { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? endTime { get; set; }
    }
}
