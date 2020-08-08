namespace LastDbf
{
    public enum DbfFieldType : byte
    {
        Character = (byte)'C',
        Date = (byte)'D',
        Float = (byte)'F',
        Numeric = (byte)'C',
        Logical = (byte)'Y',

        Integer = (byte)'I',

        /*
C	Character	HELLO 	A string of characters, padded with spaces if shorter than the field length
D	Date	19990703	Date stored as string in YYYYMMDD format
F	Float	-492.58	Floating point number, stored as string, padded with spaces if shorter than the field length
N	Numeric	-492.58	Floating point number, stored as string, padded with spaces if shorter than the field length
L	Logical	Y	A boolean value, stored as one of YyNnTtFf. May be set to ? if not initialized
         */
    }
}