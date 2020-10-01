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

        public int TotalNumberOfGroups { get; set; }

        public Cinema(Dictionary<int, int> groups, int[,] seats, int width, int height)
        {
            Groups = groups;
            Seats = seats;
            Width = width;
            Height = height;
            TotalNumberOfGroups = Groups.Sum(kv => kv.Value);
        }

        public int[] GetGroupsAsArray()
        {
            var result = new List<int>(TotalNumberOfGroups);

            for (int i = 1; i < 9; i++)
            {
                for (int j = 0; j < Groups[i]; j++)
                {
                    result.Add(i);
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
                else
                {
                    throw new Exception("Cannot seate a group at a 0 position");
                }
            }
        }

        public bool VerifyCinema()
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
            }

            return seatedGroups;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Width);
            builder.AppendLine();

            builder.Append(Height);
            builder.AppendLine();

            for (int i = 0; i < Seats.GetLength(0); i++)
            {
                for (int j = 0; j < Seats.GetLength(1); j++)
                {
                    builder.Append(Seats[i, j]);
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
    }
}
