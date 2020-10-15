using System;
using System.Collections.Generic;
using System.Text;

namespace Offline.Models
{
    public class SolveResult
    {
        public string InstanceName { get; set; }
        public string ConfigName { get; set; }
        public bool Valid { get; set; }
        public string Seated { get; set; }
        public string AddConstraintsTime { get; set; }
        public string OptimizeTime { get; set; }

        public SolveResult(string instanceName,
            string configName, bool valid, int seated, 
            int totalNumberOfPeople, string constraintsTime, string optimizeTime)
        {
            InstanceName = instanceName;
            ConfigName = configName;
            Valid = valid;
            Seated = $"{seated}/{totalNumberOfPeople}";
            AddConstraintsTime = constraintsTime;
            OptimizeTime = optimizeTime;
        }
    }
}
