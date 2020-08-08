using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LastDbf.Test
{
    [TestClass]
    public class ComplexUnitTest
    {
        private const string Folder = @"C:\TEMP\DBF\";

        [TestMethod]
        public void TestHeader()
        {
            var path = Folder + @"DB_HEADER.DBF";

            using var dbf = new DbfWriter(path, DbfVersion.dBASE_III);

            dbf.AddField(new DbfField("CHRFIELD", DbfFieldType.Character, 10));
            //dbf.AddRecord("ANDREI");
        }

        [TestMethod]
        public void TestReadSample()
        {
            var path = Folder + @"SAMPLE.dbf";

            using var reader = new DbfReader(path);

            Assert.AreEqual(4, (int)reader.Version);

            Assert.AreEqual(5, reader.Fields.Count);

            Assert.AreEqual(2,reader.RecordCount);
            
            var record = reader.Read();
            //dbf.AddRecord("ANDREI");
        }


        [TestMethod]
        public void TestMethod1()
        {
            var path = Folder + @"DB.DBF";
            var version = DbfVersion.dBASE_IV_SQL_Table;

            var w0 = new object[] { true, DateTime.Today, "HELLO!!!", 20001000.34, 345.780 };
            var w1 = new object[] { false, DateTime.Today.AddDays(-51), "BYE!!!", 10002000.34, 780.345 };

            IReadOnlyList<DbfField> fields;
            {
                using var writer = new DbfWriter(path, version);
                Assert.AreEqual(version, writer.Version);

                //dbf.AddField(new DbfField("INTFIELD", DbfFieldType.Integer));
                writer.AddField(new DbfField("LGCFIELD", DbfFieldType.Logical));
                writer.AddField(new DbfField("DATFIELD", DbfFieldType.Date));

                writer.AddField(new DbfField("CHRFIELD", DbfFieldType.Character, 10));

                writer.AddField(new DbfField("NUMFIELD", DbfFieldType.Numeric, 10, 2));
                writer.AddField(new DbfField("FLTFIELD", DbfFieldType.Float));

                Assert.AreEqual(5, writer.Fields.Count);

                writer.AddRecord(w0);
                writer.AddRecord(w1);

                fields = writer.Fields;
            }

            {
                using var reader = new DbfReader(path);
                // Header
                Assert.AreEqual(version, reader.Version);
                Assert.AreEqual(fields.Count, reader.Fields.Count);

                Assert.IsTrue(fields.Select(x => x.Name).Zip(reader.Fields.Select(x => x.Name)).All(x => x.First.Equals(x.Second)));
                Assert.IsTrue(fields.Select(x => x.Type).Zip(reader.Fields.Select(x => x.Type)).All(x => x.First.Equals(x.Second)));
                Assert.IsTrue(fields.Select(x => x.Length).Zip(reader.Fields.Select(x => x.Length)).All(x => x.First.Equals(x.Second)));
                Assert.IsTrue(fields.Select(x => x.Precision).Zip(reader.Fields.Select(x => x.Precision)).All(x => x.First.Equals(x.Second)));

                Assert.IsTrue(fields.Zip(reader.Fields).All(x => x.First.Equals(x.Second)));

                // Records
                Assert.AreEqual(2, reader.RecordCount);

                var r0 = reader.Read();
                Assert.IsNotNull(r0);

                var r1 = reader.Read();
                Assert.IsNotNull(r1);

                CollectionAssert.AreEqual(w0, r0);
                CollectionAssert.AreEqual(w1, r1);
            }
        }
    }
}