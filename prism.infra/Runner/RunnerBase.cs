using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using prism.infra.Extension;

namespace prism.infra.Runner
{
    public abstract class RunnerBase : PrismObjectBase
    {
        public string Executable { get; set; }

        protected string _workingDirectory;
        public string WorkingDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_workingDirectory))
                {
                    return Environment.CurrentDirectory;
                }
                return _workingDirectory;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    _workingDirectory = value;
                }
            }
        }
        public List<string> Arguments { get; set; } = new List<string>();
        public Process RunnerProcess { get; protected set; }
        public ProcessStartInfo RunnerStartInfo
        {
            get
            {
                return new ProcessStartInfo(Executable)
                {
                    FileName = Executable,
                    Arguments = string.Join(" ", Arguments),
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory,
                };
            }
        }
        public int ExitCode { get; protected set; } = 0;

        public string StdOut { get; protected set; }
        public string StdErr { get; protected set; }
        public Boolean IsExcussion
        {
            get
            {
                if (RunnerProcess == null) return false;
                return !RunnerProcess.HasExited;
            }
        }

        protected Boolean IsValideExecutable(string executable)
        {
            if (string.IsNullOrWhiteSpace(executable)) return false;
            if (!System.IO.File.Exists(executable) && new DirectoryInfo(executable).GetFileInSystemPath() == null) return false;
            return true;
        }

        public RunnerBase() { }
        public RunnerBase(string executable, List<string> arguments, string workingDirectory = null)
        {
            if (!IsValideExecutable(executable)) throw new ArgumentNullException(nameof(executable));
            Executable = executable;
            Arguments = arguments ?? Arguments;
            WorkingDirectory = workingDirectory;
        }

        public virtual int Execute()
        {
            throw new NotImplementedException("Execute method must be implemented in derived classes.");
        }

        public virtual async Task<int> ExecuteAsync()
        {
            ExitCode = await Task.Run<int>(Execute);
            return ExitCode;
        }

        public void Kill()
        {
            if (IsExcussion)
            {
                StdOut = null;
                StdErr = null;
                RunnerProcess?.Kill();
            }
        }
    }
}
