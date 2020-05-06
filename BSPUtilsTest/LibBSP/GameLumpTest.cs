using System;
using System.Collections.Generic;
using System.IO;
using LibBSP;
using Xunit;

namespace BSPUtilsTest.LibBSP
{
    public class GameLumpTest : IDisposable
    {
        private static BinaryReader _reader;

        public void Dispose()
        {
            _reader?.Dispose();
        }

        private static void OpenStream()
        {
            if (_reader != null)
                return;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            var stream = File.Open(@"testdata/map.bsp", FileMode.Open, FileAccess.Read);
            _reader = new BinaryReader(stream);
        }

        [Theory]
        [MemberData(nameof(GameLumpTestData))]
        public void TestGameLump(BinaryReader reader)
        {
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

        public static IEnumerable<object[]> GameLumpTestData()
        {
            OpenStream();
            _reader.BaseStream.Seek(sizeof(int) * 2 + (int) LumpType.GameLump * (sizeof(int) * 3 + 4), SeekOrigin.Begin);

            return new List<object[]>
            {
                new object[] {_reader}
            };
        }
    }
}