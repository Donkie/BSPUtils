using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BSPUtilsTest.TestUtil;
using LibBSP;
using Xunit;
using App = BSPPak;
using Program = BSPPak.Program;
using Options = BSPPak.Program.Options;

namespace BSPUtilsTest.BSPPak
{
    public class AppTest
    {
        [Fact]
        public void TestValidateValidOptions()
        {
            var opts = new Options
            {
                BSPPath = "testdata/map.bsp",
                ContentPath = "testdata"
            };
            Program.ValidateOptions(opts); // Should not throw anything
        }

        [Fact]
        public void TestValidateInvalidBSPPath()
        {
            var opts = new Options
            {
                BSPPath = "testdata/invalidFile.bsp",
                ContentPath = "testdata"
            };
            Assert.Throws<App.InvalidOptionException>(() => Program.ValidateOptions(opts));
        }

        [Fact]
        public void TestValidateInvalidContentPath()
        {
            var opts = new Options
            {
                BSPPath = "testdata/map.bsp",
                ContentPath = "testdataInvalid"
            };
            Assert.Throws<App.InvalidOptionException>(() => Program.ValidateOptions(opts));
        }

        [Fact]
        public void TestPackFiles()
        {
            using var temp = new TempFolder();
            var bspPath = $"{temp.FolderPath}/map.bsp";
            File.Copy("testdata/map.bsp", bspPath);

            var filesToPack = new[]
            {
                "testdata/map.vmf"
            };

            // Pack the files in the filesToPackList
            Program.PackFiles(new Options
            {
                BSPPath = bspPath,
                ContentPath = "testdata"
            }, filesToPack);

            // Load the BSP to make sure we can find the files

            var bsp = new BSP(bspPath);

            var pakLump = (PakfileLump)bsp.Lumps[(int)LumpType.Pakfile];
            var archive = pakLump.OpenArchiveStream(ZipArchiveMode.Read);

            // Intersect the names of the paklump's entries with our set of files. The resulting list should be of equal length as our filesToPack if all of them exist in the lump.
            Assert.Equal(filesToPack.Length, 
                archive.Entries
                    .Select(entry => entry.FullName)
                    .Intersect(filesToPack.Select(str => Program.ToRelativePath(str, "testdata"))).Count());

            pakLump.CloseArchiveStream();
        }

        [Fact]
        public void TestRelativePath()
        {
            Assert.Equal(@"Path.txt", Program.ToRelativePath(@"C:\Test\Path.txt", @"C:\Test"));
            Assert.Equal(@"Path.txt", Program.ToRelativePath(@"C:\Test\Path.txt", @"C:\Test\"));
            Assert.Equal(@"Path.txt", Program.ToRelativePath(@"C:/Test/Path.txt", @"C:/Test"));
            Assert.Equal(@"Path.txt", Program.ToRelativePath(@"C:/Test/Path.txt", @"C:/Test/"));

            Assert.Throws<ArgumentException>(() => { Program.ToRelativePath(@"C:\Test\Path.txt", @"C:\Hello"); });
        }

    }
}
