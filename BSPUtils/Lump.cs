using System.IO;

namespace BSPUtils
{
    public class Lump
    {
        protected Lump(BinaryReader reader, int index)
        {
            Index = index;
            Offset = reader.ReadInt32();
            Length = reader.ReadInt32();
            Version = reader.ReadInt32();
            IDENT = reader.ReadInt32();

            EmptyAtStart = Length == 0 && Offset == 0;
        }

        public int Offset { get; set; }
        public int Length { get; private set; }
        public int Version { get; }
        public int IDENT { get; }
        public int Index { get; }
        public int DataOrder { get; set; } // The order it appears in the BSP file
        public byte[] Data { get; protected set; }
        public bool EmptyAtStart { get; } // True if this is a 0 byte 0 offset lump

        public static Lump MakeLump(BinaryReader reader, int index)
        {
            return index switch
            {
                35 => new GameLump(reader),
                _ => new Lump(reader, index)
            };
        }

        public virtual void SetData(byte[] data)
        {
            Data = data;
        }

        public void Clear()
        {
            Data = new byte[] {0};
            Length = Data.Length;
        }

        public void WriteHeader(BinaryWriter writer)
        {
            writer.Write(Offset);
            writer.Write(Length);
            writer.Write(Version);
            writer.Write(IDENT);
        }

        public void WriteData(BinaryWriter writer)
        {
            writer.Write(Data);
        }

        public virtual void UpdateOffsets(int newDataOffset)
        {
        }
    }
}