using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;

namespace Groupping.NET.Algorithms
{
    public class CLARANSResult
    {
        public CLARANSResult(int[] medoids, int[] clusters, double[] distances, double totalDistance)
        {
            Medoids = medoids;
            Clusters = clusters;
            Distances = distances;
            TotalDistance = totalDistance;
        }

        public int[] Medoids { get; }

        public int[] Clusters { get; }

        public double[] Distances { get; }

        public double TotalDistance { get; }

        public IEnumerable<CLARANSItemResult> ItemResults
        {
            get
            {
                for (int i = 0; i < Clusters.Length; i++)
                {
                    yield return new CLARANSItemResult(
                        index: i,
                        medoidFlag: Medoids.Any(m => m == i), 
                        clusterNo: Clusters[i],
                        distance: Distances[i]);
                }
            }
        }
    }

    public class CLARANSItemResult
    {
        public CLARANSItemResult(int index, bool medoidFlag, int clusterNo, double distance)
        {
            Index = index;
            MedoidFlag = medoidFlag;
            ClusterNo = clusterNo;
            Distance = distance;
        }

        public int Index { get; }

        public bool MedoidFlag { get; }

        public int ClusterNo { get; }

        public double Distance { get; }
    }

    public class CLARANS<T>
    {
        private readonly Random _random = new Random();

        private readonly IDistance<T> _distance;
        private readonly INormalizer<T> _normalizer;
        private readonly int _n;
        private readonly int _k;
        private readonly int _maxNeighbour;
        private readonly int _numLocal;

        public CLARANSResult Result { get; }

        public CLARANS(
            T[] items,
            IDistance<T> distance,
            INormalizer<T> normalizer,
            int k,
            int maxNeighbour,
            int numLocal,
            int degreeOfParallelism = 4)
        {
            _distance = distance;
            _normalizer = normalizer;
            _n = items.Length;
            _k = k;
            _maxNeighbour = maxNeighbour;
            _numLocal = numLocal;

            var itemsNorm = normalizer
                .Normalize(items)
                .ToArray();

            Result = Enumerable.Range(0, numLocal)
                .AsParallel()
                .WithDegreeOfParallelism(degreeOfParallelism)
                .Select(_ => OptimizeOne(itemsNorm))
                .MaxBy(result => result.TotalDistance);
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
    }
}