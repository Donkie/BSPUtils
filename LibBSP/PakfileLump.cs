using System;
using System.IO;
using System.IO.Compression;

namespace LibBSP
{
    /// <summary>
    /// Represents the pak file lump in the BSP format
    /// </summary>
    public class PakfileLump : Lump
    {
        private ZipArchive _zipArchive;

        private MemoryStream _zipMemoryStream;

        public PakfileLump(BinaryReader reader) : base(reader, LumpType.Pakfile)
        {
        }

        /// <summary>
        /// Returns the lump data contents as a ZipArchive object which can be read and edited. Call
        /// <see cref="CloseArchiveStream" /> when finished with the ZipArchive object.
        /// </summary>
        /// <param name="archiveMode">Which mode to open the zip archive in</param>
        /// <returns></returns>
        public ZipArchive OpenArchiveStream(ZipArchiveMode archiveMode)
        {
            if (_zipArchive != null)
                throw new InvalidOperationException("An archive stream is already open.");

            // Open the MemoryStream as an expandable stream
            _zipMemoryStream = new MemoryStream();
            _zipMemoryStream.Write(Data, 0, Data.Length);
            _zipMemoryStream.Seek(0, SeekOrigin.Begin);

            _zipArchive = new ZipArchive(_zipMemoryStream, archiveMode);
            return _zipArchive;
        }

        public void CloseArchiveStream(bool dataWritten)
        {
            if (_zipArchive == null)
                throw new InvalidOperationException("The archive stream is not open.");

            _zipArchive.Dispose();

            if (dataWritten)
                Data = _zipMemoryStream.ToArray();

            _zipMemoryStream.Dispose();

            _zipArchive = null;
        }
    }
}