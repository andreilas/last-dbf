using System;
using System.Collections.Generic;

namespace LastDbf
{
    public abstract class DbfBase : IDisposable
    {
        public IReadOnlyList<DbfField> Fields { get; protected set; }
        
        public int RecordCount { get; protected set; }

        public abstract void Dispose();
    }
}