using prism.infra.Runner;
using prism.infra.Extension;
namespace prism.infra.Test;

[TestClass]
public class CondaRunnerTest
{
    protected static string solutionPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
    protected static string condaPath = new DirectoryInfo("conda.bat").GetFileInSystemPath();
    protected static string venvPath = Path.Combine(solutionPath, "prism.web.service\\Venv");
    [TestMethod]
    public void TestCondaRunnerExecuteCmd()
    {
        var runner = new CondaRunner(condaPath, venvPath);
        runner.ExecuteCmd("pip list");
        Assert.IsTrue(string.IsNullOrEmpty(runner.StdErr));
        Assert.Contains("pandas", runner.StdOut);
        Assert.AreEqual(0, runner.ExitCode);
    }
}
