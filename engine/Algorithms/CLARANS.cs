using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clarans.net.src;

namespace Groupping.NET.Algorithms
{
    public class CLARANS<T>
    {
        private readonly Random _random = new Random();

        private readonly T[] _items;
        private readonly IDistance<T> _distance;
        private readonly int _k;
        private readonly int _maxNeighbour;
        private readonly int _numLocal;

        public CLARANS(T[] items, IDistance<T> distance, int k, int maxNeighbour, int numLocal)
        {
            _distance = distance;
            _k = k;
            _maxNeighbour = maxNeighbour;
            _numLocal = numLocal;
        }

        public CLARANSResult Optimize(T[] items)
        {
            throw new NotImplementedException();
        }

        private CLARANSResult OptimizeOne(T[] items)
        {
            int n = items.Length;

            int[] medoids = new int[_k];
            int[] nextMedoids = new int[_k];

            int[] clusters = new int[n];
            int[] nextClusters = new int[n];

            double[] distances = new double[n];
            double[] nextDistances = new double[n];

            double totalDistance = Seed(items, medoids, clusters, distances);

            Array.Copy(medoids, nextMedoids, _k);
            Array.Copy(clusters, nextClusters, n);
            Array.Copy(distances, nextDistances, n);

            for (int iNeighbour = 0; iNeighbour < _maxNeighbour; iNeighbour++)
            {
                double nextTotalDistance = SwapOne(_items, nextMedoids, nextClusters, nextDistances);

                if (nextTotalDistance < totalDistance)
                {
                    iNeighbour = 0;
                    totalDistance = nextTotalDistance;

                    Array.Copy(nextMedoids, medoids, _k);
                    Array.Copy(nextClusters, clusters, n);
                    Array.Copy(nextDistances, distances, n);
                }
                else
                {
                    Array.Copy(medoids, nextMedoids, _k);
                    Array.Copy(clusters, nextClusters, n);
                    Array.Copy(distances, nextDistances, n);
                }
            }

            return new CLARANSResult(medoids, clusters, totalDistance);
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
            return double.MaxValue;
        }

        public class CLARANSResult
        {
            public CLARANSResult(int[] medoids, int[] clusters, double totalDistance)
            {
                Medoids = medoids;
                Clusters = clusters;
                TotalDistance = totalDistance;
            }

            public int[] Medoids { get; set; }

            public int[] Clusters { get; set; }

            public double TotalDistance { get; set; }
        }
    }
}