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
    public async Task TestCondaRunnerExecuteCmd()
    {
        var runner = new CondaRunner(condaPath, venvPath);
        await runner.ExecuteCmd("pip list");
        Thread.Sleep(1000);
        Assert.IsNull(runner.StdErr);
        Thread.Sleep(1000);
        Assert.Contains("pandas", runner.StdOut);
        Thread.Sleep(1000);
        Assert.AreEqual(0, runner.ExitCode);
    }
}
