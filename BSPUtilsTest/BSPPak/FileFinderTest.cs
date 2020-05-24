using System.Linq;
using BSPPak;
using App = BSPLumpExtract;
using Xunit;

namespace BSPUtilsTest.BSPPak
{
    public class FileFinderTest
    {
        [Fact]
        public void TestFileFinder()
        {
            var files = FileFinder.Find("testdata");

            Assert.Equal(1, files.Count);
            Assert.EndsWith("map.vmf", files.First());
        }
    }
}
