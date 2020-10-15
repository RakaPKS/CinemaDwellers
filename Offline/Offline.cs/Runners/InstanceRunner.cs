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
            var totalTime = Utils.TimeAction(() =>
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
                        Console.WriteLine("Solving via Greedy only");
                    }

                    times = solver.Solve();
                }
                else if (Options.ILPOnly)
                {
                    var solver = new ILPSolver(cinema);

                    if (Options.Debug)
                    {
                        Console.WriteLine("Solving via ILP only");
                    }

                    times = solver.Solve(Options.Tune, Options.Debug, Options.TuneOutputFile, Options.InstanceConfig.ConfigFile);
                }
                else
                {
                    if (Options.Debug)
                    {
                        Console.WriteLine("Solving via both");
                    }

                    var greedyCinema = CinemaReader.Read(Options.InstanceConfig.InstanceFile);
                    var greedySolver = new GreedySolver(greedyCinema);

                    times = greedySolver.Solve();

                    if (cinema.TotalNumberOfPeople != greedyCinema.CountSeated())
                    {
                        var ilpCinema = CinemaReader.Read(Options.InstanceConfig.InstanceFile);
                        var ilpSolver = new ILPSolver(ilpCinema);

                        times = ilpSolver.Solve(Options.Tune, Options.Debug, Options.TuneOutputFile, Options.InstanceConfig.ConfigFile);

                        cinema = ilpCinema;
                    }
                    else
                    {
                        cinema = greedyCinema;
                    }
                }

                Console.WriteLine(cinema);
                Console.WriteLine("People seated:" + cinema.CountSeated() + " out of " + cinema.TotalNumberOfPeople);
                Console.WriteLine($"Valid cinema:{cinema.Verify()}");
            }, "Time");

            Console.WriteLine($"Total Solving Time: {totalTime}");
        }
    }
}
