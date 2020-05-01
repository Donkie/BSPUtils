using System.IO;

namespace BSPUtils
{
    public class Lump
    {
        private byte[] _data;

        protected Lump(BinaryReader reader, int index)
        {
            // Read header
            Index = index;
            Offset = reader.ReadInt32();
            var length = reader.ReadInt32();
            Version = reader.ReadInt32();
            IDENT = reader.ReadInt32();

            // Read data
            var prevPosition = reader.BaseStream.Position;
            reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
            Data = reader.ReadBytes(length);
            reader.BaseStream.Seek(prevPosition, SeekOrigin.Begin);
        }

        /// <summary>
        /// Lump offset in the bsp file
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Lump format version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Lump identifier, usually not utilized
        /// </summary>
        public int IDENT { get; set; }

        /// <summary>
        /// The lump index/type
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The order it should appear in the BSP file. This is generally not necessary due to the file format structure, but is
        /// used to preserve the order that the source engine prefers.
        /// </summary>
        internal int DataOrder { get; set; }

        /// <summary>
        /// Lump data. Will update any internal lump specific data structures when set.
        /// </summary>
        public byte[] Data
        {
            get => _data;
            set
            {
                _data = value;
                ParseData();
            }
        }

        /// <summary>
        /// Reads prop header and data into the proper Lump subclass depending on the specified index
        /// </summary>
        /// <param name="reader">The BinaryReader stream</param>
        /// <param name="index">The lump index/type</param>
        /// <returns>
        /// A <see cref="Lump" /> derived object, or a <see cref="Lump" /> object depending on if a custom implementation
        /// exists for that lump type.
        /// </returns>
        public static Lump MakeLump(BinaryReader reader, int index)
        {
            return index switch
            {
                35 => new GameLump(reader),
                40 => new PakfileLump(reader),
                _ => new Lump(reader, index)
            };
        }

        /// <summary>
        /// Clears the lump data with an empty data block
        /// </summary>
        public void Clear()
        {
            Data = new byte[] {0}; // TODO: see if an actually empty lump would work for the entity lump
        }

        /// <summary>
        /// Writes the lump header to the BinaryWriter stream
        /// </summary>
        /// <param name="writer">The BinaryWriter stream</param>
        public void WriteHeader(BinaryWriter writer)
        {
            writer.Write(Offset);
            writer.Write(Data.Length);
            writer.Write(Version);
            writer.Write(IDENT);
        }

        /// <summary>
        /// Writes the lump data body to the BinaryWriter stream
        /// </summary>
        /// <param name="writer">The BinaryWriter stream</param>
        public void WriteData(BinaryWriter writer)
        {
            writer.Write(Data);
        }

        /// <summary>
        /// Call to update any internal absolute file data offsets in the lump body
        /// </summary>
        /// <param name="newDataOffset">The file offset of the lump data body</param>
        internal virtual void UpdateOffsets(int newDataOffset)
        {
        }

        /// <summary>
        /// Call to parse the Data field into any specific lump implementation structures
        /// </summary>
        protected internal virtual void ParseData()
        {
        }
    }
}