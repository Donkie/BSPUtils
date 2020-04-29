using System.IO;

namespace BSPUtils
{
    public class GameLumpItem
    {
        public GameLumpItem(BinaryReader reader)
        {
            ID = reader.ReadInt32();
            Flags = reader.ReadUInt16();
            Version = reader.ReadUInt16();
            Offset = reader.ReadInt32();
            Length = reader.ReadInt32();
        }

        public int ID { get; }
        public ushort Flags { get; }
        public ushort Version { get; }
        public int Offset { get; set; }
        public int Length { get; }
        public int LocalOffset { get; set; }
        public byte[] Data { get; set; }

        public void WriteHeader(BinaryWriter writer)
        {
            writer.Write(ID);
            writer.Write(Flags);
            writer.Write(Version);
            writer.Write(Offset);
            writer.Write(Length);
        }
    }
}