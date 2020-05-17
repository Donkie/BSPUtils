using System;
using System.Collections.Generic;
using System.Text;
using BSPUtilsTest.TestUtil;
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

    }
}
