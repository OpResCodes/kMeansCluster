namespace ClusterAnalysis
{
    public struct ClusterAlgorithmOptions
    {

        public ClusterAlgorithmOptions(int numberOfClusters) : this(numberOfClusters,3238666,true) { }

        public ClusterAlgorithmOptions(int numberOfClusters, bool normalizeData) : this(numberOfClusters,3238666,normalizeData) { }

        public ClusterAlgorithmOptions(int numberOfClusters, int randomSeed, bool normalizeData = true, int maxIterations = 100000)
        {
            NumberOfClusters = numberOfClusters;
            RandomSeed = randomSeed;
            MaximumIterationCount = maxIterations;
            NormalizeData = normalizeData;
        }

        public bool NormalizeData { get; }

        public int NumberOfClusters { get; }

        public int MaximumIterationCount { get; }

        public int RandomSeed { get; }
    }
}
