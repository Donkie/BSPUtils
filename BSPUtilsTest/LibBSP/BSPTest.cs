using System;
using System.IO;
using BSPUtilsTest.TestUtil;
using LibBSP;
using Xunit;

namespace BSPUtilsTest.LibBSP
{
    public class BSPTest : IDisposable
    {
        public void Dispose()
        {
            FileReader.Dispose();
        }

        private static readonly FileReader FileReader = new FileReader();

        [Fact]
        public void TestBSPByFilename()
        {
            var bsp = new BSP("testdata/map.bsp");
            Assert.NotNull(bsp);
        }

        [Fact]
        public void TestBSPCustomLumpTypes()
        {
            var reader = FileReader.OpenStream("map.bsp");
            var bsp = new BSP(reader);

            Assert.IsType<GameLump>(bsp.Lumps[(int) LumpType.GameLump]);
            Assert.IsType<PakfileLump>(bsp.Lumps[(int) LumpType.Pakfile]);
        }

        [Fact]
        public void TestBSPFileWrite()
        {
            string fileName = Path.GetTempPath() + Guid.NewGuid() + ".bsp";

            // Read BSP and then write to a temp file
            var reader = FileReader.OpenStream("map.bsp");
            var bsp1 = new BSP(reader);
            reader.Dispose();

            bsp1.WriteBSP(fileName);

            // Re-open the temp file and do basic consistency check
            using var fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
            using var reader2 = new BinaryReader(fs);
            var bsp2 = new BSP(reader2);
            reader2.Dispose();

            Assert.Equal(bsp1.Version, bsp2.Version);

            File.Delete(fileName);
        }

        [Fact]
        public void TestBSPGameDataContent()
        {
            var reader = FileReader.OpenStream("map.bsp");
            var bsp = new BSP(reader);
            var lump = (GameLump) bsp.Lumps[(int) LumpType.GameLump];

            Assert.Equal(2, lump.LumpItems.Count);

            Assert.Equal(0x73707270, lump.LumpItems[0].ID);
            Assert.Equal(0, lump.LumpItems[0].Flags);
            Assert.Equal(6, lump.LumpItems[0].Version);
            Assert.Equal(12, lump.LumpItems[0].Data.Length);
            Assert.Equal(new byte[] {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0}, lump.LumpItems[0].Data);

            Assert.Equal(0x64707270, lump.LumpItems[1].ID);
            Assert.Equal(0, lump.LumpItems[1].Flags);
            Assert.Equal(4, lump.LumpItems[1].Version);
            Assert.Equal(12, lump.LumpItems[1].Data.Length);
            Assert.Equal(new byte[] {0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0}, lump.LumpItems[1].Data);
        }

        [Fact]
        public void TestBSPHeader()
        {
            var reader = FileReader.OpenStream("map.bsp");
            var bsp = new BSP(reader);

            Assert.Equal(20, bsp.Version);
            Assert.Equal(1, bsp.Revision);
        }

        [Fact]
        public void TestBSPWrite()
        {
            using var ms = new MemoryStream();
            using var binaryWriter = new BinaryWriter(ms);

            // Read and then write the BSP to the memory stream
            var reader = FileReader.OpenStream("map.bsp");
            var bsp1 = new BSP(reader);
            reader.Dispose();

            bsp1.WriteBSP(binaryWriter);

            binaryWriter.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            // Now read the BSP again from the memory stream, we can then compare them to make sure the write went correct
            reader = new BinaryReader(ms);
            var bsp2 = new BSP(reader);
            reader.Dispose();

            var gl1 = (GameLump) bsp1.Lumps[(int) LumpType.GameLump];
            var gl2 = (GameLump) bsp2.Lumps[(int) LumpType.GameLump];
            Assert.Equal(gl1.LumpItems, gl2.LumpItems);

            var pak1 = (PakfileLump) bsp1.Lumps[(int) LumpType.Pakfile];
            var pak2 = (PakfileLump) bsp2.Lumps[(int) LumpType.Pakfile];
            Assert.Equal(pak1.Data, pak2.Data);
        }

        [Fact]
        public void TestLMPFileWrite()
        {
            string fileName = Path.GetTempPath() + Guid.NewGuid() + ".lmp";

            // Read BSP and then write to a temp file
            var reader = FileReader.OpenStream("map.bsp");
            var bsp1 = new BSP(reader);
            reader.Dispose();

            bsp1.WriteLMP(fileName, (int) LumpType.GameLump);

            // Re-open the temp file and do basic consistency check
            using var fs = File.Open(fileName, FileMode.Open, FileAccess.Read);
            using var reader2 = new BinaryReader(fs);

            Assert.Equal(20, reader2.ReadInt32()); // Lump header size
            Assert.Equal((int) LumpType.GameLump, reader2.ReadInt32()); // Lump index/type
            Assert.Equal(0, reader2.ReadInt32()); // Lump version
            Assert.Equal(60, reader2.ReadInt32()); // Lump data size
            Assert.Equal(1, reader2.ReadInt32()); // BSP revision

            reader2.Dispose();
            File.Delete(fileName);
        }

        [Fact]
        public void TestLMPWrite()
        {
            var reader1 = FileReader.OpenStream("map.bsp");
            var bsp = new BSP(reader1);

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            bsp.WriteLMP(writer, (int) LumpType.GameLump);
            writer.Flush();

            ms.Seek(0, SeekOrigin.Begin);

            using var reader2 = new BinaryReader(ms);

            Assert.Equal(20, reader2.ReadInt32()); // Lump header size
            Assert.Equal((int) LumpType.GameLump, reader2.ReadInt32()); // Lump index/type
            Assert.Equal(0, reader2.ReadInt32()); // Lump version
            Assert.Equal(60, reader2.ReadInt32()); // Lump data size
            Assert.Equal(1, reader2.ReadInt32()); // BSP revision

            Assert.Equal(60, ms.Length - ms.Position); // Data
        }

        [Fact]
        public void TestNotVBSP()
        {
            var reader = FileReader.OpenStream("map.vmf");
            Assert.Throws<FileFormatException>(() => new BSP(reader));
        }
    }
}