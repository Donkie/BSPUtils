using System;
using System.Collections.Generic;
using System.IO;

namespace LibBSP
{
    /// <summary>
    /// Represents the game lump format in the BSP
    /// </summary>
    public class GameLump : Lump
    {
        private const int GameLumpItemHeaderSize = 3 * sizeof(int) + 2 * sizeof(ushort);

        private int[] _offsets;

        public GameLump(BinaryReader reader) : base(reader, LumpType.GameLump)
        {
            ReadData();
        }

        public List<GameLumpItem> LumpItems { get; private set; }

        private int GameLumpHeaderSize => sizeof(int) + GameLumpItemHeaderSize * LumpItems.Count;

        /// <summary>
        /// Byte form of the GameLump data. Parses the GameLumpItem list on read, and sets it on write.
        /// </summary>
        public override byte[] Data
        {
            get
            {
                WriteData();
                return DataBytes;
            }
            set
            {
                DataBytes = value;
                ReadData();
            }
        }

        /// <summary>
        /// Parses the _data field into the array of LumpItems
        /// </summary>
        private void ReadData()
        {
            using var ms = new MemoryStream(DataBytes);
            using var reader = new BinaryReader(ms);

            // Load game lump headers and data
            var lumpCount = reader.ReadInt32();
            _offsets = new int[lumpCount];
            LumpItems = new List<GameLumpItem>(lumpCount);
            for (var i = 0; i < lumpCount; i++)
            {
                var lumpItem = GameLumpItem.FromStream(reader, Offset);
                _offsets[i] = lumpItem.Offset ?? 0;
                LumpItems.Add(lumpItem);
            }
        }

        /// <summary>
        /// Parses the array of LumpItems into the _data field
        /// </summary>
        private void WriteData()
        {
            if (_offsets.Length != LumpItems.Count)
                throw new InvalidOperationException("Call UpdateOffsets after editing the LumpItems list.");

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Write game lump headers
            writer.Write(LumpItems.Count);
            for (var i = 0; i < LumpItems.Count; i++)
                LumpItems[i].WriteHeader(writer, _offsets[i] + Offset); // Convert the relative game lump item data offset back to absolute position

            // Write game lump data
            for (var i = 0; i < LumpItems.Count; i++)
            {
                writer.BaseStream.Seek(_offsets[i], SeekOrigin.Begin);
                writer.Write(LumpItems[i].Data);
            }

            DataBytes = ms.ToArray();
        }

        /// <summary>
        /// Computes which absolute file offsets the data of the GameLumpItems should be at
        /// </summary>
        /// <param name="lumpDataOffset">The absolute file offset of the GameLump data</param>
        /// <returns>A list of absolute file offsets</returns>
        private int[] GetGameLumpItemOffsets(int lumpDataOffset)
        {
            var offsets = new int[LumpItems.Count];

            var pos = lumpDataOffset + GameLumpHeaderSize;
            for (var i = 0; i < LumpItems.Count; i++)
            {
                offsets[i] = pos;
                pos = Util.RoundUp(pos + LumpItems[i].Data.Length,
                    4); // Game lump items should appear on int boundaries in the BSP file
            }

            return offsets;
        }

        /// <summary>
        /// Update the file offsets of the GameDataLumps.
        /// </summary>
        /// <param name="newDataOffset">The file offset of the data for this lump</param>
        public override void UpdateOffsets(int newDataOffset)
        {
            _offsets = GetGameLumpItemOffsets(newDataOffset);
        }
    }
}