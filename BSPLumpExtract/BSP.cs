
using System;
using System.IO;
using System.Linq;

namespace BSPLumpExtract
{
    class BSP
    {
        private const int HeaderLumps = 64;
        private const int VBSP = 0x50534256;
        private const int VBSPHeaderSize = 4 + 4 + 16 * HeaderLumps + 4;
        private const int LMPHeaderSize = 4 + 4 + 4 + 4 + 4;

        public int Version { get; }
        public Lump[] Lumps { get; }
        public int Revision { get; }

        public BSP(BinaryReader reader)
        {
            // Read BSP Header
            if (reader.ReadInt32() != VBSP)
            {
                throw new Exception("File not VBSP");
            }

            Version = reader.ReadInt32();
            Lumps = new Lump[HeaderLumps];
            for (var i = 0; i < HeaderLumps; i++)
            {
                Lumps[i] = Lump.MakeLump(reader, i);
            }

            Revision = reader.ReadInt32();

            // Read lump contents
            foreach (var lump in Lumps)
            {
                reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
                lump.SetData(reader.ReadBytes(lump.Length));
            }

            // Store lump order based on where it is in the BSP file
            var lumpsList = Lumps.OrderBy(lump => lump.Offset).ToList();
            for (var i = 0; i < lumpsList.Count; i++)
            {
                lumpsList[i].DataOrder = i;
            }
        }
        private static int RoundUp(int numToRound, int multiple)
        {
            var remainder = numToRound % multiple;
            if (remainder == 0)
                return numToRound;

            return numToRound + multiple - remainder;
        }

        private void UpdateLumpDataOffsets()
        {
            var pos = VBSPHeaderSize;
            foreach (var lump in Lumps.OrderBy(lump => lump.DataOrder))
            {
                //var newPos = pos;
                //if (lump.EmptyAtStart && lump.Length == 0)
                //{
                //    newPos = 0;
                //}
                var newPos = lump.Length == 0 ? 0 : pos;

                //if (newPos != lump.Offset)
                //{
                //    var diff = newPos > lump.Offset ? $"+{newPos - lump.Offset}" : $"-{lump.Offset - newPos}";
                //    Console.WriteLine($"Lump {lump.Index} (Length = {Program.BytesToString(lump.Length)}) changed offset from {lump.Offset:X} to {newPos:X} ({diff})");
                //}

                // Notify the lump about us updating the offsets.
                // This is necessary for the GameLump which for some dumb reason has absolute file offsets and not local offsets
                lump.UpdateOffsets(newPos);

                lump.Offset = newPos;
                pos = RoundUp(pos + lump.Length, 4); // Lumps should appear on int boundaries in the BSP file
            }
        }

        public void ClearLump(int lumpIndex)
        {
            Lumps[lumpIndex].Clear();
        }

        public int GetLumpLength(int lumpIndex)
        {
            return Lumps[lumpIndex].Length;
        }

        public void WriteBSP(BinaryWriter writer)
        {
            UpdateLumpDataOffsets();

            // Write header
            writer.Write(VBSP);
            writer.Write(Version);
            foreach (var lump in Lumps)
            {
                lump.WriteHeader(writer);
            }
            writer.Write(Revision);

            // Write lump contents
            foreach (var lump in Lumps.OrderBy(lump => lump.Offset))
            {
                writer.Seek(lump.Offset, SeekOrigin.Begin);
                lump.WriteData(writer);
            }

            // Pad with zeros at the end until int boundary is reached
            writer.Seek(0, SeekOrigin.End);
            var bytesToWrite = RoundUp((int) writer.BaseStream.Position, 4) - (int) writer.BaseStream.Position;
            for (var i = 0; i < bytesToWrite; i++)
                writer.Write((byte) 0);
        }

        public void WriteLMP(BinaryWriter writer, int lumpIndex)
        {
            var lump = Lumps[lumpIndex];

            writer.Write(LMPHeaderSize);
            writer.Write(lump.Index);
            writer.Write(lump.Version);
            writer.Write(lump.Length);
            writer.Write(Revision);

            writer.Write(lump.Data);
        }
    }
}
