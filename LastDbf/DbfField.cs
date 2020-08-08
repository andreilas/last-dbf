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

        public override bool Equals(object obj) => obj is DbfField f && f.Name == Name && f.Type == Type && f.Length == Length && f.Precision == Precision;

        public override int GetHashCode() => unchecked(Name.GetHashCode() + Type.GetHashCode() + Length.GetHashCode() + Precision.GetHashCode());

        public override string ToString() => $"{Type}({Length}, {Precision}) {Name}";
    }
}