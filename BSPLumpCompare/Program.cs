using System;
using System.IO;
using System.Linq;
using LibBSP;

namespace BSPLumpCompare
{
    internal class Program
    {
        private static void CompareBSP(BSP bsp1, BSP bsp2)
        {
            foreach (var lump in bsp1.Lumps.OrderBy(lump => lump.Offset))
            {
                var i = (int) lump.Index;
                Console.Write($"Lump {i}:");
                var l1 = bsp1.Lumps[i];
                var l2 = bsp2.Lumps[i];

                if (l1.Offset != l2.Offset)
                {
                    var diff = l1.Offset > l2.Offset ? $"+{l1.Offset - l2.Offset}" : $"-{l2.Offset - l1.Offset}";
                    Console.Write($" Offset diff: 0x{l1.Offset:x8} vs 0x{l2.Offset:x8} ({diff})");
                }

                if (l1.Data.Length != l2.Data.Length)
                {
                    var diff = l1.Data.Length > l2.Data.Length
                        ? $"+{l1.Data.Length - l2.Data.Length}"
                        : $"-{l2.Data.Length - l1.Data.Length}";
                    Console.Write($" Length diff: {l1.Data.Length} vs {l2.Data.Length} ({diff})");
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

            var bsp1Path = args[0];
            if (!File.Exists(bsp1Path))
            {
                Console.WriteLine("Invalid bsp 1 specified");
                return;
            }

            var bsp2Path = args[1];
            if (!File.Exists(bsp1Path))
            {
                Console.WriteLine("Invalid bsp 2 specified");
                return;
            }

            // Read BSP
            Console.WriteLine("Reading and parsing BSP 1");
            var bsp1 = new BSP(bsp1Path);

            Console.WriteLine("Reading and parsing BSP 2");
            var bsp2 = new BSP(bsp2Path);

            CompareBSP(bsp1, bsp2);

            Console.WriteLine("Done!");
        }
    }
}