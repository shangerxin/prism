using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;

namespace prism.infra.Runner
{
    public class CondaRunner : RunnerBase
    {
        protected string _pipeName;
        protected string _condaPath;
        protected string _venvPath;

        public CondaRunner(string condaPath, string venvPath) : base("cmd.exe", new List<string>())
        {
            RunnerStartInfo.RedirectStandardInput = true;
            RunnerStartInfo.UseShellExecute = false;
            _pipeName = "CondaRunnerPipe_" + Guid.NewGuid().ToString();
            _condaPath = condaPath;
            _venvPath = venvPath;
        }

        public void ExecuteCmd(string cmd)
        {
            Task.Run(async () =>
            {
                using (var client = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    await client.WaitForConnectionAsync();
                    using (var writer = new StreamWriter(client))
                    {
                        await writer.WriteLineAsync($"${_condaPath} activate ${_venvPath}");
                        await writer.WriteLineAsync(cmd);
                        await writer.FlushAsync();
                    }
                }
            });
            Execute();
        }

        public void ExecuteCmds(List<string> cmds)
        {
            Task.Run(async () =>
            {
                using (var client = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    await client.WaitForConnectionAsync();
                    using (var writer = new StreamWriter(client))
                    {
                        await writer.WriteLineAsync($"${_condaPath} activate ${_venvPath}");
                        foreach (var cmd in cmds)
                        {
                            await writer.WriteLineAsync(cmd);
                        }
                        await writer.FlushAsync();
                    }
                }
            });
            Execute();
        }

        public override int Execute()
        {
            if (IsExcussion)
            {
                return ExitCode;
            }

            Task.Run(() =>
            {
                using (RunnerProcess = Process.Start(RunnerStartInfo))
                using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out))
                {
                    var writer = RunnerProcess.StandardInput;
                    client.Connect();
                    using (var reader = new StreamReader(client))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            writer.WriteLine(line);
                        }
                    }

                    StdOut = RunnerProcess.StandardOutput.ReadToEnd();
                    StdErr = RunnerProcess.StandardError.ReadToEnd();
                    RunnerProcess.WaitForExit();
                    ExitCode = RunnerProcess.ExitCode;
                }
            });
            return ExitCode;
        }

        public void Exit(int exitCode=0)
        {
            ExecuteCmd($"exit {exitCode}");
        }
    }
}

