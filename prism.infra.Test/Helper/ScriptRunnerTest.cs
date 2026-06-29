using prism.infra.Helper;

namespace prism.infra.Test;

[TestClass]
public class ScriptRunnerTest
{
    [TestMethod]
    public void TestExecute()
    {
        var runner = new ScriptRunner("cmd.exe", new List<string> { "/c", "\"echo Hello, World!\"" });
        runner.Execute();
        Assert.AreEqual(0, runner.ExitCode);
        Assert.AreEqual("Hello, World!", runner.StdOut.Trim());
    }
}
