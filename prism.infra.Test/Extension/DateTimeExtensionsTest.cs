using prism.infra.Extend;

namespace prism.infra.Test;

[TestClass]
public class DateTimeExtensionsTest
{
    [TestMethod]
    public void TestToStringWorkWeek()
    {
        var date = new DateTime(2024, 1, 1); // Tuesday
        var result = date.ToStringWorkWeek();
        Assert.AreEqual("WW01", result);
        date = new DateTime(2024, 1, 7); // Sunday
        result = date.ToStringWorkWeek();
        Assert.AreEqual("WW01", result);
        date = new DateTime(2024, 1, 8); // Monday
        result = date.ToStringWorkWeek();
        Assert.AreEqual("WW02", result);
    }

    [TestMethod]
    public void TestFromStringWorkWeek()
    {
        var workweekString = "WW01";
        var result = DateTimeExtensions.FromStringWorkWeek(workweekString);
        Assert.AreEqual(new DateTime(DateTime.Now.Year, 1, 1), result);
        workweekString = "WW02";
        result = DateTimeExtensions.FromStringWorkWeek(workweekString);
        Assert.AreEqual(new DateTime(DateTime.Now.Year, 1, 8), result);
        workweekString = "2025-WW03";
        result = DateTimeExtensions.FromStringWorkWeek(workweekString);
        Assert.AreEqual(new DateTime(2025, 1, 15), result);
    }
}
