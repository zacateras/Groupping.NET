using System;
using Groupping.NET.Algorithms;

namespace Groupping.NET
{
    public class Metric
    {
        public enum Type
        {
            Euclidean,
            Manhattan,
            Minkowski
        }

        public static IDistance<Record> Get(Type type)
        {
            switch (type)
            {
                case Type.Euclidean:
                    return new RecordEuclidianDistance();
                case Type.Manhattan:
                    return new RecordManhattanDistance();
                case Type.Minkowski:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private class RecordEuclidianDistance : IDistance<Record>
        {
            public double Measure(Record item1, Record item2)
            {
                double accumulate = 0.0;
                for (int i = 0; i < item1.Attributes.Length; i++)
                    accumulate +=
                        (item1.Attributes[i] - item2.Attributes[i]) *
                        (item1.Attributes[i] - item2.Attributes[i]);

                return Math.Sqrt(accumulate);
            }
        }

        private class RecordManhattanDistance : IDistance<Record>
        {
            public double Measure(Record item1, Record item2)
            {
                double accumulate = 0.0;
                for (int i = 0; i < item1.Attributes.Length; i++)
                    accumulate +=
                        Math.Abs(item1.Attributes[i] - item2.Attributes[i]);

                return accumulate;
            }
        }

        private class RecordMinkowskiDistance : IDistance<Record>
        {
            private readonly double _m;
            public RecordMinkowskiDistance(double m)
            {
                if (m == 0.0)
                    throw new ArgumentException("The paramter cannot be equal to 0.", nameof(m));

                _m = m;
            }

            public double Measure(Record item1, Record item2)
            {
                double accumulate = 0.0;
                for (int i = 0; i < item1.Attributes.Length; i++)
                    accumulate +=
                        Math.Pow(Math.Abs(item1.Attributes[i] - item2.Attributes[i]), _m);

                return Math.Pow(accumulate, 1.0 / _m);
            }
        }
    }
}