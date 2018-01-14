using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CsvHelper;
using Groupping.NET.Algorithms;
using MoreLinq;
using Newtonsoft.Json;

namespace Groupping.NET
{
    class Program
    {
        class Options
        {
            [Option('f', "input-file", Required = true, HelpText = "Input CSV file to be processed.")]
            public string InputFile { get; set; }

            [Option('k', "k", HelpText = "alg. param.: K - number of clusters.", DefaultValue = 3)]
            public int K { get; set; }

            [Option('n', "max-neighbour", HelpText = "alg. param.: MaxNeighbour.", DefaultValue = 5)]
            public int MaxNeighbour { get; set; }

            [Option('l', "num-local", HelpText = "alg. param.: NumLocal.", DefaultValue = 1)]
            public int NumLocal { get; set; }

            [Option('z', "normalize", HelpText = "Attribute normalization method used [No, Simple].", DefaultValue = Normalizer.Type.Simple)]
            public Normalizer.Type NormalizerType { get; set; }

            [Option('m', "metric", HelpText = "Metric used to caluclate distances [Euclidean, Manhattan, Minkowski].", DefaultValue = Metric.Type.Euclidean)]
            public Metric.Type MetricType { get; set; }

            [Option('o', "output-file", HelpText = "(Default: {input-file}.out) Output CSV file with clustering results.")]
            public string OutputFile { get; set; }

            [Option('t', "indicator-file", HelpText = "(Deafult: {input-file}.ind.out) Output file with list of clusering indicators.")]
            public string IndicatorFile { get; set; }

            [Option('d', "delimiter", HelpText = "Input CSV file record delimiter.", DefaultValue = ",")]
            public string Delimiter { get; set; }

            [Option('u', "culture", HelpText = "Culture used by the application.", DefaultValue = "en-US")]
            public string Culture { get; set; }

            [Option('h', "has-header", HelpText = "Flag indicating presence of the header record in CSV.", DefaultValue = true)]
            public bool HasHeaderRecord { get; set; }

            [Option('c', "column-names", HelpText = "Attribute column names (separated with comma, input CSV file must have a header).")]
            public string ColumnNames { get; set; }

            [Option('i', "column-nums", HelpText = "Attribute column numbers (separated with comma, input CSV file must not have a header).")]
            public string ColumnNums { get; set; }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            var isValid = CommandLine.Parser.Default.ParseArgumentsStrict(args, options);
            
            if (isValid == false)
                return;

            SetupCulture(options.Culture);

            if (string.IsNullOrEmpty(options.OutputFile))
                options.OutputFile = $"{options.InputFile}.out";

            if (string.IsNullOrEmpty(options.IndicatorFile))
                options.IndicatorFile = $"{options.InputFile}.ind.out";

            var normalizer = Normalizer.Get(options.NormalizerType);
            var metric = Metric.Get(options.MetricType);

            Console.WriteLine("Reading input file.");
            Record[] records = null;
            using (var streamReader = new StreamReader(options.InputFile))
            using (var csv = new CsvReader(streamReader))
            {
                csv.Configuration.Delimiter = options.Delimiter;
                csv.Configuration.HasHeaderRecord = options.HasHeaderRecord;

                records = EnumerateFile(options, csv).ToArray();
            }

            Console.WriteLine("Executing the algorithm.");
            var result = new CLARANS<Record>(
                records.ToArray(),
                normalizer,
                metric,
                options.K,
                options.MaxNeighbour,
                options.NumLocal).Result;

            Console.WriteLine("Writing output file.");
            using (var streamWriter = new StreamWriter(options.OutputFile))
            using (var csv = new CsvWriter(streamWriter))
            {
                csv.Configuration.Delimiter = options.Delimiter;
                csv.Configuration.HasHeaderRecord = options.HasHeaderRecord;

                if (result?.Records != null)
                    csv.WriteRecords(result.Records);
            }

            Console.WriteLine("Writing indicator file.");
            using (var streamWriter = new StreamWriter(options.IndicatorFile))
            {
                streamWriter.Write(JsonConvert.SerializeObject(result.Indicators));
                streamWriter.Flush();
            }
        }

        private static void SetupCulture(string cultureString)
        {
            var culture = new System.Globalization.CultureInfo(cultureString);
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        static IEnumerable<Record> EnumerateFile(Options options, CsvReader csv)
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

                    yield return new Record
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

                    yield return new Record
                    {
                        Index = index,
                        Attributes = attributes
                    };
                }
            }
        }
    }
}
