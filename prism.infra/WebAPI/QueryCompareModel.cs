using prism.infra.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace prism.infra.WebAPI
{
    public class QueryCompareModel: QueryModelBase
    {

        [JsonRequired]
        public string ProjectName { get; set; }

        [JsonRequired]
        public string TestJobName { get; set; }
        [JsonRequired]
        public string DataInfo { get; set; }
        public string BaseGuid { get; set; }
        public string CompareGuid { get; set; }
        public string ReferenceGuid { get; set; }
        public string QueryColumnName { get; set; }
        public string QueryBaseColumnValue { get; set; }
        public string QueryCompareColumnValue { get; set; }
        public string QueryReferenceColumnValue { get; set; }
        [JsonRequired]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryCompareMethodTypes Method { get; set; }
        public string BaseSuffix { get; set; } = "_base";
        public string CompareSuffix { get; set; } = "_cmp";
        public string ReferenceSuffix { get; set; } = "_ref";
        [JsonRequired]
        public int[] CompareColumnIndexes { get; set; } = new int[0];
        [JsonRequired]
        public int[] KeyColumnIndexes { get; set; } = new int[0];
        public int[] KeepColumnIndexes { get; set; } = new int[0];
        public int[] CompareWithPercentageRatioColumnIndexes { get; set; } = new int[0];
        // like 2:pass
        public string[] FilterRowWithColumnValues { get; set; }
        public int[] OrderByResultColumnIndexes { get; set; } = new int[0];
        public Boolean IsRevertCompareRatio { get; set; }
        // like same, changed, new, missing to no_change, new_pass, new_fail, changed.
        public Boolean IsConvertCompareResultToTestState { get; set; }
        public Boolean IsUniqueBeforeCompare { get; set; }
        public int[] RevertCompareColumnIndexes { get; set; } = new int[0];
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string Option<T>(string option, T value)
        {
            string optionValue = null;
            if(value is ICollection)
            {
                var collection = value as ICollection;
                optionValue = string.Join(" ", collection.Cast<object>()).Trim();
            }
            else if(value is Boolean)
            {
                if ((bool)(object)value)
                {
                    return $" --{option}";
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                optionValue = value?.ToString().Trim();
            }

            if (string.IsNullOrEmpty(optionValue))
            {
                return string.Empty;
            }
            return $" --{option} {optionValue}";
        }

        // Reference command line from script Prism\prism.web.service\Script\compare_results.py
        public string ToCompareCommandLine(string pythonScriptPath, string baseCSV, string compareCSV, string referenceCSV, string outputPath)
        {
            string commandLine = $"python {pythonScriptPath} {baseCSV} --output {outputPath} --compare {compareCSV} --reference {referenceCSV} --file-suffix {BaseSuffix} --compare-suffix {CompareSuffix} --reference-suffix {ReferenceSuffix} {Option("compare-column", CompareColumnIndexes)} --compare-method {Method.ToString()} {Option("is-revert-compare", IsRevertCompareRatio)} {Option("revert-compare-columns", RevertCompareColumnIndexes)} {Option("is-convert-compare-as-value-to-test-state", IsConvertCompareResultToTestState)} {Option("keep-column", KeepColumnIndexes)} {Option("key-column", KeyColumnIndexes)} {Option("percentage-ratio-columns", CompareWithPercentageRatioColumnIndexes)} {Option("unique", IsUniqueBeforeCompare)} {Option("filter-row-with-value", FilterRowWithColumnValues)} {Option("sort-by-column", OrderByResultColumnIndexes)} --file-target-must-exist";

            return commandLine;
        }
    }
}
