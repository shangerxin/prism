using prism.infra.Runner;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Helper
{
    public class ScriptRunner: RunnerBase
    {
        public ScriptRunner() : base() { }
        public ScriptRunner(string executable, List<string> arguments) : base(executable, arguments)
        {
        }

        public override int Execute()
        {
            RunnerProcess = new Process { StartInfo = RunnerStartInfo };
            using (RunnerProcess)
            {
                RunnerProcess.Start();
                RunnerProcess.WaitForExit();
                StdOut = RunnerProcess.StandardOutput.ReadToEnd();
                StdErr = RunnerProcess.StandardError.ReadToEnd();
                return RunnerProcess.ExitCode;
            }
        }
    }
}
