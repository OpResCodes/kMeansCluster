using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace ClusterAnalysis
{
    public class ClusterSolver
    {
        public static ClusterEvaluation[] Cluster(ClusterData data, int minClusterCount, int maxClusterCount, int repetitions = 20, bool normalizeData = true)
        {
            if (repetitions <= 0)
                throw new ArgumentException("repetitions > 0!");
            if (maxClusterCount - minClusterCount < 0)
                throw new ArgumentException("invalud cluster counts");

            ClusterEvaluation[] ce = new ClusterEvaluation[maxClusterCount-minClusterCount + 1];
            int probe = -1;
            for (int c = minClusterCount; c <= maxClusterCount; c++)
            {
                double inner = double.MaxValue;
                int success = 0;
                double iter = 0;
                int bestSeed = 0;
                for (int i = 0; i < repetitions; i++)
                {
                    int seed = GetSeed(RandomSeedGenerator.Generate());
                    ClusterAlgorithmOptions options = new ClusterAlgorithmOptions(c, seed, normalizeData);
                    var result = KmeansClusterAlgorithm.Analyze(data, options);
                    if (result.TerminationStatus != Status.EmptyClusters)
                    {
                        success++;
                        if(inner > result.TotalClusterToPointDistance)
                        {
                            inner = result.TotalClusterToPointDistance;
                            bestSeed = seed;
                        }
                        iter += result.Iterations;
                    }
                }
                probe++;

                if(success < 1)
                {
                    inner = -1;
                }
                ce[probe] = new ClusterEvaluation(c, success, 
                    Math.Round(iter / (double)repetitions, 2),
                    Math.Round(inner, 2),
                    bestSeed);
            }
            return ce;
        }

        private static int GetSeed(string seed)
        {
            int seedValue;
            using (var sha = SHA1.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
                seedValue = BitConverter.ToInt32(hash, 0);
            }
            return seedValue;
        }

    }

    public struct ClusterEvaluation
    {

        public ClusterEvaluation(int clusters, int runs, double iterations, double innerCluster, int bestSeed)
        {
            NumberOfClusters = clusters;
            NumberOfRuns = runs;
            InnerClusterDistance = innerCluster;
            Iterations = iterations;
            BestSeed = bestSeed;
        }

        public int NumberOfClusters { get; }

        public double Iterations { get; }

        public int NumberOfRuns { get; }

        public int BestSeed { get; }

        public double InnerClusterDistance { get; }

    }



}
