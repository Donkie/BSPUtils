using System;
using System.IO;
using System.Linq;

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
        /// <param name="lumpDataOffset">The .bsp file offset to the lump data</param>
        public static GameLumpItem FromStream(BinaryReader reader, int lumpDataOffset)
        {
            var id = reader.ReadInt32();
            var flags = reader.ReadUInt16();
            var version = reader.ReadUInt16();

            var offset = reader.ReadInt32() - lumpDataOffset; // Since the game lump item data offsets are stored as absolute file offsets we need to make them relative
            var length = reader.ReadInt32();

            // Seek to the data position, load the data then seek back again
            var prevPosition = reader.BaseStream.Position;
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var data = new byte[length];
            reader.Read(data, 0, length);
            reader.BaseStream.Seek(prevPosition, SeekOrigin.Begin);

            return new GameLumpItem(id, flags, version, data, offset);
        }

        public GameLumpItem(int id, ushort flags, ushort version, byte[] data)
        {
            ID = id;
            Flags = flags;
            Version = version;
            Data = data;
        }

        public GameLumpItem(int id, ushort flags, ushort version, byte[] data, int offset) : this(id, flags, version, data)
        {
            Offset = offset;
        }

        public int ID { get; }
        public ushort Flags { get; }
        public ushort Version { get; }
        public byte[] Data { get; }
        /// <summary>
        /// The file offset of the game lump item data chunk relative to the start of the game lump data chunk
        /// </summary>
        public int? Offset { get; }

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

        public override bool Equals(object obj)
        {
            return Equals(obj as GameLumpItem);
        }

        public bool Equals(GameLumpItem item)
        {
            return item.ID == ID && item.Flags == Flags && item.Version == Version && item.Data.SequenceEqual(Data);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ID;
                hashCode = (hashCode * 397) ^ Flags.GetHashCode();
                hashCode = (hashCode * 397) ^ Version.GetHashCode();
                hashCode = (hashCode * 397) ^ Data.GetHashCode();
                return hashCode;
            }
        }
    }
}