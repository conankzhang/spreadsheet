﻿// Assignment test implementation written by Conan Zhang, u0409453
// for CS3500 Assignment #5. October, 2014.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;
using System.IO;

namespace SpreadsheetTests
{
    /// <summary>
    /// Tests Spreadsheet.cs
    /// </summary>
    [TestClass]
    public class SpreadsheetUnitTest
    {
        //zero constructor spreadsheet
        private AbstractSpreadsheet sheet1;
        //three constructor spreadsheet
        private AbstractSpreadsheet sheet2;

        /// <summary>
        /// Set default values for every test.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            sheet1 = new Spreadsheet();
            sheet2 = new Spreadsheet(Validate, s => s.ToUpper(), "default");
        }

        /// <summary>
        /// Check for names that are valid.
        /// </summary>
        [TestMethod]
        public void ValidNames()
        {
            sheet1.SetContentsOfCell("A15", "1");
            sheet1.SetContentsOfCell("a15", "=47");
            sheet1.SetContentsOfCell("XY032", "aeotnsao.rbkatns");
            sheet1.SetContentsOfCell("BC7", "12341759124819e123");
        }

        /// <summary>
        /// Check for names that are invalid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidName1()
        {
            sheet1.SetContentsOfCell("Z", "1tneaoeoa vwzke vw ;kq vw ;kqe;ka;jeaso");
        }

        /// <summary>
        /// Check for names that are invalid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidName2()
        {
            sheet1.SetContentsOfCell("X_", "123p1hd[91230[9hui1lc8aeou");
        }

        /// <summary>
        /// Check for names that are invalid.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidName3()
        {
            sheet1.SetContentsOfCell("hello", "1tneaoeoa vwzke vw ;kq vw ;kqe;ka;jeaso");
        }

        /// <summary>
        /// Incorrect formula format.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void SetContentsOfCell1()
        {
            sheet1.SetContentsOfCell("B1", "=A1?123[8;oes");
        }

        /// <summary>
        /// Should cause circular dependency.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void SetContentsOfCell2()
        {
            sheet1.SetContentsOfCell("B1", "=A1*2");
            sheet1.SetContentsOfCell("C1", "=B1+A1");
            sheet1.SetContentsOfCell("A1", "=B1+C1");
        }

        /// <summary>
        /// Should return correct set.
        /// </summary>
        [TestMethod]
        public void SetContentsOfCell3()
        {
            sheet1.SetContentsOfCell("B1", "=A1*2");
            sheet1.SetContentsOfCell("C1", "=B1+A1");

            HashSet<string> expected = new HashSet<string>() { "A1", "B1", "C1" };

            Assert.IsTrue(expected.SetEquals(sheet1.SetContentsOfCell("A1", "aeosucrh',.b1l2348pgae0u81'23p")));
        }

        /// <summary>
        /// Setting content to null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetContentsOfCell4()
        {
            sheet1.SetContentsOfCell("B1", null);
        }

        /// <summary>
        /// Setting content with null name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetContentsOfCell5()
        {
            sheet1.SetContentsOfCell(null, "aoeu");
        }

        /// <summary>
        /// Setting content with new formula should reevaluate value.
        /// </summary>
        [TestMethod]
        public void SetContentsOfCell6()
        {
            sheet1.SetContentsOfCell("A1", "=4*3");
            Assert.AreEqual(12.0, sheet1.GetCellValue("A1"));

            sheet1.SetContentsOfCell("A1", "=5*2");
            Assert.AreEqual(10.0, sheet1.GetCellValue("A1"));
        }

        /// <summary>
        /// Setting content with null name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetContentsOfCell7()
        {
            sheet1.SetContentsOfCell(null, "=A1+B2");
        }

        /// <summary>
        /// Setting content to empty string.
        /// </summary>
        [TestMethod]
        public void SetContentsOfCell8()
        {
            sheet1.SetContentsOfCell("A1", "=A2+B2");
            sheet1.SetContentsOfCell("A1", "");            
        }

        /// <summary>
        /// Invalid name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellValue1()
        {
            sheet1.GetCellValue("1u[0'u");
        }

