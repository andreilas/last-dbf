using System;

namespace LastDbf
{
    public class DbfField
    {
        public string Name { get; }

        public DbfFieldType Type { get; }

        public int Length { get; }

        public int Precision { get; }

        public DbfField(string name, DbfFieldType type, int length = 0, int precision = 0)
        {
            Name = name;
            Type = type;
            Length = length;
            Precision = precision;

            switch (type)
            {
                case DbfFieldType.Float: 
                    if (length == 0) Length = 10; 
                    break;
            }
        }

        public int Size
        {
            get
            {
                switch (Type)
                {
                    case DbfFieldType.Character: return Length;
                    case DbfFieldType.Date: return 8;
                    case DbfFieldType.Float: return 10;
                    case DbfFieldType.Numeric: return 10;
                    case DbfFieldType.Logical: return 1;
                    case DbfFieldType.Integer: return 4;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            /*
Type code	Type	Sample value	Description
C	Character	HELLO 	A string of characters, padded with spaces if shorter than the field length
D	Date	19990703	Date stored as string in YYYYMMDD format
F	Float	-492.58	Floating point number, stored as string, padded with spaces if shorter than the field length
N	Numeric	-492.58	Floating point number, stored as string, padded with spaces if shorter than the field length
L	Logical	Y	A boolean value, stored as one of YyNnTtFf. May be set to ? if not initialized

Visual FoxPro field types
Type code	Type	Sample value	Description
T	DateTime	459599234239	A date and time, stored as a number (see below, under record reading)
I	Integer	340	Integer value, stored as a little endian 32-bit value, with the highest bit used to negate numbers
Y	Currency	99.5	Floating point number, stored as binary in (usually) 8 bytes.
            */
        }

        internal int Displacement { get; set; }

        public override bool Equals(object obj) => obj is DbfField f && f.Name == Name && f.Type == Type && f.Length == Length && f.Precision == Precision;

        public override int GetHashCode() => unchecked(Name.GetHashCode() + Type.GetHashCode() + Length.GetHashCode() + Precision.GetHashCode());

        public override string ToString() => $"{Type}({Length}, {Precision}) {Name}";
    }
}