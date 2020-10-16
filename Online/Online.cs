using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace Online
{
    class Program
    {
        /// <summary>
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">Arguments. Unused.</param>
        static void Main(string[] args)
        {
            // Our program uses multi-threading, which is incredibly slow in the debug mode of Visual Studio.
            // Run the program using ctrl+F5 to run outside of the debugger for the actual performance speed.

            for (int i = 0; i < 18; i++)
            {
                Console.WriteLine("Start of test Online" + (i + 1) + ".");
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var reader = new StreamReader("..\\..\\..\\TestCases\\Online" + (i + 1) + ".txt");
                var cinema = readCinema(reader);

                var people = readPeople(reader);

                var solver = new Solver(cinema, people);

                solver.Solve();

                Solver.printCinema(cinema);

                int totalPeople = people.Sum();
                stopwatch.Stop();
                Console.WriteLine("Seated " + Solver.countSeated(cinema) + " people out of " + totalPeople + " total people, taking " + stopwatch.Elapsed + ".");
            }
        }

        /// <summary>
        /// Read in the cinema using the given reader object.
        /// </summary>
        /// <returns>Returns the cinema in an n * m matrix.</returns>
        static int[,] readCinema(TextReader reader)
        {
            int h = int.Parse(reader.ReadLine());
            int w = int.Parse(reader.ReadLine());
            var cinema = new int[w, h];
            for (int i = 0; i < h; i++)
            {
                string line = reader.ReadLine();
                for (int j = 0; j < w; j++)
                {
                    cinema[j, i] = line[j] - 48;
                }
            }

            return cinema;
        }

        /// <summary>
        /// Read in the people using the given reader object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns>Returns a list of integers, representing the people visiting the cinema.</returns>
        static List<int> readPeople(TextReader reader)
        {
            var people = new List<int>();
            int nextGroup = reader.Read() - 48;
            while (nextGroup != 0)
            {
                people.Add(nextGroup);
                nextGroup = reader.Read() - 48;
                while (nextGroup < 0 || nextGroup > 9)
                    nextGroup = reader.Read() - 48;
            }
            reader.Close();
            return people;
        }

    }

    public class Solver
    {
        public int[,] cinema;
        public List<int> people;

        /// <summary>
        /// Initialize class data.
        /// </summary>
        /// <param name="cinema">2d matrix of the cinema.</param>
        /// <param name="people">People to seat in the cinema.</param>
        public Solver(int[,] cinema, List<int> people)
        {
            this.cinema = cinema;
            this.people = people;
        }

        /// <summary>
        /// Run the solver. Modifies the cinema object of this class to be the result.
        /// </summary>
        public void Solve()
        {
            // Initialize a matrix containing all the possible seating costs, so we can modify it, instead of having to recalculate it every time.
            var seatData = initializeSeatData(cinema, cinema.GetLength(0), cinema.GetLength(1));

            for (int i = 0; i < people.Count; i++)
            {
                (int x, int y) = findBestPos(seatData, people[i]);
                if ((x, y) != (-1, -1))
                {
                    placeGroup(cinema, x, y, people[i]);
                    updateSeatData(cinema, seatData, x, y, people[i]);
                }
            }
        }

        /// <summary>
        /// Calculate what seating a group of every size in every location costs.
        /// </summary>
        /// <param name="cinema">cinema object to run these calculations on.</param>
        /// <param name="w">width of the cinema</param>
        /// <param name="h">height of the cinema</param>
        /// <returns>Returns a list of 2d matrices of size 8. Every matrix contains the cost for every seat to seat a group of (index +1) there.</returns>
        private int[][,] initializeSeatData(int[,] cinema, int w, int h)
        {
            int[][,] result = new int[8][,];
            for (int i = 0; i < 8; i++)
                result[i] = new int[w, h];

            for (int k = 0; k < 8; k++)
                for (int j = 0; j < h; j++)
                    for (int i = 0; i < w; i++)
                        result[k][i, j] = countDisabledSeats(cinema, i, j, k + 1);

            return result;
        }

        /// <summary>
        /// Update the seating cost matrix when placing a group of groupSize at (x, y).
        /// </summary>
        /// <param name="cinema">Cinema to seat people in.</param>
        /// <param name="seatData">Matrix to update</param>
        /// <param name="x">X location of seated group.</param>
        /// <param name="y">Y location of seated group.</param>
        /// <param name="groupSize">Size of seated group.</param>
        private void updateSeatData(int[,] cinema, int[][,] seatData, int x, int y, int groupSize)
        {
            for (int k = 0; k < 8; k++)
                for (int j = -2; j < 3; j++)
                    for (int i = -(4 + k); i < groupSize + 4; i++)
                    {
                        bool outsideBar = (i <= -(3 + k) || i >= groupSize + 2) && (Math.Abs(j) == 2);
                        bool insidePixel = Math.Abs(j) == 1 && (i == -(4 + k) || i == groupSize + 3);

                        if (outsideBar || insidePixel)
                            continue;

                        if (inRange(cinema, x + i, y + j))
                            seatData[k][x + i, y + j] = countDisabledSeats(cinema, x + i, y + j, k + 1);
                    }
        }

        /// <summary>
        /// Count how many seats will be disabled if we place a group of size groupSize at (x,y).
        /// </summary>
        /// <param name="cinema">Cinema to seat people in.</param>
        /// <param name="x">X location of seated group.</param>
        /// <param name="y">Y location of seated group.</param>
        /// <param name="groupSize">Size of seated group.</param>
        /// <returns>Returns how many seats will be disabled if we place the group here. Returns -1 if the group does not fit.</returns>
        private int countDisabledSeats(int[,] cinema, int x, int y, int groupSize)
        {
            int result = 0;

            if (!doesGroupFit(cinema, x, y, groupSize))
                return -1;

            for (int j = -1; j < 2; j++)
            {
                for (int i = -1; i < groupSize + 1; i++)
                {
                    if (inRange(cinema, x + i, y + j) && cinema[x + i, y + j] == 1)
                        result++;
                }
            }
            result += (inRange(cinema, x - 2, y) && cinema[x - 2, y] == 1) ? 1 : 0;
            result += (inRange(cinema, x + groupSize + 1, y) && cinema[x + groupSize + 1, y] == 1) ? 1 : 0;

            return (result == 0) ? -1 : result;
        }

        /// <summary>
        /// Can you fit a group of size groupSize at (x,y)?
        /// </summary>
        /// <param name="cinema">Cinema to seat people in.</param>
        /// <param name="x">X position to seat group.</param>
        /// <param name="y">Y position to seat group.</param>
        /// <param name="groupSize">Size of group to seat.</param>
        /// <returns></returns>
        private bool doesGroupFit(int[,] cinema, int x, int y, int groupSize)
        {
            for (int i = 0; i < groupSize; i++)
                if (x + i >= cinema.GetLength(0) || cinema[x + i, y] != 1)
                    return false;
            return true;
        }

        /// <summary>
        /// Find the best position to seat a group of size groupSize in the cinema.
        /// </summary>
        /// <param name="seatData">Array of 2d matrices containing seating cost data.</param>
        /// <param name="groupSize">Size of group to seat.</param>
        /// <returns>(x,y) tuple of where the place the group. If the tuple is (-1, -1), the group cannot be seated.</returns>
        private (int, int) findBestPos(int[][,] seatData, int groupSize)
        {
            // setting up multi-threading
            int noThreads, noPortThreads;

            ThreadPool.GetMaxThreads(out noThreads, out noPortThreads);

            noThreads = Math.Min(noThreads, 16);

            var threads = new Thread[noThreads];

            var threadResults = new ((int, int), int)[noThreads];

            // create the threads
            for (int k = 0; k < noThreads; k++)
            {
                int index = k;
                threadResults[index] = ((-1, -1), int.MaxValue);

                threads[k] = new Thread(() => {
                    for (int j = ((seatData[groupSize - 1].GetLength(1) / noThreads) + 1) * index; j < ((seatData[groupSize - 1].GetLength(1) / noThreads) + 1) * (index + 1) && j < seatData[groupSize - 1].GetLength(1); j++)
                        for (int i = 0; i < seatData[groupSize - 1].GetLength(0); i++)
                        {
                            if (seatData[groupSize - 1][i, j] != -1 && seatData[groupSize - 1][i, j] < threadResults[index].Item2)
                            {
                                threadResults[index] = ((i, j), seatData[groupSize - 1][i, j]);
                            }
                        }
                });
            }

            // run the threads
            for (int k = 0; k < noThreads; k++)
            {
                threads[k].Start();
            }

            // wait for the threads to finish
            for (int k = 0; k < noThreads; k++)
            {
                threads[k].Join();
            }

            // join the results to find the best value of the matrix.
            (var result, var value) = ((-1, -1), int.MaxValue);

            for (int i = 0; i < noThreads; i++)
            {
                if (threadResults[i].Item2 < value)
                {
                    result = threadResults[i].Item1;
                    value = threadResults[i].Item2;
                }

            }

            return result;
        }

        /// <summary>
        /// Place a group of size groupSize at (x,y). Updates cinema.
        /// </summary>
        /// <param name="cinema">Cinema to place group in.</param>
        /// <param name="x">X position to place group.</param>
        /// <param name="y">Y position to place group.</param>
        /// <param name="groupSize">Size of group to place.</param>
        private void placeGroup(int[,] cinema, int x, int y, int groupSize)
        {
            // disable all the seats around the group
            for (int i = -1; i < groupSize + 1; i++)
                for (int j = -1; j < 2; j++)
                    if (inRange(cinema, x + i, y + j))
                        cinema[x + i, y + j] = 0;

            if (inRange(cinema, x - 2, y))
                cinema[x - 2, y] = 0;

            if (inRange(cinema, x + groupSize + 1, y))
                cinema[x + groupSize + 1, y] = 0;

            // place the group
            for (int i = 0; i < groupSize; i++)
                if (inRange(cinema, x + i, y))
                    cinema[x + i, y] = 2;
        }

        /// <summary>
        /// Count how many people are seated in cinema.
        /// </summary>
        /// <param name="cinema">Cinema to count seated people.</param>
        /// <returns>Amount of people seated as an integer.</returns>
        public static int countSeated(int[,] cinema)
        {
            int res = 0;
            for (int i = 0; i < cinema.GetLength(0); i++)
                for (int j = 0; j < cinema.GetLength(1); j++)
                    res += cinema[i, j] == 2 ? 1 : 0;
            return res;
        }

        /// <summary>
        /// Print a cinema to the screen.
        /// </summary>
        /// <param name="cinema">Cinema to print.</param>
        public static void printCinema(int[,] cinema)
        {
            var res = "";

            for (int j = 0; j < cinema.GetLength(1); j++)
            {
                for (int i = 0; i < cinema.GetLength(0); i++)
                {
                    // make sure all entries are the same length, for a pretty cinema
                    if (cinema[i, j] == -1)
                        res += " -1";
                    else if (cinema[i, j] >= 10)
                        res += " " + cinema[i, j];
                    else
                        res += "  " + cinema[i, j];
                }
                Console.WriteLine(res);
                res = "";
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Checks if (x,y) is in range of the matrix cinema.
        /// </summary>
        /// <param name="cinema">Matrix to check if (x,y) is in range.</param>
        /// <param name="x">X position to check if is in range.</param>
        /// <param name="y">Y position to check if is in range.</param>
        /// <returns></returns>
        public static bool inRange(int[,] cinema, int x, int y)
        {
            bool res = x >= 0 && x < cinema.GetLength(0) && y >= 0 && y < cinema.GetLength(1);
            return res;
        }
    }
}