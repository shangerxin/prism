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
}
