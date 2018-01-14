using System;
using System.Collections.Generic;
using System.Linq;
using Groupping.NET.Algorithms;

namespace Groupping.NET
{
    public class RecordResult
    {
        public RecordResult(int index, bool isMedoid, int clusterNo, double clusterDistance, double silhuetteIndex)
        {
            Index = index;
            IsMedoid = isMedoid;
            ClusterNo = clusterNo;
            ClusterDistance = clusterDistance;
            SilhouetteIndex = silhuetteIndex;
        }

        public int Index { get; }

        public bool IsMedoid { get; }

        public int ClusterNo { get; }

        public double ClusterDistance { get; }

        public double SilhouetteIndex { get; set; }
    }

    public class Result<T>
    {
        private readonly T[] _items;
        private readonly IDistance<T> _distance;
        private readonly int _n;
        private readonly int _k;
        private readonly int[] _medoidIndices;
        private readonly int[] _clusterNums;

        public RecordResult[] Records { get; }
        public double TotalClusterDistance { get; }
        public double GlobalSilhuetteIndex { get; }

        public Result(
            T[] items,
            IDistance<T> distance,
            int k,
            int[] medoidIndices,
            int[] clusterNums)
        {
            _items = items;
            _distance = distance;
            _n = _items.Length;
            _k = k;
            _medoidIndices = medoidIndices;
            _clusterNums = clusterNums;

            Records = EnumerateRecordResults().ToArray();

            TotalClusterDistance = Records.Sum(r => r.ClusterDistance);
            GlobalSilhuetteIndex = Records.Average(r => r.SilhouetteIndex);
        }

        private IEnumerable<RecordResult> EnumerateRecordResults()
        {
            int[] clusterQty = new int[_k];
            double[] clusterDistance = new double[_n];
            double[,] avgClusterDistance = new double[_n, _k];

            for (int i = 0; i < _clusterNums.Length; i++)
            {
                // count total number of items in each cluster
                clusterQty[_clusterNums[i]] += 1;

                // calculate distance to own medoid
                clusterDistance[i] = _distance.Measure(_items[i], _items[_medoidIndices[_clusterNums[i]]]);

                // calculate average distance to each cluster
                for (int j = i + 1; j < _clusterNums.Length; j++)
                {
                    var dist = _distance.Measure(_items[i], _items[j]);
                    avgClusterDistance[i, _clusterNums[j]] += dist;
                    avgClusterDistance[j, _clusterNums[i]] += dist;
                }
            }

            for (int i = 0; i < _clusterNums.Length; i++)
            {
                for (int j = 0; j < _k; j++)
                {
                    if (j == _clusterNums[i])
                    {
                        // own cluster
                        avgClusterDistance[i, j] = clusterQty[j] == 1
                            ? 0
                            : avgClusterDistance[i, j] / (clusterQty[j] - 1);
                    }
                    else
                    {
                        // other cluster
                        avgClusterDistance[i, j] = clusterQty[j] == 0
                            ? double.MaxValue
                            : avgClusterDistance[i, j] / clusterQty[j];
                    }
                }
            }

            double calculateSilhuette(int i)
            {
                double a = double.MaxValue;
                double b = double.MaxValue;

                for (int j = 0; j < _k; j++)
                {
                    double d = avgClusterDistance[i, j];
                    if (j == _clusterNums[i])
                        a = d;
                    else if (b > d)
                        b = d;
                }

                return (b - a) / Math.Max(b, a);
            }

            for (int i = 0; i < _clusterNums.Length; i++)
            {
                yield return new RecordResult(
                    index: i,
                    isMedoid: _medoidIndices.Any(m => m == i),
                    clusterNo: _clusterNums[i],
                    clusterDistance: clusterDistance[i],
                    silhuetteIndex: calculateSilhuette(i));
            }
        }
    }
}