using System;
using System.IO;
using System.IO.Compression;
using LibBSP;

namespace BSPPak
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // TODO: use a proper parameter parsing plugin
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Usage: BSPPak \"C:/Path/To/Map.bsp\" \"C:/Path/To/Content/Folder\"");
                Console.WriteLine("Usage: BSPPak -d \"C:/Path/To/Map.bsp\" \"C:/Path/To/Content/Folder\"");
                Console.WriteLine(
                    "The content folder can contain a file named \".pakfilter\" which contains .gitignore-like syntax for matching which files to pack. " +
                    "Remember that the .pakfilter file matches inversely to a regular .gitignore, .gitignore acts as a blacklist while .pakfilter acts as a whitelist.");
                Console.WriteLine("Use the -d flag to mark it as a dry run where the bsp will not get edited.");
                return;
            }

            bool isDryRun;
            string bspPath;
            string contentPath;
            if (args.Length == 2)
            {
                isDryRun = false;
                bspPath = args[0];
                contentPath = args[1];
            }
            else
            {
                isDryRun = true;
                bspPath = args[1];
                contentPath = args[2];
            }

            var files = FileFinder.Find(contentPath);

            if (isDryRun)
            {
                foreach (var item in files)
                    Console.WriteLine(ToRelativePath(item, contentPath));
                Console.WriteLine($"Found {files.Count} files matching the filter.");
                Console.WriteLine("Finished, dry run");
                return;
            }

            if (files.Count == 0)
            {
                Console.WriteLine("No files found that matches the filter. Exiting.");
                return;
            }

            Console.WriteLine($"Found {files.Count} files matching the filter.");

            if (!File.Exists(bspPath))
            {
                Console.WriteLine("Invalid bsp specified");
                return;
            }

            Console.WriteLine("Reading and parsing BSP");
            var bsp = new BSP(bspPath);

            var pakLump = (PakfileLump) bsp.Lumps[40];
            var archive = pakLump.OpenArchiveStream(ZipArchiveMode.Update);

            var i = 0;
            foreach (var file in files)
            {
                if (i % 50 == 0)
                    Console.WriteLine($"{i}/{files.Count} Zipping files...");

                var relPath = ToRelativePath(file, contentPath);
                var entry = archive.GetEntry(relPath);
                if (entry != null)
                    Console.WriteLine($"{relPath} already packed, overwriting...");
                else
                    entry = archive.CreateEntry(relPath, CompressionLevel.NoCompression);

                using var entryWriter = entry.Open();
                using var fileReader = File.Open(file, FileMode.Open, FileAccess.Read);
                fileReader.CopyTo(entryWriter);

                i++;
            }

            pakLump.CloseArchiveStream();

            Console.WriteLine("Writing BSP");
            bsp.WriteBSP(bspPath);

            Console.WriteLine("Done!");
        }

        private static string ToRelativePath(string fullPath, string relDir)
        {
            if (!fullPath.StartsWith(relDir))
                throw new ArgumentException("relDir is not a parent directory to fullPath!");

            return fullPath.Substring(relDir.Length).TrimStart('/', '\\');
        }
    }
}