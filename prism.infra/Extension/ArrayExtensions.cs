using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Extension
{
    public static class ArrayExtensions
    {
        public static int ToArrayIndex<T>(this T[] array, T value)
        {
             if(array == null) throw new ArgumentNullException("array");
        
             for(var i = 0; i < array.Length; i++) {
                if (array[i] == null && value == null) return i;
                if (array[i] != null && array[i].Equals(value)) return i;
             }

             return -1;
        }

        public static int[] ToArrayIndexes<T>(this T[] array, IEnumerable<T> values) { 
            if( array == null) throw new ArgumentNullException("array");
            if( values == null) throw new ArgumentNullException("values");

            int[] indexes = new int[values.Count()];
            for(var i = 0; i < values.Count(); i++) {
                indexes[i] = array.ToArrayIndex(values.ElementAt(i));
            }
            return indexes;
        }
    }
}
