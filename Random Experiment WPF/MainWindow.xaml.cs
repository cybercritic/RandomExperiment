using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
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
using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.Threading;

namespace Random_Experiment_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public Random myRandom = new Random(Math.Abs((int)DateTime.Now.Ticks));

        public MainWindow()
        {
            InitializeComponent();

            this.PopulateGraph();

            TimeZone localZone = TimeZone.CurrentTimeZone;
            TimeSpan currentOffset = localZone.GetUtcOffset(DateTime.Now);

            this.lbTimeZone.Content = $"UTC{(currentOffset.Hours > 0 ? "+" : "")}{currentOffset.Hours}";

            string macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();

            string idRaw = macAddr + "_5.some_salt_8899211";
            string idSHA = SHA.GetSHAString(SHA.GetHash(idRaw));

            this.lbID.Content = idSHA;

            int rndBase = this.myRandom.Next(65536) * 256;
            int rndOffset = this.myRandom.Next(Math.Abs((int)DateTime.UtcNow.Ticks % 65536)) + 256;
            for (int i = 0; i < rndOffset; i++)
                this.myRandom.Next(rndBase);

            Thread computeThread = new Thread(this.Compute);
            computeThread.Start(null);
        }

        private void Compute(object data)
        {
            Stopwatch time = new Stopwatch();
            time.Start();

            double s0 = 0, s1 = 0, s2 = 0;
            List<double> values = new List<double>();

            while(true)
            {
                float primary = (float)this.myRandom.NextDouble();
                values.Add(primary);
                
                Thread.Sleep(100);
                if (time.Elapsed.Minutes >= 5)
                {
                    List<double> submit = new List<double>(values);
                    Dispatcher.Invoke(() => this.SubmitData(submit));
                    values.Clear();
                    time.Restart();
                }
            }
        }

        private void SubmitData(List<double> values)
        {
            values.Sort();
            double median = 0f;
            if (values.Count % 2 == 1)
                median = values[values.Count / 2];
            else
                median = (values[values.Count / 2] + values[values.Count / 2 - 1]) / 2.0;
            double mean = values.Sum() / values.Count;

            var squares_query =
                from double value in values
                select (value - mean) * (value - mean);
            double sum_of_squares = squares_query.Sum();

            double std_dev = Math.Sqrt(sum_of_squares / values.Count());


        }

        public void PopulateGraph()
        {
         
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = new ChartValues<double> { 4, 6, 5, 2 ,4 }
                },
                new LineSeries
                {
                    Title = "Series 2",
                    Values = new ChartValues<double> { 6, 7, 3, 4 ,6 },
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Series 3",
                    Values = new ChartValues<double> { 4,2,7,2,7 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 15
                }
            };

            Labels = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            YFormatter = value => value.ToString("C");

            //modifying the series collection will animate and update the chart
            /*SeriesCollection.Add(new LineSeries
            {
                Title = "Series 4",
                Values = new ChartValues<double> { 5, 3, 2, 4 },
                LineSmoothness = 0, //0: straight lines, 1: really smooth lines
                PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
                PointGeometrySize = 50,
                PointForeground = Brushes.Gray
            });*/

            //modifying any series values will also animate and update the chart
            //SeriesCollection[3].Values.Add(5d);

            DataContext = this;
        }
    }
}
