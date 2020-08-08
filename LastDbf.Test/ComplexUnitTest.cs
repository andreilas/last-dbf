using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LastDbf.Test
{
    [TestClass]
    public class ComplexUnitTest
    {
        [TestMethod]
        public void TestHeader()
        {
            var path = @"C:\TEMP\DB_HEADER.DBF";

            using var dbf = new DbfBase();

            dbf.Create(path, DbfVersion.dBASE_III);

            dbf.AddField(new DbfField("CHRFIELD", DbfFieldType.Character, 10));
            //dbf.AddRecord("ANDREI");
        }

        [TestMethod]
        public void TestMethod1()
        {
            var path = @"C:\TEMP\DB.DBF";

            var w0 = new object[] {123, true, DateTime.Today, "HELLO!!!", 20001000.34, 345.780};
            var w1 = new object[] {456, false, DateTime.Today.AddDays(-51), "BYE!!!", 10002000.34, 780.345};

            //using var dbf = new DbfBase();

            {
                using var dbf = new DbfBase();

                dbf.Create(path, DbfVersion.dBASE_IV_SQL_Table);

                //dbf.AddField(new DbfField("INTFIELD", DbfFieldType.Integer));
                dbf.AddField(new DbfField("LGCFIELD", DbfFieldType.Logical));
                dbf.AddField(new DbfField("DATFIELD", DbfFieldType.Date));

                dbf.AddField(new DbfField("CHRFIELD", DbfFieldType.Character, 10));

                dbf.AddField(new DbfField("NUMFIELD", DbfFieldType.Numeric, 10, 2));
                dbf.AddField(new DbfField("FLTFIELD", DbfFieldType.Float));

                Assert.AreEqual(6, dbf.Fields.Count);

                dbf.AddRecord(w0);
                dbf.AddRecord(w1);
            }

            {
                using var dbf = new DbfBase();

                dbf.Open(path);

                Assert.AreEqual(6, dbf.Fields.Count);
                Assert.AreEqual(2, dbf.Records.Count);

                var r0 = dbf.Records[0];
                var r1 = dbf.Records[1];

                CollectionAssert.AreEqual(w0, r0);
                CollectionAssert.AreEqual(w1, r1);
            }
        }
    }
}