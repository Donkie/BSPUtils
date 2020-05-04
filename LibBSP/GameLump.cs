using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibBSP
{
    /// <summary>
    /// Represents the game lump format in the BSP
    /// </summary>
    public class GameLump : Lump
    {
        private const int GameLumpItemHeaderSize = 3 * sizeof(int) + 2 * sizeof(ushort);

        public GameLump(BinaryReader reader) : base(reader, LumpType.GameLump)
        {
        }

        public List<GameLumpItem> LumpItems { get; private set; }

        private int GameLumpHeaderSize => sizeof(int) + GameLumpItemHeaderSize * LumpItems.Count;

        /// <summary>
        /// Parses the Data field into the array of LumpItems
        /// </summary>
        protected override void ParseData()
        {
            using var ms = new MemoryStream(Data);
            using var reader = new BinaryReader(ms);

            // Load game lump headers and data
            var lumpCount = reader.ReadInt32();
            LumpItems = new List<GameLumpItem>(lumpCount);
            for (var i = 0; i < lumpCount; i++)
                LumpItems.Add(GameLumpItem.FromStream(reader));
        }

        /// <summary>
        /// Computes which absolute file offsets the data of the GameLumpItems should be at
        /// </summary>
        /// <param name="lumpDataOffset">The absolute file offset of the GameLump data</param>
        /// <returns>A list of absolute file offsets</returns>
        private IList<int> GetGameLumpItemOffsets(int lumpDataOffset)
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
            var offsets = GetGameLumpItemOffsets(newDataOffset);

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Write game lump headers
            writer.Write(LumpItems.Count);
            for (var i = 0; i < LumpItems.Count; i++)
                LumpItems[i].WriteHeader(writer, offsets[i]);

            // Write game lump data
            for (var i = 0; i < LumpItems.Count; i++)
            {
                writer.BaseStream.Seek(offsets[i], SeekOrigin.Begin);
                writer.Write(LumpItems[i].Data);
            }

            Data = ms.ToArray();
        }
    }
}