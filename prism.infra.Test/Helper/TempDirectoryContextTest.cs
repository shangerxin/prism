using prism.infra.Helper;

namespace prism.infra.Test;

[TestClass]
public class TempDirectoryContextTest
{
    [TestMethod]
    public void TestCreateFile()
    {
        string filePath;
        using(var context = new TempDirectoryContext("TestTempDirectoryContext"))
        {
            string fileName = "test.txt";
            string content = "Hello, World!";
            filePath = context.CreateFile(fileName, content);
            Assert.IsTrue(File.Exists(filePath));
            Assert.AreEqual(content, File.ReadAllText(filePath));
        }
        Assert.IsTrue(!File.Exists(filePath));
    }
}
