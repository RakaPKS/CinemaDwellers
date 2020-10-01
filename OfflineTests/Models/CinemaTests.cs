using Offline.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Xunit.Sdk;

namespace Offline.Models.Tests
{
    [TestClass]
    public class CinemaTests
    {
        [TestMethod]
        public void GetTotalNumberOfGroupsTest()
        {
            var groups = new Dictionary<int, int>
            {
                {1, 7},
                {2, 7},
                {3, 7},
                {4, 4},
                {5, 4},
                {6, 3},
                {7, 1},
                {8, 0},
            };

            var cinema = new Cinema(groups, null, 0, 0);

            Assert.AreEqual(33, cinema.GetTotalNumberOfGroups());
        }

        [TestMethod]
        public void GetGroupAsArrayTest()
        {
            var groups = new Dictionary<int, int>
            {
                {1, 7},
                {2, 7},
                {3, 7},
                {4, 4},
                {5, 4},
                {6, 3},
                {7, 1},
                {8, 0},
            };

            var cinema = new Cinema(groups, null, 0, 0);

            var expectedArray = new int[] { 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 7 };

            CollectionAssert.AreEqual(expectedArray, cinema.GetGroupsAsArray());
        }
    }
}