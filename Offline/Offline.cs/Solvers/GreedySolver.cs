using Offline.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Offline
{
    public class GreedySolver
    {
        private Cinema Cinema { get; set; }

        public GreedySolver(Cinema cinema)
        {
            Cinema = cinema;
        }

        public Dictionary<string, string> Solve()
        {
            var times = new Dictionary<string, string>();

            var solveTime = Utils.TimeAction(() =>
            {
                Random r = new Random();
                for (int gz = 8; gz >= 1; gz--)
                {
                    if (!Cinema.Groups.ContainsKey(gz)) continue;
                    for (int group_no = 0; group_no < Cinema.Groups[gz]; group_no++)
                    {
                        var positions = Cinema.LegalStartPositions[gz - 1];
                        int best_amt = 1000 * 1000;
                        var best_pos = (-1, -1);

                        Parallel.ForEach(positions, pos =>
                        {
                            {
                                int amt = Cinema.CountBad(pos.Item1, pos.Item2, gz);
                                if (amt < best_amt)
                                {
                                    best_amt = amt;
                                    best_pos = pos;
                                }
                                else if (amt == best_amt)
                                {
                                    // Random choice
                                    if (r.Next(10) <= 1)
                                    {
                                        best_amt = amt;
                                        best_pos = pos;
                                    }
                                }
                            }
                        });

                        if (best_pos.Item1 > -1)
                        {

                            Cinema.CountBad(best_pos.Item1, best_pos.Item2, gz, true);
                            Cinema.CalculateAvailableSeats();
                            Cinema.UpdateLegalStartPositions();
                        }
                    }
                }
            }, "Solve Time");

            times.Add("Total", solveTime);

            return times;
        }
    }
}