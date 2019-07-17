using System;
using System.Security.Cryptography;
using System.Text;


namespace ClusterAnalysis
{
    public class MetaClusterTester
    {
        public static string Cluster(ClusterData data)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Clusters;Seed;Iterations;C2P_Avg;C2C_Avg");
            int clusters = data.NumberOfClusters;
            for (int c = 1; c < clusters+6; c++)
            {
                int clCnt = clusters + c;
                for (int i = 0; i < 500; i++)
                {
                    int seed = GetSeed(RandomSeedGenerator.Generate());
                    ClusterData d = new ClusterData();
                    d.MaximumIterationCount = data.MaximumIterationCount;
                    d.NumberOfClusters = clCnt;
                    d.RandomSeed = seed;
                    d.RawData = data.RawData;
                    var result = ClusterBuilder.Analyze(d);
                    b.AppendLine(
                        $"{clCnt};{seed};{result.Iterations};{result.AverageClusterToPointDistance};{result.AverageInterClusterDistance}");
                }
            }
            return b.ToString();
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


}
