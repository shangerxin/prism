using prism.infra.Enum;
using prism.infra.WebAPI;

namespace prism.infra.Test;

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
}
