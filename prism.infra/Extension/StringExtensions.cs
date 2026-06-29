using System;
using System.Collections.Generic;
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
    }
}
