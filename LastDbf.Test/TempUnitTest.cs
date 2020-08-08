using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LastDbf.Test
{
    [TestClass]
    public class TempUnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            using var m = new MemoryStream();

            var data = new MyStruct { FirstByte = 0xA, LastByte = 0xB };

            Write(m, data);

            var bytes = m.ToArray();

            Assert.AreEqual(0x18 + 8, bytes.Length);
            Assert.AreEqual(data.FirstByte, bytes[0]);
            Assert.AreEqual(data.LastByte, bytes[0x18 + 8 - 1]);
        }

        [Serializable]
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi, Pack = 1)]
        private struct MyStruct
        {
            [FieldOffset(0x00)] public byte FirstByte;

            [FieldOffset(0x18 + 8 - 1)] public byte LastByte;
        }

        private static object Read(Stream stream, Type t)
        {
            var buffer = new byte[Marshal.SizeOf(t)];
            for (var read = 0; read < buffer.Length; read += stream.Read(buffer, read, buffer.Length)) { }
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var o = Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), t);
            gcHandle.Free();
            return o;
        }

        private static void Write(Stream stream, object o)
        {
            var buffer = new byte[Marshal.SizeOf(o.GetType())];
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(o, gcHandle.AddrOfPinnedObject(), true);
            stream.Write(buffer, 0, buffer.Length);
            gcHandle.Free();

        }
    }
}