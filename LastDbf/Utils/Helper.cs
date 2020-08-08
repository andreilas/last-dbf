using System;
using System.IO;
using System.Runtime.InteropServices;

namespace LastDbf
{
    internal static class Helper
    {
        public static object ReadValue(this Stream stream, Type t)
        {
            var buffer = new byte[Marshal.SizeOf(t)];
            for (var read = 0; read < buffer.Length; read += stream.Read(buffer, read, buffer.Length)) { }
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), t);
            }
            finally
            {
                gcHandle.Free();
            }
        }

        public static void WriteValue(this Stream stream, object o)
        {
            var buffer = new byte[Marshal.SizeOf(o.GetType())];
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(o, gcHandle.AddrOfPinnedObject(), true);
                stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                gcHandle.Free();
            }
        }

        public static int SwapEndianness(int value)
        {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;

            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
        }
    }
}