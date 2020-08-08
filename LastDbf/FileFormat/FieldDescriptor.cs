using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;

namespace LastDbf
{
    [StructLayout(LayoutKind.Explicit)]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
    internal struct FieldDescriptor
    {
        /*
    Offset	Size	Type	Sample value	Description
    0x00	11	string	PRODID 	Field name (padded with NULL-bytes)
    0x0b	1	char	C	Field type
    0x0c	4	uint32	 	Field data address in memory
    0x10	1	byte	10	Field length
    0x11	1	byte	4	Field decimal count
    0x12	2	 	 	Reserved for dBASE IIIPlus/LAN
    0x14	1	byte	 	Work area ID
    0x15	2	 	 	Reserved for dBASE IIIPlus/LAN
    0x17	1	byte	 	SET FIELDS flag
    0x18	8	 	 	Reserved
         */
        [FieldOffset(0x00)] private byte NameByte0;
        [FieldOffset(0x01)] private byte NameByte1;
        [FieldOffset(0x02)] private byte NameByte2;
        [FieldOffset(0x03)] private byte NameByte3;
        [FieldOffset(0x04)] private byte NameByte4;
        [FieldOffset(0x05)] private byte NameByte5;
        [FieldOffset(0x06)] private byte NameByte6;
        [FieldOffset(0x07)] private byte NameByte7;
        [FieldOffset(0x08)] private byte NameByte8;
        [FieldOffset(0x09)] private byte NameByte9;
        [FieldOffset(0x10)] private byte NameByteA;

        [FieldOffset(0x0b)] public byte FieldType;

        [FieldOffset(0x0c)] public UInt32 Displacement;

        [FieldOffset(0x10)] public byte FieldLength;
        [FieldOffset(0x11)] public byte DecimalCount;

        [FieldOffset(0x12)] private byte Reserved3Plus2A;

        [FieldOffset(0x14)] private byte WorkAreaID;

        [FieldOffset(0x15)] private byte Reserved3Plus2B;

        [FieldOffset(0x17)] private byte SetFieldsFlag;
        [FieldOffset(0x18)] private byte Reserved;
        [FieldOffset(0x18 + 8 - 1)] private byte LastByte;

        public static int SizeOf => Marshal.SizeOf(typeof(FieldDescriptor));

        public string FieldName
        {
            get
            {
                var bytes = new byte[]
                {
                    NameByte0,
                    NameByte1,
                    NameByte2,
                    NameByte3,
                    NameByte4,
                    NameByte5,
                    NameByte6,
                    NameByte7,
                    NameByte8,
                    NameByte9,
                    NameByteA,
                    0 // C-lang end of string mark
                };

                return string.Join("", bytes.TakeWhile(x => x != 0).Select(x => (char)x).ToArray());
            }

            set
            {
                var i = 0;
                foreach (var c in value.ToCharArray()) SetByte(i++, (byte)c);
                if (i < 11) SetByte(i, 0);
            }
        }

        private void SetByte(int i, byte b)
        {
            // NB: I do not want using unprotected code!
            switch (i)
            {
                case 0: NameByte0 = b; break;
                case 1: NameByte1 = b; break;
                case 2: NameByte2 = b; break;
                case 3: NameByte3 = b; break;
                case 4: NameByte4 = b; break;
                case 5: NameByte5 = b; break;
                case 6: NameByte6 = b; break;
                case 7: NameByte7 = b; break;
                case 8: NameByte8 = b; break;
                case 9: NameByte9 = b; break;
            }
        }
    }
}
