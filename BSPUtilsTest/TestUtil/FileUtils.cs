using System;
using System.IO;
using System.Security.Cryptography;

namespace BSPUtilsTest.TestUtil
{
    public static class FileUtils
    {
        public static string ComputeChecksum(Stream fileStream)
        {
            using var md5 = new MD5CryptoServiceProvider();
            var hash = md5.ComputeHash(fileStream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public static string ComputeChecksum(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return ComputeChecksum(stream);
        }
    }
}
