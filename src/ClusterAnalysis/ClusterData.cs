using System;
using System.Text;


namespace ClusterAnalysis
{
    public class ClusterData
    {
        public ClusterData()
        {
            RandomSeed = 3238666;
        }

        public int NumberOfClusters { get; set; }

        public int MaximumIterationCount { get; set; }

        public int RandomSeed { get; set; }

        public double[][] RawData { get; set; }

        public string PrintRawData()
        {
            StringBuilder sb = new StringBuilder();
            int cols = RawData[0].Length;

            sb.Append("Row;Col1");
            for (int col = 1; col < cols; col++)
            {
                sb.AppendFormat(";Col{0}", (col + 1).ToString());
            }
            sb.Append(Environment.NewLine);

            for (int row = 0; row < RawData.Length; row++)
            {
                sb.Append($"{row.ToString()}");
                for (int col = 0; col < RawData[row].Length; col++)
                {
                    sb.AppendFormat(";{0}", RawData[row][col].ToString("N4"));
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

    }


}