        /// <summary>
        /// Null name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellValue2()
        {
            sheet1.GetCellValue(null);
        }

        /// <summary>
        /// Correct value returned.
        /// </summary>
        [TestMethod]
        public void GetCellValue3()
        {
            sheet1.SetContentsOfCell("A1234", "uahtnsueoahtnsueoahts',.'123.y'ay.',");
            Assert.AreEqual("uahtnsueoahtnsueoahts',.'123.y'ay.',", sheet1.GetCellValue("A1234") );
        }

        /// <summary>
        /// Correct value returned.
        /// </summary>
        [TestMethod]
        public void GetCellValue4()
        {
            sheet1.SetContentsOfCell("A1234", "451235.532");
            Assert.AreEqual(451235.532, sheet1.GetCellValue("A1234"));
        }

        /// <summary>
        /// Formula error returned.
        /// </summary>
        [TestMethod]
        public void GetCellValue5()
        {
            sheet1.SetContentsOfCell("A1234", "=A1/0");
            Assert.IsTrue(sheet1.GetCellValue("A1234") is FormulaError);
        }

        /// <summary>
        /// Sheet 3 read.
        /// </summary>
        [TestMethod]
        public void GetCellValue6()
        {
            sheet2.SetContentsOfCell("A1", "1.5");
            sheet2.Save("GetCellValue6.xml");

            AbstractSpreadsheet sheet3 = new Spreadsheet("GetCellValue6.xml", Validate, s => s.ToUpper(), "default" );

            Assert.AreEqual(1.5, sheet3.GetCellValue("A1"));
        }

        /// <summary>
        /// Get value of empty cell.
        /// </summary>
        [TestMethod]
        public void GetCellValue7l()
        {
            Assert.AreEqual("", sheet1.GetCellValue("A1"));
        }

        /// <summary>
        /// Sees default spreadsheet changed value.
        /// </summary>
        [TestMethod]
        public void Changed1()
        {
            Assert.IsFalse(sheet1.Changed);
        }

        /// <summary>
        /// Sees spreadsheet changed after setting cell of contents.
        /// </summary>
        [TestMethod]
        public void Changed2()
        {
            sheet1.SetContentsOfCell("ABSTNEOUHTNSAUEBMJQBKKHENUDEOHUJCRL1234678056131234102839471", "12358901298e1235");
            Assert.IsTrue(sheet1.Changed);
        }

        /// <summary>
        /// Sees spreadsheet changed after setting cell of contents and saving.
        /// </summary>
        [TestMethod]
        public void Changed3()
        {
            sheet1.SetContentsOfCell("aoecrbkpAOEU123901234789", "12.5235");
            sheet1.Save("Changed3");
            Assert.IsFalse(sheet1.Changed);
        }

        /// <summary>
        /// Tests save method with different contents.
        /// </summary>
        [TestMethod]
        public void Save1()
        {
            sheet1.SetContentsOfCell("AOEUaoeuAOEUaoeu1234123412341234", "2.52389");
            sheet1.SetContentsOfCell("A1", "testing");
            sheet1.SetContentsOfCell("Zaoeutnh1", "=432+535");
            sheet1.Save("Save1");
        }

        /// <summary>
        /// Tests save method with different contents.
        /// </summary>
        [TestMethod]
        public void GetSavedVersion1()
        {
            sheet1.Save("GetSavedVersion1.xml");
            Assert.AreEqual("default", sheet1.GetSavedVersion("GetSavedVersion1.xml") );
        }

        /// <summary>
        /// Tests for trying to read from nonexistent file.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void GetSavedVersion2()
        {
            Assert.AreEqual("default", sheet1.GetSavedVersion("DoesNotExist.xml"));
        }

        /*--------------------------------------------------------PS4 TESTS--------------------------------------------------------*/

