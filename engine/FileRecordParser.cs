using System;
using System.Collections.Generic;
using System.Linq;

namespace Groupping.NET
{
    public class FileRecordParser
    {
        private readonly string _separator;

        public FileRecordParser(string separator)
        {
            _separator = separator;
        }

        public FileRecord Parse(string line)
        {
            string[] parts = line.Split(_separator);

            return new FileRecord
            {
                Parts = parts.Select(double.Parse).ToArray()
            };
        }
    }

    public class FileRecord
    {
        public double[] Parts { get; set; }
    }

    public class FileRecordEuclidianDistance : IDistance<FileRecord>
    {
        public double Measure(FileRecord t1, FileRecord t2)
        {
            double accumulate = 0.0;
            for (int i = 0; i < t1.Parts.Length; i++)
                accumulate +=
                    (t1.Parts[i] - t2.Parts[i]) *
                    (t1.Parts[i] - t2.Parts[i]);

            return Math.Sqrt(accumulate);
        }
    }

    public class FileRecordManhattanDistance : IDistance<FileRecord>
    {
        public double Measure(FileRecord t1, FileRecord t2)
        {
            double accumulate = 0.0;
            for (int i = 0; i < t1.Parts.Length; i++)
                accumulate +=
                    Math.Abs(t1.Parts[i] - t2.Parts[i]);

            return accumulate;
        }
    }
}