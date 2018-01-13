using System;
using System.IO;
using CommandLine;

namespace Groupping.NET
{
    class Program
    {
        class Options
        {
            [Option('f', "file", Required = true, HelpText = "Input file to be processed.")]
            public string InputFile { get; set; }

            [Option('n', "max-neighbour", HelpText = "Algorithm MaxNeighbour parameter.")]
            public int MaxNeighbour { get; set; }

            [Option('l', "num-local", HelpText = "Algorithm NumLocal parameter.")]
            public int NumLocal { get; set; }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            var isValid = CommandLine.Parser.Default.ParseArgumentsStrict(args, options);

            using (var streamReader = new StreamReader(options.InputFile))
            {
                int i = 0;
                while(streamReader.ReadLine() != null) ++i;
            }
        }
    }
}
