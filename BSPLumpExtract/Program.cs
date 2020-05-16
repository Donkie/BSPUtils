using System;
using System.IO;
using System.Linq;
using CommandLine;
using LibBSP;

namespace BSPLumpExtract
{
    public class Program
    {
        public const int ExitFail = 1;
        private const int MinLumpIndex = 0;
        private const int MaxLumpIndex = 63;

        /// <summary>
        /// Performs the lump extraction. Assumes that the options have been validated.
        /// </summary>
        /// <param name="opts">The lump extraction options</param>
        /// <exception cref="ArgumentException">Thrown if opts.BSPPath is not a file path</exception>
        /// <exception cref="LumpEmptyException">Thrown if the specified lump is already empty</exception>
        public static void ExtractLump(Options opts)
        {
            var lumpIndex = opts.LumpIndex;
            var bspFolder = Path.GetDirectoryName(opts.BSPPath);
            var bspName = Path.GetFileNameWithoutExtension(opts.BSPPath);

            if (bspFolder == null || bspName == null || Directory.Exists(opts.BSPPath))
                throw new ArgumentException("Invalid BSP path specified");

            // Read BSP
            Console.WriteLine("Reading and parsing BSP");
            var bsp = new BSP(opts.BSPPath);

            if (bsp.Lumps[lumpIndex].Data.Length <= 1)
                throw new LumpEmptyException();

            // True if we will modify the original BSP
            var willModify = !opts.DontClear;

            // Backup original BSP
            if (opts.MakeBackup && willModify)
            {
                Console.WriteLine("Saving BSP backup");
                var backupBSPPath = Path.Combine(bspFolder, $"{bspName}.bsp.orig");
                File.Move(opts.BSPPath, backupBSPPath);
            }

            // Write lump
            if (!opts.DontExtract)
            {
                Console.WriteLine("Writing .lmp file");
                var lmpPath = Path.Combine(bspFolder, $"{bspName}_l_{lumpIndex}.lmp");
                bsp.WriteLMP(lmpPath, lumpIndex);
            }

            // Clear lump
            if (!opts.DontClear)
            {
                Console.WriteLine("Clearing lump");
                bsp.Lumps[lumpIndex].Clear();
            }

            // Clear lump and write new BSP
            if (willModify)
            {
                Console.WriteLine("Writing to original file");
                bsp.WriteBSP(opts.BSPPath);
            }

            Console.WriteLine("Done");
        }

        /// <summary>
        /// Validates the options.
        /// </summary>
        /// <param name="opts">The lump extraction options</param>
        /// <exception cref="InvalidOptionException">Thrown if opts.BSPPath is not a valid file or opts.LumpIndex is not a valid lump index.</exception>
        public static void ValidateOptions(Options opts)
        {
            if (!File.Exists(opts.BSPPath))
                throw new InvalidOptionException("Invalid .bsp file specified.");

            if (opts.LumpIndex < MinLumpIndex || opts.LumpIndex > MaxLumpIndex)
                throw new InvalidOptionException("Invalid lump index specified, must be between 0 and 63 inclusive.");
        }

        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    try
                    {
                        ValidateOptions(opts);
                        ExtractLump(opts);
                    }
                    catch (LumpEmptyException)
                    {
                        Console.WriteLine("Lump empty, nothing to do.");
                    }
                    catch (InvalidOptionException e)
                    {
                        Console.WriteLine(e.Message);
                        Environment.ExitCode = ExitFail;
                    }
                });
        }

        public class Options
        {
            [Option('X', "no-extract", Required = false, HelpText = "Don't create any .lmp file.", SetName = "Main")]
            public bool DontExtract { get; set; }

            [Option('C', "no-clear", Required = false, HelpText = "Don't clear the lump data from the .bsp file.",
                SetName = "Main")]
            public bool DontClear { get; set; }

            [Option('b', "backup",
                HelpText =
                    "Create a <filename>.orig backup file in the same directory as the .bsp before modifying the original.")]
            public bool MakeBackup { get; set; }

            [Value(0, MetaName = "BSP file", HelpText = "The .bsp file to process.", Required = true)]
            public string BSPPath { get; set; }

            [Value(1, MetaName = "Lump index", HelpText = "The lump index to process.", Required = true)]
            public int LumpIndex { get; set; }
        }
    }
}