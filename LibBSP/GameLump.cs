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

        public GameLump(BinaryReader reader) : base(reader, 35)
        {
        }

        public List<GameLumpItem> LumpItems { get; private set; }

        private int GameLumpHeaderSize => sizeof(int) + GameLumpItemHeaderSize * LumpItems.Count;

        /// <summary>
        /// Parses the Data field into the array of LumpItems
        /// </summary>
        protected internal override void ParseData()
        {
            using var ms = new MemoryStream(Data);
            using var reader = new BinaryReader(ms);

            // Load game lump headers and data
            var lumpCount = reader.ReadInt32();
            LumpItems = new List<GameLumpItem>(lumpCount);
            for (var i = 0; i < lumpCount; i++)
                LumpItems.Add(new GameLumpItem(reader));

            // Sort the lump items by file offset to preserve their order when writing later
            LumpItems = LumpItems.OrderBy(item => item.Offset).ToList();
        }

        private void UpdateGameLumpItemOffsets(int lumpDataOffset)
        {
            var pos = lumpDataOffset + GameLumpHeaderSize;
            foreach (var item in LumpItems)
            {
                item.Offset = pos;
                pos = Util.RoundUp(pos + item.Data.Length,
                    4); // Game lump items should appear on int boundaries in the BSP file
            }
        }

        /// <summary>
        /// Update the file offsets of the GameDataLumps.
        /// </summary>
        /// <param name="newDataOffset">The file offset of the data for this lump</param>
        internal override void UpdateOffsets(int newDataOffset)
        {
            UpdateGameLumpItemOffsets(newDataOffset);

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Write game lump headers
            writer.Write(LumpItems.Count);
            foreach (var gameLumpItem in LumpItems)
                gameLumpItem.WriteHeader(writer);

            // Write game lump data
            foreach (var gameLumpItem in LumpItems)
            {
                writer.BaseStream.Seek(gameLumpItem.Offset, SeekOrigin.Begin);
                writer.Write(gameLumpItem.Data);
            }

            Data = ms.ToArray();
        }
    }
}