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

        public Cinema(Dictionary<int, int> groups, int[,] seats, int width, int height)
        {
            Groups = groups;
            Seats = seats;
            Width = width;
            Height = height;
        }

        public int GetTotalNumberOfGroups()
        {
            return Groups.Sum(kv => kv.Value);
        }

        public int[] GetGroupsAsArray()
        {
            var temp = new List<int>(GetTotalNumberOfGroups());

            for (int i = 1; i < 9; i++)
            {
                for (int j = 0; j < Groups[i]; j++)
                {
                    temp.Add(i);
                }
            }

            return temp.ToArray();
        }

        public void SeatGroup(int startX, int startY, int groupSize)
        {
            for (int x = startX; x <= groupSize; x++)
            {
                Seats[x, startY] = 2;
            }
        }

        public bool VerifyCinema()
        {
            var firstPerson = (-1, -1);
            var groupSize = 0;
            var seatedGroups = new Dictionary<(int, int), int>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
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

            return false;
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
