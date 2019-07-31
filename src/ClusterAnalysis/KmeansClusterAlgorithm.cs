using MathNet.Numerics.Statistics;
using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("ClusterAnalysis.Tests")]

namespace ClusterAnalysis
{
    /// <summary>
    /// Use this class to determine clusters from your data
    /// </summary>
    public class KmeansClusterAlgorithm
    {
        public static ClusterResult Analyze(ClusterData input, ClusterAlgorithmOptions options)
        {
            bool clusterIsAssigned;
            bool clusterResultChanged = false;
            bool maxIterationsReached = false;

            ClusterResult result = InitializeClusters(input, options);
            result.NormalizedData = (options.NormalizeData) ? Normalize(input) : input.RawData;

            do
            {
                clusterIsAssigned = UpdateClusterMeanValues(result, input, options);
                if (clusterIsAssigned)
                {
                    clusterResultChanged = UpdateClusterAssignment(result,input, options);
                    result.Iterations++;
                    maxIterationsReached = (result.Iterations >= options.MaximumIterationCount);
                }

            } while (clusterIsAssigned && clusterResultChanged && !maxIterationsReached);

            if (!clusterIsAssigned)
                result.TerminationStatus = Status.EmptyClusters;
            else if (maxIterationsReached)
                result.TerminationStatus = Status.MaxIterationsReached;
            else if (!clusterResultChanged)
                result.TerminationStatus = Status.Convergence;

            if(clusterIsAssigned)
            {
                CalculateObjective(result, input, options);
            }

            return result;
        }

        private static void CalculateObjective(ClusterResult result, ClusterData input, ClusterAlgorithmOptions options)
        {
            //cluster-point-distance
            double[] clusterTotals = new double[options.NumberOfClusters];
            int[] clusterNumbers = new int[options.NumberOfClusters];
            for (int r = 0; r < input.RowCount; r++)
            {
                int clusterId = result.ClusterAssignment[r];
                var m = result.ClusterMeanValues[clusterId];
                var d = result.NormalizedData[r];
                var w = input.AttributeWeights;
                clusterTotals[clusterId] += GetDistance(m, d, w);
                clusterNumbers[clusterId] += 1;
            }
            double withinScatter = 0;
            for (int c = 0; c < options.NumberOfClusters; c++)
            {
                withinScatter += clusterNumbers[c] * clusterTotals[c];
            }
            result.TotalClusterToPointDistance = withinScatter;
        }

        internal static bool UpdateClusterAssignment(ClusterResult result, ClusterData data, ClusterAlgorithmOptions options)
        {
            bool updated = false;
            //iterate each data point
            for (int row = 0; row < data.RowCount; row++)
            {
                //determine distance to each cluster
                double[] distances = new double[options.NumberOfClusters];
                for (int cluster = 0; cluster < options.NumberOfClusters; cluster++)
                {
                    distances[cluster] = GetDistance(
                        result.ClusterMeanValues[cluster],
                        result.NormalizedData[row],
                        data.AttributeWeights
                        );
                }
                //update assignment with best found cluster
                int closestCluster = FindMinimumIndex(distances);
                if (closestCluster != result.ClusterAssignment[row])
                {
                    updated = true;
                    result.ClusterAssignment[row] = closestCluster;
                }
            }
            return updated;
        }

        internal static double GetDistance(double[] m, double[] d, double[] w)
        {
            double sqaredDev = 0;
            for (int i = 0; i < m.Length; i++)
            {
                sqaredDev += w[i] * Math.Pow(m[i] - d[i],2d);
            }
            return sqaredDev;
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

        internal static bool UpdateClusterMeanValues(ClusterResult result, ClusterData data, ClusterAlgorithmOptions options)
        {
            //create array for cluster totals
            double[][] totals = new double[options.NumberOfClusters][];
            for (int c = 0; c < options.NumberOfClusters; c++)
            {
                totals[c] = new double[data.ColumnCount + 1];//additional col for assignment counting
            }
            //fill that cluster total array and count assigned points
            for (int row = 0; row < result.ClusterAssignment.Length; row++)
            {
                int cluster = result.ClusterAssignment[row];
                totals[cluster][data.ColumnCount] += 1;//count number of assigned points

                for (int col = 0; col < data.ColumnCount; col++)
                {
                    totals[cluster][col] += data.AttributeWeights[col] * result.NormalizedData[row][col];
                }
            }

            //update the mean by dividing through number of assigned points
            for (int cluster = 0; cluster < options.NumberOfClusters; cluster++)
            {
                if (totals[cluster][data.ColumnCount] == 0)
                    return false;
                for (int col = 0; col < data.ColumnCount; col++)
                {
                    result.ClusterMeanValues[cluster][col] =
                        totals[cluster][col] / totals[cluster][data.ColumnCount];
                }
            }
            return true;
        }

        internal static ClusterResult InitializeClusters(ClusterData input, ClusterAlgorithmOptions options)
        {
            ClusterResult result = new ClusterResult();
            Random random = new Random(options.RandomSeed);
            result.ClusterAssignment = new int[input.RowCount];
            // ensure each cluster gets one data point
            for (int row = 0; row < options.NumberOfClusters; row++)
            {
                result.ClusterAssignment[row] = row;
            }
            //assign rest of data points randomly
            for (int row = options.NumberOfClusters; row < input.RowCount; row++)
            {
                result.ClusterAssignment[row] = random.Next(0, options.NumberOfClusters);
            }
            //initialize mean value matrix (without data)
            result.ClusterMeanValues = new double[options.NumberOfClusters][];
            int colCount = input.RawData[0].Length;
            for (int cluster = 0; cluster < options.NumberOfClusters; cluster++)
            {
                result.ClusterMeanValues[cluster] = new double[colCount];
            }
            return result;
        }

        internal static double[][] Normalize(ClusterData data)
        {
            double[][] rawData = data.RawData;

            // initialize result matrix
            double[][] normalizedValues = new double[data.RowCount][];
            for (int row = 0; row < data.RowCount; row++)
            {
                normalizedValues[row] = new double[rawData[row].Length];
            }

            // normalize values
            for (int col = 0; col < data.ColumnCount; col++)
            {
                double[] colData = GetColData(col, rawData);
                double mean = ArrayStatistics.Mean(colData);
                double stDev = ArrayStatistics.PopulationStandardDeviation(colData);
                for (int row = 0; row < data.RowCount; row++)
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
