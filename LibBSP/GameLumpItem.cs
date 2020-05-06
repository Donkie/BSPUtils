using System;
using System.IO;

namespace LibBSP
{
    /// <summary>
    /// Represents a data item in the game lump
    /// </summary>
    public class GameLumpItem
    {
        /// <summary>
        /// Constructs a GameLumpItem using the header and data information from a BinaryReader. The reader stream is seek-ed to the end of the header after everything is read.
        /// </summary>
        /// <param name="reader">The BinaryReader stream</param>
        public static GameLumpItem FromStream(BinaryReader reader)
        {
            var id = reader.ReadInt32();
            var flags = reader.ReadUInt16();
            var version = reader.ReadUInt16();

            var offset = reader.ReadInt32();
            var length = reader.ReadInt32();

            // Seek to the data position, load the data then seek back again
            var prevPosition = reader.BaseStream.Position;
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var data = new byte[length];
            reader.Read(data, 0, length);
            reader.BaseStream.Seek(prevPosition, SeekOrigin.Begin);

            return new GameLumpItem(id, flags, version, data);
        }

        public GameLumpItem(int id, ushort flags, ushort version, byte[] data)
        {
            ID = id;
            Flags = flags;
            Version = version;
            Data = data;
        }

        public int ID { get; }
        public ushort Flags { get; }
        public ushort Version { get; }
        public byte[] Data { get; }

        /// <summary>
        /// Writes the header of the game lump item to the BinaryWriter stream
        /// </summary>
        /// <param name="writer">The BinaryWriter stream</param>
        /// <param name="dataOffset">The absolute file offset of the data for this GameLumpItem</param>
        public void WriteHeader(BinaryWriter writer, int dataOffset)
        {
            writer.Write(ID);
            writer.Write(Flags);
            writer.Write(Version);
            writer.Write(dataOffset);
            writer.Write(Data.Length);
        }
    }
}