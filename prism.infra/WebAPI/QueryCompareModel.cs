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
    public class QueryCompareModel : QueryModelBase
    {

        [JsonRequired]
        public string ProjectName { get; set; }

        [JsonRequired]
        public string TestJobName { get; set; }
        [JsonRequired]
        public string DataInfo { get; set; }

        protected string _baseGuid;
        public string BaseGuid
        {
            get
            {
                if (string.IsNullOrEmpty(_baseGuid))
                {
                    if (Guids.Length > 0)
                    {
                        _baseGuid = Guids[0];
                    }
                }
                return _baseGuid;
            }
            set
            {
                _baseGuid = value;
            }
        }
        protected string _compareGuid;
        public string CompareGuid
        {
            get
            {
                if (string.IsNullOrEmpty(_compareGuid))
                {
                    if (Guids.Length > 1)
                    {
                        _compareGuid = Guids[1];
                    }
                }
                return _compareGuid;
            }
            set
            {
                _compareGuid = value;
            }
        }

        protected string _referenceGuid;
        public string ReferenceGuid
        {
            get
            {
                if (string.IsNullOrEmpty(_referenceGuid))
                {
                    if (Guids.Length > 2)
                    {
                        _referenceGuid = Guids[2];
                    }
                }
                return _referenceGuid;
            }

            set
            {
                _referenceGuid = value;
            }
        }
        public string[] Guids { get; set; } = new string[0];
        public string QueryColumnName { get; set; }
        protected string _queryBaseColumnValue;
        public string QueryBaseColumnValue
        {
            get
            {
                if (string.IsNullOrEmpty(_queryBaseColumnValue))
                {
                    if (ColumnValues.Length > 0)
                    {
                        _queryBaseColumnValue = ColumnValues[0];
                    }
                }
                return _queryBaseColumnValue;
            }
            set
            {
                _queryBaseColumnValue = value;
            }
        }

        protected string _queryCompareColumnValue;
        public string QueryCompareColumnValue
        {
            get
            {
                if (string.IsNullOrEmpty(_queryCompareColumnValue))
                {
                    if (ColumnValues.Length > 1)
                    {
                        _queryCompareColumnValue = ColumnValues[1];
                    }
                }
                return _queryCompareColumnValue;
            }
            set
            {
                _queryCompareColumnValue = value;
            }
        }
        protected string _queryReferenceColumnValue;
        public string QueryReferenceColumnValue
        {
            get
            {
                if (string.IsNullOrEmpty(_queryReferenceColumnValue))
                {
                    if (ColumnValues.Length > 2)
                    {
                        _queryReferenceColumnValue = ColumnValues[2];
                    }
                }
                return _queryReferenceColumnValue;
            }
            set
            {
                _queryReferenceColumnValue = value;
            }
        }
        public string[] ColumnValues { get; set; } = new string[0];
        [JsonRequired]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public QueryCompareMethodTypes Method { get; set; }
        protected string _baseSuffix;
        public string BaseSuffix
        {
            get
            {
                if (string.IsNullOrEmpty(_baseSuffix))
                {
                    _baseSuffix = QueryBaseColumnValue ?? "_base";
                }
                return _baseSuffix;
            }
            set { _baseSuffix = value; }
        }

        protected string _compareSuffix;
        public string CompareSuffix
        {
            get
            {
                if (string.IsNullOrEmpty(_compareSuffix))
                {
                    _compareSuffix = QueryCompareColumnValue ?? "_cmp";
                }
                return _compareSuffix;
            }
            set { _compareSuffix = value; }
        }

        protected string _referenceSuffix;
        public string ReferenceSuffix
        {
            get
            {
                if (string.IsNullOrEmpty(_referenceSuffix))
                {
                    _referenceSuffix = QueryReferenceColumnValue ?? "_ref";
                }
                return _referenceSuffix;
            }
            set { _referenceSuffix = value; }
        }
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
            if (value is ICollection)
            {
                var collection = value as ICollection;
                optionValue = string.Join(" ", collection.Cast<object>()).Trim();
            }
            else if (value is Boolean)
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