        /// <summary>
        /// Check for names that are valid.
        /// </summary>
        [TestMethod]
        public void ValidCellNames()
        {
            sheet1.SetContentsOfCell("x1", "1");
            sheet1.SetContentsOfCell("X1", "2");
            sheet1.SetContentsOfCell("x2", "3");
            sheet1.SetContentsOfCell("y15", "4");
            sheet1.SetContentsOfCell("a3", "5");
            sheet1.SetContentsOfCell("X34", "5");

            Assert.AreNotEqual(sheet1.GetCellContents("x1"), sheet1.GetCellContents("X1"));
        }

        /// <summary>
        /// Check for invalid name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidName4()
        {
            sheet1.SetContentsOfCell("25", "1");
        }

        /// <summary>
        /// Check for invalid name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidName5()
        {
            sheet1.SetContentsOfCell("2x", "1");
        }

        /// <summary>
        /// Check for invalid name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidName6()
        {
            sheet1.SetContentsOfCell("&", "1");
        }

        /// <summary>
        /// Check for invalid name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidName7()
        {
            sheet1.SetContentsOfCell("", "1");
        }

        /// <summary>
        /// Get all nonempty cells.
        /// </summary>
        [TestMethod]
        public void GetNonEmpty()
        {
            sheet1.SetContentsOfCell("a1", "");
            sheet1.SetContentsOfCell("a2", "1");
            sheet1.SetContentsOfCell("a3", "2");

            HashSet<string> expected = new HashSet<string>() { "a2", "a3" };

            Assert.IsTrue(expected.SetEquals(sheet1.GetNamesOfAllNonemptyCells()));
        }

        /// <summary>
        /// Get contents of null cell.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContents1()
        {
            sheet1.GetCellContents("");
        }

        /// <summary>
        /// Get contents of invalid cell.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetCellContents2()
        {
            sheet1.GetCellContents("123416f89015");
        }

        /// <summary>
        /// Get contents of cell that didn't exist.
        /// </summary>
        [TestMethod]
        public void GetCellContents3()
        {
            Assert.AreEqual("", sheet1.GetCellContents("A5"));

        }

        /// <summary>
        /// Set contents of null cell.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellDouble1()
        {
            String s = null;

            sheet1.SetContentsOfCell(s, "1");
        }

        /// <summary>
        /// Set contents of invalid cell.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellDouble2()
        {
            sheet1.SetContentsOfCell("123416f89015", "1");
        }

        /// <summary>
        /// Should return correct set.
        /// </summary>
        [TestMethod]
        public void SetCellDouble3()
        {
            sheet1.SetContentsOfCell("B1", "=A1*2");
            sheet1.SetContentsOfCell("C1","=B1+A1");

            HashSet<string> expected = new HashSet<string>() { "A1", "B1", "C1" };

            Assert.IsTrue(expected.SetEquals(sheet1.SetContentsOfCell("A1", "1")));
        }

        /// <summary>
        /// Set contents of cell with null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetCellString1()
        {
            sheet1.SetContentsOfCell("A1", null);
        }

        /// <summary>
        /// Set contents of null cell.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellString2()
        {
            String s = null;

            sheet1.SetContentsOfCell(s, "cheese");
        }

        /// <summary>
        /// Set contents of invalid cell.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellString3()
        {
            sheet1.SetContentsOfCell("*!@$#)&)aoeuhtlsl',.p", "moon");
        }

        /// <summary>
        /// Should return correct set.
        /// </summary>
        [TestMethod]
        public void SetCellString4()
        {
            sheet1.SetContentsOfCell("B1", "=A1*2");
            sheet1.SetContentsOfCell("C1", "=B1+A1");

            HashSet<string> expected = new HashSet<string>() { "A1", "B1", "C1" };

            Assert.IsTrue(expected.SetEquals(sheet1.SetContentsOfCell("A1", "sky")));
        }

        /// <summary>
        /// Set contents of cell with null.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetCellFormula1()
        {
            sheet1.SetContentsOfCell("A1", null);
        }

        /// <summary>
        /// Set contents of null cell.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellFormula2()
        {
            String s = null;

            sheet1.SetContentsOfCell(s, "=A1+B2");
        }

