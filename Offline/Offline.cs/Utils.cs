using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Offline.Models; 

namespace Offline
{
    public class Utils
    {
        
        public static double[] GenerateArrayOfOnes(int count)
        {
            var result = new double[count];

            for (int i = 0; i < count; i++)
            {
                result[i] = 1.0;
            }

            return result;
        }

        public enum SeatingResult
        {
            HorizontalViolation = 1,
            VerticalViolation = 2,
            DiagnolViolation = 3,
            NoViolation = 4
        }

        public static SeatingResult AreTwoSeatedGroupsValid(int x1, int y1, int x2, int y2, int s1, int s2)
        {
            if (x1 == x2 && y1 == y2)
            {
                return SeatingResult.HorizontalViolation;
            }

            if (y1 == y2)
            {
                if (x1 < x2)
                {
                    if (x2 - (x1 + (s1 - 1)) <= 2)
                    {
                        return SeatingResult.HorizontalViolation;
                    }
                }
                else if (x1 > x2)
                {
                    if (x1 - (x2 + (s2 - 1)) <= 2)
                    {
                        return SeatingResult.HorizontalViolation;
                    }
                }
            }
            else if (x1 == x2)
            {
                if (Math.Abs(y2 - y1) < 2)
                {
                    return SeatingResult.VerticalViolation;
                }
            }
            else if (Math.Abs(y1 - y2) == 1)
            {
                if (x1 < x2)
                {
                    if (x2 - (x1 + (s1 - 1)) <= 1)
                    {
                        return SeatingResult.DiagnolViolation;
                    }
                }
                else if (x1 > x2)
                {
                    if (x1 - (x2 + (s2 - 1)) <= 1)
                    {
                        return SeatingResult.DiagnolViolation;
                    }
                }
            }

            return SeatingResult.NoViolation;
        }

        public static string TimeAction(Action action, string actionName)
        {
            var timer = new Stopwatch();
            timer.Start();
            action();
            timer.Stop();

            var ts = timer.Elapsed;

            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            return elapsedTime;
        }

        public static (T, string) TimeFunction<T>(Func<T> func, string functionName)
        {
            var timer = new Stopwatch();
            timer.Start();
            var result = func();
            timer.Stop();

            var ts = timer.Elapsed;

            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            return (result, elapsedTime);
        }

        public static T[] GetColumn<T>(T[,] matrix, int columnNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(0))
                    .Select(x => matrix[x, columnNumber])
                    .ToArray();
        }

        public static T[] GetRow<T>(T[,] matrix, int rowNumber)
        {
            return Enumerable.Range(0, matrix.GetLength(1))
                    .Select(x => matrix[rowNumber, x])
                    .ToArray();
        }
    }
}
