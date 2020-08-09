﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

                case DbfFieldType.Numeric:
                case DbfFieldType.Float:
                    {
                        var d = (decimal)Convert.ChangeType(value, typeof(decimal));
                        var s = d.ToString("F" + field.Precision, CultureInfo.InvariantCulture.NumberFormat);
                        
                        if( s.Length > field.Length) throw  new InvalidCastException(field.Name);
                        s = new string(' ', field.Length - s.Length) + s; // justify right

                        return ToBytes(s, field.Length);
                    }

                case DbfFieldType.Logical:
                    return new[] { (bool)value ? (byte)'T' : (byte)'F' };

                case DbfFieldType.Integer:
                    {
                        var v = (int)value;
                        return BitConverter.GetBytes(Helper.SwapEndianness(v));
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
            var h = new Header
            {
                Version = (byte)Version,
                RecordCount = (uint)RecordCount,
                HeaderBytes = (ushort)(Header.SizeOf + _fields.Count * FieldDescriptor.SizeOf + 1), // + 1 - end of header make (0x0D)
                RecordBytes = (ushort)CurrentRecordSize(),
                LastUpdateDate = DateTime.Today
            };

            _writeStream.Position = 0;

            _writeStream.WriteValue(h);
            _writeStream.Flush();
        }

        private int CurrentRecordSize() => _fields.Sum(x => x.Size) + 1; // + 1 -  deleted mark

        private void WriteFields()
        {
            _writeStream.Position = Header.SizeOf;

            foreach (var field in _fields)
            {
                var f = new FieldDescriptor
                {
                    FieldName = field.Name,
                    FieldType = (byte)field.Type,
                    FieldLength = (byte)field.Length,
                    DecimalCount = (byte)field.Precision,
                    Displacement = (uint)field.Displacement
                };

                _writeStream.WriteValue(f);
                _writeStream.Flush();
            }

            _writeStream.WriteByte(0x0d); // The End of Fields mark
        }
    }
}
