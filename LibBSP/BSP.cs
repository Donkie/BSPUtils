using System.IO;
using System.Linq;

namespace BSPUtils
{
    /// <summary>
    /// Contains the lumps and meta data of a BSP file, along with functions to read and write to .bsp and .lmp formats.
    /// </summary>
    public class BSP
    {
        private const int HeaderLumps = 64;
        private const int VBSP = ('P' << 24) + ('S' << 16) + ('B' << 8) + 'V';
        private const int HeaderLumpSize = 3 * sizeof(int) + 4 * sizeof(char);
        private const int VBSPHeaderSize = 3 * sizeof(int) + HeaderLumpSize * HeaderLumps;
        private const int LMPHeaderSize = 5 * sizeof(int);

        /// <summary>
        /// Creates a new BSP object and reads its header and lump data from a BinaryReader stream
        /// </summary>
        /// <param name="bspReader">The BinaryReader stream</param>
        public BSP(BinaryReader bspReader)
        {
            Read(bspReader);
        }

        /// <summary>
        /// Creates a new BSP object and reads its header and lump data from a file path. The file is opened with read
        /// permissions.
        /// </summary>
        /// <param name="bspPath">The absolute file path to the bsp file</param>
        public BSP(string bspPath)
        {
            using var bspFile = File.Open(bspPath, FileMode.Open, FileAccess.Read);
            using var bspReader = new BinaryReader(bspFile);
            Read(bspReader);
        }

        /// <summary>
        /// BSP format version
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// Array of the BSP lumps
        /// </summary>
        public Lump[] Lumps { get; private set; }

        /// <summary>
        /// Map revision
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// Parses BSP header and lump data from a BinaryReader stream
        /// </summary>
        /// <param name="reader">The BinaryReader stream</param>
        /// <exception cref="FileFormatException"></exception>
        private void Read(BinaryReader reader)
        {
            // Read BSP Header and lump data
            if (reader.ReadInt32() != VBSP)
                throw new FileFormatException("File not VBSP");

            Version = reader.ReadInt32();
            Lumps = new Lump[HeaderLumps];
            for (var i = 0; i < HeaderLumps; i++)
                Lumps[i] = Lump.MakeLump(reader, i);

            Revision = reader.ReadInt32();

            // Store lump order based on where it is in the BSP file
            // This is done in order to preserve the preferred order from the BSP compiler
            var lumpsList = Lumps.OrderBy(lump => lump.Offset).ToList();
            for (var i = 0; i < lumpsList.Count; i++)
                lumpsList[i].DataOrder = i;
        }

        /// <summary>
        /// Updates the lump data offsets in the lump headers. Must be called when any lump's data size has changed.
        /// </summary>
        private void UpdateLumpDataOffsets()
        {
            var pos = VBSPHeaderSize;
            foreach (var lump in Lumps.OrderBy(lump => lump.DataOrder))
            {
                // Notify the lump about us updating the offsets.
                // This is necessary for the GameLump which for some dumb reason has absolute file offsets and not local offsets
                lump.UpdateOffsets(pos);

                lump.Offset = pos;
                pos = Util.RoundUp(pos + lump.Data.Length, 4); // Lumps should appear on int boundaries in the BSP file
            }
        }

        /// <summary>
        /// Writes the BSP header and lump data using a BinaryWriter stream
        /// </summary>
        /// <param name="writer">The BinaryWriter stream</param>
        public void WriteBSP(BinaryWriter writer)
        {
            UpdateLumpDataOffsets();

            // Write header
            writer.Write(VBSP);
            writer.Write(Version);
            foreach (var lump in Lumps)
                lump.WriteHeader(writer);
            writer.Write(Revision);

            // Write lump contents
            foreach (var lump in Lumps.OrderBy(lump => lump.Offset))
            {
                writer.Seek(lump.Offset, SeekOrigin.Begin);
                lump.WriteData(writer);
            }

            // Pad with zeros at the end until int boundary is reached
            writer.Seek(0, SeekOrigin.End);
            var bytesToWrite = Util.RoundUp((int) writer.BaseStream.Position, 4) - (int) writer.BaseStream.Position;
            for (var i = 0; i < bytesToWrite; i++)
                writer.Write((byte) 0);
        }

        /// <summary>
        /// Writes the BSP header and lump data to a specific file. The file does not need to exist, but gets truncated if it
        /// does.
        /// </summary>
        /// <param name="bspPath">The path to the bsp file to be written</param>
        public void WriteBSP(string bspPath)
        {
            using var bspFile = File.Open(bspPath, FileMode.Create, FileAccess.Write);
            using var bspWriter = new BinaryWriter(bspFile);
            WriteBSP(bspWriter);
        }

        /// <summary>
        /// Writes lump data using a BinaryWriter stream
        /// </summary>
        /// <param name="writer">The BinaryWriter stream</param>
        /// <param name="lumpIndex">The lump index to write</param>
        public void WriteLMP(BinaryWriter writer, int lumpIndex)
        {
            var lump = Lumps[lumpIndex];

            writer.Write(LMPHeaderSize);
            writer.Write(lump.Index);
            writer.Write(lump.Version);
            writer.Write(lump.Data.Length);
            writer.Write(Revision);

            writer.Write(lump.Data);
        }

        /// <summary>
        /// Writes lump data to a lump file using a specific file. The file does not need to exist, but gets truncated if it
        /// does.
        /// </summary>
        /// <param name="lmpPath">The path to the lmp file to be written</param>
        /// <param name="lumpIndex">The lump index to write</param>
        public void WriteLMP(string lmpPath, int lumpIndex)
        {
            using var lmpFile = File.Open(lmpPath, FileMode.Create, FileAccess.Write);
            using var lmpWriter = new BinaryWriter(lmpFile);
            WriteLMP(lmpWriter, lumpIndex);
        }
    }
}