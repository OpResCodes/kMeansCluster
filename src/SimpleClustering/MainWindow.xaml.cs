using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClusterAnalysis;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace SimpleClustering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private ClusterData clusterData;

        public MainWindow()
        {
            InitializeComponent();
            this.ChartyMcChart.Visibility = Visibility.Hidden;

            TestData();
        }

        private void TestData()
        {
            Random r = new Random();
            int rows = 200;
            int cols = 10;

            double[][] raw = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                raw[i] = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    raw[i][j] = Math.Round(r.NextDouble() * 500, 0);
                }
            }
            string[] attributes = new string[cols];
            for (int i = 0; i < cols; i++)
            {
                attributes[i] = "Attribute " + i.ToString();
            }
            clusterData = new ClusterData(raw, attributes);
            RefreshInputGrid();
        }

        private void RefreshInputGrid()
        {
            InputDataGrid.Items.Clear();
            InputDataGrid.Columns.Clear();
            InputDataGrid.Columns.Add(new DataGridTextColumn() { Header = "Observation", Binding = new Binding("Observation") });
            for (int i = 0; i < clusterData.ColumnCount; i++)
            {
                InputDataGrid.Columns.Add(new DataGridTextColumn()
                {
                    Header = clusterData.AttributeNames[i],
                    Binding = new Binding(clusterData.AttributeNames[i].Replace(' ', '_'))
                });
            }

            //weights
            dynamic row = new ExpandoObject();
            ((IDictionary<string, Object>)row)["Observation"] = "Weights ";
            for (int c = 0; c < clusterData.ColumnCount; c++)
            {
                ((IDictionary<string, Object>)row)[clusterData.AttributeNames[c].Replace(' ', '_')] = clusterData.AttributeWeights[c];
            }
            InputDataGrid.Items.Add(row);
            for (int r = 0; r < clusterData.RowCount; r++)
            {
                row = new ExpandoObject();
                ((IDictionary<string, Object>)row)["Observation"] = "Observation " + (r + 1).ToString();
                for (int c = 0; c < clusterData.ColumnCount; c++)
                {
                    ((IDictionary<string, Object>)row)[clusterData.AttributeNames[c].Replace(' ', '_')] = clusterData.RawData[r][c];
                }
                InputDataGrid.Items.Add(row);
            }


        }

        private void LoadCsvButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Csv files (*.csv)|*.csv|Text files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileName = openFileDialog.FileName;
                    clusterData = RawDataReader.ReadFromCsv(fileName, ';');
                    RefreshInputGrid();
                }
                catch
                {
                    MessageBox.Show("Failed to load csv - wrong Format?", "Loading failed.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ClusterButton_Click(object sender, RoutedEventArgs e)
        {
            ClusterButton.IsEnabled = false;
            LoadCsvButton.IsEnabled = false;
            var analysis = await Task.Run(() => ClusterSolver.Cluster(clusterData, 2, 20, 50,true));


            OutputDataGrid.Items.Clear();
            OutputDataGrid.Columns.Clear();

            OutputDataGrid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Clusters",
                Binding = new Binding("NumberOfClusters")
            });
            OutputDataGrid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Successfull Runs",
                Binding = new Binding("NumberOfRuns")
            });
            OutputDataGrid.Columns.Add(new DataGridTextColumn()
            {
                Header = "Iterations",
                Binding = new Binding("Iterations")
            });
            OutputDataGrid.Columns.Add(new DataGridTextColumn()
            {
                Header = "InnerCluster",
                Binding = new Binding("InnerClusterDistance")
            });
            OutputDataGrid.Columns.Add(new DataGridTextColumn()
            {
                Header = "InterCluster",
                Binding = new Binding("InterClusterDistance")
            });

            foreach (var a in analysis)
            {
                OutputDataGrid.Items.Add(a);
            }
            this.ChartyMcChart.Visibility = Visibility.Visible;
            ChartyMcChart.Update(analysis);
            ClusterButton.IsEnabled = true;
            LoadCsvButton.IsEnabled = true;
        }
    }
}
