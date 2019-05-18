using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelCursework
{

    class Program
    {
        public static int ThreadsCount = 1;
        public static int ChunkSize = Calculations.ColumnsNumber / ThreadsCount;

        

        static void Main(string[] args)
        {
            List<Thread> threads = new List<Thread>();
            double[,] dataMatrixParallel = new double[Calculations.RowsNumber, Calculations.ColumnsNumber];
            double[,] dataMatrixOneThread = new double[Calculations.RowsNumber, Calculations.ColumnsNumber];

            var pTimerStart = DateTime.Now;

            if (ThreadsCount > Calculations.ColumnsNumber)
            {
                ThreadsCount = Calculations.ColumnsNumber;
                ChunkSize = 1;
            }

            DateTime timerStart = DateTime.Now;

            Parallel.For(0, Calculations.ColumnsNumber, i =>
            {
                dataMatrixParallel[0, i] = Calculations.GetExactResult(0, Calculations.H * i);
            });

            Parallel.For(0, Calculations.RowsNumber, i =>
            {
                dataMatrixParallel[i, 0] = Calculations.GetExactResult(Calculations.T * i, 0);
                dataMatrixParallel[i, Calculations.ColumnsNumber - 1] = 
                Calculations.GetExactResult(Calculations.T * i, (Calculations.ColumnsNumber - 1) * Calculations.H);
            });

            for (int i = 1; i < Calculations.RowsNumber; i++)
            {
                int count = 0;

                for (int j = 0; j < ThreadsCount; j++)
                {
                    threads.Add(new Thread(delegate () { Task(i, count++, dataMatrixParallel); }));
                    threads[j].Start();
                }
                for (int j = 0; j < ThreadsCount; j++)
                {
                    threads[j].Join();
                }

                threads.Clear();
            }

            Console.WriteLine($"Parallel time: {(DateTime.Now - timerStart).TotalMilliseconds}");

            using (var writer = new StreamWriter("output_approximate.txt"))
            {
                for (int i = 0; i < Calculations.RowsNumber; i++)
                {
                    for (int j = 0; j < Calculations.ColumnsNumber; j++)
                    {
                        writer.Write(dataMatrixParallel[i, j].ToString("0.00000") + " ");
                    }
                    writer.Write(Environment.NewLine);
                }
            }

            timerStart = DateTime.Now;

            for (int i = 0; i < Calculations.RowsNumber; i++)
            {
                for (int j = 0; j < Calculations.ColumnsNumber; j++)
                {
                    dataMatrixOneThread[i, j] =
                        Calculations.GetExactResult(Calculations.T * i, Calculations.H * j);
                }
            }

            Console.WriteLine($"One thread time: {(DateTime.Now - timerStart).TotalMilliseconds}");

            using (var writer = new StreamWriter("output_exact.txt"))
            {
                for (int i = 0; i < Calculations.RowsNumber; i++)
                {
                    for (int j = 0; j < Calculations.ColumnsNumber; j++)
                    {
                        writer.Write(dataMatrixOneThread[i, j].ToString("0.00000") + " ");
                    }
                    writer.Write(Environment.NewLine);
                }
            }

            Deviations(dataMatrixOneThread, dataMatrixParallel, Calculations.RowsNumber, Calculations.ColumnsNumber);

            Console.ReadLine();
        }

        public static void Task(int row, int index, double[,] matrix)
        {
            int indexFrom;
            int indexTo;

            if (index == ThreadsCount - 1)
            {
                indexFrom = index * ChunkSize;
                indexTo = Calculations.ColumnsNumber - 1;
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
                Calculations.GetApproximateResult(matrix, row, j);
            }
        }

        static void Deviations(
            double[,] matrix1,
            double[,] matrix2,
            int rowsNumber, int columnsNumber)
        {
            double absolute = double.MinValue;
            int iTemp = 0;
            int jTemp = 0;

            for (int i = 0; i < rowsNumber; i++)
            {
                for (int j = 0; j < columnsNumber; j++)
                {
                    double temp_val = Math.Abs(matrix1[i,j] - matrix2[i,j]);

                    if (temp_val > absolute)
                    {
                        absolute = temp_val;

                        iTemp = i;
                        jTemp = j;

                    }
                }
            }

            Console.WriteLine($"Absolute deviation: {absolute}");
            Console.WriteLine($"Relative deviation: {absolute / matrix2[iTemp, jTemp]}");
        }
    }
}
