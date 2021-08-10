using CommandLine;
using System;
using System.Collections.Generic;

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
            public bool ClearIconCache { get; set; }

            [Option ('s', "skipexisting", Required = false, Default = false, HelpText = "Skips folders with existing desktop.ini folder.")]
            public bool SkipExisting { get; set; }

            [Option ('c', "shortcover", Required = false, Default = false, HelpText = "Uses shorter cover design to reveal more contents.")]
            public bool ShortCover { get; set; }

            [Option ('m', "maxthumbs", Required = false, Default = 3, HelpText = "Define maximum number of content to include in thumbnail.")]
            public int MaxThumbCount { get; set; }

            [Option ('t', "maxthreads", Required = false, Default = 1, HelpText = "Define maximum number of threads to use.")]
            public int MaxThreads { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(OptionHandler)
                .WithNotParsed(ParseErrorHandler);
        }

        static void OptionHandler(Options opt)
        {
            
        }

        static void ParseErrorHandler(IEnumerable<Error> errs)
        {
            
        }
    }
}
