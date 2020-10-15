using CsvHelper;
using Offline.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Offline.Runners
{
    public class ExperimentRunner
    {
        private ProgramOptions Options { get; set; }

        private int NumberOfInstances { get; set; }
        private const string InstancesFolder = @"../../../instances/";
        private const string ConfigsFolder = @"../../../configs/";
        private const string ResultsFolder = @"../../../results/";
        private const string TuneFolder = @"../../../tune/";

        public ExperimentRunner(ProgramOptions options)
        {
            Options = options;
            NumberOfInstances = Directory.GetFiles(InstancesFolder).Length;
        }

        public void Run()
        {
            foreach (var experimentId in Options.ExperimentsConfig.Ids)
            {
                if (experimentId == 1)
                {
                    RunTuneVsNoTune();
                }
                else if (experimentId == 2)
                {
                    RunGreedyVsNoGreedy();
                }
                else if (experimentId == 3)
                {
                    RunTuning();
                }
                else if (experimentId == 4)
                {
                    RunNormalSolver();
                }
                else if (experimentId == 5)
                {
                    RunTryGreedyFirst();
                }
            }
        }

        private void RunTryGreedyFirst()
        {
            var resultsFile = $"{ResultsFolder}greedy_first_{DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss")}.csv";

            using (var writer = new StreamWriter(resultsFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader(typeof(TryGreedyFirstResult));
                csv.NextRecord();
            }

            for (int i = 1; i <= NumberOfInstances; i++)
            {
                using (var stream = File.Open(resultsFile, FileMode.Append))
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.HasHeaderRecord = false;

                    var instanceFile = $"{InstancesFolder}/Exact{i}.txt";
                    var configFile = $"{ConfigsFolder}tune_Exact{i}_0.prm";

                    var cinema = CinemaReader.Read(instanceFile);

                    var greedyCinema = CinemaReader.Read(instanceFile);
                    var greedySolver = new GreedySolver(greedyCinema);

                    var totalTime = Utils.TimeAction(() =>
                    {
                        var times = greedySolver.Solve();

                        if (cinema.TotalNumberOfPeople != greedyCinema.CountSeated())
                        {
                            var ilpCinema = CinemaReader.Read(instanceFile);
                            var ilpSolver = new ILPSolver(ilpCinema);

                            times = ilpSolver.Solve(Options.Tune, Options.Debug, Options.TuneOutputFile, configFile);

                            cinema = ilpCinema;
                        }
                        else
                        {
                            cinema = greedyCinema;
                        }
                    }, "Time");

                    var result = new TryGreedyFirstResult
                    {
                        InstanceFile = instanceFile,
                        ConfigFile = configFile,

                        TotalNumberOfGroups = cinema.TotalNumberOfGroups,
                        TotalNumberOfPeople = cinema.TotalNumberOfPeople,

                        TotalTime = totalTime,

                        Seated = cinema.CountSeated(),
                        Valid = cinema.Verify(),
                        Capacity = cinema.InitialCapacity
                    };

                    csv.WriteRecord(result);
                    csv.NextRecord();
                }
            }
        }

        private void RunNormalSolver()
        {
            var resultsFile = $"{ResultsFolder}normal_solver_{DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss")}.csv";

            using (var writer = new StreamWriter(resultsFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader(typeof(NormalILPSolverResult));
                csv.NextRecord();
            }

            for (int i = 1; i <= NumberOfInstances; i++)
            {
                using (var stream = File.Open(resultsFile, FileMode.Append))
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.HasHeaderRecord = false;

                    var instanceFile = $"{InstancesFolder}/Exact{i}.txt";
                    var configFile = $"{ConfigsFolder}tune_Exact{i}_0.prm";

                    var cinema = CinemaReader.Read(instanceFile);

                    var solver = new ILPSolver(cinema);
                    var times = solver.Solve(false, Options.Debug, paramFile: configFile);

                    var result = new NormalILPSolverResult
                    {
                        InstanceFile = instanceFile,
                        ConfigFile = configFile,

                        TotalNumberOfGroups = cinema.TotalNumberOfGroups,
                        TotalNumberOfPeople = cinema.TotalNumberOfPeople,

                        OptimizationTime = times["Optimizing"],
                        ConstraintTime = times["Add Constraints"],

                        Seated = cinema.CountSeated(),
                        Valid = cinema.Verify(),
                        Capacity = cinema.InitialCapacity
                    };

                    csv.WriteRecord(result);
                    csv.NextRecord();
                }
            }
        }

        private void RunTuneVsNoTune()
        {
            var resultsFile = $"{ResultsFolder}tune_vs_no_tune_{DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss")}.csv";

            using (var writer = new StreamWriter(resultsFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader(typeof(TuneVsNoTuneResult));
                csv.NextRecord();
            }

            for (int i = 1; i <= NumberOfInstances; i++)
            {
                using (var stream = File.Open(resultsFile, FileMode.Append))
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.HasHeaderRecord = false;

                    var instanceFile = $"{InstancesFolder}/Exact{i}.txt";
                    var configFile = $"{ConfigsFolder}tune_Exact{i}_0.prm";

                    var cinemaTuned = CinemaReader.Read(instanceFile);
                    var cinemaNotTuned = CinemaReader.Read(instanceFile); ;

                    var solverTuned = new ILPSolver(cinemaTuned);
                    var timesTuned = solverTuned.Solve(false, Options.Debug, paramFile: configFile);

                    var solverNotTuned = new ILPSolver(cinemaNotTuned);
                    var timesNotuned = solverNotTuned.Solve(false, Options.Debug);

                    var result = new TuneVsNoTuneResult
                    {
                        InstanceFile = instanceFile,
                        ConfigFileTuned = configFile,
                        TotalNumberOfGroups = cinemaTuned.TotalNumberOfGroups,
                        TotalNumberOfPeople = cinemaTuned.TotalNumberOfPeople,

                        OptimizationTimeNotTuned = timesNotuned["Optimizing"],
                        OptimizationTimeTuned = timesTuned["Optimizing"],

                        SeatedNotTuned = cinemaNotTuned.CountSeated(),
                        SeatedTuned = cinemaTuned.CountSeated(),

                        ValidNotTuned = cinemaNotTuned.Verify(),
                        ValidTuned = cinemaTuned.Verify()
                    };

                    csv.WriteRecord(result);
                    csv.NextRecord();
                }
            }
        }

        private void RunGreedyVsNoGreedy()
        {
            var resultsFile = $"{ResultsFolder}greedy_vs_ilp_{DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss")}.csv";

            using (var writer = new StreamWriter(resultsFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader(typeof(GreedyVsILPResult));
                csv.NextRecord();
            }

            for (int i = 1; i <= NumberOfInstances; i++)
            {
                using (var stream = File.Open(resultsFile, FileMode.Append))
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.HasHeaderRecord = false;

                    var instanceFile = $"{InstancesFolder}/Exact{i}.txt";
                    var configFile = $"{ConfigsFolder}tune_Exact{i}_0.prm";

                    var cinemaILP = CinemaReader.Read(instanceFile);
                    var cinemaGreedy = CinemaReader.Read(instanceFile); ;

                    var solverILP = new ILPSolver(cinemaILP);
                    var timesILP = solverILP.Solve(false, Options.Debug, paramFile: configFile);

                    var greedySolver = new GreedySolver(cinemaGreedy);
                    var timesGreedy = greedySolver.Solve();

                    var result = new GreedyVsILPResult
                    {
                        InstanceFile = instanceFile,
                        ConfigFile = configFile,
                        TotalNumberOfGroups = cinemaILP.TotalNumberOfGroups,
                        TotalNumberOfPeople = cinemaILP.TotalNumberOfPeople,

                        ILPTime = timesILP["Total"],
                        GreedyTime = timesGreedy["Total"],

                        SeatedGreedy = cinemaGreedy.CountSeated(),
                        SeatedILP = cinemaILP.CountSeated(),

                        ValidGreedy = cinemaGreedy.Verify(),
                        ValidILP = cinemaILP.Verify(),

                        Capacity = cinemaGreedy.InitialCapacity
                    };

                    csv.WriteRecord(result);
                    csv.NextRecord();
                }
            }
        }

        private void RunTuning()
        {
            var resultsFile = $"{ResultsFolder}greedy_vs_ilp_{DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss")}.csv";

            using (var writer = new StreamWriter(resultsFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteHeader(typeof(GreedyVsILPResult));
                csv.NextRecord();
            }

            for (int i = 1; i <= NumberOfInstances; i++)
            {
                var instanceFile = $"{InstancesFolder}/Exact{i}.txt";
                var configFile = $"{ConfigsFolder}tune_Exact{i}_0.prm";
                var tuneFile = $"{TuneFolder}tune_Exact{i}_0.prm";

                var cinemaILP = CinemaReader.Read(instanceFile);

                var solverILP = new ILPSolver(cinemaILP);
                solverILP.Solve(true, Options.Debug, tuneFile, configFile);
            }
        }
    }

    public class TuneVsNoTuneResult
    {
        public string InstanceFile { get; set; }
        public int TotalNumberOfGroups { get; set; }

        public string ConfigFileTuned { get; set; }

        public string OptimizationTimeTuned { get; set; }
        public string OptimizationTimeNotTuned { get; set; }

        public bool ValidTuned { get; set; }
        public bool ValidNotTuned { get; set; }

        public int SeatedTuned { get; set; }
        public int SeatedNotTuned { get; set; }
        public int TotalNumberOfPeople { get; set; }
    }

    public class GreedyVsILPResult
    {
        public string InstanceFile { get; set; }
        public int TotalNumberOfGroups { get; set; }
        public int TotalNumberOfPeople { get; internal set; }
        public string ConfigFile { get; set; }

        public string ILPTime { get; set; }
        public string GreedyTime { get; set; }

        public bool ValidILP { get; set; }
        public bool ValidGreedy { get; set; }

        public int SeatedILP { get; set; }
        public int SeatedGreedy { get; set; }

        public int Capacity { get; set; }

    }

    public class NormalILPSolverResult
    {
        public string InstanceFile { get; set; }

        public string ConfigFile { get; set; }

        public string OptimizationTime { get; set; }

        public string ConstraintTime { get; set; }

        public int TotalNumberOfGroups { get; set; }

        public int TotalNumberOfPeople { get; set; }

        public bool Valid { get; set; }

        public int Seated { get; set; }

        public int Capacity { get; set; }
    }

    public class TryGreedyFirstResult
    {
        public string InstanceFile { get; set; }

        public string ConfigFile { get; set; }

        public string TotalTime { get; set; }

        public int TotalNumberOfGroups { get; set; }

        public int TotalNumberOfPeople { get; set; }

        public bool Valid { get; set; }

        public int Seated { get; set; }

        public int Capacity { get; set; }
    }
}
