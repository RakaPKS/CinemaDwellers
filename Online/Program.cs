using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;

namespace newOnline
{
    class Program
    {
        static void Main(string[] args)
        {
            int h = int.Parse(Console.ReadLine());
            int w = int.Parse(Console.ReadLine());
            var cinema = new int[w, h];
            for (int i = 0; i < h; i++)
            {
                string line = Console.ReadLine();
                int len = line.Length;
                for (int j = 0; j < w; j++)
                {
                    cinema[j, i] = line[j] - 48;
                }
            }

            var people = new List<int>();
            int nextGroup;
            while ((nextGroup = Console.Read() - 48) != 0)
            {
                Console.Read();
                people.Add(nextGroup);
            }

            var seatData = initializeSeatData(cinema, w, h);

            for (int i = 0; i < people.Count; i++)
            {
                (int x, int y) = findBestPos(seatData, people[i]);
                if ((x, y) != (-1, -1))
                {
                    placeGroup(cinema, x, y, people[i]);
                    updateSeatData(cinema, seatData, x, y, people[i]);
                }
            }

            printCinema(cinema);

            int totalPeople = people.Sum();

            Console.WriteLine("Seated " + countSeated(cinema) + " people out of " + totalPeople + " total people.");

            Console.ReadLine();
        }

        static int[][,] initializeSeatData(int[,] cinema, int w, int h)
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

        static void updateSeatData(int[,] cinema, int[][,] seatData, int x, int y, int groupSize)
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

        static int countDisabledSeats(int[,] cinema, int x, int y, int groupSize)
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

        static bool doesGroupFit(int[,] cinema, int x, int y, int groupSize)
        {
            for (int i = 0; i < groupSize; i++)
                if (x + i >= cinema.GetLength(0) || cinema[x + i, y] != 1)
                    return false;
            return true;
        }

        static (int, int) findBestPos(int[][,] seatData, int groupSize)
        {

            int noThreads, noPortThreads;

            ThreadPool.GetMaxThreads(out noThreads, out noPortThreads);

            noThreads = Math.Min(noThreads, 8);// seatData[groupSize - 1].GetLength(1));

            var threads = new Thread[noThreads];

            var threadResults = new ((int, int), int)[noThreads];

            for (int k = 0; k < noThreads; k++)
            {
                threadResults[k] = ((-1, -1), int.MaxValue);
                int index = k;
                threads[k] = new Thread(() => {
                    for (int j = (seatData[groupSize - 1].GetLength(1) / (noThreads)) * k ; j < (seatData[groupSize - 1].GetLength(1) / noThreads) * (k + 1) && j < seatData[groupSize - 1].GetLength(1); j++)
                        for (int i = 0; i < seatData[groupSize - 1].GetLength(0); i++)
                        {
                            if (seatData[groupSize - 1][i, j] != -1 && seatData[groupSize - 1][i, j] < threadResults[index].Item2)
                            {
                                threadResults[index] = ((i, j), seatData[groupSize - 1][i, j]);
                            }
                        }
                });
            }

            for (int k = 0; k < noThreads; k++)
            {
                threads[k].Start();
            }

            for (int k = 0; k < noThreads; k++)
            {
                threads[k].Join();
            }

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

        static void placeGroup(int[,] cinema, int x, int y, int groupSize)
        {
            for (int i = -1; i < groupSize + 1; i++)
                for (int j = -1; j < 2; j++)
                    if (inRange(cinema, x + i, y + j))

                        cinema[x + i, y + j] = 0;

            if (inRange(cinema, x - 2, y))
                cinema[x - 2, y] = 0;

            if (inRange(cinema, x + groupSize + 1, y))
                cinema[x + groupSize + 1, y] = 0;

            for (int i = 0; i < groupSize; i++)
                if (inRange(cinema, x + i, y))
                    cinema[x + i, y] = 2;
        }

        static int countSeated(int[,] cinema)
        {
            int res = 0;
            for (int i = 0; i < cinema.GetLength(0); i++)
                for (int j = 0; j < cinema.GetLength(1); j++)
                    res += cinema[i, j] == 2 ? 1 : 0;
            return res;
        }

        static void printCinema(int[,] cinema)
        {
            var res = "";

            for (int j = 0; j < cinema.GetLength(1); j++)
            {
                for (int i = 0; i < cinema.GetLength(0); i++)
                {
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

        static bool inRange(int[,] cinema, int x, int y)
        {
            bool res = x >= 0 && x < cinema.GetLength(0) && y >= 0 && y < cinema.GetLength(1);
            return res;
        }
    }
}