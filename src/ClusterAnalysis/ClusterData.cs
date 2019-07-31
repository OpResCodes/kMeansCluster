using System;
using System.Text;


namespace ClusterAnalysis
{
    public class ClusterData
    {
        public ClusterData(double[][] rawData, string[] attributeNames, double[] attributeWeights = null)
        {
            RawData = rawData;
            RowCount = RawData.Length;
            ColumnCount = RawData[0].Length;
            if (attributeWeights != null)
            {
                AttributeWeights = attributeWeights;
            }
            else
            {
                AttributeWeights = new double[ColumnCount];
                for (int col = 0; col < ColumnCount; col++)
                {
                    AttributeWeights[col] = 1;
                }
            }
            AttributeNames = attributeNames;
        }
               
        public double[][] RawData { get; }

        public double[] AttributeWeights { get; }

        public string[] AttributeNames { get; }

        public int RowCount { get; }

        public int ColumnCount { get; }

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
