using prism.infra.Helper;

namespace prism.infra.Test.Helper
{
    [TestClass]
    public sealed class CalcuatorTest
    {
        [TestMethod]
        public void TestGeomean()
        {
            double[] values = { 1, 2, 3, 4, 5 };
            double expected = Math.Pow(1 * 2 * 3 * 4 * 5, 1.0 / 5);
            double actual = Calculator.Geomean(values);
            Assert.AreEqual(expected, actual, 0.0001);
        }
    }
}
