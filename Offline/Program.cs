using CsvHelper;
using Offline.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Offline
{
    class Program
    {
        static void Main(string[] args)
        {
            var debug = true;
            var runAllInstances = true;
            var greedy = true;

            var instanceFolder = Path.GetFullPath(@"../../../instances/");
            var configFolder = Path.GetFullPath(@"../../../configs/");
            var resultFolder = Path.GetFullPath(@"../../../results/");
            var tuneResultFolder = Path.GetFullPath(@"../../../tune_results/");


            if (!Directory.Exists(instanceFolder))
            {
                Directory.CreateDirectory(instanceFolder);
            }

            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }

            if (!Directory.Exists(resultFolder))
            {
                Directory.CreateDirectory(resultFolder);
            }

            if (!Directory.Exists(tuneResultFolder))
            {
                Directory.CreateDirectory(tuneResultFolder);
            }
            var numberOfInstances = Directory.GetFiles(instanceFolder).Length;
            var records = new List<SolveResult>();

            if (runAllInstances)
            {
                for (int i = 20; i <= 21; i++) //numberOfInstances; i++)
                {
                    var instanceName = $"Exact{i}";
                    var configName = $"tune_Exact{i}_0.prm";
                    var configPath = configFolder + configName;

                    Console.WriteLine($"Solving {instanceName}");

                    var config = SolverConfig.Parse(configFolder + "config_default" + ".txt"); ;

                    if (!File.Exists(configPath))
                    {
                        configPath = configFolder + "config_default" + ".txt";
                        Console.WriteLine("Loaded default config.");
                    }

                    var cinema = CinemaReader.Read(instanceFolder + instanceName + ".txt");



                    if (greedy)
                    {
                        var greedysolver = new GreedySolver(cinema);
                        var optimizeTime = Utils.TimeAction(() => greedysolver.Solve(), "Optimizing");
                        //var result = 
                        //var greedySeatedCinema = result;
                        //Console.WriteLine(cinema);
                        if (!cinema.Verify()) { throw new Exception("Cinema was not valid!!!!");}
                        //Console.WriteLine($"RunTime for {instanceName}: " + optimizeTime);
                        Console.WriteLine(cinema.countSeated() + ", " + optimizeTime);
                        continue;
                    }

                    var solver = new ILPSolver(cinema, config, instanceName);


                    if (debug)
                    {
                        Console.WriteLine(cinema);
                    }

                    Cinema seatedCinema = null;
                    Dictionary<string, string> times = null;

                    try
                    {
                        (seatedCinema, times) = solver.Solve(configPath);
                    }
                    catch (Exception e)
                    {
                        using (var writer = new StreamWriter($"{resultFolder}result_{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.csv"))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(records);
                        }

                        throw e;
                    }

                    if (debug)
                    {
                        Console.WriteLine(seatedCinema);
                        Console.WriteLine("People seated:" + seatedCinema.countSeated() + " out of " + seatedCinema.TotalNumberOfPeople);
                        Console.WriteLine($"Valid cinema:{seatedCinema.Verify()}");

                        Console.WriteLine("Times:");

                        foreach (var time in times)
                        {
                            Console.WriteLine($"RunTime for {time.Key}: " + time.Value);
                        }
                    }

                    var record = new SolveResult(instanceName, configName, seatedCinema.Verify(),
                        seatedCinema.countSeated(), seatedCinema.TotalNumberOfPeople,
                        times["Add Constraints"], times["Optimizing"]);

                    records.Add(record);

                    Console.WriteLine($"Done!");
                }

                using (var writer = new StreamWriter($"{resultFolder}result_{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(records);
                }
            }
            else
            {
                int instance = 10;
                var configName = $"tune_Exact{instance}_0.prm";
                var instanceName = $"Exact{instance}";

                Console.WriteLine($"Solving {instanceName}");

                var config = SolverConfig.Parse(configFolder + "config_default" + ".txt"); ;

                if (File.Exists(configFolder + configName))
                {
                    config = SolverConfig.Parse(configFolder + configName + ".txt");
                    Console.WriteLine("Using custom config from file", configName);
                }

                var cinema = CinemaReader.Read(instanceFolder + instanceName + ".txt");

                var solver = new ILPSolver(cinema, config, instanceName);

                if (debug)
                {
                    Console.WriteLine(cinema);
                }

                (var seatedCinema, var times) = solver.Solve(configFolder + configName);

                if (debug)
                {
                    Console.WriteLine(seatedCinema);
                    Console.WriteLine("People seated:" + seatedCinema.countSeated() + " out of " + seatedCinema.TotalNumberOfPeople);
                    Console.WriteLine($"Valid cinema:{seatedCinema.Verify()}");

                    Console.WriteLine("Times:");

                    foreach (var time in times)
                    {
                        Console.WriteLine($"RunTime for {time.Key}: " + time.Value);
                    }
                }
            }
        }
    }
}
