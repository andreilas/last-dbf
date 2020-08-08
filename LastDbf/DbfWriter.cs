using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace LastDbf
{
    public class DbfWriter : DbfBase
    {
        public readonly DbfVersion Version;

        private readonly List<DbfField> _fields = new List<DbfField>();

        private readonly FileStream _writeStream;

        private bool _writingMode;

        public DbfWriter(string path, DbfVersion version)
        {
            Version = version;
            _writeStream = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            Fields = new ReadOnlyCollection<DbfField>(_fields);
        }

        public void AddField(DbfField field)
        {
            field.Displacement = CurrentRecordSize();
            _fields.Add(field);
        }

        public void AddRecord(params object[] values)
        {
            if (values.Length != _fields.Count) throw new ArgumentException("Invalid number of items");

            if (!_writingMode) StartWriting();

            //_writeStream.Position = DbfHeaderStruct.SizeOf + DbfFieldHeaderStruct.SizeOf * _fields.Count + 1;

            _writeStream.WriteValue(' '); // Not deleted mark
            var bytes = _fields.Zip(values, PackValue).SelectMany(x => x).ToArray();
            _writeStream.Write(bytes, 0, bytes.Length);

            ++RecordCount;
        }

        private static IEnumerable<byte> PackValue(DbfField field, object value)
        {
            switch (field.Type)
            {
                case DbfFieldType.Character:
                    {
                        if (!(value is string s)) throw new InvalidCastException(field.Name);
                        if (s.Length > field.Length) throw new InvalidCastException(field.Name);

                        return ToBytes(s, field.Length);
                    }

                case DbfFieldType.Date:
                    {
                        var d = (DateTime)value;
                        var s = $"{d.Year:0000}{d.Month:00}{d.Day:00}";
                        var bytes = Encoding.Default.GetBytes(s);
                        return bytes;
                    }

                case DbfFieldType.Float:
                    {
                        var s = value.ToString().Replace(",", ".");
                        return ToBytes(s, 10);
                    }

                case DbfFieldType.Logical:
                    return new[] { (bool)value ? (byte)'T' : (byte)'F' };

                case DbfFieldType.Integer:
                    {
                        var v = (int)value;
                        return BitConverter.GetBytes(Helper.SwapEndianness(v));
                    }

                case DbfFieldType.Numeric:
                    {
                        var s = value.ToString().Replace(",", ".");
                        return ToBytes(s, 10);
                    }

                default:
                    throw new ArgumentOutOfRangeException(field.Type.ToString());
            }

            IEnumerable<byte> ToBytes(string s, int length)
            {
                var bytes = Encoding.Default.GetBytes(s);
                foreach (var b in bytes) yield return b;

                // fill with blanks
                var n = field.Length - bytes.Length;
                while (n-- > 0) yield return (byte)' ';
            }
        }

        private void StartWriting()
        {
            WriteHeader();
            WriteFields();
            _writingMode = true;
        }

        public override void Dispose()
        {
            if (!_writingMode)
                StartWriting(); // write header & fields. no data

            _writeStream.WriteByte(0x1A); // write eof

            if (_writingMode)
                WriteHeader(); // write again to save record count etc.

            _writeStream.WriteByte(0xA1); // eof
            _writeStream.Flush();
            _writeStream.Dispose();
        }

        //private enum Mode
        //{
        //    Initial,
        //    OpenRead,
        //    OpenWrite,
        //    Writing,
        //    Disposed
        //}

        private void WriteHeader()
        {
            var h = new DbfHeaderStruct
            {
                Version = (byte)Version,
                RecordCount = (uint)RecordCount,
                HeaderBytes = (ushort)(DbfHeaderStruct.SizeOf + _fields.Count * DbfHeaderStruct.SizeOf + 1), // + 1 - end of header make (0x0D)
                RecordBytes = (ushort)CurrentRecordSize(),
                LastUpdateDate = DateTime.Today
            };

            _writeStream.Position = 0;

            _writeStream.WriteValue(h);
        }

        private int CurrentRecordSize() => _fields.Sum(x => x.Size) + 1; // + 1 -  deleted mark

        private void WriteFields()
        {
            _writeStream.Position = DbfHeaderStruct.SizeOf;

            foreach (var field in _fields)
            {
                var f = new DbfFieldHeaderStruct
                {
                    FieldName = field.Name,
                    FieldType = (byte)field.Type,
                    FieldLength = (byte)field.Length,
                    DecimalCount = (byte)field.Precision,
                    Displacement = (uint)field.Displacement
                };

                _writeStream.WriteValue(f);
            }

            _writeStream.WriteByte(0x0d); // The End of Fields mark
        }
    }
}
