using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelCursework
{
    class CalculationsImplicit
    {
        public static int ColumnsNumber = 11;
        public static int RowsNumber = 1001;
        public static double H = (double) 1 / (ColumnsNumber - 1);
        public static double T = (double) 1 / (RowsNumber - 1);

        static public double GetExactResult(double x, double t)
        {
            return 
                Math.Pow(
                0.05 * Math.Exp(-(x + 3 * t)) - 1
                , -3);
        }

        static public void SolveTridiagonalMatrix(double[] alpha, double[] beta,
            double[] gamma, double[] func, double[] x)
        {
            int size = ColumnsNumber - 2;
            double[] aRight = new double[size];
            double[] bRight = new double[size];
            double[] aLeft = new double[size];
            double[] bLeft = new double[size];

            var task1 = Task.Factory.StartNew(() =>
            {
                aRight[0] = -gamma[0] / alpha[0];
                bRight[0] = func[0] / alpha[0];

                for (int i = 1; i <= size / 2; ++i)
                {
                    aRight[i] = -gamma[i] / (beta[i] * aRight[i - 1] + alpha[i]);
                    bRight[i] = (func[i] - beta[i] * bRight[i - 1]) / (beta[i] * aRight[i - 1] + alpha[i]);
                }
            });

            var task2 = Task.Factory.StartNew(() =>
            {
                aLeft[size - 1] = -beta[size - 1] / alpha[size - 1];
                bLeft[size - 1] = func[size - 1] / alpha[size - 1];

                for (int i = size - 2; i > size / 2; --i)
                {
                    aLeft[i] = -beta[i] / (gamma[i] * aLeft[i + 1] + alpha[i]);
                    bLeft[i] = (func[i] - gamma[i] * bLeft[i + 1]) / (gamma[i] * aLeft[i + 1] + alpha[i]);
                }
            });

            Task.WaitAll(task1, task2);

            x[size / 2] = (bRight[size / 2] + aRight[size / 2] * bLeft[size / 2 + 1]) /
              (1 - aRight[size / 2] * aLeft[size / 2 + 1]);

            task1 = Task.Factory.StartNew(() =>
            {
                for (int i = size / 2 - 1; i >= 0; i--)
                {
                    x[i] = aRight[i] * x[i + 1] + bRight[i];
                }
            });

            task2 = Task.Factory.StartNew(() =>
            {
                for (int i = size / 2 + 1; i < size; i++)
                {
                    x[i] = aLeft[i] * x[i - 1] + bLeft[i];
                }
            });

            Task.WaitAll(task1, task2);
        }

        static public void Process()
        {
            var timerStart = DateTime.Now;

            var omega = new double[RowsNumber, ColumnsNumber];

            for (int i = 0; i < ColumnsNumber; ++i)
            {
                omega[0, i] = GetExactResult(i * H, 0);
            }

            for (int i = 0; i < RowsNumber; ++i)
            {
                omega[i, 0] = GetExactResult(0, T * i);
                omega[i, ColumnsNumber - 1] = GetExactResult(1, T * i);
            }

            double sigma = T / H / H;

            double[] alpha = 
                Enumerable.Repeat(1 + 2 * sigma,
                    ColumnsNumber - 2).ToArray();

            double[] beta = Enumerable.Repeat(-sigma,
                    ColumnsNumber - 2).ToArray();

            double[] gamma = Enumerable.Repeat(-sigma,
                    ColumnsNumber - 2).ToArray();

            double[] func = new double[ColumnsNumber - 2];
            double[] x = new double[ColumnsNumber - 2];

            for (int j = 1; j < RowsNumber; ++j)
            {
                for (int i = 0; i < ColumnsNumber - 2; ++i)
                {
                    func[i] = omega[j - 1, i + 1];
                }
                
                func[0] += omega[j, 0] * sigma;
                func[ColumnsNumber - 3] += omega[j, ColumnsNumber - 1] * sigma;

                SolveTridiagonalMatrix(alpha, beta, gamma, func, x);

                for (int i = 1; i < ColumnsNumber - 1; ++i)
                {
                    omega[j, i] = x[i - 1];
                }
            }

            Console.WriteLine($"Time: {(DateTime.Now - timerStart).TotalMilliseconds}");

            double AbsoluteAccParall = 0;
            double RelativeParall = 0;

            File.WriteAllText("outputImplicit.txt", "");
            using (var writer = new StreamWriter("outputImplicit.txt"))
            {
                for (int i = 0; i < RowsNumber; i++)
                {
                    for (int j = 0; j < ColumnsNumber; j++)
                    {
                        writer.Write(omega[i, j].ToString("0.00000") + " ");
                    }
                    writer.Write("\n");
                }
            }

            for (int i = 0; i < ColumnsNumber; i += 2)
            {
                for (int j = 0; j < RowsNumber; j += 30)
                {
                    if (AbsoluteAccParall < Math.Abs(omega[j,i] - GetExactResult(i * H, j * T)))
                    {
                        AbsoluteAccParall = Math.Abs(omega[j,i] - GetExactResult(i * H, j * T));
                        RelativeParall = AbsoluteAccParall / omega[j, i] * 100;
                    }
                }
            }

            Console.WriteLine($"Absolute deviation =  {AbsoluteAccParall}");
            Console.WriteLine($"Relative deviation =  {RelativeParall}");
        }
    }
}
