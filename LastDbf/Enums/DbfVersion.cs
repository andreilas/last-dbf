using System.Diagnostics.CodeAnalysis;

namespace LastDbf
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum DbfVersion
    {
        dBASE_III = 0x03,
        dBASE_IV = 0x04,
        dBASE_IV_SQL_Table = 0x43

        /*
0x02	0000 0010	FoxBase 1.0
0x03	0000 0011	FoxBase 2.x / dBASE III
0x83	1000 0011	FoxBase 2.x / dBASE III with memo file
0x30	0011 0000	Visual FoxPro
0x31	0011 0001	Visual FoxPro with auto increment
0x32	0011 0010	Visual FoxPro with varchar/varbinary
0x43	0100 0011	dBASE IV SQL Table, no memo file
0x63	0110 0011	dBASE IV SQL System, no memo file
0x8b	1000 1011	dBASE IV with memo file
0xcb	1100 1011	dBASE IV SQL Table with memo file
0xfb	1111 1011	FoxPro 2
0xf5	1111 0101	FoxPro 2 with memo file
         */
    }
}