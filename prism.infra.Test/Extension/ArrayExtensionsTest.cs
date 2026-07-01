using prism.infra.Extension;

namespace prism.infra.Test;

[TestClass]
public class ArrayExtensionsTest
{
    [TestMethod]
    public void TestToArrayIndex()
    {
        var ary = new[] { 1, 2, 3 };

        int index1 = ary.ToArrayIndex(2);
        Assert.AreEqual(1, index1);

        int index2 = ary.ToArrayIndex(4);
        Assert.AreEqual(-1, index2);
    }

    [TestMethod]
    public void TestToArrayIndexes()
    {
        var ary = new[] { 1, 2, 3, 4 };

        int[] indexes = ary.ToArrayIndexes(new[] { 2, 4, 5 });
        Assert.AreEqual(3, indexes.Length);
        Assert.AreEqual(1, indexes[0]);
        Assert.AreEqual(3, indexes[1]);
        Assert.AreEqual(-1, indexes[2]);
    }
}
