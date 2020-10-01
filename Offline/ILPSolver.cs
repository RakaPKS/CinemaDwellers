using Gurobi;
using Offline.Models;
using System;
using System.Diagnostics;
using System.Threading;

namespace Offline
{
    public class ILPSolver
    {
        private Cinema Cinema { get; set; }

        private double[] GroupSizes { get; set; }

        public ILPSolver(Cinema cinema)
        {
            Cinema = cinema;
            GroupSizes = Cinema.GetGroupsAsArray();
        }

        public void Solve()
        {
            try
            {
                // Create an empty environment, set options and start
                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", "mip1.log");
                env.Start();

                // Create empty model
                GRBModel model = new GRBModel(env);

                var grbSeated = Utils.TimeFunction(() => AddSeatedBinaryVariables(model), "Add Decision Variables");

                Utils.TimeAction(() => AddContraints(model, grbSeated), "Add Constraints");

                Utils.TimeAction(() => AddObjective(model, grbSeated), "Add Objective");

                Utils.TimeAction(() => model.Optimize(), "Optimizing");

                SeatGroups(grbSeated);

                Console.WriteLine(Cinema);
                Console.WriteLine(Cinema.Verify());

                // Dispose of model and env
                model.Dispose();
                env.Dispose();

            }
            catch (GRBException e)
            {
                Console.WriteLine("Error code: " + e.ErrorCode + ". " + e.Message);
            }
        }

        private void AddContraints(GRBModel model, GRBVar[,,] seated)
        {
            AddOnlyOnePositionPerGroupConstraint(model, seated);

            AddDoNotSeatOutOfBoundsConstraint(model, seated);

            AddDistanceConstraints(model, seated);
        }

        private void AddDistanceConstraints(GRBModel model, GRBVar[,,] seated)
        {
            for (int g1 = 0; g1 < Cinema.TotalNumberOfGroups; g1++)
            {
                for (int g2 = 0; g2 < Cinema.TotalNumberOfGroups; g2++)
                {
                    if (g1 < g2)
                    {
                        for (int x1 = 0; x1 < Cinema.Width; x1++)
                        {
                            for (int y1 = 0; y1 < Cinema.Height; y1++)
                            {
                                for (int x2 = 0; x2 < Cinema.Width; x2++)
                                {
                                    for (int y2 = 0; y2 < Cinema.Height; y2++)
                                    {
                                        if (Utils.AreTwoSeatedGroupsValid(x1, y1, x2, y2, (int)GroupSizes[g1], (int)GroupSizes[g2]) != Utils.SeatingResult.NoViolation)
                                        {
                                            model.AddConstr(seated[x1, y1, g1] + seated[x2, y2, g2], GRB.LESS_EQUAL, 1, "Distance constaint");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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

                        for (int i = 0; i < (int)GroupSizes[g]; i++)
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
                        expr.AddTerm(GroupSizes[g], seated[x, y, g]);
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
                            Cinema.SeatGroup(x, y, (int)GroupSizes[g]);
                        }
                    }
                }
            }
        }
    }
}
