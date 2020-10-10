using Offline.Models;
using System;
using System.Collections.Generic;

namespace Offline.Runners
{
    public class InstanceRunner
    {
        private ProgramOptions Options { get; set; }

        public InstanceRunner(ProgramOptions options)
        {
            Options = options;
        }

        public void Run()
        {
            var cinema = CinemaReader.Read(Options.InstanceConfig.InstanceFile);
            Dictionary<string, string> times = new Dictionary<string, string>();

            Console.WriteLine("Solving: " + Options.InstanceConfig.InstanceFile);
            Console.WriteLine(cinema);

            if (Options.GreedyOnly)
            {
                var solver = new GreedySolver(cinema);

                if (Options.Debug)
                {
                    Console.WriteLine("Solving via Greedy");
                }

                times = solver.Solve();
            }
            else if (Options.ILPOnly)
            {
                var solver = new ILPSolver(cinema);

                if (Options.Debug)
                {
                    Console.WriteLine("Solving via ILP");
                }


                times = solver.Solve(Options.Tune, Options.Debug, Options.TuneOutputFile, Options.InstanceConfig.ConfigFile);
            }
            else
            {
                // implement option to run both
                return;
            }

            Console.WriteLine(cinema);
            Console.WriteLine("People seated:" + cinema.CountSeated() + " out of " + cinema.TotalNumberOfPeople);
            Console.WriteLine($"Valid cinema:{cinema.Verify()}");

            foreach (var time in times)
            {
                Console.WriteLine($"RunTime for {time.Key}: " + time.Value);
            }
        }
    }
}
