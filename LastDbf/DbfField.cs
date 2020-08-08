namespace LastDbf
{
    public class DbfField
    {
        public string Name { get; }

        public DbfFieldType Type { get; }

        public int Length { get; }

        public int Precision { get; }

        public DbfField(string name, DbfFieldType type, int length = -1, int precision = -1)
        {
            Name = name;
            Type = type;
            Length = length;
            Precision = precision;
        }
    }
}