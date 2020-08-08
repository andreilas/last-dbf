using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace LastDbf
{
    [StructLayout(LayoutKind.Explicit)]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    internal struct Header
    {
        [FieldOffset(0x00)] public byte Version;

        [FieldOffset(0x01)] private byte YY;
        [FieldOffset(0x02)] private byte MM;
        [FieldOffset(0x03)] private byte DD;

        [FieldOffset(0x04)] public UInt32 RecordCount;
        [FieldOffset(0x08)] public UInt16 HeaderBytes;
        [FieldOffset(0x0a)] public UInt16 RecordBytes;

        [FieldOffset(0x0c)] private byte Reserved3;
        [FieldOffset(0x0f)] private byte Reserved3Plus0;
        [FieldOffset(0x1c)] private byte Reserved4;

        [FieldOffset(0x20 - 1)] private byte LastByte;

        /*
Offset	Size	Type	Sample value	Description
0x00	1	byte	0x03	Version byte
0x01	3	string	990307	Date of last update in YYMMDD format (where YY is equal to year minus 1900)
0x04	4	uint32	15	Number of records in table
0x08	2	uint16	400	Number of bytes in the header
0x0a	2	uint16	64	Number of bytes in a record
0x0c	3	 	 	Reserved
0x0f	13	 	 	Reserved for dBASE IIIPlus/LAN
0x1c	4	 	 	Reserved
0x20	 	 	 	Field descriptors
…	1	byte	0x0d	Field terminator
         */

        public static int SizeOf => Marshal.SizeOf(typeof(Header));

        public DateTime LastUpdateDate
        {
            get => new DateTime(YY + 1900, MM, DD);
            set
            {
                YY = (byte)(value.Year - 1900);
                MM = (byte)value.Month;
                DD = (byte)value.Day;
            }
        }
    }
}