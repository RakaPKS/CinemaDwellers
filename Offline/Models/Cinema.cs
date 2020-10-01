using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Offline.Models
{
    public class Cinema
    {
        public Dictionary<int, int> Groups { get; set; }
        public int[,] Seats { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public bool[,,] AvailableSeats { get; set; }
        public int TotalNumberOfGroups { get; set; }
        public int TotalNumberOfPeople { get; private set; }
        public int[] GroupSizes { get; set; }

        public (int, int)[][] LegalStartPositions { get; private set; }

        public Cinema(Dictionary<int, int> groups, int[,] seats, int width, int height)
        {
            Seats = seats;
            Width = width;
            Height = height;
            CalculateAvailableSeats();

            Groups = groups; //new Dictionary<int, int>();

            // Filter groups that don't fit as pre-processing step 
            // foreach (var g in groups)
            // {
            //     if (GetLegalStartingPositions(g.Key - 1).Length > 0)
            //     {
            //         Groups[g.Key] = g.Value;
            //     }
            // }

            TotalNumberOfGroups = Groups.Sum(kv => kv.Value);
            GroupSizes = GetGroupsAsArray();
            TotalNumberOfPeople = GroupSizes.Sum(); 

            // Initialize legal start positions for each possible group size 
            LegalStartPositions = new (int, int)[8][];
            for (int g = 0; g < 8; g++)
            {
                LegalStartPositions[g] = GetLegalStartingPositions(g);
            }


        }

        public void CalculateAvailableSeats()
        {
            AvailableSeats = new bool[Width, Height, 8];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x==0 & y ==1)
                    {
                        Console.WriteLine("Debug time");
                    }
                    // Store how many available we seats we have starting from this point
                    int upTo = 7;
                    for (int i = 0; i < 8; i++)
                    {
                        if (x + i >= Width || Seats[x + i, y] == 0)
                        {
                            // If upto=0 i cannot seat anyone
                            // If upto=1 i can seat a group of 1 
                            upTo = i;
                            break;
                        }
                    }
                    // Use 
                    for (int groupSize = 1; groupSize <= 8; groupSize++)
                    {
                        if (upTo >= groupSize)
                        {
                            AvailableSeats[x, y, groupSize - 1] = true;
                        }
                    }
                }
            }
        }


        public (int, int)[] GetLegalStartingPositions(int group_size)
        {
            var result = new List<(int, int)>();
            var seats = Seats;

            for (int row = 0; row < Width; row++)
            {
                for (int col = 0; col < Height; col++)
                {
                    if (AvailableSeats[row, col, group_size])
                    {
                        result.Add((row, col));
                    }
                }

            }
            return result.ToArray();
        }

        public void SeatGroup(int startX, int startY, int groupSize)
        {
            for (int x = startX; x <= startX + (groupSize - 1); x++)
            {
                if (Seats[x, startY] == 1)
                {
                    Seats[x, startY] = 2;
                }
                else if (Seats[x, startY] == 0)
                {
                    throw new Exception("Cannot seat a group at a 0 position");
                }
            }
        }

        public bool Verify()
        {
            var seatedGroups = FindAllSeatedGroups();

            foreach (var seatedGroup1 in seatedGroups)
            {
                (var x1, var y1) = seatedGroup1.Key;
                var s1 = seatedGroup1.Value;

                foreach (var seatedGroup2 in seatedGroups)
                {
                    (var x2, var y2) = seatedGroup2.Key;
                    var s2 = seatedGroup2.Value;

                    if (!(x1 == x2 && y1 == y2))
                    {
                        var validationResult = Utils.AreTwoSeatedGroupsValid(x1, y1, x2, y2, s1, s2);

                        if (validationResult != Utils.SeatingResult.NoViolation)
                        {
                            return false;
                        }
                    }

                }
            }

            return true;
        }

        private Dictionary<(int, int), int> FindAllSeatedGroups()
        {
            var firstPerson = (-1, -1);
            var groupSize = 0;
            var seatedGroups = new Dictionary<(int, int), int>();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (Seats[x, y] == 2)
                    {
                        if (firstPerson == (-1, -1))
                        {
                            firstPerson = (x, y);
                            groupSize++;
                        }
                        else
                        {
                            groupSize++;
                        }
                    }
                    else
                    {
                        if (firstPerson != (-1, -1))
                        {
                            seatedGroups[firstPerson] = groupSize;
                            groupSize = 0;
                            firstPerson = (-1, -1);
                        }
                    }
                }

                if (firstPerson != (-1, -1))
                {
                    seatedGroups[firstPerson] = groupSize;
                    groupSize = 0;
                    firstPerson = (-1, -1);
                }
            }

            return seatedGroups;
        }

        public int countSeated()
        {
            int res = 0;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    res += Seats[i, j] == 2 ? 1 : 0;
            return res;
        }

        

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Width);
            builder.AppendLine();

            builder.Append(Height);
            builder.AppendLine();

            for (int i = 0; i < Seats.GetLength(1); i++)
            {
                for (int j = 0; j < Seats.GetLength(0); j++)
                {
                    builder.Append(Seats[j, i]);
                }
                builder.AppendLine();
            }

            foreach (var group in Groups)
            {
                builder.Append(group.Value);
                builder.Append(" ");
            }

            return builder.ToString();
        }

        private int[] GetGroupsAsArray()
        {
            var result = new List<int>(TotalNumberOfGroups);

            for (int i = 1; i < 9; i++)
            {
                if (Groups.ContainsKey(i))
                {
                    for (int j = 0; j < Groups[i]; j++)
                    {
                        result.Add(i);
                    }

                }

            }

            return result.ToArray();
        }

    }
}
