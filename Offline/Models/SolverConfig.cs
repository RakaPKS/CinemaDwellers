using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Offline
{
    public class SolverConfig
    {
        //The MIPFocus parameter allows you to modify your high-level solution strategy, depending on your goals.
        //By default, the Gurobi MIP solver strikes a balance between finding new feasible solutions and proving 
        //that the current solution is optimal.If you are more interested in finding feasible solutions quickly, 
        //you can select MIPFocus=1. If you believe the solver is having no trouble finding good quality solutions, 
        //and wish to focus more attention on proving optimality, select MIPFocus = 2.If the best objective bound is 
        //moving very slowly(or not at all), you may want to try MIPFocus=3 to focus on the bound.
        public int MIPFocus { get; set; } = 0;

        //Controls the amount of fill allowed during presolve aggregation.Larger values generally 
        //lead to presolved models with fewer rows and columns, but with more constraint matrix non-zeros.
        public int AggFill { get; set; } = -1;

        //Controls the presolve level. A value of -1 corresponds to an automatic setting.Other options 
        //are off (0), conservative(1), or aggressive(2). More aggressive application of presolve takes 
        //more time, but can sometimes lead to a significantly tighter model.
        public int Presolve { get; set; } = -1;

        // Algorithm used to solve continuous models or the root node of a MIP model.
        // Options are: -1=automatic, 0=primal simplex, 1=dual simplex, 2=barrier, 3=concurrent, 4=deterministic concurrent, 5=deterministic concurrent simplex.
        // In the current release, the default Automatic (-1) setting will typically choose non-deterministic concurrent(Method= 3) 
        // for an LP, barrier (Method = 2) for a QP or QCP, and dual(Method= 1) for the MIP root node.Only the simplex and barrier algorithms 
        // are available for continuous QP models. Only primal and dual simplex are available for solving the root of an MIQP model.Only barrier is available for continuous QCP models.
        // Concurrent optimizers run multiple solvers on multiple threads simultaneously, and choose the one that finishes first.Method= 3 and Method = 4 will run dual simplex, barrier,
        // and sometimes primal simplex (depending on the number of available threads). Method=5 will run both primal and dual simplex.The deterministic options(Method= 4 and Method = 5) 
        // give the exact same result each time, while Method=3 is often faster but can produce different optimal bases when run multiple times.
        public int Method { get; set; } = -1;

        //Limits the number of nodes explored by MIP-based heuristics(such as RINS). 
        //Exploring more nodes can produce better solutions, but it generally takes longer.
        public int SubMIPNodes { get; set; } = 500;

        // Controls the presolve sparsify reduction. This reduction can sometimes significantly 
        //reduce the number of nonzero values in the presolved model. Value 0 shuts off the reduction, 
        // while value 1 forces it on. The default value of -1 chooses automatically.
        public int PreSparsify { get; set; } = -1;

        //Limits the number of passes performed by presolve.The default setting (-1) chooses the number of passes automatically.
        //You should experiment with this parameter when you find that presolve is consuming a large fraction of total solve time.
        public int PrePasses { get; set; } = -1;

        //Controls Mixed Integer Rounding(MIR) cut generation.Use 0 to disable these cuts, 1 
        //for moderate cut generation, or 2 for aggressive cut generation.
        //The default -1 value chooses automatically. Overrides the Cuts parameter.
        public int MIRCuts { get; set; } = -1;

        //Global cut aggressiveness setting. Use value 0 to shut off cuts, 1 for moderate cut generation, 
        //2 for aggressive cut generation, and 3 for very aggressive cut generation. 
        //This parameter is overridden by the parameters that control individual cut types (e.g., CliqueCuts).
        public int Cuts { get; set; } = -1;

        //Controls flow cover cut generation. Use 0 to disable these cuts, 1 for moderate cut generation, or 
        //2 for aggressive cut generation. The default -1 value chooses automatically. Overrides the Cuts parameter.
        public int FlowCoverCuts { get; set; } = -1;
        public int Tune { get; set; } = 0;

        public static SolverConfig Parse(string filepath)
        {
            var paramaters = typeof(SolverConfig).GetProperties().Select(p => p.Name);
            string pattern = @"([a-zA-Z]*)\s*=\s*(\d);";
            var config = new SolverConfig();

            using (StreamReader reader = File.OpenText(filepath))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    Match m = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

                    if (m.Success)
                    {
                        var parameterName = paramaters
                            .FirstOrDefault(p => p.ToLower() == m.Groups[1].Value.ToLower()); 

                        if (!string.IsNullOrWhiteSpace(parameterName))
                        {
                            var parameterValue = int.Parse(m.Groups[2].Value);
                            PropertyInfo propertyInfo = config.GetType().GetProperty(parameterName);
                            propertyInfo.SetValue(config, parameterValue);
                        }
                        else
                        {
                            throw new Exception($"Invalid parameter: {parameterName} in config: {filepath}");
                        }
                    }
                }
            }

            return config;
        }
    }
}
