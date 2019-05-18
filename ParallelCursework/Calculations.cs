using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelCursework
{
    static class Calculations
    {
        public static int ColumnsNumber = 10;
        public static int RowsNumber = 1000;

        public static double H = (double) 1 / ColumnsNumber;
        public static double T = (double) 1 / RowsNumber;

        public static double GetExactResult(double t, double x)
        {
            return 
                Math.Pow(
                0.05 * Math.Exp(-(x + 3 * t)) - 1
                , -3);
        }

        public static void GetApproximateResult(double[,] matrix, int i, int j)
        {
            if (i < 1 ||
                j < 1 ||
                i > RowsNumber - 1 ||
                j > ColumnsNumber - 2)
            {
                throw new ArgumentException("Position out of calculated area");
            }

            matrix[i, j] = 
                T / H * ((matrix[i - 1, j - 1] - 2 * matrix[i - 1, j] + matrix[i - 1, j + 1]) / H +
                2 * Math.Pow(matrix[i - 1, j], 1 / 3) * (matrix[i - 1, j + 1] - matrix[i - 1, j - 1]))
                + matrix[i - 1, j];
        }

    }
}
