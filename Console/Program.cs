using CommandLine;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thumbnail_Generator_Library;

namespace Thumbnail_Generator_Console
{
    class Program
    {

        public class Options
        {
            [Option ('d', "directory", Required = true, HelpText = "Sets the directory to process.")]
            public string TargetDirectory { get; set; }

            [Option ('r', "recursive", Required = false, Default = false, HelpText = "Process all subdirectories as well.")]
            public bool Recurse { get; set; }

            [Option ('i', "iconcache", Required = false, Default = false, HelpText = "Clears Windows Explorer icon cache and restarts it automatically.")]
            public bool ClearCache { get; set; }

            [Option ('s', "skipexisting", Required = false, Default = false, HelpText = "Skips folders with existing desktop.ini folder.")]
            public bool SkipExisting { get; set; }

            [Option ('c', "shortcover", Required = false, Default = false, HelpText = "Uses shorter cover design to reveal more contents.")]
            public bool ShortCover { get; set; }

            [Option ('m', "maxthumbs", Required = false, Default = 3, HelpText = "Define maximum number of content to include in thumbnail.")]
            public int MaxThumbCount { get; set; }

            [Option ('t', "maxthreads", Required = false, Default = 1, HelpText = "Define maximum number of threads to use.")]
            public int MaxThreads { get; set; }
        }

        static async Task Main(string[] args)
        {
            await Task.Run(() => {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsedAsync(OptionHandler).Wait();
            });
        }

        static async Task OptionHandler(Options opts)
        {
            ProgressBarOptions options = new()
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            using (ProgressBar progress = new(100, "Processing Directories", options))
            {
                await ProcessHandler.GenerateThumbnailsForFolder(
                    progress.AsProgress<float>(),
                    opts.TargetDirectory,
                    opts.MaxThumbCount,
                    opts.MaxThreads,
                    opts.Recurse,
                    opts.ClearCache,
                    opts.SkipExisting,
                    opts.ShortCover
                );
            }

            Console.WriteLine("Finished Job!");
        }
    }
}
