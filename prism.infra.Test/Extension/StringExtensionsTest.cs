using prism.infra.Extension;

namespace prism.infra.Test;

[TestClass]
public class StringExtensionsTest
{
    private enum EnumTypes
    {
        a, b
    }

    [TestMethod]
    public void TestSplitToList()
    {
        string s = "1, 2, 3, , 4,5";

        CollectionAssert.AreEqual(s.SplitToList(), new List<string> { "1", "2", "3", "4", "5" });
    }

    [TestMethod]
    public void ToEnumTest()
    {
        Assert.AreEqual(EnumTypes.a, "a".ToEnum<EnumTypes>());
        Assert.AreEqual(EnumTypes.b, "B".ToEnum<EnumTypes>());
        Assert.AreEqual(EnumTypes.b, "C".ToEnum(EnumTypes.b));
        Assert.AreEqual(EnumTypes.a, "D".ToEnum<EnumTypes>());
    }
}
