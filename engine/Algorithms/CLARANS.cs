using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;

namespace Groupping.NET.Algorithms
{
    public class CLARANS<T>
    {
        private readonly Random _random = new Random();

        private readonly INormalizer<T> _normalizer;
        private readonly IDistance<T> _distance;
        private readonly int _n;
        private readonly int _k;
        private readonly int _maxNeighbour;
        private readonly int _numLocal;

        public Result<T> Result { get; }

        public CLARANS(
            T[] items,
            INormalizer<T> normalizer,
            IDistance<T> distance,
            int k,
            int maxNeighbour,
            int numLocal,
            int degreeOfParallelism = 4)
        {
            _normalizer = normalizer;
            _distance = distance;
            _n = items.Length;
            _k = k;
            _maxNeighbour = maxNeighbour;
            _numLocal = numLocal;

            var itemsNorm = normalizer
                .Normalize(items)
                .ToArray();

            var claransResult = Enumerable.Range(0, numLocal)
                .AsParallel()
                .WithDegreeOfParallelism(degreeOfParallelism)
                .Select(_ => OptimizeOne(itemsNorm))
                .MaxBy(result => result.TotalClusterDistance);

            Result = new Result<T>(
                items,
                _distance,
                _k,
                claransResult.MedoidIndices,
                claransResult.ClusterNums);
        }

        private CLARANSResult OptimizeOne(T[] items)
        {
            int[] medoids = new int[_k];
            int[] nextMedoids = new int[_k];

            int[] clusters = new int[_n];
            int[] nextClusters = new int[_n];

            double[] distances = new double[_n];
            double[] nextDistances = new double[_n];

            double totalDistance = Seed(items, medoids, clusters, distances);

            Array.Copy(medoids, nextMedoids, _k);
            Array.Copy(clusters, nextClusters, _n);
            Array.Copy(distances, nextDistances, _n);

            for (int iNeighbour = 0; iNeighbour < _maxNeighbour; iNeighbour++)
            {
                double nextTotalDistance = SwapOne(items, nextMedoids, nextClusters, nextDistances);

                if (nextTotalDistance < totalDistance)
                {
                    // repeat the neighbourhood search if a better neighbour found
                    iNeighbour = 0;
                    totalDistance = nextTotalDistance;

                    Array.Copy(nextMedoids, medoids, _k);
                    Array.Copy(nextClusters, clusters, _n);
                    Array.Copy(nextDistances, distances, _n);
                }
                else
                {
                    Array.Copy(medoids, nextMedoids, _k);
                    Array.Copy(clusters, nextClusters, _n);
                    Array.Copy(distances, nextDistances, _n);
                }
            }

            return new CLARANSResult(medoids, clusters, distances, totalDistance);
        }

        private double SwapOne(T[] items, int[] medoids, int[] clusters, double[] distances)
        {
            int n = items.Length;
            int swapMedoid = _random.Next(_k);
            int swapItem;

            do
                swapItem = _random.Next(n);
            while (medoids.Any(medoid => medoid == swapItem));

            medoids[swapMedoid] = swapItem;
            T newMedoid = items[medoids[swapMedoid]];

            for (int i = 0; i < n; ++i)
            {
                double dist = _distance.Measure(items[i], newMedoid);
                if (distances[i] > dist)
                {
                    clusters[i] = swapMedoid;
                    distances[i] = dist;
                }
                else if (clusters[i] == swapMedoid)
                {
                    distances[i] = dist;
                    clusters[i] = swapMedoid;
                    for (int j = 0; j < _k; ++j)
                    {
                        if (j != swapMedoid)
                        {
                            dist = _distance.Measure(items[i], items[medoids[j]]);
                            if (distances[i] > dist)
                            {
                                clusters[i] = j;
                                distances[i] = dist;
                            }
                        }
                    }
                }
            }

            return distances.Sum();
        }

        private double Seed(T[] items, int[] medoids, int[] clusters, double[] distances)
        {
            int n = items.Length;
            int k = medoids.Length;
            
            // randomly pick the first medoid
            medoids[0] = _random.Next(n);

            Array.Fill(distances, double.MaxValue);

            for (int j = 1; j < k; j++)
            {
                // Loop over the samples and compare them to the most recent medoid.
                // Store the distance from each sample to its closest medoid in scores.
                for (int i = 0; i < n; i++)
                {
                    // compute the distance between this sample and the current medoid
                    double dist = _distance.Measure(items[i], items[medoids[j - 1]]);
                    if (dist < distances[i])
                    {
                        distances[i] = dist;
                        clusters[i] = j - 1;
                    }
                }

                // pick the next medoid
                double cutoff = _random.NextDouble() * distances.Sum();
                double cost = 0.0;
                int index = 0;
                for (; index < n; index++)
                {
                    cost += distances[index];
                    if (cost >= cutoff)
                        break;
                }

                medoids[j] = index;
            }

            for (int i = 0; i < n; i++)
            {
                // compute the distance between this sample and the current medoid
                double dist = _distance.Measure(items[i], items[medoids[k - 1]]);
                if (dist < distances[i])
                {
                    distances[i] = dist;
                    clusters[i] = k - 1;
                }
            }

            return distances.Sum();
        }

        private class CLARANSResult
        {
            public CLARANSResult(int[] medoidIndices, int[] clusterNums, double[] clusterDistances, double totalClusterDistance)
            {
                MedoidIndices = medoidIndices;
                ClusterNums = clusterNums;
                ClusterDistances = clusterDistances;
                TotalClusterDistance = totalClusterDistance;
            }

            public int[] MedoidIndices { get; }

            public int[] ClusterNums { get; }

            public double[] ClusterDistances { get; }

            public double TotalClusterDistance { get; }
        }
    }
}