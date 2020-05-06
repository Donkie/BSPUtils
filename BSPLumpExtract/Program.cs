using System;
using System.IO;
using CommandLine;
using LibBSP;

namespace BSPLumpExtract
{
    public class Program
    {
        public const int ExitFail = 1;

        /// <summary>
        /// Performs the lump extraction. Assumes that the options have been validated.
        /// </summary>
        /// <param name="opts">The lump extraction options</param>
        public static void Process(Options opts)
        {
            var lumpIndex = opts.LumpIndex;
            var bspFolder = Path.GetDirectoryName(opts.BSPPath);
            var bspName = Path.GetFileNameWithoutExtension(opts.BSPPath);

            if (bspFolder == null || bspName == null)
                throw new ArgumentException("Invalid BSP path specified");

            // Read BSP
            Console.WriteLine("Reading and parsing BSP");
            var bsp = new BSP(opts.BSPPath);

            if (bsp.Lumps[lumpIndex].Data.Length <= 1)
            {
                Console.WriteLine("Lump empty, nothing to do. Exiting.");
                return;
            }

            // Backup original BSP
            var willModify = !opts.DontClear;
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

            // Clear lump and write new BSP
            if (!opts.DontClear)
            {
                Console.WriteLine("Clearing lump in BSP and overwriting original file");
                bsp.Lumps[lumpIndex].Clear();
                bsp.WriteBSP(opts.BSPPath);
            }

            Console.WriteLine("Done");
        }

        /// <summary>
        /// Validates the options.
        /// </summary>
        /// <param name="opts">The lump extraction options</param>
        private static void ParseOptions(Options opts)
        {
            if (!File.Exists(opts.BSPPath))
            {
                Console.WriteLine("Invalid .bsp file specified.");
                Environment.ExitCode = ExitFail;
                return;
            }

            if (opts.LumpIndex < 0 || opts.LumpIndex > 63)
            {
                Console.WriteLine("Invalid lump index specified, must be between 0 and 63 inclusive.");
                Environment.ExitCode = ExitFail;
                return;
            }

            Process(opts);
        }

        /// <summary>
        /// Program entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(ParseOptions);
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