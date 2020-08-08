using System.Collections.Generic;

namespace LastDbf
{
    public static class DbfExtensions
    {
        public static IEnumerable<object[]> Records(this DbfReader dbf, bool skipDeleted = true)
        {
            dbf.Reset();
            while (true)
            {
                var values = dbf.Read(skipDeleted);
                if(values == null) break;
                yield return values;
            }
        }
    }
}