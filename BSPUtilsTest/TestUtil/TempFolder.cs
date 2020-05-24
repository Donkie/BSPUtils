using System;
using System.IO;
using System.Linq;

namespace BSPUtilsTest.TestUtil
{
    /// <summary>
    /// Represents a temporary folder on disk which is cleared when the object is disposed
    /// </summary>
    public class TempFolder : IDisposable
    {
        private static readonly Random Random = new Random();

        public readonly string FolderPath;

        public TempFolder()
        {
            var folderName = $"bsputilstest_{RandomString(16)}";
            FolderPath = Path.Combine(Path.GetTempPath(), folderName);
            Directory.CreateDirectory(FolderPath);
        }

        /// <summary>
        /// Dispose the object, removing the created folder
        /// </summary>
        public void Dispose()
        {
            Directory.Delete(FolderPath, true);
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}