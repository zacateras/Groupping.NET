using System;
using System.Collections.Generic;
using System.Linq;
using Groupping.NET.Algorithms;

namespace Groupping.NET
{
    public class Record
    {
        public int Index { get; set; }

        public double[] Attributes { get; set; }

        public override string ToString()
        {
            return $"{Index} [{string.Join(", ", Attributes)}]";
        }
    }

    public class RecordNormalizer : INormalizer<Record>
    {
        public IEnumerable<Record> Normalize(IEnumerable<Record> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (!items.Any())
                yield break;

            int attrCount = items
                .First()
                .Attributes
                .Count();

            double[] attrMax = new double[attrCount];
            double[] attrMin = new double[attrCount];
            double[] attrDiff = new double[attrCount];

            Array.Fill(attrMax, double.MinValue);
            Array.Fill(attrMin, double.MaxValue);

            foreach (var item in items)
            {
                for (int i = 0; i < attrCount; i++)
                {
                    if (item.Attributes[i] > attrMax[i])
                        attrMax[i] = item.Attributes[i];
                    if (item.Attributes[i] < attrMin[i])
                        attrMin[i] = item.Attributes[i];
                }
            }

            for (int i = 0; i < attrCount; i++)
                attrDiff[i] = attrMax[i] - attrMin[i];

            foreach (var item in items)
            {
                yield return new Record
                {
                    Index = item.Index,
                    Attributes = item
                        .Attributes
                        .Select((attr, i) => (attr - attrMin[i]) / attrDiff[i])
                        .ToArray()
                };
            }
        }
    }

    public class RecordEuclidianDistance : IDistance<Record>
    {
        public double Measure(Record t1, Record t2)
        {
            double accumulate = 0.0;
            for (int i = 0; i < t1.Attributes.Length; i++)
                accumulate +=
                    (t1.Attributes[i] - t2.Attributes[i]) *
                    (t1.Attributes[i] - t2.Attributes[i]);

            return Math.Sqrt(accumulate);
        }
    }

    public class RecordManhattanDistance : IDistance<Record>
    {
        public double Measure(Record t1, Record t2)
        {
            double accumulate = 0.0;
            for (int i = 0; i < t1.Attributes.Length; i++)
                accumulate +=
                    Math.Abs(t1.Attributes[i] - t2.Attributes[i]);

            return accumulate;
        }
    }

    // TODO : Consider implementing Minkowski distance
}