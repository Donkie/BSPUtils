using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BSPLumpExtract
{
    class Program
    {
        static void CompareBSP(BSP bsp1, BSP bsp2)
        {

            foreach(var lump in bsp1.Lumps.OrderBy(lump => lump.Offset))
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
                    {
                        if (l1.Data[b] != l2.Data[b])
                        {
                            Console.WriteLine($"Byte {b} diff: {l1.Data[b]:X} vs {l2.Data[b]:X}");
                        }
                    }
                }
            }
        }

        //static void Main(string[] args)
        //{
        //    if (args.Length != 2)
        //    {
        //        Console.WriteLine("Usage: BSPLumpExtract \"C:/Path/To/Map.bsp\" 0");
        //        Console.WriteLine("Where the number is which lump you want to extract.");
        //        return;
        //    }

        //    var bspPath = args[0];
        //    if (!File.Exists(bspPath))
        //    {
        //        Console.WriteLine("Invalid file specified");
        //        return;
        //    }

        //    var bsp2Path = args[1];
        //    if (!File.Exists(bspPath))
        //    {
        //        Console.WriteLine("Invalid file specified");
        //        return;
        //    }

        //    // Read BSP
        //    Console.WriteLine("Reading and parsing BSP 1");
        //    var origFile = File.Open(bspPath, FileMode.Open, FileAccess.Read);
        //    var reader = new BinaryReader(origFile);
        //    var bsp1 = new BSP(reader);
        //    reader.Dispose();
        //    origFile.Dispose();

        //    Console.WriteLine("Reading and parsing BSP 2");
        //    var origFile2 = File.Open(bsp2Path, FileMode.Open, FileAccess.Read);
        //    var reader2 = new BinaryReader(origFile2);
        //    var bsp2 = new BSP(reader2);
        //    reader.Dispose();
        //    origFile.Dispose();

        //    CompareBSP(bsp1, bsp2);

        //    Console.WriteLine("Done!");
        //}

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: BSPLumpExtract \"C:/Path/To/Map.bsp\" 0");
                Console.WriteLine("Where the number is which lump you want to extract.");
                return;
            }

            var bspPath = args[0];
            if (!File.Exists(bspPath))
            {
                Console.WriteLine("Invalid file specified");
                return;
            }

            if (!int.TryParse(args[1], out var lumpIndex))
            {
                Console.WriteLine("Invalid lump index specified");
                return;
            }

            var bspFolder = Path.GetDirectoryName(bspPath);
            var bspName = Path.GetFileNameWithoutExtension(bspPath);

            // Read BSP
            Console.WriteLine("Reading and parsing BSP");
            var origFile = File.Open(bspPath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(origFile);
            var bsp = new BSP(reader);
            reader.Dispose();
            origFile.Dispose();

            if (bsp.GetLumpLength(lumpIndex) <= 1)
            {
                Console.WriteLine("Lump already cleared, nothing to extract. Exiting.");
                return;
            }

            // Backup original BSP
            Console.WriteLine("Saving BSP backup");
            var backupBSPPath = Path.Combine(bspFolder, $"{bspName}.bsp.orig");
            File.Move(bspPath, backupBSPPath);

            // Write lump
            Console.WriteLine("Writing .lmp file");
            var lmpPath = Path.Combine(bspFolder, $"{bspName}_l_{lumpIndex}.lmp");
            using var lmpFile = File.Open(lmpPath, FileMode.Create, FileAccess.Write);
            using var lmpWriter = new BinaryWriter(lmpFile);
            bsp.WriteLMP(lmpWriter, lumpIndex);

            // Clear lump and write new BSP
            Console.WriteLine("Clearing lump in BSP and overwriting original file");
            bsp.ClearLump(lumpIndex);
            using var bspFile = File.Open(bspPath, FileMode.Create, FileAccess.Write);
            using var bspWriter = new BinaryWriter(bspFile);
            bsp.WriteBSP(bspWriter);

            Console.WriteLine("Done!");
        }

        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture) + suf[place];
        }
    }
}
