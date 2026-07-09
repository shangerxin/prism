using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace prism.infra.Helper
{
    public static class Calculator 
    {
        public static double Geomean(double[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Array cannot be empty or null.");

            double logSum = values.Where(x=> x != 0.0).Sum(val => Math.Log(Convert.ToDouble(val)));
            double noneZeroCount = values.Count(x => x != 0.0);

            return Math.Exp(logSum / noneZeroCount);
        }
    }

}
