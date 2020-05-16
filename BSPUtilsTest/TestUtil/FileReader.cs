using System;
using System.Collections.Generic;
using System.IO;

namespace BSPUtilsTest.TestUtil
{
    /// <summary>
    /// Test utility class that assists in opening and disposing test data files
    /// </summary>
    public class FileReader : IDisposable
    {
        private readonly List<BinaryReader> _readers = new List<BinaryReader>();

        /// <summary>
        /// Dispose all opened file streams
        /// </summary>
        public void Dispose()
        {
            foreach (var binaryReader in _readers)
                binaryReader.Dispose();
        }

        /// <summary>
        /// Open a BinaryReader stream to the file which will get automatically disposed as the FileReader is disposed
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public BinaryReader OpenStream(string fileName)
        {
            // ReSharper disable once StringLiteralTypo
            var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var reader = new BinaryReader(stream);
            _readers.Add(reader);
            return reader;
        }
    }
}