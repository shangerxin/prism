using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Extension
{
    public static class DirectoryExtensions
    {
        public static string GetFileInSystemPath(this DirectoryInfo directory)
        {
            // Retrieve the system PATH environment variable
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv)) return null;

            // Split paths using the OS-specific separator (';' on Windows, ':' on Linux/macOS)
            string[] paths = pathEnv.Split(Path.PathSeparator);

            foreach (string path in paths)
            {
                // Combine the environment directory with your file name securely
                string fullPath = Path.Combine(path.Trim(), directory.Name);

                if (File.Exists(fullPath))
                {
                    directory = new DirectoryInfo(fullPath);
                    return fullPath;
                }
            }

            return null;
        }
    }
}
