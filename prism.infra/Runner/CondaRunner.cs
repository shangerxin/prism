using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Threading;
using System.Runtime.CompilerServices;

namespace prism.infra.Runner
{
    public class CondaRunner : RunnerBase
    {
        protected string _pipeName;
        protected string _condaPath;
        protected string _venvPath;
        protected void WorkerExecute(Func<Task> func)
        {
            var thread = new Thread(async () => await func());
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
        }

        public CondaRunner(string condaPath, string venvPath) : base("cmd.exe", new List<string>())
        {
            RunnerStartInfo.RedirectStandardInput = true;
            RunnerStartInfo.UseShellExecute = false;
            _pipeName = "CondaRunnerPipe_" + Guid.NewGuid().ToString();
            _condaPath = condaPath;
            _venvPath = venvPath;
        }

        public async Task<int> ExecuteCmd(string cmd)
        {
            WorkerExecute(async () =>
            {
                using (var client = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
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
            await ExecuteAsync();
            return ExitCode;
        }

        public async Task<int> ExecuteCmds(List<string> cmds)
        {
            WorkerExecute(async () =>
            {
                using (var client = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
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
            await ExecuteAsync();
            return ExitCode;
        }

        public override int Execute()
        {
            if (IsExcussion)
            {
                return ExitCode;
            }

            WorkerExecute(async () =>
            {
                using (RunnerProcess = Process.Start(RunnerStartInfo))
                using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
                using (var writer = RunnerProcess.StandardInput)
                {
                    await client.ConnectAsync();
                    string cmd;
                    using (var cmdReader = new StreamReader(client))
                    {
                        while ((cmd = await cmdReader.ReadLineAsync()) != null)
                        {
                            if (writer.BaseStream.CanWrite)
                            {
                                writer.WriteLine(cmd);
                            }
                            else
                            {
                                Trace.WriteLine($"StandardInput is closed. Cannot write command. {cmd}");
                            }
                        }
                    }
                    

                    StdOut = await RunnerProcess.StandardOutput.ReadToEndAsync();
                    StdErr = await RunnerProcess.StandardError.ReadToEndAsync();
                    RunnerProcess.WaitForExit();
                    ExitCode = RunnerProcess.ExitCode;
                }
            });
            return ExitCode;
        }

        public async Task<int> Exit(int exitCode=0)
        {
            return await ExecuteCmd($"exit {exitCode}");
        }
    }
}

