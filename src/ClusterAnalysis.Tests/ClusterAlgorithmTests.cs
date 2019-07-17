using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace ClusterAnalysis.Tests
{
    [TestClass]
    public class ClusterAlgorithmTests
    {

        [TestMethod]
        public void TestClusterAlgorithm()
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
    }
}
