using Gurobi;
using Offline.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Offline
{
    public class ILPSolver
    {
        private Cinema Cinema { get; set; }

        public ILPSolver(Cinema cinema)
        {
            Cinema = cinema;
        }

        public Dictionary<string, string> Solve(bool tune, bool debug, string tuneOutputFile = null, string paramFile = null)
        {
            var times = new Dictionary<string, string>();

            var totalTime = Utils.TimeAction(() =>
            {
                try
                {
                    // Create an empty environment, set options and start
                    GRBEnv env = new GRBEnv(true);

                    // Use gurobi's parameter reader to load the best file
                    if (paramFile != null)
                    {
                        env.ReadParams(paramFile);
                    }

                    if (!debug)
                    {
                        env.Set(GRB.IntParam.OutputFlag, 0);
                    }

                    env.Start();

                    // Create empty model
                    GRBModel model = new GRBModel(env);

                    (var grbSeated, var addDecisionVariableTime) = Utils.TimeFunction(() => AddSeatedBinaryVariables(model), "Add Decision Variables");

                    var addConstraintsTime = Utils.TimeAction(() => AddContraints(model, grbSeated), "Add Constraints");

                    var addObjectiveTime = Utils.TimeAction(() => AddObjective(model, grbSeated), "Add Objective");

                    var optimizeTime = Utils.TimeAction(() => model.Optimize(), "Optimizing");

                    if (tune)
                    {
                        model.Tune();

                        model.GetTuneResult(0);
                        model.Write(tuneOutputFile);
                    }

                    SeatGroups(grbSeated);

                    // Dispose of model and env
                    model.Dispose();
                    env.Dispose();

                    times.Add("Add Decision Variables", addDecisionVariableTime);
                    times.Add("Add Constraints", addConstraintsTime);
                    times.Add("Add Objective", addObjectiveTime);
                    times.Add("Optimizing", optimizeTime);
                }
                catch (GRBException e)
                {
                    Console.WriteLine("Error code: " + e.ErrorCode + ". " + e.Message);
                    throw e;
                }
            }, "Total");

            times.Add("Total", totalTime);

            return times;
        }

        private void AddContraints(GRBModel model, GRBVar[,,] seated)
        {
            AddOnlyOnePositionPerGroupConstraint(model, seated);

            AddDoNotSeatOutOfBoundsConstraint(model, seated);

            AddDistanceConstraintsParallel(model, seated);
        }

        private void AddDistanceConstraints(GRBModel model, GRBVar[,,] seated)
        {
            for (int g1 = 0; g1 < Cinema.TotalNumberOfGroups; g1++)
            {
                for (int g2 = 0; g2 < Cinema.TotalNumberOfGroups; g2++)
                {
                    if (g1 < g2)
                    {
                        var size1 = Cinema.GroupSizes[g1];
                        var size2 = Cinema.GroupSizes[g2];
                        // Loop over all legal start positions for group 1 
                        foreach (var pos1 in Cinema.LegalStartPositions[size1 - 1])
                        {
                            var x1 = pos1.Item1;
                            var y1 = pos1.Item2;

                            // Collect invalid seats 
                            var invalidSeats = Cinema.GetInvalidSeats(x1, y1, size1, size2);
                            foreach (var pos2 in invalidSeats)
                            {
                                if (Cinema.LegalStartPositions[size2 - 1].Any(m => m == pos2))
                                {
                                    var x2 = pos2.Item1;
                                    var y2 = pos2.Item2;

                                    model.AddConstr(seated[x1, y1, g1] + seated[x2, y2, g2], GRB.LESS_EQUAL, 1, "Distance constaint");
                                }
                            }

                        }
                    }
                }
            }
        }

        private void AddDistanceConstraintsParallel(GRBModel model, GRBVar[,,] seated)
        {
            ConcurrentBag<GRBLinExpr> constraints = new ConcurrentBag<GRBLinExpr>();

            Parallel.For(0, Cinema.TotalNumberOfGroups, g1 =>
            {
                {
                    for (int g2 = 0; g2 < Cinema.TotalNumberOfGroups; g2++)
                    {
                        if (g1 < g2)
                        {
                            var size1 = Cinema.GroupSizes[g1];
                            var size2 = Cinema.GroupSizes[g2];
                            // Loop over all legal start positions for group 1 
                            foreach (var pos1 in Cinema.LegalStartPositions[size1 - 1])
                            {
                                var x1 = pos1.Item1;
                                var y1 = pos1.Item2;

                                // Collect invalid seats 
                                var invalidSeats = Cinema.GetInvalidSeats(x1, y1, size1, size2);
                                foreach (var pos2 in invalidSeats)
                                {
                                    if (Cinema.LegalStartPositions[size2 - 1].Any(m => m == pos2))
                                    {
                                        var x2 = pos2.Item1;
                                        var y2 = pos2.Item2;

                                        constraints.Add(new GRBLinExpr(seated[x1, y1, g1] + seated[x2, y2, g2]));
                                    }
                                }

                            }
                        }
                    }
                }
            });

            foreach (var constraint in constraints)
            {
                model.AddConstr(constraint, GRB.LESS_EQUAL, 1, "Distance constaint");
            }
        }

        private void AddDoNotSeatOutOfBoundsConstraint(GRBModel model, GRBVar[,,] seated)
        {
            for (int x = 0; x < Cinema.Width; x++)
            {
                for (int y = 0; y < Cinema.Height; y++)
                {
                    for (int g = 0; g < Cinema.TotalNumberOfGroups; g++)
                    {
                        var anyZeroes = false;

                        for (int i = 0; i < Cinema.GroupSizes[g]; i++)
                        {
                            if (x + i >= Cinema.Width || Cinema.Seats[x + i, y] == 0)
                            {
                                anyZeroes = true;
                                break;
                            }
                        }

                        if (anyZeroes)
                        {
                            model.AddConstr(seated[x, y, g], GRB.LESS_EQUAL, 0, "Out of bounds constraint");
                        }
                    }
                }
            }
        }

        private void AddObjective(GRBModel model, GRBVar[,,] seated)
        {
            var expr = new GRBLinExpr();

            for (int x = 0; x < Cinema.Width; x++)
            {
                for (int y = 0; y < Cinema.Height; y++)
                {
                    for (int g = 0; g < Cinema.TotalNumberOfGroups; g++)
                    {
                        expr.AddTerm(Cinema.GroupSizes[g], seated[x, y, g]);
                    }
                }
            }

            model.SetObjective(expr, GRB.MAXIMIZE);
        }

        private GRBVar[,,] AddSeatedBinaryVariables(GRBModel model)
        {
            var grbSeated = new GRBVar[Cinema.Width, Cinema.Height, Cinema.TotalNumberOfGroups];

            for (int x = 0; x < Cinema.Width; x++)
            {
                for (int y = 0; y < Cinema.Height; y++)
                {
                    for (int g = 0; g < Cinema.TotalNumberOfGroups; g++)
                    {
                        grbSeated[x, y, g] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "seated" + x + y + g);
                    }
                }
            }

            return grbSeated;
        }

        private void AddOnlyOnePositionPerGroupConstraint(GRBModel model, GRBVar[,,] seated)
        {
            var coeff = Utils.GenerateArrayOfOnes(Cinema.Width * Cinema.Height);

            for (int g = 0; g < Cinema.TotalNumberOfGroups; g++)
            {
                var result = new GRBVar[Cinema.Width * Cinema.Height];
                var resultCount = 0;

                for (int x = 0; x < Cinema.Width; x++)
                {
                    for (int y = 0; y < Cinema.Height; y++)
                    {
                        result[resultCount] = seated[x, y, g];
                        resultCount++;
                    }
                }

                var expr = new GRBLinExpr();
                expr.AddTerms(coeff, result);

                model.AddConstr(expr, GRB.LESS_EQUAL, 1, "Only One Position Per Group Constraint");
            }
        }

        private void SeatGroups(GRBVar[,,] seated)
        {
            for (int x = 0; x < Cinema.Width; x++)
            {
                for (int y = 0; y < Cinema.Height; y++)
                {
                    for (int g = 0; g < Cinema.TotalNumberOfGroups; g++)
                    {
                        if (seated[x, y, g].X > 0)
                        {
                            Cinema.SeatGroup(x, y, Cinema.GroupSizes[g]);
                        }
                    }
                }
            }
        }
    }
}
