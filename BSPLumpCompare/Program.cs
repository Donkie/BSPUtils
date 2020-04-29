using System;
using System.IO;
using System.Linq;
using BSPUtils;

namespace BSPLumpCompare
{
    class Program
    {
        private static void CompareBSP(BSP bsp1, BSP bsp2)
        {
            foreach (var lump in bsp1.Lumps.OrderBy(lump => lump.Offset))
            {
                Console.Write($"Lump {lump.Index}:");
                var i = lump.Index;
                var l1 = bsp1.Lumps[i];
                var l2 = bsp2.Lumps[i];

                if (l1.Offset != l2.Offset)
                {
                    var diff = l1.Offset > l2.Offset ? $"+{l1.Offset - l2.Offset}" : $"-{l2.Offset - l1.Offset}";
                    Console.Write($" Offset diff: 0x{l1.Offset:x8} vs 0x{l2.Offset:x8} ({diff})");
                }

                if (l1.Length != l2.Length)
                {
                    var diff = l1.Length > l2.Length ? $"+{l1.Length - l2.Length}" : $"-{l2.Length - l1.Length}";
                    Console.Write($" Length diff: {l1.Length} vs {l2.Length} ({diff})");
                }
                else
                {
                    Console.WriteLine();
                    for (var b = 0; b < l1.Data.Length; b++)
                        if (l1.Data[b] != l2.Data[b])
                            Console.WriteLine($"Byte {b} diff: {l1.Data[b]:X} vs {l2.Data[b]:X}");
                }
            }
        }

        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: BSPCompare \"C:/Path/To/Map1.bsp\" \"C:/Path/To/Map1.bsp\"");
                return;
            }

            var bspPath = args[0];
            if (!File.Exists(bspPath))
            {
                Console.WriteLine("Invalid bsp 1 specified");
                return;
            }

            var bsp2Path = args[1];
            if (!File.Exists(bspPath))
            {
                Console.WriteLine("Invalid bsp 2 specified");
                return;
            }

            // Read BSP
            Console.WriteLine("Reading and parsing BSP 1");
            var origFile = File.Open(bspPath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(origFile);
            var bsp1 = new BSP(reader);
            reader.Dispose();
            origFile.Dispose();

            Console.WriteLine("Reading and parsing BSP 2");
            var origFile2 = File.Open(bsp2Path, FileMode.Open, FileAccess.Read);
            var reader2 = new BinaryReader(origFile2);
            var bsp2 = new BSP(reader2);
            reader.Dispose();
            origFile.Dispose();

            CompareBSP(bsp1, bsp2);

            Console.WriteLine("Done!");
        }
    }
}