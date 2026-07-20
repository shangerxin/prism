using prism.infra.Enum;
using System.Text.Json;
using prism.model.Model.WebAPI;

namespace prism.model.Test;

[TestClass]
public class TestQueryCompareModel
{
    [TestMethod]
    public void TestToCompareCommandLine()
    {
        var model = new QueryCompareModel()
        {
            ProjectName = "TestProject",
            TestJobName = "TestJob",
            DataInfo = "TestData",
            BaseGuid = "BaseGuid",
            CompareGuid = "CompareGuid",
            ReferenceGuid = "ReferenceGuid",
            QueryColumnName = "QueryColumn",
            QueryBaseColumnValue = "BaseValue",
            QueryCompareColumnValue = "CompareValue",
            QueryReferenceColumnValue = "ReferenceValue",
            Method = QueryCompareMethodTypes.ratio,
            BaseSuffix = "_base",
            CompareSuffix = "_compare",
            ReferenceSuffix = "_reference",
            CompareColumnIndexes = new int[] { 1, 2 },
            KeyColumnIndexes = new int[] { 0 },
            KeepColumnIndexes = new int[] { 3, 4 },
            CompareWithPercentageRatioColumnIndexes = new int[] { 5 },
            FilterRowWithColumnValues = new string[] { "Filter1", "Filter2" },
            OrderByResultColumnIndexes = new int[] { 6 },
            IsRevertCompareRatio = true,
            IsConvertCompareResultToTestState = false,
            IsUniqueBeforeCompare = true,
            RevertCompareColumnIndexes = new int[] { 7 },
            StartTime = DateTime.Now.AddHours(-1),
            EndTime = DateTime.Now
        };
        var cmd = model.ToCompareCommandLine("pythonScriptPath", "baseCSV", "compareCSV", "referenceCSV", "outputPath");

        Assert.AreEqual($"python pythonScriptPath baseCSV --output outputPath --compare compareCSV --reference referenceCSV --file-suffix _base --compare-suffix _compare --reference-suffix _reference {model.Option("compare-column", new int[] { 1, 2 })} --compare-method ratio {model.Option("is-revert-compare", true)} {model.Option("revert-compare-columns", new int[] { 7 })} {model.Option("is-convert-compare-as-value-to-test-state", false)} {model.Option("keep-column", new int[] { 3, 4 })} {model.Option("key-column", new int[] { 0 })} {model.Option("percentage-ratio-columns", new int[] { 5 })} {model.Option("unique", true)} {model.Option("filter-row-with-value", new string[] { "Filter1", "Filter2" })} {model.Option("sort-by-column", new int[] { 6 })} --file-target-must-exist", cmd);
    }

    [TestMethod]
    public void TestOption()
    {
        string? value = null;
        var model = new QueryCompareModel();
        string option = model.Option("abc", value);
        Assert.AreEqual("", option);

        int[] values = new int[0];
        option = model.Option("abc", values);
        Assert.AreEqual("", option);

        values = new int[] { 1, 2, 3 };
        option = model.Option("abc", values);
        Assert.AreEqual(" --abc 1 2 3", option);

        var isTrue = true;
        option = model.Option("abc", isTrue);
        Assert.AreEqual(" --abc", option);
        isTrue = false;
        option = model.Option("abc", isTrue);
        Assert.AreEqual("", option);
    }

    [TestMethod]
    public void TestQueryModelWithColumnNames()
    {
        var json = """
                        {
              "ProjectName": "Huggingface",
              "TestJobName": "PyTorch_OOB",
              "DataInfo": "pytorch_oob",
              "BaseGuid": null,
              "CompareGuid": null,
              "ReferenceGuid": null,
              "QueryColumnName": "workweek",
              "QueryBaseColumnValue": "",
              "QueryCompareColumnValue": "",
              "QueryReferenceColumnValue": "",
              "ColumnValues": ["202624","202621","202618"],
              "Method": "ratio",
              "BaseSuffix": "",
              "CompareSuffix": "",
              "ReferenceSuffix": "",
              "DataTableColumnNames": ["Workload","Batch_size", "Eager_XPU_T2_ms","Eager_CUDA_T2_ms"],
              "CompareColumnIndexes": null,
              "CompareColumnNames": [
                "Eager_XPU_T2_ms","Eager_CUDA_T2_ms"
              ],
              "KeyColumnIndexes": null,
              "KeyColumnNames":[
                "Workload","Batch_size"
              ],
              "KeepColumnIndexes": null,
              "KeepColumnNames":[
                "Workload","Eager_XPU_T2_ms"
              ],
              "CompareWithPercentageRatioColumnIndexes": [
              ],
              "FilterRowWithColumnValues": [

              ],
              "OrderByResultColumnIndexes": [
                0,1
              ],
              "IsRevertCompareRatio": false,
              "IsConvertCompareResultToTestState": false,
              "IsUniqueBeforeCompare": false,
              "RevertCompareColumnIndexes": [
              ]
            }

            """;
        var query = JsonSerializer.Deserialize<QueryCompareModel>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        Assert.AreEqual("202624", query.QueryBaseColumnValue);
        Assert.AreEqual("202621", query.QueryCompareColumnValue);
        Assert.AreEqual("202618", query.QueryReferenceColumnValue);

        CollectionAssert.AreEqual(new int[] { 2, 3 }, query.CompareColumnIndexes);
        CollectionAssert.AreEqual(new int[] { 0, 1 }, query.KeyColumnIndexes);
        CollectionAssert.AreEqual(new int[] { 0, 2 }, query.KeepColumnIndexes);
    }
}
