using System;
using System.IO;
using BSPUtilsTest.TestUtil;
using LibBSP;
using Xunit;

namespace BSPUtilsTest.LibBSP
{
    public class LumpTest : IDisposable
    {
        public void Dispose()
        {
            FileReader.Dispose();
        }

        private static readonly FileReader FileReader = new FileReader();

        [Fact]
        public void TestClearLump()
        {
            // Read BSP, clear lump, write BSP, read again, confirm lump is cleared
            var reader = FileReader.OpenStream("map.bsp");
            var bsp = new BSP(reader);

            var lump = bsp.Lumps[(int) LumpType.Entities];
            var lumpSize = lump.Data.Length;
            lump.Clear();

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            bsp.WriteBSP(writer);

            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            using var reader2 = new BinaryReader(ms);
            var bsp2 = new BSP(reader2);

            Assert.Single(bsp2.Lumps[(int) LumpType.Entities].Data);
        }
    }
}