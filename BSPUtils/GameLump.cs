using System.IO;

namespace BSPUtils
{
    public class GameLump : Lump
    {
        public GameLumpItem[] LumpItems;

        public GameLump(BinaryReader reader) : base(reader, 35)
        {
        }

        public override void SetData(byte[] data)
        {
            Data = data;
            ParseData();
        }

        private void ParseData()
        {
            using var ms = new MemoryStream(Data);
            using var reader = new BinaryReader(ms);

            // Load game lump headers
            var lumpCount = reader.ReadInt32();
            LumpItems = new GameLumpItem[lumpCount];
            for (var i = 0; i < lumpCount; i++)
            {
                LumpItems[i] = new GameLumpItem(reader);
                LumpItems[i].LocalOffset = LumpItems[i].Offset - Offset;
            }

            // Load game lump data
            foreach (var gameLumpItem in LumpItems)
            {
                reader.BaseStream.Seek(gameLumpItem.LocalOffset, SeekOrigin.Begin);
                gameLumpItem.Data = reader.ReadBytes(gameLumpItem.Length);
            }
        }

        public override void UpdateOffsets(int newDataOffset)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Write game lump headers
            writer.Write(LumpItems.Length);
            foreach (var gameLumpItem in LumpItems)
            {
                gameLumpItem.Offset = newDataOffset + gameLumpItem.LocalOffset;
                gameLumpItem.WriteHeader(writer);
            }

            // Write game lump data
            foreach (var gameLumpItem in LumpItems)
            {
                writer.BaseStream.Seek(gameLumpItem.LocalOffset, SeekOrigin.Begin);
                writer.Write(gameLumpItem.Data);
            }

            Data = ms.ToArray();
        }
    }
}