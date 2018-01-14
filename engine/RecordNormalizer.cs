using System;
using System.Collections.Generic;
using System.Linq;
using Groupping.NET.Algorithms;

namespace Groupping.NET
{
    public class Normalizer
    {
        public enum Type
        {
            No,
            Simple
        }

        public static INormalizer<Record> Get(Type type)
        {
            switch (type)
            {
                case Type.No:
                    return new NoNormalizer();
                case Type.Simple:
                    return new SimpleNormalizer();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private class NoNormalizer : INormalizer<Record>
        {
            public IEnumerable<Record> Normalize(IEnumerable<Record> items)
            {
                if (items == null)
                    throw new ArgumentNullException(nameof(items));

                if (!items.Any())
                    yield break;

                foreach (var item in items)
                {
                    yield return new Record
                    {
                        Index = item.Index,
                        Attributes = (double[])item.Attributes.Clone()
                    };
                }
            }
        }

        private class SimpleNormalizer : INormalizer<Record>
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
    }
}