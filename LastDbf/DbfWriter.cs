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
            field.Displacement = _fields.Sum(x => x.Size);
            _fields.Add(field);
        }

        public void AddRecord(params object[] values)
        {
            //EnsureModeIsCorrect(Mode.OpenWrite, Mode.Writing);

            if (!_writingMode) StartWriting();

            _writeStream.Position = DbfHeaderStruct.SizeOf + DbfFieldHeaderStruct.SizeOf * _fields.Count + 1;

            var i = 0;
            foreach (var value in values)
            {
                var bytes = PackValues(_fields[i++], value).ToArray();
                _writeStream.Write(bytes, 0, bytes.Length);
            }

            ++RecordCount;
        }

        private static IEnumerable<byte> PackValues(DbfField field, object value)
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
                    return Encoding.Default.GetBytes($"{value:YYYYMMdd}");

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
            WriteHeader(0);
            WriteFields();
            _writingMode = true;
        }


        public override void Dispose()
        {
            WriteHeader(RecordCount);

            if (!_writingMode) WriteFields();

            _writeStream?.Dispose();
        }

        //private enum Mode
        //{
        //    Initial,
        //    OpenRead,
        //    OpenWrite,
        //    Writing,
        //    Disposed
        //}

        private void WriteHeader(int recordCount)
        {
            var h = new DbfHeaderStruct
            {
                Version = (byte)Version,
                RecordCount = (uint)recordCount,
                HeaderBytes = (ushort)(DbfHeaderStruct.SizeOf + recordCount * DbfHeaderStruct.SizeOf),
                RecordBytes = (ushort)_fields.Sum(x => x.Size),
                LastUpdateDate = DateTime.Today
            };

            _writeStream.Position = 0;

            _writeStream.WriteValue(h);
        }

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


        //private void EnsureModeIsCorrect(params Mode[] modes)
        //{
        //    if (!modes.Contains(_mode)) throw new InvalidOperationException($"{_mode}");
        //}
    }
}
