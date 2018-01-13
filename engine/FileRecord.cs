using System;

namespace Groupping.NET
{
    public class FileRecord
    {
        public int Index { get; set; }

        public double[] Attributes { get; set; }

        public override string ToString()
        {
            return $"{Index} [{string.Join(", ", Attributes)}]";
        }
    }

    public class FileRecordEuclidianDistance : IDistance<FileRecord>
    {
        public double Measure(FileRecord t1, FileRecord t2)
        {
            double accumulate = 0.0;
            for (int i = 0; i < t1.Attributes.Length; i++)
                accumulate +=
                    (t1.Attributes[i] - t2.Attributes[i]) *
                    (t1.Attributes[i] - t2.Attributes[i]);

            return Math.Sqrt(accumulate);
        }
    }

    public class FileRecordManhattanDistance : IDistance<FileRecord>
    {
        public double Measure(FileRecord t1, FileRecord t2)
        {
            double accumulate = 0.0;
            for (int i = 0; i < t1.Attributes.Length; i++)
                accumulate +=
                    Math.Abs(t1.Attributes[i] - t2.Attributes[i]);

            return accumulate;
        }
    }
}