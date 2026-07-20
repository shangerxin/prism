using prism.infra.Enum;
using prism.infra.Extension;
using prism.infra.WebAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace prism.model.Model.WebAPI
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
                    _baseSuffix = string.IsNullOrEmpty(QueryBaseColumnValue) ? "_base" : QueryBaseColumnValue;
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
                    _compareSuffix = string.IsNullOrEmpty(QueryCompareColumnValue) ? "_cmp" : QueryCompareColumnValue;
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
                    _referenceSuffix = string.IsNullOrEmpty(QueryReferenceColumnValue) ? "_ref" : QueryReferenceColumnValue;
                }
                return _referenceSuffix;
            }
            set { _referenceSuffix = value; }
        }

        public string[] DataTableColumnNames { get; set; } = new string[0];
        protected int[] _compareColumnIndexes;
        public int[] CompareColumnIndexes
        {
            get
            {
                if (_compareColumnIndexes == null)
                {
                    if (CompareColumnNames == null || CompareColumnNames.Length == 0)
                    {
                        _compareColumnIndexes = new int[] { };
                    }
                    else
                    {
                        _compareColumnIndexes = DataTableColumnNames.ToArrayIndexes(CompareColumnNames).Where(x => x >= 0).ToArray();
                    }
                }

                return _compareColumnIndexes;
            }

            set
            {
                _compareColumnIndexes = value;
            }
        }
        public string[] CompareColumnNames { get; set; } = new string[0];

        protected int[] _keyColumnIndexes;
        public int[] KeyColumnIndexes
        {
            get
            {
                if (_keyColumnIndexes == null)
                {
                    if (KeyColumnNames == null || KeyColumnNames.Length == 0)
                    {
                        _keyColumnIndexes = new int[] { };
                    }
                    else
                    {
                        _keyColumnIndexes = DataTableColumnNames.ToArrayIndexes(KeyColumnNames).Where(x => x >= 0).ToArray();
                    }
                }

                return _keyColumnIndexes;
            }

            set
            {
                _keyColumnIndexes = value;
            }
        }
        public string[] KeyColumnNames { get; set; } = new string[0];


        protected int[] _keepColumnIndexes;
        public int[] KeepColumnIndexes
        {
            get
            {
                if (_keepColumnIndexes == null)
                {
                    if (KeepColumnNames == null || KeepColumnNames.Length == 0)
                    {
                        _keepColumnIndexes = new int[] { };
                    }
                    else
                    {
                        _keepColumnIndexes = DataTableColumnNames.ToArrayIndexes(KeepColumnNames).Where(x => x >= 0).ToArray();
                    }
                }

                return _keepColumnIndexes;
            }

            set
            {
                _keepColumnIndexes = value;
            }
        }
        public string[] KeepColumnNames { get; set; } = new string[0];

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
        public int EffectiveDigits { get; set; } = 2;
        public string RegressionPattern { get; set; } = "missing";

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

        public string ToCompareCommandLine(string pythonScriptPath, string baseCSV, string compareCSV, string outputPath)
        {
            string commandLine = $"python {pythonScriptPath} {baseCSV} --output {outputPath} --compare {compareCSV} --file-suffix {BaseSuffix} --compare-suffix {CompareSuffix} {Option("compare-column", CompareColumnIndexes)} --compare-method {Method.ToString()} {Option("is-revert-compare", IsRevertCompareRatio)} {Option("revert-compare-columns", RevertCompareColumnIndexes)} {Option("is-convert-compare-as-value-to-test-state", IsConvertCompareResultToTestState)} {Option("keep-column", KeepColumnIndexes)} {Option("key-column", KeyColumnIndexes)} {Option("percentage-ratio-columns", CompareWithPercentageRatioColumnIndexes)} {Option("unique", IsUniqueBeforeCompare)} {Option("filter-row-with-value", FilterRowWithColumnValues)} {Option("sort-by-column", OrderByResultColumnIndexes)} --file-target-must-exist";

            return commandLine;
        }

        public string ToFilterCommandLine(string pythonScriptPath, string inputCSV, string outputPath)
        {
            string commandLine = $"python {pythonScriptPath} {inputCSV} --output {outputPath} {Option("filter-row-with-value", FilterRowWithColumnValues)} {Option("sort-by-column", OrderByResultColumnIndexes)} --file-target-must-exist";
            return commandLine;
        }
    }
}
