using System;
using System.Collections.Generic;
using System.Text;
using LibBSP;
using Xunit;

namespace BSPUtilsTest.LibBSP
{
    public class UtilTest
    {
        [Fact]
        public void TestRoundUp()
        {
            Assert.Equal(4, Util.RoundUp(3, 4));
            Assert.Equal(4, Util.RoundUp(4, 4));
            Assert.Equal(8, Util.RoundUp(5, 4));

            Assert.Equal(16, Util.RoundUp(5, 16));
            Assert.Equal(32, Util.RoundUp(20, 16));
        }
    }
}
