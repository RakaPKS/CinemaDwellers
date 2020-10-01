using Gurobi;
using Offline.Models;
using System;

namespace Offline
{
    public class ILPSolver
    {
        private Cinema Cinema { get; set; }

        public ILPSolver(Cinema cinema)
        {
            Cinema = cinema;
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

                var grbSeated = AddSeatedBinaryVariables(model);

                Console.WriteLine(grbSeated.Length);

                // Optimize model
                model.Optimize();

                Console.WriteLine("Obj: " + model.ObjVal);

                // Dispose of model and env
                model.Dispose();
                env.Dispose();

            }
            catch (GRBException e)
            {
                Console.WriteLine("Error code: " + e.ErrorCode + ". " + e.Message);
            }
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
    }
}
