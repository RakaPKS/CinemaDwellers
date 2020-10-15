using Offline.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Xunit.Sdk;
using System.IO;
using System;
using System.Linq;

namespace Offline.Models.Tests
{
    [TestClass]
    public class CinemaTests
    {
        private Cinema Cinema { get; set; }

        [TestInitialize]
        public void InitializeCinema()
        {
            Cinema = CinemaReader.Read(Path.GetFullPath(@"../../../TestFiles/") + "test_instance.txt");
        }

        [TestMethod]
        public void GetTotalNumberOfGroupsTest()
        {
            Assert.AreEqual(33, Cinema.TotalNumberOfGroups);
        }

        [TestMethod]
        public void GetGroupAsArrayTest()
        {
            var expectedArray = new int[] { 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 7 };

            CollectionAssert.AreEqual(expectedArray, Cinema.GroupSizes);
        }

        [TestMethod]
        public void CalculateAvailableSeatsTest()
        {
            var expectedTrueAmount = 228;
            var amount_of_trues = 0;
            for (int i = 0; i < Cinema.AvailableSeats.GetLength(0); i++)
            {
                for (int j = 0; j < Cinema.AvailableSeats.GetLength(1); j++)
                {
                    if (Cinema.AvailableSeats[i, j, 0])
                    {
                        amount_of_trues++;

                    }
                    if (Cinema.Seats[i, j] == 1)
                    {
                        if (!Cinema.AvailableSeats[i, j, 0])
                        {
                            throw new Exception(i + " " + j);
                        }
                    }
                }
            }
            CollectionAssert.AllItemsAreUnique(Cinema.GetLegalStartingPositions(0));
            Assert.AreEqual(expectedTrueAmount, amount_of_trues);
        }

        [TestMethod]
        public void SeatGroupTest()
        {
            Cinema.SeatGroup(1, 0, 2);

            Assert.AreEqual(2, Cinema.Seats[1, 0]);
            Assert.AreEqual(2, Cinema.Seats[2, 0]);
            Assert.AreEqual(0, Cinema.Seats[3, 0]);

            Cinema.SeatGroup(4, 0, 2);

            Assert.AreEqual(2, Cinema.Seats[4, 0]);
            Assert.AreEqual(2, Cinema.Seats[5, 0]);
            Assert.AreEqual(1, Cinema.Seats[6, 0]);

            Assert.ThrowsException<Exception>(() => Cinema.SeatGroup(0, 0, 1));
        }

        [TestMethod]
        public void VerifyCinemaTest_Horizontal_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(4, 0, 2);

            Assert.IsFalse(Cinema.Verify());
        }

        [TestMethod]
        public void VerifyCinemaTest_Diagnol_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(3, 1, 2);

            Assert.IsFalse(Cinema.Verify());
        }

        [TestMethod]
        public void VerifyCinemaTest_Vertical_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(1, 1, 2);

            Assert.IsFalse(Cinema.Verify());
        }

        [TestMethod]
        public void VerifyCinemaTest_Horizontal_No_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(5, 0, 2);

            Assert.IsTrue(Cinema.Verify());
        }

        [TestMethod]
        public void VerifyCinemaTest_Vertical_No_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(1, 2, 2);

            Assert.IsTrue(Cinema.Verify());
        }

        [TestMethod]
        public void VerifyCinemaTest_Diagonal_No_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(4, 1, 2);

            Assert.IsTrue(Cinema.Verify());
        }

        [TestMethod()]
        public void GetInvalidSeatsTest()
        {
            // Top left corner
            var invalidSeats = Cinema.GetInvalidSeats(0, 0, 1, 1);

            Assert.AreEqual(5, invalidSeats.Length);

            CollectionAssert.Contains(invalidSeats, (0, 0));
            CollectionAssert.Contains(invalidSeats, (1, 0));
            CollectionAssert.Contains(invalidSeats, (2, 0));
            CollectionAssert.Contains(invalidSeats, (1, 1));
            CollectionAssert.Contains(invalidSeats, (0, 1));

            // Top right corner
            invalidSeats = Cinema.GetInvalidSeats(16, 0, 1, 1);

            Assert.AreEqual(5, invalidSeats.Length);

            CollectionAssert.Contains(invalidSeats, (16, 0));
            CollectionAssert.Contains(invalidSeats, (15, 0));
            CollectionAssert.Contains(invalidSeats, (14, 0));
            CollectionAssert.Contains(invalidSeats, (15, 1));
            CollectionAssert.Contains(invalidSeats, (16, 1));

            // Bottom left corner
            invalidSeats = Cinema.GetInvalidSeats(0, 16, 1, 1);

            Assert.AreEqual(5, invalidSeats.Length);

            CollectionAssert.Contains(invalidSeats, (0, 16));
            CollectionAssert.Contains(invalidSeats, (0, 15));
            CollectionAssert.Contains(invalidSeats, (1, 16));
            CollectionAssert.Contains(invalidSeats, (2, 16));
            CollectionAssert.Contains(invalidSeats, (1, 15));

            // Bottom right corner
            invalidSeats = Cinema.GetInvalidSeats(16, 16, 1, 1);

            Assert.AreEqual(5, invalidSeats.Length);

            CollectionAssert.Contains(invalidSeats, (16, 16));
            CollectionAssert.Contains(invalidSeats, (15, 16));
            CollectionAssert.Contains(invalidSeats, (14, 16));
            CollectionAssert.Contains(invalidSeats, (16, 15));
            CollectionAssert.Contains(invalidSeats, (15, 15));

            // Middle
            invalidSeats = Cinema.GetInvalidSeats(3, 3, 2, 2);

            Assert.AreEqual(17, invalidSeats.Length);

            CollectionAssert.Contains(invalidSeats, (0, 3));

            CollectionAssert.Contains(invalidSeats, (1, 2));
            CollectionAssert.Contains(invalidSeats, (1, 3));
            CollectionAssert.Contains(invalidSeats, (1, 4));

            CollectionAssert.Contains(invalidSeats, (2, 2));
            CollectionAssert.Contains(invalidSeats, (2, 3));
            CollectionAssert.Contains(invalidSeats, (2, 4));

            CollectionAssert.Contains(invalidSeats, (3, 2));
            CollectionAssert.Contains(invalidSeats, (3, 3));
            CollectionAssert.Contains(invalidSeats, (3, 4));

            CollectionAssert.Contains(invalidSeats, (4, 2));
            CollectionAssert.Contains(invalidSeats, (4, 3));
            CollectionAssert.Contains(invalidSeats, (4, 4));

            CollectionAssert.Contains(invalidSeats, (5, 2));
            CollectionAssert.Contains(invalidSeats, (5, 3));
            CollectionAssert.Contains(invalidSeats, (5, 4));

            CollectionAssert.Contains(invalidSeats, (6, 3));
        }
    }
}