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
        /// Reads the header of a game lump item
        /// </summary>
        /// <param name="reader">The BinaryReader stream</param>
        public GameLumpItem(BinaryReader reader)
        {
            ID = reader.ReadInt32();
            Flags = reader.ReadUInt16();
            Version = reader.ReadUInt16();
            Offset = reader.ReadInt32();

            var length = reader.ReadInt32();

            // Seek to the data position, load the data then seek back again
            var prevPosition = reader.BaseStream.Position;
            reader.BaseStream.Seek((int) Offset, SeekOrigin.Begin);
            Data = reader.ReadBytes(length);
            reader.BaseStream.Seek(prevPosition, SeekOrigin.Begin);
        }

        public GameLumpItem(int id, ushort flags, ushort version, byte[] data)
        {
            ID = id;
            Flags = flags;
            Version = version;
            Data = data;
        }

        public int ID { get; }
        public ushort Flags { get; set; }
        public ushort Version { get; }
        public int? Offset { get; set; }
        public byte[] Data { get; set; }

        /// <summary>
        /// Writes the header of the game lump item to the BinaryWriter stream
        /// </summary>
        /// <param name="writer">The BinaryWriter stream</param>
        public void WriteHeader(BinaryWriter writer)
        {
            if(Offset == null)
                throw new InvalidOperationException("Offset must be set in order to write the header.");

            writer.Write(ID);
            writer.Write(Flags);
            writer.Write(Version);
            writer.Write((int) Offset);
            writer.Write(Data.Length);
        }
    }
}