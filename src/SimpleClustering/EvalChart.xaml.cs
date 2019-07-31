using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;

namespace SimpleClustering
{
    /// <summary>
    /// Interaction logic for EvalChart.xaml
    /// </summary>
    public partial class EvalChart : UserControl
    {
        public EvalChart()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection();
            YFormatter = (x) => x.ToString();
            DataContext = this;
        }

        public void Update(ClusterAnalysis.ClusterEvaluation[] evaluations)
        {

            var lineInner = new LineSeries
            {
                Title = "Within-Scatter (W_log)",
                LineSmoothness = 0,
                Values = new ChartValues<double>(evaluations.Select(e => (e.InnerClusterDistance != double.NaN) ? Math.Log(e.InnerClusterDistance) : -1))
            };
            Labels = evaluations.Select(e => e.NumberOfClusters.ToString()).ToArray();
            SeriesCollection.Clear();
            SeriesCollection.Add(lineInner);
            DataContext = null;
            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }

        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

    }
}
