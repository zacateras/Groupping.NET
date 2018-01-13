using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CsvHelper;
using Groupping.NET.Algorithms;
using MoreLinq;

namespace Groupping.NET
{
    class Program
    {
        class Options
        {
            [Option('f', "input-file", Required = true, HelpText = "Input file to be processed.")]
            public string InputFile { get; set; }

            [Option('o', "output-file", HelpText = "Output file with clustering results.")]
            public string OutputFile { get; set; }

            [Option('d', "delimiter", HelpText = "CSV record delimiter.", DefaultValue = ",")]
            public string Delimiter { get; set; }

            [Option('h', "has-header-record", HelpText = "Flag indicating presence of the header record in CSV.", DefaultValue = true)]
            public bool HasHeaderRecord { get; set; }

            [Option('c', "column-names", HelpText = "Attribute column names (separated with comma).")]
            public string ColumnNames { get; set; }

            [Option('i', "column-nums", HelpText = "Attribute column numbers (separated with comma).")]
            public string ColumnNums { get; set; }

            [Option('k', "k", HelpText = "Numer of clusters (K).", DefaultValue = 3)]
            public int K { get; set; }

            [Option('n', "max-neighbour", HelpText = "Algorithm MaxNeighbour parameter.", DefaultValue = 5)]
            public int MaxNeighbour { get; set; }

            [Option('l', "num-local", HelpText = "Algorithm NumLocal parameter.", DefaultValue = 1)]
            public int NumLocal { get; set; }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            var isValid = CommandLine.Parser.Default.ParseArgumentsStrict(args, options);
            
            if (isValid == false)
                return;

            if (string.IsNullOrEmpty(options.OutputFile))
                options.OutputFile = $"{options.InputFile}.out";

            Console.WriteLine("Reading input file.");
            FileRecord[] records = null;
            using (var streamReader = new StreamReader(options.InputFile))
            {
                var csv = new CsvReader(streamReader);

                csv.Configuration.Delimiter = options.Delimiter;
                csv.Configuration.HasHeaderRecord = options.HasHeaderRecord;

                records = EnumerateFile(options, csv).ToArray();
            }

            Console.WriteLine("Executing the algorithm.");
            var clarans = new CLARANS<FileRecord>(
                records.ToArray(),
                new FileRecordEuclidianDistance(),
                options.K,
                options.MaxNeighbour,
                options.NumLocal);

            Console.WriteLine("Writing output file.");
            using (var streamWriter = new StreamWriter(options.OutputFile))
            {
                var csv = new CsvWriter(streamWriter);

                csv.Configuration.Delimiter = options.Delimiter;
                csv.Configuration.HasHeaderRecord = options.HasHeaderRecord;

                csv.WriteHeader<CLARANS<FileRecord>.CLARANSItemResult>();
                clarans?.Result?.ItemResults.ForEach(csv.WriteRecord);
            }
        }

        static IEnumerable<FileRecord> EnumerateFile(Options options, CsvReader csv)
        {
            if (options.HasHeaderRecord)
            {
                csv.Read();
                csv.ReadHeader();

                List<string> headerColumnNames;
                if (options.ColumnNames != null)
                {
                    headerColumnNames = options
                        .ColumnNames
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
                else
                {
                    var msf = csv.Configuration.MissingFieldFound;
                    csv.Configuration.MissingFieldFound = null;

                    headerColumnNames = new List<string>();
                    for (int i = 0; csv[i] != null; i++)
                        headerColumnNames.Add(csv[i]);

                    csv.Configuration.MissingFieldFound = msf;
                }

                while (csv.Read())
                {
                    var index = csv.Context.Row;
                    var attributes = headerColumnNames
                        .Select(columnName => csv[columnName])
                        .Select(double.Parse)
                        .ToArray();

                    yield return new FileRecord
                    {
                        Index = index,
                        Attributes = attributes
                    };
                }
            }
            else
            {
                List<int> headerColumnNums = null;
                if (options.ColumnNums != null)
                {
                    headerColumnNums = options
                        .ColumnNums
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();
                }

                while (csv.Read())
                {
                    if (headerColumnNums == null)
                    {
                        var msf = csv.Configuration.MissingFieldFound;
                        csv.Configuration.MissingFieldFound = null;

                        headerColumnNums = new List<int>();
                        for (int i = 0; csv[i] != null; ++i)
                            headerColumnNums.Add(i);

                        csv.Configuration.MissingFieldFound = msf;
                    }

                    var index = csv.Context.Row;
                    var attributes = headerColumnNums
                        .Select(columnNum => csv[columnNum])
                        .Select(double.Parse)
                        .ToArray();

                    yield return new FileRecord
                    {
                        Index = index,
                        Attributes = attributes
                    };
                }
            }
        }
    }
}
