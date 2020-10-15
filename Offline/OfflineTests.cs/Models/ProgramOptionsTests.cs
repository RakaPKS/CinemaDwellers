using Microsoft.VisualStudio.TestTools.UnitTesting;
using Offline.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Offline.Models.Tests
{
    [TestClass()]
    public class ProgramOptionsTests
    {
        [TestMethod()]
        public void ParseTest()
        {
            var instanceArgs = new string[]
            {
                "-d",
                "-m",
                "instance",
                "-i",
                "./instance.txt",
                "-c",
                "./config.txt",
                "-ilpo",
                "-go",
                "-t",
                "-to",
                "./tune.txt"
            };

            var options = ProgramOptions.Parse(instanceArgs);

            Assert.AreEqual(ProgramMode.Instance, options.Mode);
            Assert.IsTrue(options.Debug);
            Assert.IsTrue(options.ILPOnly);
            Assert.IsTrue(options.GreedyOnly);
            Assert.IsTrue(options.Tune);
            Assert.AreEqual("./tune.txt", options.TuneOutputFile);

            Assert.AreEqual("./instance.txt", options.InstanceConfig.InstanceFile);
            Assert.AreEqual("./config.txt", options.InstanceConfig.ConfigFile);

            var experimentsArgs = new string[]
            {
                "-d",
                "-m",
                "experiments",
                "-e",
                "1",
                "-e",
                "2",
                "-e",
                "3",
                "-i",
                "./instance/",
                "-c",
                "./config/",
            };

            options = ProgramOptions.Parse(experimentsArgs);

            Assert.AreEqual(ProgramMode.Experiments, options.Mode);
            Assert.IsTrue(options.Debug);

            Assert.AreEqual("./instance/", options.ExperimentsConfig.InstancesFolder);
            Assert.AreEqual("./config/", options.ExperimentsConfig.ConfigsFolder);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, options.ExperimentsConfig.Ids);

        }
    }
}