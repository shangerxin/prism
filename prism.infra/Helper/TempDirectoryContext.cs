using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Helper
{
    public class TempDirectoryContext : PrismObjectBase, IDisposable
    {
        private bool _isDisposed;
        private string _tempPath;
        public TempDirectoryContext(string prefixName)
        {
            string tempPath = Path.GetTempPath();
            _tempPath = Path.Combine(tempPath, "prism", prefixName, Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempPath);
        }

        public string CreateFile(string fileName, string content, Boolean isCreateEmptyFile=false, Encoding utf8WithoutBom = null)
        {
            content = content ?? string.Empty;
            if (string.IsNullOrEmpty(content) && !isCreateEmptyFile)
            { 
                Trace.WriteLine("Warning: Content is null or empty. File will not be created.");
                return null;
            }

            utf8WithoutBom = utf8WithoutBom ?? new UTF8Encoding(false);
            string filePath = Path.Combine(_tempPath, fileName);
            File.WriteAllText(filePath, content, utf8WithoutBom);
            return filePath;
        }

        public string TempPath => _tempPath;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _isDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TempDirectoryContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    if (Directory.Exists(_tempPath))
                    {
                        Directory.Delete(_tempPath, true);
                    }
                }
                catch
                {
                    // Ignore any exceptions during cleanup
                }
            }

            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
