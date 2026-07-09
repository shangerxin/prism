using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Extension
{
    public static class ListExtensions
    {
        public static int ToListIndex<T>(this List<T> list, T value)
        {
            if (list == null) throw new ArgumentNullException("list");

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] == null && value == null) return i;
                if (list[i] != null && list[i].Equals(value)) return i;
            }

            return -1;
        }

        public static int[] ToListIndexes<T>(this List<T> list, IEnumerable<T> values)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (values == null) throw new ArgumentNullException("values");

            int[] indexes = new int[values.Count()];
            for (var i = 0; i < values.Count(); i++)
            {
                indexes[i] = list.ToListIndex(values.ElementAt(i));
            }
            return indexes;
        }
    }
}
