using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageProcessor
{
    public static class Extensions
    {
        public static int[,] Round(this double[,] array)
        {
            var result = new int[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); i++)
                    result[i, j] = (int)array[i, j];
            return result;
        }

        public static int[,] Round(this float[,] array)
        {
            var result = new int[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); i++)
                    result[i, j] = (int)array[i, j];
            return result;
        }
    }
}
