using Offline.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Offline
{
    public class CinemaReader
    {
        public static Cinema Read(string filepath)
        {
            var groups = new Dictionary<int, int>(8);
            int[,] seats = null;
            int height = 0;
            int width = 0;

            using (StreamReader reader = File.OpenText(filepath))
            {
                height = int.Parse(reader.ReadLine());
                width = int.Parse(reader.ReadLine());

                seats = new int[width, height];

                for (int i = 0; i < height; i++)
                {
                    string line = reader.ReadLine();
                    for (int j = 0; j < width; j++)
                    {
                        seats[j, i] = line[j] - 48;
                    }
                }

                for (int groupSize = 1; groupSize < 9; groupSize++)
                {
                    var count = reader.Read() - 48;
                    reader.Read();
                    groups.Add(groupSize, count);
                }
            }

            return new Cinema(groups, seats, width, height);
        }
    }
}
