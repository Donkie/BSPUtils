using System;
using System.Collections.Generic;
using System.IO;

namespace BSPUtilsTest.TestUtil
{
    public class FileReader : IDisposable
    {
        private readonly List<BinaryReader> _readers = new List<BinaryReader>();

        public void Dispose()
        {
            foreach (var binaryReader in _readers)
                binaryReader.Dispose();
        }

        public BinaryReader OpenStream(string fileName)
        {
            // ReSharper disable once StringLiteralTypo
            var stream = File.Open(Path.Combine("testdata", fileName), FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(stream);
            _readers.Add(reader);
            return reader;
        }
    }
}