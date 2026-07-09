using prism.infra.Extension;

namespace prism.infra.Test;

[TestClass]
public class ListExtensionsTest
{
    [TestMethod]
    public void TestToListIndex()
    {
        var ary = new List<int> { 1, 2, 3 };

        int index1 = ary.ToListIndex(2);
        Assert.AreEqual(1, index1);

        int index2 = ary.ToListIndex(4);
        Assert.AreEqual(-1, index2);
    }

    [TestMethod]
    public void TestToListIndexes()
    {
        var ary = new List<int> { 1, 2, 3, 4 };

        int[] indexes = ary.ToListIndexes(new int[] { 2, 4, 5 });
        Assert.AreEqual(3, indexes.Length);
        Assert.AreEqual(1, indexes[0]);
        Assert.AreEqual(3, indexes[1]);
        Assert.AreEqual(-1, indexes[2]);
    }
}
