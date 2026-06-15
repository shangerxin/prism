using prism.infra.Extension;

namespace prism.infra.Test;

[TestClass]
public class StringExtensionsTest
{
    [TestMethod]
    public void TestSplitToList()
    {
        string s = "1, 2, 3, , 4,5";

        Assert.AreEqual(s.SplitToList(), new List<string> { "1", "2", "3", "4", "5" });
    }
}
