using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Extension
{
    public static class StringExtensions
    {
        public static List<string> SplitToList(this string str, char separator=',')
        {
            if (string.IsNullOrEmpty(str))
                return new List<string>();
            var result = str.Split(separator).Where(x=>!String.IsNullOrWhiteSpace(x)).Select(s => s.Trim()).ToList();
            return result;
        }

        public static T ToEnum<T>(this string str, T defaultValue = default) where T : struct, System.Enum
        {
            try
            {
                var result = (T)System.Enum.Parse(typeof(T), str, true);
                return result;
            }
            catch { 
                Trace.WriteLine($"Failed to convert string '{str}' to enum of type {typeof(T).Name}");
                return defaultValue;
            }
        }
    }
}
