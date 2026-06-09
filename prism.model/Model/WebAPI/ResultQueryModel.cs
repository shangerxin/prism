using prism.infra.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.model
{
    public class ResultQueryModel: QueryModelBase
    {
        public ICollection<string> SelectColumns { get; set; }
        public IDictionary<string, List<string>> GroupResultsWithColumnValues {  get; set; }
    }
}
