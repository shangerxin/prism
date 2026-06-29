using prism.infra.Extension;
namespace prism.infra.Test;


[TestClass]
public class DirectoryExtensionsTest
{
    [TestMethod]
    public void TestGetFileInSystemPath()
    {
        string fileName = "cmd.exe";
        string filePath = new DirectoryInfo(fileName).GetFileInSystemPath();

        Assert.IsNotNull(filePath);
        Assert.IsTrue(File.Exists(filePath));
    }
}
