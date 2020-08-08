using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LastDbf
{
    public class DbfReader : DbfBase
    {
        public readonly DbfVersion Version;

        public int Records { get; }

        private readonly FileStream _readStream;
        
        public DbfReader(string path)
        {
            _readStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            var header = (DbfHeaderStruct)_readStream.Read(typeof(DbfHeaderStruct));
            Version = (DbfVersion)header.Version;
            Records = (int)header.Records;

            Fields = new ReadOnlyCollection<DbfField>(ReadHeaders().ToList());

            IEnumerable<DbfField> ReadHeaders()
            {
                while (true)
                {
                    var position = _readStream.Position;
                    if (_readStream.ReadByte() == 0x0d) break;
                    _readStream.Position = position;

                    var f = (DbfFieldStruct)_readStream.Read(typeof(DbfFieldStruct));

                    yield return new DbfField(f.FieldName, (DbfFieldType)f.FieldType, f.FieldLength, f.DecimalCount);
                }
            }
        }
        
        public object[] Read()
        {
            return null;
        }


        public override void Dispose()
        {
            _readStream.Dispose();
        }
    }
}