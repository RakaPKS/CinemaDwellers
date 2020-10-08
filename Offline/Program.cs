﻿using CsvHelper;
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
            var runAllInstances = false;

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
                for (int i = 1; i <= numberOfInstances; i++)
                {
                    var configName = $"config{i}";
                    var instanceName = $"Exact{i}";

                    Console.WriteLine($"Solving {instanceName}");

                    var config = SolverConfig.Parse(configFolder + "config_default" + ".txt"); ;

                    if (File.Exists(configFolder + configName + ".txt"))
                    {
                        config = SolverConfig.Parse(configFolder + configName + ".txt");
                    }

                    var cinema = CinemaReader.Read(instanceFolder + instanceName + ".txt");

                    var solver = new ILPSolver(cinema, config, instanceName);

                    if (debug)
                    {
                        Console.WriteLine(cinema);
                    }

                    Cinema seatedCinema = null;
                    Dictionary<string, string> times = null;

                    try
                    {
                        (seatedCinema, times) = solver.Solve();
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
                var configName = $"config2";
                var instanceName = $"Exact2";

                Console.WriteLine($"Solving {instanceName}");

                var config = SolverConfig.Parse(configFolder + "config_default" + ".txt"); ;

                if (File.Exists(configFolder + configName + ".txt"))
                {
                    config = SolverConfig.Parse(configFolder + configName + ".txt");
                }

                var cinema = CinemaReader.Read(instanceFolder + instanceName + ".txt");

                var solver = new ILPSolver(cinema, config, instanceName);

                if (debug)
                {
                    Console.WriteLine(cinema);
                }

                (var seatedCinema, var times) = solver.Solve();

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
