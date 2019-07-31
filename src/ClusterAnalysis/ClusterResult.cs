using System;
using System.Text;

namespace ClusterAnalysis
{
    public class ClusterResult
    {

        public ClusterResult()
        {
            Iterations = 0;
            TerminationStatus = Status.Unknown;
        }

        public int[] ClusterAssignment { get; set; }

        public double[][] ClusterMeanValues { get; set; }

        public double[][] NormalizedData { get; set; }

        public double TotalClusterToPointDistance { get; set; }

        public int Iterations { get; set; }

        public Status TerminationStatus { get; set; }

        public string PrintCsvResult()
        {
            int colCount = NormalizedData[0].Length;
            StringBuilder b = new StringBuilder();
            b.Append("Row;Type;Cluster");
            for (int c = 0; c < colCount; c++)
            {
                b.Append($";PROP_NORM_{c + 1}");
            }
            b.Append(Environment.NewLine);

            int clusterCount = ClusterMeanValues.Length;
            for (int cl = 0; cl < clusterCount; cl++)
            {
                b.Append($"C{cl};CLUS;{cl}");
                for (int c = 0; c < colCount; c++)
                {
                    b.Append($";{ClusterMeanValues[cl][c]}");
                }
                b.Append(Environment.NewLine);
            }
            int rowCount = NormalizedData.Length;
            for (int row = 0; row < rowCount; row++)
            {
                b.Append($"{row};DATA;{ClusterAssignment[row]}");
                for (int col = 0; col < colCount; col++)
                {
                    b.Append($";{NormalizedData[row][col]}");
                }
                b.Append(Environment.NewLine);
            }
            return b.ToString();
        }

    }

    public enum Status
    {
        MaxIterationsReached,
        Convergence,
        EmptyClusters,
        Unknown
    }


}
