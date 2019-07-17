using System;
using System.Text;

namespace ClusterAnalysis
{
    public class ClusterResult
    {

        public ClusterResult()
        {
            Iterations = 0;
        }

        public int[] ClusterAssignment { get; set; }

        public double[][] ClusterMeanValues { get; set; }

        public double[][] NormalizedData { get; set; }

        public double AverageClusterToPointDistance {
            get
            {
                int rowCount = ClusterAssignment.Length;
                int clusterCount = ClusterMeanValues.Length;
                int colCount = ClusterMeanValues[0].Length;
                //initialize
                double[] TotalDistance = new double[clusterCount];
                int[] pointCount = new int[clusterCount];
                for (int row = 0; row < rowCount; row++)
                {
                    int cluster = ClusterAssignment[row];
                    pointCount[cluster]++;
                    TotalDistance[cluster] += GetDistance(ClusterMeanValues[cluster],
                        NormalizedData[row]);
                }
                double total = 0;
                for (int cl = 0; cl < clusterCount; cl++)
                {
                    total += TotalDistance[cl] / pointCount[cl];
                }
                return total / clusterCount;
            }
        }

        private double GetDistance(double[] v1, double[] v2)
        {
            double d = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                d += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            }
            return Math.Sqrt(d);
        }

        public double AverageInterClusterDistance {
            get
            {
                double dst = 0;
                int c = ClusterMeanValues.Length;
                for (int cl = 0; cl < c-1; cl++)
                {
                    for (int cl2 = 0; cl2 < c; cl2++)
                    {
                        dst += GetDistance(ClusterMeanValues[cl], ClusterMeanValues[cl2]);
                    }
                }
                var cnx = (c * (c-1) ) / 2;
                return dst / (double)cnx;
            }
        }

        public int Iterations { get; set; }
        
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


}
