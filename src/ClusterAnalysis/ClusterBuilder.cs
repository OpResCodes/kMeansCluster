using MathNet.Numerics.Statistics;
using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("ClusterAnalysis.Tests")]

namespace ClusterAnalysis
{
    /// <summary>
    /// Use this class to determine clusters from your data
    /// </summary>
    public class ClusterBuilder
    {
        public static ClusterResult Analyze(ClusterData clusterData)
        {
            ClusterResult result = new ClusterResult();
            bool clusterIsAssigned = false;
            bool clusterResultChanged = false;
            bool maxIterationsReached = false;

            result.NormalizedData = Normalize(clusterData.RawData);
            InitializeClusters(clusterData, result);

            do
            {
                clusterIsAssigned = UpdateClusterMeanValues(result);
                if (clusterIsAssigned)
                {
                    clusterResultChanged = UpdateClusterAssignment(result);
                    result.Iterations++;
                    maxIterationsReached = (result.Iterations >= clusterData.MaximumIterationCount);
                }

            } while (clusterIsAssigned && clusterResultChanged && !maxIterationsReached);

            return result;
        }

        internal static bool UpdateClusterAssignment(ClusterResult result)
        {
            bool updated = false;
            int colCount = result.ClusterMeanValues[0].Length;
            int clCount = result.ClusterMeanValues.Length;
            int rowCount = result.NormalizedData.Length;

            //iterate each data point
            for (int row = 0; row < rowCount; row++)
            {
                //determine distance to each cluster
                double[] distances = new double[clCount];
                for (int cluster = 0; cluster < clCount; cluster++)
                {
                    distances[cluster] = GetDistance(
                        result.ClusterMeanValues[cluster],
                        result.NormalizedData[row]
                        );
                }
                //update assignment with best found cluster
                int foundCluster = FindMinimumIndex(distances);
                if (foundCluster != result.ClusterAssignment[row])
                {
                    updated = true;
                    result.ClusterAssignment[row] = foundCluster;
                }
            }
            return updated;
        }

        internal static double GetDistance(double[] means, double[] data)
        {
            double sqaredDev = 0;
            for (int i = 0; i < means.Length; i++)
            {
                sqaredDev += (means[i] - data[i]) * (means[i] - data[i]);
            }
            return Math.Sqrt(sqaredDev);
        }

        internal static int FindMinimumIndex(double[] distances)
        {
            double minValue = double.MaxValue;
            int idx = 0;
            for (int i = 0; i < distances.Length; i++)
            {
                if (distances[i] < minValue)
                {
                    idx = i;
                    minValue = distances[i];
                }
            }
            return idx;
        }

        internal static bool UpdateClusterMeanValues(ClusterResult result)
        {
            int clusterCount = result.ClusterMeanValues.Length;
            int colCount = result.ClusterMeanValues[0].Length;
            //create array for cluster totals
            double[][] totals = new double[clusterCount][];
            for (int c = 0; c < clusterCount; c++)
            {
                totals[c] = new double[colCount + 1];//additional col for assignment counting
            }
            //fill that cluster total array and count assigned points
            for (int row = 0; row < result.ClusterAssignment.Length; row++)
            {
                int cluster = result.ClusterAssignment[row];
                totals[cluster][colCount] += 1;//count number of assigned points
                for (int col = 0; col < colCount; col++)
                {
                    totals[cluster][col] += result.NormalizedData[row][col];
                }
            }
            //update the mean by dividing through number of assigned points
            for (int cluster = 0; cluster < clusterCount; cluster++)
            {
                if (totals[cluster][colCount] == 0)
                    return false;
                for (int col = 0; col < colCount; col++)
                {
                    result.ClusterMeanValues[cluster][col] =
                        totals[cluster][col] / totals[cluster][colCount];
                }
            }
            return true;
        }

        internal static void InitializeClusters(ClusterData input, ClusterResult result)
        {
            Random random = new Random(input.RandomSeed);
            int dataPoints = input.RawData.Length;
            result.ClusterAssignment = new int[dataPoints];
            // ensure each cluster gets one data point
            for (int row = 0; row < input.NumberOfClusters; row++)
            {
                result.ClusterAssignment[row] = row;
            }
            //assign rest of data points randomly
            for (int row = input.NumberOfClusters; row < dataPoints; row++)
            {
                result.ClusterAssignment[row] = random.Next(0, input.NumberOfClusters);
            }
            //initialize mean value matrix
            result.ClusterMeanValues = new double[input.NumberOfClusters][];
            int colCount = input.RawData[0].Length;
            for (int cluster = 0; cluster < input.NumberOfClusters; cluster++)
            {
                result.ClusterMeanValues[cluster] = new double[colCount];
            }
        }

        internal static double[][] Normalize(double[][] rawData)
        {
            int colCount = rawData[0].Length;
            int rowCount = rawData.Length;

            // initialize result matrix
            double[][] normalizedValues = new double[rowCount][];
            for (int row = 0; row < rowCount; row++)
            {
                normalizedValues[row] = new double[rawData[row].Length];
            }

            // normalize values
            for (int col = 0; col < colCount; col++)
            {
                double[] colData = GetColData(col, rawData);
                double mean = ArrayStatistics.Mean(colData);
                double stDev = ArrayStatistics.PopulationStandardDeviation(colData);
                for (int row = 0; row < rowCount; row++)
                {
                    normalizedValues[row][col] = (rawData[row][col] - mean) / stDev;
                }
            }
            return normalizedValues;
        }

        internal static double[] GetColData(int col, double[][] rawData)
        {
            double[] column = new double[rawData.Length];
            for (int i = 0; i < column.Length; i++)
            {
                column[i] = rawData[i][col];
            }
            return column;
        }
    }

}
