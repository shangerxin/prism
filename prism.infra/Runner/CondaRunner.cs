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
using prism.infra.Extension;
using prism.infra.Helper;

namespace prism.infra.Runner
{
    public class CondaRunner : ScriptRunner
    {
        protected string _condaPath;
        protected string _venvPath;
        protected List<string> _cmds;
        protected string _activeCondaCmd;

        public CondaRunner(string condaPath, string venvPath, List<string> cmds = null, string workingDirectory = null) : base()
        {
            _condaPath = File.Exists(condaPath) ? condaPath : new DirectoryInfo(condaPath).GetFileInSystemPath();
            _venvPath = venvPath;
            _cmds = cmds ?? new List<string>();

            if(_condaPath == null || _venvPath == null)
            {
                throw new ArgumentException("Invalid conda or virtual environment path.");
            }

            _activeCondaCmd = $"call {_condaPath} activate {_venvPath}";
            WorkingDirectory = workingDirectory;
        }

        public int ExecuteCmd(string cmd, string workingDirectory = null)
        {
            _cmds.Clear();
            _cmds.Add(cmd);
            WorkingDirectory = workingDirectory;
            return Execute();
        }

        public int ExecuteCmds(List<string> cmds, string workingDirectory = null)
        {
            _cmds.Clear();
            _cmds.AddRange(cmds);
            WorkingDirectory = workingDirectory;
            return Execute();
        }

        public override int Execute()
        {
            if(_cmds.Count == 0)
            {
                return 0;
            }
            _cmds.Insert(0, _activeCondaCmd);
            using (var context = new TempDirectoryContext("CondaRunner"))
            {
                var content = string.Join(Environment.NewLine, _cmds);
                var bat = context.CreateFile("run.bat", content);
                Executable = bat;
                return base.Execute();
            }
        }

    }
}

