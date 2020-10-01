using System;
using System.IO;

namespace Offline
{
    class Program
    {
        static void Main(string[] args)
        {
            var cinema = CinemaReader.Read(Path.GetFullPath(@"..\..\..\..\instances\") + "30.txt");
            Console.WriteLine(cinema);
            var ilpSolver = new ILPSolver(cinema);
            ilpSolver.Solve();
        }
    }
}
