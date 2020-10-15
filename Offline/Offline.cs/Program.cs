using CsvHelper;
using Offline.Models;
using Offline.Runners;

namespace Offline
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ProgramOptions.Parse(args);

            if (options.Mode == ProgramMode.Experiments)
            {
                var runner = new ExperimentRunner(options);
                runner.Run();
            }
            else
            {
                var runner = new InstanceRunner(options);
                runner.Run();

            }
        }
    }
}
