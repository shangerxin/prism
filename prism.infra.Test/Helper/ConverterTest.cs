namespace prism.infra.Test;

using prism.infra.Helper;

[TestClass]
public class ConverterTest
{
    [TestMethod]
    public void TestJsonToCsv()
    {
        string json = "[{\"Name\":\"John\",\"Age\":30},{\"Name\":\"Jane\",\"Age\":25}]";
        string expectedCsv = "Name,Age\r\nJohn,30\r\nJane,25\r\n";
        string actualCsv = Converter.JsonToCsv(json);
        Assert.AreEqual(expectedCsv, actualCsv);
    }

    [TestMethod]
    public void TestCsvToJson()
    {
        string csv = "Name,Age\r\nJohn,30\r\nJane,25\r\n";
        string expectedJson = "[{\"Name\":\"John\",\"Age\":30},{\"Name\":\"Jane\",\"Age\":25}]";
        string actualJson = Converter.CsvToJson(csv);
        Assert.AreEqual(expectedJson, actualJson);
    }

    [TestMethod]
    public void TestFilterRegressionCsv()
    {
        string csv = "Name,Age\r\nJohn,30\r\nJane,25\r\n";
        string expectedFilteredCsv = "Name,Age\r\nJohn,30";
        string actualFilteredCsv = Converter.FilterRegressionCsv(csv, "John");
        Assert.AreEqual(expectedFilteredCsv, actualFilteredCsv);

        csv = "Name,Age,CompareResult,Gender\r\nJohn,30,missing,male\r\nJane,25,pass,female\r\n";
        expectedFilteredCsv = "Name,Age,CompareResult,Gender\r\nJohn,30,missing,male";
        actualFilteredCsv = Converter.FilterRegressionCsv(csv, ".+,\\s*missing\\s*,");
        Assert.AreEqual(expectedFilteredCsv, actualFilteredCsv);
    }
}
