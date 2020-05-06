using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xunit;
using LibBSP;

namespace BSPUtilsTest.LibBSP
{
    public class GameLumpTest : IDisposable
    {
        private static BinaryReader _reader;

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
        public void TestGameLumpItem(BinaryReader reader)
        {
            var lumpItem = GameLumpItem.FromStream(reader);
            Assert.Equal(19320, lumpItem.ID);
        }

        public static IEnumerable<object[]> GameLumpTestData()
        {
            OpenStream();
            _reader.BaseStream.Seek(sizeof(int) * 2 + (int) LumpType.GameLump * (sizeof(int) * 3 + 4), SeekOrigin.Begin);

            return new List<object[]>
            {
                new object[]{_reader}
            };
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
