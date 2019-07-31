using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterAnalysis
{
    public class RawDataReader
    {
        private static readonly CultureInfo culture = new CultureInfo("en-US");

        public static ClusterData ReadFromCsv(string fileName, params char[] separator)
        {
            using (var reader = new StreamReader(fileName))
            {
                var attributes = GetAttributeNames(reader, separator);
                var weights = GetAttributeWeights(reader, separator);
                var rawData = GetRawData(reader, separator);
                ClusterData clusterData = new ClusterData(rawData, attributes, weights);
                return clusterData;
            }
        }

        private static double[][] GetRawData(StreamReader reader, params Char[] sep)
        {
            string readLine = null;

            int colCount = -1;

            List<double[]> data = new List<double[]>();

            while (reader.Peek() >= 0)
            {
                readLine = reader.ReadLine().Trim();
                if (!string.IsNullOrWhiteSpace(readLine))
                {
                    var col = readLine.Split(sep);
                    
                    if (colCount == -1)
                        colCount = col.Length - 1;

                    if (col.Length - 1 != colCount)
                        throw new ArgumentException($"Inconsistent number of columns in row: '{readLine}'");

                    double[] rowValues = new double[colCount];

                    for (int i = 1; i < col.Length; i++)
                    {
                        rowValues[i - 1] = double.Parse(col[i].Trim(),culture);
                    }
                    data.Add(rowValues);
                }
            }
            return data.ToArray();
        }
        
        private static double[] GetAttributeWeights(StreamReader reader, params Char[] separator)
        {
            if (reader.Peek() < 0)
                throw new ArgumentException("Data file empty!");

            string textRow = reader.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(textRow))
                throw new ArgumentException("Data file empty!");

            string[] textCols = textRow.Split(separator);
            double[] w = new double[textCols.Length - 1];
            for (int c = 1; c < textCols.Length; c++)
            {
                w[c - 1] = double.Parse(textCols[c]);
            }
            return w;
        }

        private static string[] GetAttributeNames(StreamReader reader, params char[] separator)
        {
            string textRow = reader.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(textRow))
                throw new ArgumentException("Data file empty!");

            string[] header = textRow.Split(separator);
            string[] labels = new string[header.Length - 1];
            for (int col = 1; col < header.Length; col++)
            {
                labels[col - 1] = header[col].Trim();
            }
            return labels;
        }
    }
}
