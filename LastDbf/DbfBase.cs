using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace LastDbf
{
    public class DbfBase : IDisposable
    {
        public IReadOnlyList<DbfField> Fields { get; private set; }

        public IReadOnlyList<object[]> Records { get; private set; }

        public DbfVersion Version;

        private List<DbfField> _fields = new List<DbfField>();

        public void Create(string path, DbfVersion version)
        {
            Fields = _fields;
            Records = _records;
        }

        public void Open(string path)
        {
            //File file = File.Open(path, )

            Fields = _fields;
            Records = _records;
        }
        
        public void AddField(DbfField field)
        {
            _fields.Add(field);
        }

        private readonly List<object[]> _records = new List<object[]>();

        public void AddRecord(params object[] values)
        {
            _records.Add(values);
        }

        public void Dispose()
        {
        }
    }
}