        /// <summary>
        /// Set contents of invalid cell.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void SetCellFormula3()
        {
            sheet1.SetContentsOfCell("1234h80aaeoubtns',.{{", "=B3+C2");
        }

        /// <summary>
        /// Circular dependency from inserted formula.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void SetCellFormula4()
        {
            sheet1.SetContentsOfCell("B1", "=A1*2");
            sheet1.SetContentsOfCell("C1", "=B1+A1");

            HashSet<string> expected = new HashSet<string>() { "A1", "B1", "C1" };

            Assert.IsTrue(expected.SetEquals(sheet1.SetContentsOfCell("A1", "=A1+A1")));
        }

        /// <summary>
        /// Should return correct set.
        /// </summary>
        [TestMethod]
        public void SetCellFormula5()
        {
            sheet1.SetContentsOfCell("B1", "=A1*2");
            sheet1.SetContentsOfCell("C1", "=B1+A1");

            HashSet<string> expected = new HashSet<string>() { "A1", "B1", "C1" };

            Assert.IsTrue(expected.SetEquals(sheet1.SetContentsOfCell("A1", "=D2+C5")));
        }

        /// <summary>
        /// Should return correct set.
        /// </summary>
        [TestMethod]
        public void SetCellFormula6()
        {
            sheet1.SetContentsOfCell("A2", "=5");
            sheet1.SetContentsOfCell("A3", "=A2+A1");
            sheet1.SetContentsOfCell("a1", "=A3+A2");

            HashSet<string> expected = new HashSet<string>() { "A1", "A3", "a1" };

            Assert.IsTrue(expected.SetEquals(sheet1.SetContentsOfCell("A1", "=3+A2")));
        }

        /// <summary>
        /// Should return correct set after setting cells again.
        /// </summary>
        [TestMethod]
        public void SetCellTwice()
        {
            sheet1.SetContentsOfCell("B1", "=A1*2");
            sheet1.SetContentsOfCell("C1", "=B1+A1");
            sheet1.SetContentsOfCell("D1", "=B1+C1");
            sheet1.SetContentsOfCell("E1", "=C1+D1");
            sheet1.SetContentsOfCell("C1", "7");
            sheet1.SetContentsOfCell("D1", "sun");
            sheet1.SetContentsOfCell("E1","=2");

            HashSet<string> expected = new HashSet<string>() { "A1", "B1" };

            Assert.IsTrue(expected.SetEquals(sheet1.SetContentsOfCell("A1", "1")));
        }

        /// <summary>
        /// Get dependents of null name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDirectDependents1()
        {
            PrivateObject sheetAccess = new PrivateObject(sheet1);

            String s = null;

            sheetAccess.Invoke("GetDirectDependents", s);
        }

        /// <summary>
        /// Get dependents of invalid name.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidNameException))]
        public void GetDirectDependents2()
        {
            PrivateObject sheetAccess = new PrivateObject(sheet1);

            sheetAccess.Invoke("GetDirectDependents", "1234aaoeuhtsh123[gahoeust");
        }

        /// <summary>
        /// Get correct dependents.
        /// </summary>
        [TestMethod]
        public void GetDirectDependents3()
        {
            sheet1.SetContentsOfCell("A1", "5");
            sheet1.SetContentsOfCell("B1", "=A1*A1");
            sheet1.SetContentsOfCell("C1", "=B1+A1");
            sheet1.SetContentsOfCell("D1", "=B1-C1");

            PrivateObject sheetAccess = new PrivateObject(sheet1);

            HashSet<string> expected = new HashSet<string>() { "B1", "C1" };

            Assert.IsTrue(expected.SetEquals((HashSet<string>)sheetAccess.Invoke("GetDirectDependents", "A1")));
        }

