using Offline.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Xunit.Sdk;
using System.IO;
using System;

namespace Offline.Models.Tests
{
    [TestClass]
    public class CinemaTests
    {
        private Cinema Cinema { get; set; }

        [TestInitialize]
        public void CinemaInitisalize()
        {
            Cinema = CinemaReader.Read(Path.GetFullPath(@"..\..\..\TestFiles\") + "test_instance.txt");
        }

        [TestMethod]
        public void GetTotalNumberOfGroupsTest()
        {
            Assert.AreEqual(33, Cinema.GetTotalNumberOfGroups());
        }

        [TestMethod]
        public void GetGroupAsArrayTest()
        {
            var expectedArray = new int[] { 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 7 };

            CollectionAssert.AreEqual(expectedArray, Cinema.GetGroupsAsArray());
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

            Assert.IsFalse(Cinema.VerifyCinema());
        }

        [TestMethod]
        public void VerifyCinemaTest_Diagnol_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(3, 1, 2);

            Assert.IsFalse(Cinema.VerifyCinema());
        }

        [TestMethod]
        public void VerifyCinemaTest_Vertical_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(1, 1, 2);

            Assert.IsFalse(Cinema.VerifyCinema());
        }

        [TestMethod]
        public void VerifyCinemaTest_Horizontal_No_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(5, 0, 2);

            Assert.IsTrue(Cinema.VerifyCinema());
        }

        [TestMethod]
        public void VerifyCinemaTest_Vertical_No_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(1, 2, 2);

            Assert.IsTrue(Cinema.VerifyCinema());
        }

        [TestMethod]
        public void VerifyCinemaTest_Diagnol_No_Violation()
        {
            Cinema.SeatGroup(1, 0, 2);
            Cinema.SeatGroup(4, 1, 2);

            Assert.IsTrue(Cinema.VerifyCinema());
        }
    }
}