﻿using CommandLine;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Offline.Models
{
    public enum ProgramMode
    {
        Experiments,
        Instance
    }

    public class ExperimentsConfig
    {
        public List<int> Ids { get; set; }

        public ExperimentsConfig()
        {
            Ids = new List<int>();
        }
    }

    public class InstanceConfig
    {
        public string InstanceFile { get; set; }
        public string ConfigFile { get; set; }
    }

    public class ProgramOptions
    {
        public bool Debug { get; set; } = false;
        public bool Tune { get; set; } = false;
        public string TuneOutputFile { get; set; }
        public bool GreedyOnly { get; set; } = false;
        public bool ILPOnly { get; set; } = false;
        public ProgramMode Mode { get; set; } = ProgramMode.Instance;
        public ExperimentsConfig ExperimentsConfig { get; set; }
        public InstanceConfig InstanceConfig { get; set; }

        public ProgramOptions()
        {
            ExperimentsConfig = new ExperimentsConfig();
            InstanceConfig = new InstanceConfig();
        }

        public static ProgramOptions Parse(string[] args)
        {
            var programOptions = new ProgramOptions();

            var options = new OptionSet {
                { "d|debug", "run the program in debug mode", d => programOptions.Debug = d != null },
                { "m|mode=", "the mode the program should run in: experiments for experiments | instance (default) for single instance.", m => programOptions.Mode = Enum.Parse<ProgramMode>(m, true) },
                { "e|exp=", "the id of experiment", (int id) => programOptions.ExperimentsConfig.Ids.Add(id)},
                { "i|instance=", "path to a instance file executed by this program", i => programOptions.InstanceConfig.InstanceFile = i},
                { "c|config=", "path to config file (.pgm) executed by this program", c => programOptions.InstanceConfig.ConfigFile = c},
                { "to|tuneOutputFile=", "path to output file in which the best tune parameters are stored", to => programOptions.TuneOutputFile = to},
                { "t|tune", "tune the ILP solver to determine best parameters", t => programOptions.Tune = t != null},
                { "go|greedyOnly", "only use the greedy to solve the instance", go => programOptions.GreedyOnly = go != null},
                { "ilpo|ilpOnly", "only use ilp to solve the instance", ilpo => programOptions.ILPOnly = ilpo != null}

            };

            options.Parse(args);

            return programOptions;
        }
    }
}
