using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace LastDbf
{
    public class DbfReader : DbfBase
    {
        public readonly DbfVersion Version;

        private readonly FileStream _readStream;

        private readonly int _dataOffset;
        private readonly int _recordSize;

        private int _recordIndex;

        public DbfReader(string path)
        {
            _readStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var header = (DbfHeaderStruct)_readStream.Read(typeof(DbfHeaderStruct));
            Version = (DbfVersion)header.Version;
            RecordCount = (int)header.RecordCount;
            _dataOffset = header.HeaderBytes;
            _recordSize = header.RecordBytes;

            Fields = new ReadOnlyCollection<DbfField>(ReadFieldHeaders().ToList());

            IEnumerable<DbfField> ReadFieldHeaders()
            {
                while (true)
                {
                    var position = _readStream.Position;
                    if (_readStream.ReadByte() == 0x0d) break;
                    _readStream.Position = position;

                    var f = (DbfFieldHeaderStruct)_readStream.Read(typeof(DbfFieldHeaderStruct));

                    yield return new DbfField(f.FieldName, (DbfFieldType)f.FieldType, f.FieldLength, f.DecimalCount);
                }
            }
        }

        public object[] Read(bool skipDeleted = true)
        {
            if (_recordIndex >= RecordCount) return null;

            var bytes = new byte[_recordSize];

            while (true)
            {
                if (_recordIndex >= RecordCount) return null;

                _readStream.Position = _dataOffset + _recordIndex * _recordSize;
                var read = _readStream.Read(bytes, 0, _recordSize);

                if (read < _recordSize)
                {
                    _recordIndex = RecordCount;
                    return null;
                }

                ++_recordIndex;

                if (!skipDeleted || bytes[0] == ' ') break;
            } 
            
            return UnpackRecord(bytes).ToArray();
        }

        private IEnumerable<object> UnpackRecord(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));

            var deleted = reader.ReadChar();
            yield return deleted != ' ';

            foreach (var field in Fields)
            {

                switch (field.Type)
                {
                    case DbfFieldType.Character:
                        {
                            var unpackRecord = ReadString(field.Length);
                            yield return unpackRecord;
                            break;
                        }
                    case DbfFieldType.Date:
                        {
                            var v = ReadString(field.Size);
                            var match = Regex.Match(v, @"^(\d\d\d\d)(\d\d)(\d\d)$");
                            if (!match.Success) throw new InvalidDataException($"{field}: '{v}'");

                            var g = match.Groups;
                            var date = new DateTime(ParseGroup(1), ParseGroup(2), ParseGroup(3));
                            yield return date;
                            break;

                            int ParseGroup(int n) => int.Parse(match.Groups[n].Value);

                        }
                    case DbfFieldType.Numeric:
                        {
                            var v = ReadString(10).Trim();
                            yield return decimal.Parse(v, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        }
                    case DbfFieldType.Float:
                        {
                            var v = ReadString(10).Trim();
                            yield return decimal.Parse(v, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        }
                    case DbfFieldType.Logical:
                        {
                            var y = char.ToLower((char)reader.ReadByte());
                            yield return y == 'y' || y == 't';
                            break;
                        }
                    case DbfFieldType.Integer:
                        throw new ArgumentOutOfRangeException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            string ReadString(int length) => string.Join("", reader.ReadChars(length));
        }

        public override void Dispose()
        {
            _readStream.Dispose();
        }

        public void Reset() => _recordIndex = 0;
    }
}