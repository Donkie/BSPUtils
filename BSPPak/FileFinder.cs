using System.Collections.Generic;
using System.IO;
using System.Linq;
using MAB.DotIgnore;

namespace BSPPak
{
    /// <summary>
    /// Library used to find and filter files in a directory
    /// </summary>
    public static class FileFinder
    {
        private const string FilterFileName = ".pakfilter";

        /// <summary>
        /// Returns whether the specified directory has a ".pakfilter" file or not
        /// </summary>
        /// <param name="dir">The directory to check</param>
        /// <returns></returns>
        private static bool HasFilterFile(string dir)
        {
            return File.Exists(Path.Combine(dir, FilterFileName));
        }

        /// <summary>
        /// Returns an array of files contained in the specified directory and recursively in sub-directories.
        /// If the directory has a ".pakfilter" file, it will be used to filter which files to find.
        /// The .pakfilter file contains a list of filtering rules with the same syntax as the .gitignore file in Git. However,
        /// contrary to the .gitignore file which is a blacklist, the .pakfilter file is a whitelist.
        /// </summary>
        /// <param name="dir">The directory to find files in</param>
        /// <returns>An array of absolute file paths</returns>
        public static IReadOnlyCollection<string> Find(string dir)
        {
            var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            if (!HasFilterFile(dir))
                return files;

            var ignoreList = new IgnoreList(Path.Combine(dir, FilterFileName));
            return files.Where(file => ignoreList.IsIgnored(file, false)).ToList();
        }
    }
}