        /// <summary>
        /// Circular dependency3
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(CircularException))]
        public void CircularDependency1()
        {
            sheet1.SetContentsOfCell("A1", "=B1*2");
            sheet1.SetContentsOfCell("B1", "=C1*2");
            sheet1.SetContentsOfCell("C1", "=A1*2");

        }

        /*--------------------------------------------------------UNIT TEST FUNCTIONS/MEMBERS--------------------------------------------------------*/
        private Regex names;

        /// <summary>
        /// Delegate for checking valid variable.
        /// </summary>
        /// <param name="s">String to normalize.</param>
        /// <returns>The normalized version of the variable.</returns>
        private Boolean Validate(String s)
        {
            names = new Regex(@"^[a-zA-Z]+[0-9]+$");
            return names.IsMatch(s);
        }

        /*--------------------------------------------------------PS5 GRADING TEST FUNCTIONS/MEMBERS--------------------------------------------------------*/
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        /// Verifies cells and their values, which must alternate.
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="constraints"></param>
        public void VV(AbstractSpreadsheet sheet, params object[] constraints)
        {
            for (int i = 0; i < constraints.Length; i += 2)
            {
                if (constraints[i + 1] is double)
                {
                    Assert.AreEqual((double)constraints[i + 1], (double)sheet.GetCellValue((string)constraints[i]), 1e-9);
                }
                else
                {
                    Assert.AreEqual(constraints[i + 1], sheet.GetCellValue((string)constraints[i]));
                }
            }
        }

        /// <summary>
        /// For setting a spreadsheet cell.
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public IEnumerable<string> Set(AbstractSpreadsheet sheet, string name, string contents)
        {
            List<string> result = new List<string>(sheet.SetContentsOfCell(name, contents));
            return result;
        }

        /// <summary>
        /// Tests IsValid
        /// </summary>
        [TestMethod()]
        public void IsValidTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "x");
        }

        /// <summary>
        /// Tests IsValid
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void IsValidTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("A1", "x");
        }

        /// <summary>
        /// Tests IsValid
        /// </summary>
        [TestMethod()]
        public void IsValidTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "= A1 + C1");
        }

        /// <summary>
        /// Tests IsValid
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void IsValidTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("B1", "= A1 + C1");
        }

        /// <summary>
        /// Tests Normalize
        /// </summary>
        [TestMethod()]
        public void NormalizeTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            Assert.AreEqual("", s.GetCellContents("b1"));
        }

        /// <summary>
        /// Tests Normalize
        /// </summary>
        [TestMethod()]
        public void NormalizeTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
            ss.SetContentsOfCell("B1", "hello");
            Assert.AreEqual("hello", ss.GetCellContents("b1"));
        }

        /// <summary>
        /// Tests Normalize
        /// </summary>
        [TestMethod()]
        public void NormalizeTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("a1", "5");
            s.SetContentsOfCell("A1", "6");
            s.SetContentsOfCell("B1", "= a1");
            Assert.AreEqual(5.0, (double)s.GetCellValue("B1"), 1e-9);
        }

        /// <summary>
        /// Tests Normalize
        /// </summary>
        [TestMethod()]
        public void NormalizeTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
            ss.SetContentsOfCell("a1", "5");
            ss.SetContentsOfCell("A1", "6");
            ss.SetContentsOfCell("B1", "= a1");
            Assert.AreEqual(6.0, (double)ss.GetCellValue("B1"), 1e-9);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void EmptySheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            VV(ss, "A1", "");
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void OneString()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneString(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void OneString(AbstractSpreadsheet ss)
        {
            Set(ss, "B1", "hello");
            VV(ss, "B1", "hello");
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void OneNumber()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneNumber(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void OneNumber(AbstractSpreadsheet ss)
        {
            Set(ss, "C1", "17.5");
            VV(ss, "C1", 17.5);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void OneFormula()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneFormula(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void OneFormula(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "5.2");
            Set(ss, "C1", "= A1+B1");
            VV(ss, "A1", 4.1, "B1", 5.2, "C1", 9.3);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void Changed()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.IsFalse(ss.Changed);
            Set(ss, "C1", "17.5");
            Assert.IsTrue(ss.Changed);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void DivisionByZero1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            DivisionByZero1(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void DivisionByZero1(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "0.0");
            Set(ss, "C1", "= A1 / B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void DivisionByZero2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            DivisionByZero2(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void DivisionByZero2(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "5.0");
            Set(ss, "A3", "= A1 / 0.0");
            Assert.IsInstanceOfType(ss.GetCellValue("A3"), typeof(FormulaError));
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void EmptyArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            EmptyArgument(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void EmptyArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "C1", "= A1 + B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void StringArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            StringArgument(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void StringArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "hello");
            Set(ss, "C1", "= A1 + B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void ErrorArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ErrorArgument(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void ErrorArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "");
            Set(ss, "C1", "= A1 + B1");
            Set(ss, "D1", "= C1");
            Assert.IsInstanceOfType(ss.GetCellValue("D1"), typeof(FormulaError));
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void NumberFormula1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            NumberFormula1(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void NumberFormula1(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "C1", "= A1 + 4.2");
            VV(ss, "C1", 8.3);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        [TestMethod()]
        public void NumberFormula2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            NumberFormula2(ss);
        }

        /// <summary>
        /// Simple Tests
        /// </summary>
        public void NumberFormula2(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "= 4.6");
            VV(ss, "A1", 4.6);
        }


        /// <summary>
        /// Repeated Simple Tests
        /// </summary>
        [TestMethod()]
        public void RepeatSimpleTests()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "A1", "17.32");
            Set(ss, "B1", "This is a test");
            Set(ss, "C1", "= A1+B1");
            OneString(ss);
            OneNumber(ss);
            OneFormula(ss);
            DivisionByZero1(ss);
            DivisionByZero2(ss);
            StringArgument(ss);
            ErrorArgument(ss);
            NumberFormula1(ss);
            NumberFormula2(ss);
        }

        /// <summary>
        /// Formula
        /// </summary>
        [TestMethod()]
        public void Formulas()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Formulas(ss);
        }

        /// <summary>
        /// Formula
        /// </summary>
        public void Formulas(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.4");
            Set(ss, "B1", "2.2");
            Set(ss, "C1", "= A1 + B1");
            Set(ss, "D1", "= A1 - B1");
            Set(ss, "E1", "= A1 * B1");
            Set(ss, "F1", "= A1 / B1");
            VV(ss, "C1", 6.6, "D1", 2.2, "E1", 4.4 * 2.2, "F1", 2.0);
        }

        /// <summary>
        /// Formula
        /// </summary>
        [TestMethod()]
        public void Formulasa()
        {
            Formulas();
        }

        /// <summary>
        /// Formula
        /// </summary>
        [TestMethod()]
        public void Formulasb()
        {
            Formulas();
        }

        /// <summary>
        /// Multiple
        /// </summary>
        [TestMethod()]
        public void Multiple()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            AbstractSpreadsheet s2 = new Spreadsheet();
            Set(s1, "X1", "hello");
            Set(s2, "X1", "goodbye");
            VV(s1, "X1", "hello");
            VV(s2, "X1", "goodbye");
        }

        /// <summary>
        /// Multiple
        /// </summary>
        [TestMethod()]
        public void Multiplea()
        {
            Multiple();
        }

        /// <summary>
        /// Multiple
        /// </summary>
        [TestMethod()]
        public void Multipleb()
        {
            Multiple();
        }

        /// <summary>
        /// Multiple
        /// </summary>
        [TestMethod()]
        public void Multiplec()
        {
            Multiple();
        }

        /// <summary>
        /// Reading/writing spreadsheets
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("q:\\missing\\save.txt");
        }

        /// <summary>
        /// Reading/writing spreadsheets
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet("q:\\missing\\save.txt", s => true, s => s, "");
        }

        /// <summary>
        /// Reading/writing spreadsheets
        /// </summary>
        [TestMethod()]
        public void SaveTest3()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            Set(s1, "A1", "hello");
            s1.Save("save1.txt");
            s1 = new Spreadsheet("save1.txt", s => true, s => s, "default");
            Assert.AreEqual("hello", s1.GetCellContents("A1"));
        }

        /// <summary>
        /// Reading/writing spreadsheets
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest4()
        {
            using (StreamWriter writer = new StreamWriter("save2.txt"))
            {
                writer.WriteLine("This");
                writer.WriteLine("is");
                writer.WriteLine("a");
                writer.WriteLine("test!");
            }
            AbstractSpreadsheet ss = new Spreadsheet("save2.txt", s => true, s => s, "");
        }

        /// <summary>
        /// Reading/writing spreadsheets
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest5()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("save3.txt");
            ss = new Spreadsheet("save3.txt", s => true, s => s, "version");
        }

        /// <summary>
        /// Reading/writing spreadsheets
        /// </summary>
        [TestMethod()]
        public void SaveTest6()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s, "hello");
            ss.Save("save4.txt");
            Assert.AreEqual("hello", new Spreadsheet().GetSavedVersion("save4.txt"));
        }

        /// <summary>
        /// Reading/writing spreadsheets
        /// </summary>
        [TestMethod()]
        public void SaveTest7()
        {
            using (XmlWriter writer = XmlWriter.Create("save5.txt"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("spreadsheet");
                writer.WriteAttributeString("version", "");

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A1");
                writer.WriteElementString("contents", "hello");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A2");
                writer.WriteElementString("contents", "5.0");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A3");
                writer.WriteElementString("contents", "4.0");
                writer.WriteEndElement();

                writer.WriteStartElement("cell");
                writer.WriteElementString("name", "A4");
                writer.WriteElementString("contents", "= A2 + A3");
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            AbstractSpreadsheet ss = new Spreadsheet("save5.txt", s => true, s => s, "");
            VV(ss, "A1", "hello", "A2", 5.0, "A3", 4.0, "A4", 9.0);
        }

        /// <summary>
        /// Reading/writing spreadsheets
        /// </summary>
        [TestMethod()]
        public void SaveTest8()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "A1", "hello");
            Set(ss, "A2", "5.0");
            Set(ss, "A3", "4.0");
            Set(ss, "A4", "= A2 + A3");
            ss.Save("save6.txt");
            using (XmlReader reader = XmlReader.Create("save6.txt"))
            {
                int spreadsheetCount = 0;
                int cellCount = 0;
                bool A1 = false;
                bool A2 = false;
                bool A3 = false;
                bool A4 = false;
                string name = null;
                string contents = null;

                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "spreadsheet":
                                Assert.AreEqual("default", reader["version"]);
                                spreadsheetCount++;
                                break;

                            case "cell":
                                cellCount++;
                                break;

                            case "name":
                                reader.Read();
                                name = reader.Value;
                                break;

                            case "contents":
                                reader.Read();
                                contents = reader.Value;
                                break;
                        }
                    }
                    else
                    {
                        switch (reader.Name)
                        {
                            case "cell":
                                if (name.Equals("A1")) { Assert.AreEqual("hello", contents); A1 = true; }
                                else if (name.Equals("A2")) { Assert.AreEqual(5.0, Double.Parse(contents), 1e-9); A2 = true; }
                                else if (name.Equals("A3")) { Assert.AreEqual(4.0, Double.Parse(contents), 1e-9); A3 = true; }
                                else if (name.Equals("A4")) { contents = contents.Replace(" ", ""); Assert.AreEqual("=A2+A3", contents); A4 = true; }
                                else Assert.Fail();
                                break;
                        }
                    }
                }
                Assert.AreEqual(1, spreadsheetCount);
                Assert.AreEqual(4, cellCount);
                Assert.IsTrue(A1);
                Assert.IsTrue(A2);
                Assert.IsTrue(A3);
                Assert.IsTrue(A4);
            }
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void Formula1()
        {
            Formula1(new Spreadsheet());
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        public void Formula1(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a2 + a3");
            Set(ss, "a2", "= b1 + b2");
            Assert.IsInstanceOfType(ss.GetCellValue("a1"), typeof(FormulaError));
            Assert.IsInstanceOfType(ss.GetCellValue("a2"), typeof(FormulaError));
            Set(ss, "a3", "5.0");
            Set(ss, "b1", "2.0");
            Set(ss, "b2", "3.0");
            VV(ss, "a1", 10.0, "a2", 5.0);
            Set(ss, "b2", "4.0");
            VV(ss, "a1", 11.0, "a2", 6.0);
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void Formula2()
        {
            Formula2(new Spreadsheet());
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        public void Formula2(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a2 + a3");
            Set(ss, "a2", "= a3");
            Set(ss, "a3", "6.0");
            VV(ss, "a1", 12.0, "a2", 6.0, "a3", 6.0);
            Set(ss, "a3", "5.0");
            VV(ss, "a1", 10.0, "a2", 5.0, "a3", 5.0);
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void Formula3()
        {
            Formula3(new Spreadsheet());
        }


        /// <summary>
        /// Fun with formulas
        /// </summary>
        public void Formula3(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a3 + a5");
            Set(ss, "a2", "= a5 + a4");
            Set(ss, "a3", "= a5");
            Set(ss, "a4", "= a5");
            Set(ss, "a5", "9.0");
            VV(ss, "a1", 18.0);
            VV(ss, "a2", 18.0);
            Set(ss, "a5", "8.0");
            VV(ss, "a1", 16.0);
            VV(ss, "a2", 16.0);
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void Formula4()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Formula1(ss);
            Formula2(ss);
            Formula3(ss);
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void Formula4a()
        {
            Formula4();
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void MediumSheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            MediumSheet(ss);
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        public void MediumSheet(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "1.0");
            Set(ss, "A2", "2.0");
            Set(ss, "A3", "3.0");
            Set(ss, "A4", "4.0");
            Set(ss, "B1", "= A1 + A2");
            Set(ss, "B2", "= A3 * A4");
            Set(ss, "C1", "= B1 + B2");
            VV(ss, "A1", 1.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 3.0, "B2", 12.0, "C1", 15.0);
            Set(ss, "A1", "2.0");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 4.0, "B2", 12.0, "C1", 16.0);
            Set(ss, "B1", "= A1 / A2");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void MediumSheeta()
        {
            MediumSheet();
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void MediumSave()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            MediumSheet(ss);
            ss.Save("save7.txt");
            ss = new Spreadsheet("save7.txt", s => true, s => s, "default");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
        }

        /// <summary>
        /// Fun with formulas
        /// </summary>
        [TestMethod()]
        public void MediumSavea()
        {
            MediumSave();
        }

        /// <summary>
        /// A long chained formula.  If this doesn't finish within 60 seconds, it fails.
        /// </summary>
        [TestMethod()]
        public void LongFormulaTest()
        {
            object result = "";
            Thread t = new Thread(() => LongFormulaHelper(out result));
            t.Start();
            t.Join(60 * 1000);
            if (t.IsAlive)
            {
                t.Abort();
                Assert.Fail("Computation took longer than 60 seconds");
            }
            Assert.AreEqual("ok", result);
        }

        /// <summary>
        /// PS5 Grading Test.
        /// </summary>
        /// <param name="result"></param>
        public void LongFormulaHelper(out object result)
        {
            try
            {
                AbstractSpreadsheet s = new Spreadsheet();
                s.SetContentsOfCell("sum1", "= a1 + a2");
                int i;
                int depth = 100;
                for (i = 1; i <= depth * 2; i += 2)
                {
                    s.SetContentsOfCell("a" + i, "= a" + (i + 2) + " + a" + (i + 3));
                    s.SetContentsOfCell("a" + (i + 1), "= a" + (i + 2) + "+ a" + (i + 3));
                }
                s.SetContentsOfCell("a" + i, "1");
                s.SetContentsOfCell("a" + (i + 1), "1");
                Assert.AreEqual(Math.Pow(2, depth + 1), (double)s.GetCellValue("sum1"), 1.0);
                s.SetContentsOfCell("a" + i, "0");
                Assert.AreEqual(Math.Pow(2, depth), (double)s.GetCellValue("sum1"), 1.0);
                s.SetContentsOfCell("a" + (i + 1), "0");
                Assert.AreEqual(0.0, (double)s.GetCellValue("sum1"), 0.1);
                result = "ok";
            }
            catch (Exception e)
            {
                result = e;
            }
        }
    }
}
