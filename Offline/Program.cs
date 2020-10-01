using System;

namespace Offline
{
    class Program
    {
        static void Main(string[] args)
        {
            var cinema = CinemaReader.Read("C:\\Users\\Julia\\Desktop\\uni\\MADS\\project\\instances\\instance3.txt");
            Console.WriteLine(cinema);
            var ilpSolver = new ILPSolver(cinema);
            ilpSolver.Solve();
        }
    }
}
