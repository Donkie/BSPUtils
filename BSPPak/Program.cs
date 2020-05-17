using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CommandLine;
using LibBSP;

namespace BSPPak
{
    public class Program
    {
        public const int ExitFail = 1;

        public static void PackFiles(Options opts, IReadOnlyCollection<string> files)
        {
            Console.WriteLine($"Found {files.Count} files matching the filter.");

            Console.WriteLine($"Reading from {Path.GetFileName(opts.BSPPath)}");
            var bsp = new BSP(opts.BSPPath);

            var pakLump = (PakfileLump) bsp.Lumps[(int) LumpType.Pakfile];
            var archive = pakLump.OpenArchiveStream(ZipArchiveMode.Update);

            var i = 0;
            foreach (var file in files)
            {
                if (i % 50 == 0)
                    Console.WriteLine($"{i}/{files.Count} Zipping files...");

                var relPath = ToRelativePath(file, opts.ContentPath);
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

            Console.WriteLine($"Writing to {Path.GetFileName(opts.BSPPath)}");
            bsp.WriteBSP(opts.BSPPath);

            Console.WriteLine("Done!");
        }

        public static void PrintFiles(Options opts, IReadOnlyCollection<string> files)
        {
            foreach (var item in files)
                Console.WriteLine(ToRelativePath(item, opts.ContentPath));
            Console.WriteLine($"Found {files.Count} files matching the filter.");
        }

        public static void ValidateOptions(Options opts)
        {
            if (!File.Exists(opts.BSPPath))
                throw new InvalidOptionException("Invalid .bsp file specified.");

            if (!Directory.Exists(opts.ContentPath))
                throw new InvalidOptionException("Invalid content folder specified.");
        }

        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    try
                    {
                        ValidateOptions(opts);
                    }
                    catch (InvalidOptionException e)
                    {
                        Console.WriteLine(e.Message);
                        Environment.ExitCode = ExitFail;
                        return;
                    }

                    var files = FileFinder.Find(opts.ContentPath);

                    if (opts.DryRun)
                        PrintFiles(opts, files);
                    else
                        PackFiles(opts, files);
                });
        }

        public static string ToRelativePath(string fullPath, string relDir)
        {
            if (!fullPath.StartsWith(relDir))
                throw new ArgumentException("relDir is not a parent directory to fullPath!");

            return fullPath.Substring(relDir.Length).TrimStart('/', '\\');
        }

        public class Options
        {
            [Option('d', "dry-run", Required = false,
                HelpText =
                    "Only output a list of files found in the content folder that passes the filter. The BSP file won't be edited.")]
            public bool DryRun { get; set; }

            [Value(0, MetaName = "BSP file", HelpText = "The .bsp file to process.", Required = true)]
            public string BSPPath { get; set; }

            [Value(1, MetaName = "Content path", HelpText = "The path to the content folder.", Required = true)]
            public string ContentPath { get; set; }
        }
    }
}