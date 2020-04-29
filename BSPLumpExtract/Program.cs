using System;
using System.IO;
using BSPUtils;

namespace BSPLumpExtract
{
    class Program
    {
        private static void Main(string[] args)
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
    }
}