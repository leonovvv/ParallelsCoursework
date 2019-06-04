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
        public static int ThreadsCount = 4;

        static void Main(string[] args)
        {
            //CalculationsExplicit.Process();
            CalculationsImplicit.Process();
            Console.ReadKey();
        }
    }
}
