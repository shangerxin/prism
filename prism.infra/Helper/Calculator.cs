using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prism.infra.Helper
{
    public static class Calculator
    {
        public static double Geomean<T>(T[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Array cannot be empty or null.");

            double logSum = values.Sum(val => Math.Log(Convert.ToDouble(val)));

            return Math.Exp(logSum / values.Length);
        }
    }

}
