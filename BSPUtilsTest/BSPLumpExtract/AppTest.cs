using System;
using System.IO;
using BSPUtilsTest.TestUtil;
using Xunit;
using App = BSPLumpExtract;
using Program = BSPLumpExtract.Program;
using Options = BSPLumpExtract.Program.Options;

namespace BSPUtilsTest.BSPLumpExtract
{
    public class AppTest
    {
        [Fact]
        public void TestValidateValidOptions()
        {
            var opts = new Options
            {
                BSPPath = "testdata/map.bsp",
                LumpIndex = 1
            };
            Program.ValidateOptions(opts); // Should not throw anything
        }

        [Fact]
        public void TestValidateInvalidFile()
        {
            var opts = new Options
            {
                BSPPath = "testdata/invalidFile.bsp"
            };
            Assert.Throws<App.InvalidOptionException>(() => Program.ValidateOptions(opts));
        }

        [Fact]
        public void TestValidateInvalidLump()
        {
            var opts = new Options
            {
                BSPPath = "testdata/map.bsp",
                LumpIndex = -1
            };
            Assert.Throws<App.InvalidOptionException>(() => Program.ValidateOptions(opts));

            opts.LumpIndex = 64;
            Assert.Throws<App.InvalidOptionException>(() => Program.ValidateOptions(opts));
        }

        [Fact]
        public void TestMalformedBSPPath()
        {
            // Throw on empty path
            Assert.Throws<ArgumentException>(() => Program.ExtractLump(new Options
            {
                BSPPath = ""
            }));

            // Throw on directory
            Assert.Throws<ArgumentException>(() => Program.ExtractLump(new Options
            {
                BSPPath = "testdata"
            }));
        }

        [Fact]
        public void TestBackup()
        {
            using var temp = new TempFolder();
            var bspPath = $"{temp.FolderPath}/map.bsp";
            File.Copy("testdata/map.bsp", bspPath);

            var origChecksum = FileUtils.ComputeChecksum(bspPath);
            var origEditDate = File.GetLastWriteTime(bspPath);
            
            Program.ExtractLump(new Options
            {
                BSPPath = bspPath,
                MakeBackup = true
            });

            var backupBspPath = $"{temp.FolderPath}/map.bsp.orig";
            Assert.True(File.Exists(backupBspPath));
            Assert.Equal(origChecksum, FileUtils.ComputeChecksum(backupBspPath));

            // Should only make a backup if necessary, that is, if the file was modified
            Assert.NotEqual(origEditDate, File.GetLastWriteTime(bspPath));
        }

        [Fact]
        public void TestLmpFileDontExtract()
        {
            using var temp = new TempFolder();
            var bspPath = $"{temp.FolderPath}/map.bsp";
            File.Copy("testdata/map.bsp", bspPath);

            const int lumpIndex = 0;
            Program.ExtractLump(new Options
            {
                BSPPath = bspPath,
                LumpIndex = lumpIndex,
                DontExtract = true
            });

            var lumpFileName = $"map_l_{lumpIndex}.lmp";
            var lumpPath = $"{temp.FolderPath}/{lumpFileName}";

            Assert.False(File.Exists(lumpPath));
        }

        [Fact]
        public void TestLmpFileExtract()
        {
            using var temp = new TempFolder();
            var bspPath = $"{temp.FolderPath}/map.bsp";
            File.Copy("testdata/map.bsp", bspPath);

            const int lumpIndex = 0;
            Program.ExtractLump(new Options
            {
                BSPPath = bspPath,
                LumpIndex = lumpIndex
            });

            var lumpFileName = $"map_l_{lumpIndex}.lmp";
            var lumpPath = $"{temp.FolderPath}/{lumpFileName}";

            Assert.True(File.Exists(lumpPath));
        }

        [Fact]
        public void TestLmpClear()
        {
            using var temp = new TempFolder();
            var bspPath = $"{temp.FolderPath}/map.bsp";
            File.Copy("testdata/map.bsp", bspPath);

            const int lumpIndex = 0;
            Program.ExtractLump(new Options
            {
                BSPPath = bspPath,
                LumpIndex = lumpIndex
            });

            // Trying to extract it twice should result in the second one failing
            Assert.Throws<App.LumpEmptyException>(() =>
            {
                Program.ExtractLump(new Options
                {
                    BSPPath = bspPath,
                    LumpIndex = lumpIndex
                });
            });
        }

        [Fact]
        public void TestLmpDontClear()
        {
            using var temp = new TempFolder();
            var bspPath = $"{temp.FolderPath}/map.bsp";
            File.Copy("testdata/map.bsp", bspPath);

            const int lumpIndex = 0;
            Program.ExtractLump(new Options
            {
                BSPPath = bspPath,
                LumpIndex = lumpIndex,
                DontClear = true
            });

            // It should be okay to run it twice if we don't clear it the first time
            Program.ExtractLump(new Options
            {
                BSPPath = bspPath,
                LumpIndex = lumpIndex
            });
        }
    }
}
