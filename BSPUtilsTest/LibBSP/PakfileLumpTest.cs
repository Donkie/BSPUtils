using System;
using System.IO;
using System.IO.Compression;
using BSPUtilsTest.TestUtil;
using LibBSP;
using Xunit;

namespace BSPUtilsTest.LibBSP
{
    public class PakfileLumpTest : IDisposable
    {
        public void Dispose()
        {
            FileReader.Dispose();
        }

        private static readonly FileReader FileReader = new FileReader();

        [Fact]
        public void TestPakfileLump()
        {
            var reader = FileReader.OpenStream("testdata/map.bsp");
            reader.BaseStream.Seek(sizeof(int) * 2 + (int) LumpType.Pakfile * (sizeof(int) * 3 + 4), SeekOrigin.Begin);

            var lump = new PakfileLump(reader);

            // Can't close an archive stream if none is opened
            Assert.Throws<InvalidOperationException>(() => lump.CloseArchiveStream());

            var archive = lump.OpenArchiveStream(ZipArchiveMode.Update);

            // Can't open another stream
            Assert.Throws<InvalidOperationException>(() => lump.OpenArchiveStream(ZipArchiveMode.Update));

            Assert.Equal(4, archive.Entries.Count);

            // Add item to the archive
            var entry = archive.CreateEntry("test", CompressionLevel.NoCompression);
            var stream = entry.Open();
            stream.WriteByte(123);
            stream.Dispose();

            // Close and re-open the archive to confirm that the data was written properly
            lump.CloseArchiveStream();
            archive = lump.OpenArchiveStream(ZipArchiveMode.Read);

            Assert.Equal(5, archive.Entries.Count);

            // Fetch the same entry again and confirm that the data is intact
            entry = archive.GetEntry("test");
            Assert.NotNull(entry);
            stream = entry.Open();

            Assert.Equal(1, stream.Length);
            Assert.Equal(123, stream.ReadByte());

            lump.CloseArchiveStream();
        }
    }
}