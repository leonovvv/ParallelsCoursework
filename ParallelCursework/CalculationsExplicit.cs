using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelCursework
{
    static class CalculationsExplicit
    {
        public static int ChunkSize = ColumnsNumber / Program.ThreadsCount;

        public static int ColumnsNumber = 11;
        public static int RowsNumber = 1001;

        private static double H = (double) 1 / (ColumnsNumber - 1);
        private static double T = (double) 1 / (RowsNumber - 1);
        private static double[,] parallelDataMatrix = new double[RowsNumber, ColumnsNumber];
        private static double[,] singleThreadDataMatrix = new double[RowsNumber, ColumnsNumber];

        private static double GetExactResult(double t, double x)
        {
            return 
                Math.Pow(
                0.05 * Math.Exp(-(x + 3 * t)) - 1
                , -3);
        }

        private static void GetApproximateResult(double[,] matrix, int i, int j)
        {
            matrix[i, j] = 
                T / H * ((matrix[i - 1, j - 1] - 2 * matrix[i - 1, j] + matrix[i - 1, j + 1]) / H +
                2 * Math.Pow(matrix[i - 1, j], 1 / 3) * (matrix[i - 1, j + 1] - matrix[i - 1, j - 1]))
                + matrix[i - 1, j];
        }

        private static void Task(int row, int index, double[,] matrix)
        {
            int indexFrom;
            int indexTo;

            if (index == Program.ThreadsCount - 1)
            {
                indexFrom = index * ChunkSize;
                indexTo = ColumnsNumber - 1;
            }
            else
            {
                indexFrom = index * ChunkSize;
                indexTo = (index + 1) * ChunkSize - 1;
            }

            if (indexFrom == 0)
                indexFrom = 1;

            for (int j = indexFrom; j < indexTo; j++)
            {
                GetApproximateResult(matrix, row, j);
            }
        }

        public static void Process()
        {
            var threads = new List<Thread>();

            var timerStart = DateTime.Now;

            if (Program.ThreadsCount > ColumnsNumber)
            {
                Program.ThreadsCount = ColumnsNumber;
                ChunkSize = 1;
            }

            Parallel.For(0, RowsNumber, i =>
            {
                parallelDataMatrix[i, 0] = GetExactResult(T * i, 0);
                parallelDataMatrix[i, ColumnsNumber - 1] =
                GetExactResult(T * i, 1);
            });

            Parallel.For(1, ColumnsNumber - 1, i =>
            {
                parallelDataMatrix[0, i] = GetExactResult(0, H * i);
            });

            for (int i = 1; i < RowsNumber; i++)
            {
                int count = 0;

                for (int j = 0; j < Program.ThreadsCount; j++)
                {
                    threads.Add(new Thread(delegate () { Task(i, count++, parallelDataMatrix); }));
                    threads[j].Start();
                }
                for (int j = 0; j < Program.ThreadsCount; j++)
                {
                    threads[j].Join();
                }

                threads.Clear();
            }

            Console.WriteLine($"Time parallel: {(DateTime.Now - timerStart).TotalMilliseconds}");

            timerStart = DateTime.Now;

            for (int i = 0; i < RowsNumber; i++)
            {
                for (int j = 0; j < ColumnsNumber; j++)
                {
                    singleThreadDataMatrix[i, j] = GetExactResult(i * T, j * H);
                }
            }

            Console.WriteLine($"Time one thread: {(DateTime.Now - timerStart).TotalMilliseconds}");

            File.WriteAllText("outputSingleThread.txt", "");

            for (int i = 0; i < RowsNumber; i++)
            {
                for (int j = 0; j < ColumnsNumber; j++)
                {
                    File.AppendAllText("outputSingleThread.txt",
                        singleThreadDataMatrix[i, j].ToString("0.00000") + " ");
                }
                File.AppendAllText("outputSingleThread.txt", "\n");
            }


            File.WriteAllText("outputParallel.txt", "");

            for (int i = 0; i < RowsNumber; i++)
            {
                for (int j = 0; j < ColumnsNumber; j++)
                {
                    File.AppendAllText("outputParallel.txt",
                        parallelDataMatrix[i, j].ToString("0.00000") + " ");
                }
                File.AppendAllText("outputParallel.txt", "\n");
            }
        }

        public static void CalculateDeviations()
        {
            double absolute = -1;
            int t_i = 0;
            int t_j = 0;
            double relative;

            for (int i = 0; i < RowsNumber; i++)
            {
                for (int j = 0; j < ColumnsNumber; j++)
                {
                    double temp_val = 
                        Math.Abs(parallelDataMatrix[i, j] - singleThreadDataMatrix[i, j]);

                    if (temp_val > absolute)
                    {
                        absolute = temp_val;

                        t_i = i;
                        t_j = j;

                    }
                }
            }

            Console.WriteLine($"Absolute deviation: {absolute}");

            relative = absolute / singleThreadDataMatrix[t_i, t_j];
            Console.WriteLine($"Relative deviation: {relative}");
        }
    }
}
