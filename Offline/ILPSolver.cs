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
                var totalNumberOfGroups = Cinema.GetTotalNumberOfGroups();
                // Create an empty environment, set options and start
                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", "mip1.log");
                env.Start();

                // Create empty model
                GRBModel model = new GRBModel(env);

                var seated = model.AddVars(Cinema.Width * Cinema.Height * totalNumberOfGroups, GRB.BINARY);
                var sizes = model.AddVars(totalNumberOfGroups, GRB.INTEGER);

                Console.WriteLine(seated.Length);

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
    }
}
