using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;


namespace ClusterAnalysis
{
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

        private static bool UpdateClusterAssignment(ClusterResult result)
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
                int foundCluster = FindMinimum(distances);
                if (foundCluster != result.ClusterAssignment[row])
                {
                    updated = true;
                    result.ClusterAssignment[row] = foundCluster;
                }
            }
            return updated;
        }

        private static double GetDistance(double[] means, double[] data)
        {
            double sqaredDev = 0;
            for (int i = 0; i < means.Length; i++)
            {
                sqaredDev += (means[i] - data[i]) * (means[i] - data[i]);
            }
            return Math.Sqrt(sqaredDev);
        }

        private static int FindMinimum(double[] distances)
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

        private static bool UpdateClusterMeanValues(ClusterResult result)
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

        private static void InitializeClusters(ClusterData input, ClusterResult result)
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

        private static double[][] Normalize(double[][] rawData)
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

        private static double[] GetColData(int col, double[][] rawData)
        {
            double[] column = new double[rawData.Length];
            for (int i = 0; i < column.Length; i++)
            {
                column[i] = rawData[i][col];
            }
            return column;
        }
    }

    public class RandomSeedGenerator
    {

        public static string Generate()
        {
            return Generate(DEFAULT_MIN_PASSWORD_LENGTH,
                            DEFAULT_MAX_PASSWORD_LENGTH);
        }

        public static string Generate(int length)
        {
            return Generate(length, length);
        }

        public static string Generate(int minLength,
                                  int maxLength)
        {
            if (minLength <= 0 || maxLength <= 0 || minLength > maxLength)
                return null;
            char[][] charGroups = new char[][]
            {
            PASSWORD_CHARS_LCASE.ToCharArray(),
            PASSWORD_CHARS_UCASE.ToCharArray(),
            PASSWORD_CHARS_NUMERIC.ToCharArray(),
            PASSWORD_CHARS_SPECIAL.ToCharArray()
            };
            int[] charsLeftInGroup = new int[charGroups.Length];
            for (int i = 0; i < charsLeftInGroup.Length; i++)
                charsLeftInGroup[i] = charGroups[i].Length;
            int[] leftGroupsOrder = new int[charGroups.Length];
            for (int i = 0; i < leftGroupsOrder.Length; i++)
                leftGroupsOrder[i] = i;
            byte[] randomBytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);
            int seed = BitConverter.ToInt32(randomBytes, 0);
            Random random = new Random(seed);
            char[] password = null;
            if (minLength < maxLength)
                password = new char[random.Next(minLength, maxLength + 1)];
            else
                password = new char[minLength];
            int nextCharIdx;
            int nextGroupIdx;
            int nextLeftGroupsOrderIdx;
            int lastCharIdx;
            int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
            for (int i = 0; i < password.Length; i++)
            {
                if (lastLeftGroupsOrderIdx == 0)
                    nextLeftGroupsOrderIdx = 0;
                else
                    nextLeftGroupsOrderIdx = random.Next(0,
                                                         lastLeftGroupsOrderIdx);
                nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];
                lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;
                if (lastCharIdx == 0)
                    nextCharIdx = 0;
                else
                    nextCharIdx = random.Next(0, lastCharIdx + 1);
                password[i] = charGroups[nextGroupIdx][nextCharIdx];
                if (lastCharIdx == 0)
                    charsLeftInGroup[nextGroupIdx] =
                                              charGroups[nextGroupIdx].Length;
                else
                {
                    if (lastCharIdx != nextCharIdx)
                    {
                        char temp = charGroups[nextGroupIdx][lastCharIdx];
                        charGroups[nextGroupIdx][lastCharIdx] =
                                    charGroups[nextGroupIdx][nextCharIdx];
                        charGroups[nextGroupIdx][nextCharIdx] = temp;
                    }
                    charsLeftInGroup[nextGroupIdx]--;
                }

                if (lastLeftGroupsOrderIdx == 0)
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                else
                {
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                    {
                        int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                        leftGroupsOrder[lastLeftGroupsOrderIdx] =
                                    leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }
                    lastLeftGroupsOrderIdx--;
                }
            }
            return new string(password);
        }

        private static int DEFAULT_MIN_PASSWORD_LENGTH = 8;
        private static int DEFAULT_MAX_PASSWORD_LENGTH = 18;
        private static string PASSWORD_CHARS_LCASE = "abcdefgijkmnopqrstwxyz";
        private static string PASSWORD_CHARS_UCASE = "ABCDEFGHJKLMNPQRSTWXYZ";
        private static string PASSWORD_CHARS_NUMERIC = "23456789";
        private static string PASSWORD_CHARS_SPECIAL = "@";
    }


}
