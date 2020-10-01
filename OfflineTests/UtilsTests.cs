using Microsoft.VisualStudio.TestTools.UnitTesting;
using Offline;
using System;
using System.Collections.Generic;
using System.Text;
using static Offline.Utils;

namespace Offline.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod]
        public void AreTwoSeatedGroupsValidTest()
        {
            // Horizontal Violations
            Assert.AreEqual(SeatingResult.HorizontalViolation, Utils.AreTwoSeatedGroupsValid(0, 0, 3, 0, 2, 2));
            Assert.AreEqual(SeatingResult.HorizontalViolation, Utils.AreTwoSeatedGroupsValid(3, 0, 0, 0, 2, 2));
            Assert.AreEqual(SeatingResult.HorizontalViolation, Utils.AreTwoSeatedGroupsValid(0, 0, 2, 0, 1, 1));
            Assert.AreEqual(SeatingResult.HorizontalViolation, Utils.AreTwoSeatedGroupsValid(0, 0, 1, 0, 1, 1));
            Assert.AreEqual(SeatingResult.HorizontalViolation, Utils.AreTwoSeatedGroupsValid(0, 0, 3, 0, 2, 1));
            Assert.AreEqual(SeatingResult.HorizontalViolation, Utils.AreTwoSeatedGroupsValid(3, 0, 0, 0, 1, 2));
            Assert.AreEqual(SeatingResult.HorizontalViolation, Utils.AreTwoSeatedGroupsValid(0, 0, 2, 0, 1, 2));

            Assert.AreEqual(SeatingResult.NoViolation, Utils.AreTwoSeatedGroupsValid(0, 0, 4, 0, 2, 1));
            Assert.AreEqual(SeatingResult.NoViolation, Utils.AreTwoSeatedGroupsValid(4, 0, 0, 0, 2, 1));
            Assert.AreEqual(SeatingResult.NoViolation, Utils.AreTwoSeatedGroupsValid(0, 0, 3, 0, 1, 1));

            // Diagnol Violations
            Assert.AreEqual(SeatingResult.DiagnolViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 0, x2: 1, y2: 1, s1: 1, s2: 1));
            Assert.AreEqual(SeatingResult.DiagnolViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 0, x2: 1, y2: 1, s1: 1, s2: 1));
            Assert.AreEqual(SeatingResult.DiagnolViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 0, x2: 2, y2: 1, s1: 2, s2: 1));
            Assert.AreEqual(SeatingResult.DiagnolViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 0, x2: 2, y2: 1, s1: 2, s2: 1));
            Assert.AreEqual(SeatingResult.DiagnolViolation, Utils.AreTwoSeatedGroupsValid(x1: 3, y1: 0, x2: 2, y2: 1, s1: 2, s2: 1));

            Assert.AreEqual(SeatingResult.NoViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 2, x2: 2, y2: 0, s1: 5, s2: 5));
            Assert.AreEqual(SeatingResult.NoViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 0, x2: 2, y2: 1, s1: 1, s2: 1));

            // Vertical Violations
            Assert.AreEqual(SeatingResult.VerticalViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 0, x2: 0, y2: 1, s1: 1, s2: 1));
            Assert.AreEqual(SeatingResult.VerticalViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 2, x2: 0, y2: 1, s1: 5, s2: 5));

            Assert.AreEqual(SeatingResult.NoViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 2, x2: 0, y2: 0, s1: 1, s2: 1));
            Assert.AreEqual(SeatingResult.NoViolation, Utils.AreTwoSeatedGroupsValid(x1: 0, y1: 2, x2: 0, y2: 0, s1: 5, s2: 5));
        }

        [TestMethod()]
        public void GenerateArrayOfOnesTest()
        {
            var expected = new double[10] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

            CollectionAssert.AreEqual(expected, Utils.GenerateArrayOfOnes(10));
        }
    }
}