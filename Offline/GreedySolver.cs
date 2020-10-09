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
    public class GreedySolver
    {
        private Cinema Cinema { get; set; }

        public GreedySolver(Cinema cinema)
        {
            Cinema = cinema;
        }

        public Cinema Solve()
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
                    foreach (var pos in positions)
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
                            if (r.Next(10) <= 1){
                                best_amt = amt; 
                                best_pos = pos; 
                            }
                        }
                    }
                    if (best_pos.Item1 > -1)
                    {

                        //Cinema.SeatGroup(best_pos.Item1, best_pos.Item2, gz);
                        Cinema.CountBad(best_pos.Item1, best_pos.Item2, gz, true);
                        //Console.WriteLine(Cinema);
                        Cinema.CalculateAvailableSeats();
                        Cinema.UpdateLegalStartPositions();
                        //Console.WriteLine($"Seating group {group_no} of size {gz} at {best_pos.Item1}, {best_pos.Item2}");
                    }
                }
            }
            return Cinema;
        }
    }
}