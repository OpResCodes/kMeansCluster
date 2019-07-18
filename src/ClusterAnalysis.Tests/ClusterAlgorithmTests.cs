using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace ClusterAnalysis.Tests
{
    [TestClass]
    public class ClusterAlgorithmTests
    {

        [TestMethod]
        public void TestClusterAlgorithmOnExample()
        {
            var result = ClusterBuilder.Analyze(new ClusterData()
            {
                MaximumIterationCount = 10000,
                NumberOfClusters = 5,
                RandomSeed = 1173737853,
                RawData = GetTestData()
            });
            Assert.IsTrue(result.ClusterAssignment.Length > 0);
            Trace.WriteLine($"Iterations: {result.Iterations}");
            Trace.WriteLine(result.PrintCsvResult());
        }

        [TestMethod]
        public void TestBigSet()
        {
            ClusterData d = new ClusterData()
            {
                MaximumIterationCount = 10000000,
                NumberOfClusters = 3,
                RawData = GetRandomDataSet(5000, 5, 2000)
            };
            ClusterResult result = ClusterBuilder.Analyze(d);
            Assert.IsTrue(result.Iterations > 0);
            Trace.WriteLine($"Iterations: {result.Iterations}");
            Trace.WriteLine(result.PrintCsvResult());
        }

        [TestMethod]
        public void MultipleClusterConstruction()
        {
            var cd = new ClusterData()
            {
                RawData = GetTestData(),
                MaximumIterationCount = 10000,
                NumberOfClusters = 1,
                RandomSeed = 1
            };
            string meta = MetaClusterTester.Cluster(cd);
            Trace.WriteLine(meta);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Distance_Calculation_Euclidean()
        {
            double[] means = new double[] { 3, 4, 5 };
            double[] data = new double[] { 10, 10, 10 };
            double result = 0;
            for (int i = 0; i < means.Length; i++)
            {
                result = result + Math.Pow(means[i] - data[i], 2);
            }
            result = Math.Sqrt(result);
            Assert.AreEqual(result, ClusterBuilder.GetDistance(means, data));
        }

        [TestMethod]
        public void Can_Find_Minimum()
        {
            double[] data = new double[] { -5, 10, 3, 100, -6.01, -6.02, 7, -3 };
            int expectedIndex = 5;
            int foundIndex = ClusterBuilder.FindMinimumIndex(data);
            Assert.AreEqual(expectedIndex, foundIndex);
        }

        [TestMethod]
        public void Calculate_Means()
        {
            double[][] data = new double[5][];
            //1. cluster
            data[0] = new double[] { 2, 7 };
            data[1] = new double[] { 3, 3 };
            //2. cluster
            data[2] = new double[] { 1, 100 };
            data[3] = new double[] { 2, 200 };
            data[4] = new double[] { 3, 300 };

            ClusterResult r = new ClusterResult();
            r.ClusterAssignment = new int[] { 0, 0, 1, 1, 1 };
            r.NormalizedData = data;
            r.ClusterMeanValues = new double[2][];
            r.ClusterMeanValues[0] = new double[2] { 0, 0 };
            r.ClusterMeanValues[1] = new double[2] { 0, 0 };
            ClusterBuilder.UpdateClusterMeanValues(r);
            double exp_First = 2.5;
            double exp_Second = 5.0;
            Assert.AreEqual(exp_First, r.ClusterMeanValues[0][0]);
            Assert.AreEqual(exp_Second, r.ClusterMeanValues[0][1]);
            exp_First = 2d;
            exp_Second = 200d;
            Assert.AreEqual(exp_First, r.ClusterMeanValues[1][0]);
            Assert.AreEqual(exp_Second, r.ClusterMeanValues[1][1]);
        }

        [TestMethod]
        public void Initialize_Clusters()
        {
            ClusterData d = new ClusterData()
            {
                NumberOfClusters = 3,
                MaximumIterationCount = 10,
                RawData = new double[10][]
            };
            for (int i = 0; i < d.RawData.Length; i++)
            {
                d.RawData[i] = new double[5];
            }
            //should provide initialized mean values and assignment
            ClusterResult r = new ClusterResult();
            ClusterBuilder.InitializeClusters(d, r);

            int[] count = new int[3];
            for (int i = 0; i < d.RawData.Length; i++)
            {
                int idx = r.ClusterAssignment[i];
                count[idx] += 1;
            }
            for (int i = 0; i < d.NumberOfClusters; i++)
            {
                //check if every cluster received at least one data point
                Assert.IsTrue(count[i] > 0);
                //check if the column mean values are initialized
                Assert.IsTrue(r.ClusterMeanValues[i].Length == d.RawData[0].Length);
            }
        }

        [TestMethod]
        public void Assign_Clusters()
        {
            ClusterResult cr = new ClusterResult();
            cr.ClusterAssignment = new int[3] { 0, 1, 0 };
            cr.ClusterMeanValues = new double[2][];
            cr.ClusterMeanValues[0] = new double[2] { 100, 1000 };
            cr.ClusterMeanValues[1] = new double[2] { 5, 10 };
            cr.NormalizedData = new double[3][];
            cr.NormalizedData[0] = new double[2] { 80, 900 };
            cr.NormalizedData[1] = new double[2] { 200, 2000 };
            cr.NormalizedData[2] = new double[2] { 7, 20 };
            ClusterBuilder.UpdateClusterAssignment(cr);
            Assert.AreEqual(cr.ClusterAssignment[0], 0);
            Assert.AreEqual(cr.ClusterAssignment[1], 0);
            Assert.AreEqual(cr.ClusterAssignment[2], 1);
        }

        public double[][] GetTestData()
        {
            double[][] testData = new double[20][];

            testData[0] = new double[] { 65.0, 220.0 };
            testData[1] = new double[] { 73.0, 160.0 };
            testData[2] = new double[] { 59.0, 110.0 };
            testData[3] = new double[] { 61.0, 120.0 };
            testData[4] = new double[] { 75.0, 150.0 };
            testData[5] = new double[] { 67.0, 240.0 };
            testData[6] = new double[] { 68.0, 230.0 };
            testData[7] = new double[] { 70.0, 220.0 };
            testData[8] = new double[] { 62.0, 130.0 };
            testData[9] = new double[] { 66.0, 210.0 };
            testData[10] = new double[] { 77.0, 190.0 };
            testData[11] = new double[] { 75.0, 180.0 };
            testData[12] = new double[] { 74.0, 170.0 };
            testData[13] = new double[] { 70.0, 210.0 };
            testData[14] = new double[] { 61.0, 110.0 };
            testData[15] = new double[] { 58.0, 100.0 };
            testData[16] = new double[] { 66.0, 230.0 };
            testData[17] = new double[] { 59.0, 120.0 };
            testData[18] = new double[] { 68.0, 210.0 };
            testData[19] = new double[] { 61.0, 130.0 };

            return testData;
        }

        private double[][] GetRandomDataSet(int rowCount, int colCount, double maxValue)
        {
            Random r = new Random();
            double[][] testData = new double[rowCount][];
            for (int row = 0; row < rowCount; row++)
            {
                testData[row] = new double[colCount];
                for (int col = 0; col < colCount; col++)
                {
                    testData[row][col] = r.NextDouble() * maxValue;
                }
            }
            return testData;
        }
    }
}
