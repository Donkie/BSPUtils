using System;
using System.IO;
using System.IO.Compression;
using BSPUtils;

namespace BSPPak
{
    class Program
    {
        private static void Main(string[] args)
        {
            // TODO: use a proper parameter parsing plugin
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Usage: BSPPak \"C:/Path/To/Map.bsp\" \"C:/Path/To/Content/Folder\"");
                Console.WriteLine("Usage: BSPPak -d \"C:/Path/To/Map.bsp\" \"C:/Path/To/Content/Folder\"");
                Console.WriteLine("The content folder can contain a file named \".pakfilter\" which contains .gitignore-like syntax for matching which files to pack. " +
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

            var files = FileFinder.Find(contentPath, true);
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }

            if (isDryRun)
            {
                Console.WriteLine("Finished, dry run");
                return;
            }

            if (!File.Exists(bspPath))
            {
                Console.WriteLine("Invalid bsp specified");
                return;
            }

            Console.WriteLine("Reading and parsing BSP");
            var bspFile = File.Open(bspPath, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(bspFile);
            var bsp = new BSP(reader);
            reader.Dispose();
            bspFile.Dispose();

            var pakLump = (PakfileLump)bsp.Lumps[40];
            var archive = pakLump.OpenArchiveStream(ZipArchiveMode.Update);

            Console.WriteLine("Zipping files");
            foreach (var file in files)
            {
                var relPath = ToRelativePath(file, contentPath);
                if (archive.GetEntry(relPath) != null)
                {
                    Console.WriteLine($"{relPath} already packed, ignoring.");
                    continue;
                }

                var entry = archive.CreateEntry(relPath, CompressionLevel.NoCompression);
                using var entryWriter = entry.Open();
                using var fileReader = File.Open(file, FileMode.Open, FileAccess.Read);
                fileReader.CopyTo(entryWriter);
            }

            pakLump.CloseArchiveStream(true);

            Console.WriteLine("Writing BSP");
            using var bspFile2 = File.Open(bspPath, FileMode.Create, FileAccess.Write);
            using var bspWriter = new BinaryWriter(bspFile2);
            bsp.WriteBSP(bspWriter);

            //archive = pakLump.OpenArchiveStream(ZipArchiveMode.Update);

            //foreach (var zipArchiveEntry in archive.Entries)
            //{
            //    Console.WriteLine($"{zipArchiveEntry.Length} {zipArchiveEntry.Name} {zipArchiveEntry.FullName}");
            //}

            //pakLump.CloseArchiveStream(true);
        }

        private static string ToRelativePath(string fullPath, string relDir)
        {
            if (!fullPath.StartsWith(relDir))
                throw new Exception("reldir is not a parent directory to fullpath!");

            return fullPath.Substring(relDir.Length).TrimStart('/', '\\');
        }
    }
}
