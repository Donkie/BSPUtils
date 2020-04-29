using System.IO;
using System.IO.Compression;

namespace BSPUtils
{
    public class PakfileLump : Lump
    {
        public PakfileLump(BinaryReader reader) : base(reader, 40)
        {
        }

        private MemoryStream _zipMemoryStream;
        private ZipArchive _zipArchive;

        public ZipArchive OpenArchiveStream(ZipArchiveMode archiveMode)
        {
            _zipMemoryStream = new MemoryStream();
            _zipMemoryStream.Write(Data, 0, Data.Length);
            _zipMemoryStream.Seek(0, SeekOrigin.Begin);

            _zipArchive = new ZipArchive(_zipMemoryStream, archiveMode);
            return _zipArchive;
        }

        public void CloseArchiveStream(bool dataWritten)
        {
            _zipArchive.Dispose();

            if (dataWritten)
            {
                SetData(_zipMemoryStream.ToArray());
            }

            _zipMemoryStream.Dispose();
        }
    }
}
