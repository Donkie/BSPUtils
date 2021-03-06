using System;
using System.IO;
using BSPUtilsTest.TestUtil;
using LibBSP;
using Xunit;

namespace BSPUtilsTest.LibBSP
{
    public class GameLumpTest : IDisposable
    {
        public void Dispose()
        {
            FileReader.Dispose();
        }

        private static readonly FileReader FileReader = new FileReader();

        [Fact]
        public void TestGameLumpAddItem()
        {
            var reader = FileReader.OpenStream("testdata/map.bsp");
            reader.BaseStream.Seek(sizeof(int) * 2 + (int) LumpType.GameLump * (sizeof(int) * 3 + 4), SeekOrigin.Begin);

            var newItem = new GameLumpItem(3, 4, 5, new byte[] {1, 3, 3, 7});

            var lump = new GameLump(reader);

            var originalLumpCount = lump.LumpItems.Count;

            lump.LumpItems.Add(newItem);

            Assert.Equal(originalLumpCount + 1, lump.LumpItems.Count);

            // Write the LumpItems table into the Data byte array, then parse the data array back into the LumpItems table
            // Will throw InvalidOperation since we edited the LumpItems but didn't call UpdateOffsets
            Assert.Throws<InvalidOperationException>(() => lump.Data);

            lump.UpdateOffsets(lump.Offset);
            var rawData = lump.Data;
            lump.Data = rawData;

            Assert.Equal(originalLumpCount + 1, lump.LumpItems.Count);
            Assert.Equal(lump.LumpItems[originalLumpCount], newItem);
        }

        [Fact]
        public void TestGameLumpItemEquality()
        {
            var item1 = new GameLumpItem(3, 4, 5, new byte[] {1, 3, 3, 7});
            var item2 = new GameLumpItem(3, 4, 5, new byte[] {1, 3, 3, 7});
            var item3 = new GameLumpItem(5, 4, 5, new byte[] {1, 3, 3, 7});

            Assert.Equal(item1, item2);
            Assert.NotEqual(item2, item3);

            Assert.IsType<int>(item1.GetHashCode());
        }

        [Fact]
        public void TestGameLumpRead()
        {
            var reader = FileReader.OpenStream("testdata/map.bsp");
            reader.BaseStream.Seek(sizeof(int) * 2 + (int) LumpType.GameLump * (sizeof(int) * 3 + 4), SeekOrigin.Begin);

            var lump = new GameLump(reader);
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
        public void TestGameLumpReadWrite()
        {
            var reader = FileReader.OpenStream("testdata/map.bsp");
            reader.BaseStream.Seek(sizeof(int) * 2 + (int) LumpType.GameLump * (sizeof(int) * 3 + 4), SeekOrigin.Begin);

            // Grab the raw lump data from the stream
            var lumpHeaderPos = reader.BaseStream.Position;
            var lumpDataPos = reader.ReadInt32();
            var lumpDataLength = reader.ReadInt32();

            var gameLumpData = new byte[lumpDataLength];
            reader.BaseStream.Seek(lumpDataPos, SeekOrigin.Begin);
            reader.BaseStream.Read(gameLumpData, 0, lumpDataLength);

            // Seek back to where we were
            reader.BaseStream.Seek(lumpHeaderPos, SeekOrigin.Begin);

            // Read the lump like normal, which also parses the raw lump data into the GameLumpItems list
            var lump = new GameLump(reader);

            // lump.Data is parsed from the GameLumpItems list on read, since we done no changes to the GameLumpItems list, this should be equal to the original raw data
            Assert.Equal(gameLumpData, lump.Data);
        }
    }
}