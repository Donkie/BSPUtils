using System.IO;
using System.Linq;
using MAB.DotIgnore;

namespace BSPUtils
{
    public class FileFinder
    {
        private const string FilterFileName = ".pakfilter";

        public static bool HasFilterFile(string dir)
        {
            return File.Exists(Path.Combine(dir, FilterFileName));
        }

        public static string[] Find(string dir, bool useFilterFile)
        {
            if(useFilterFile && !HasFilterFile(dir))
                throw new FileNotFoundException($"Tried finding files using a filter file but {FilterFileName} is missing.", FilterFileName);

            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            if (!useFilterFile)
                return files;

            var ignoreList = new IgnoreList(Path.Combine(dir, FilterFileName));
            return files.Where(file => ignoreList.IsIgnored(file, false)).ToArray();
        }
    }
}
