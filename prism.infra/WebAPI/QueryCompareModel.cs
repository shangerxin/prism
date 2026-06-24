using prism.infra.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.WebAPI
{
    public class QueryCompareModel: QueryModelBase
    {
        public string ProjectName { get; set; }
        public string TestJobName { get; set; }
        public string DataInfo { get; set; }
        public string BaseGuid { get; set; }
        public string CompareGuid { get; set; }
        public string ReferenceGuid { get; set; }
        public string QueryColumnName { get; set; }
        public string QueryBaseColumnValue { get; set; }
        public string QueryCompareColumnValue { get; set; }
        public string QueryReferenceColumnValue { get; set; }
        public QueryCompareMethodTypes Method { get; set; }
        public string BaseSuffix { get; set; }
        public string CompareSuffix { get; set; }
        public string ReferenceSuffix { get; set; }
        public int[] KeyColumnIndexes { get; set; }
        public int[] CompareWithPercentageRatioColumnIndexes { get; set; }
        public string[] FilterRowWithColumnValues { get; set; }
        public int[] OrderByResultColumnIndexes { get; set; }
        public Boolean IsRevertCompareRatio { get; set; }
        // like same, changed, new, missing to no_change, new_pass, new_fail, changed.
        public Boolean IsConvertCompareResultToTestState { get; set; }
        public Boolean IsUniqueBeforeCompare { get; set; }
        public int[] RevertCompareColumnIndexes { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
