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
using Microsoft.Win32;
using System.ServiceModel;
using Random_Experiment_WPF.RandomExperimentServer;

namespace Random_Experiment_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public SeriesCollection GlobalCollection { get; set; }
        public SeriesCollection LocalCollection { get; set; }
        
        public string[] LabelsLocal { get; set; }
        public string[] LabelsGlobal { get; set; }
        public Func<double, string> YFormatterA { get; set; }
        public Func<double, string> YFormatterB { get; set; }
        public Random myRandom = new Random(Math.Abs((int)DateTime.Now.Ticks));
        public Info myUser { get; set; }
        private Thread computeThread { get; set; }
        private bool StopThread = false;
        private RandomServerClient myService;
        private List<SQLData> myLocalData { get; set; }
        private List<SQLData> myGlobalData { get; set; }
       
        public struct Info 
        { 
            public string user { get; set; }
            public int time_zone { get; set; }
        }
        private bool first = true;

        public MainWindow()
        {
            InitializeComponent();
            myService = new RandomServerClient();
            myService.Endpoint.Address = new EndpointAddress(string.Format("http://{0}:3030", "server.pypem.com"));//server.pypem.com//127.0.0.1
            myService.InnerChannel.OperationTimeout = new TimeSpan(0, 2, 30);

            TimeZone localZone = TimeZone.CurrentTimeZone;
            TimeSpan currentOffset = localZone.GetUtcOffset(DateTime.Now);

            this.lbTimeZone.Content = $"UTC{(currentOffset.Hours > 0 ? "+" : "")}{currentOffset.Hours}";
           
            string macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();

            string idRaw = macAddr + "_5.some_stuff_8899211";
            string idSHA = SHA.GetSHAString(SHA.GetHash(idRaw));

            this.lbID.Text = idSHA;

            this.myUser = new Info() { user = idSHA, time_zone = currentOffset.Hours };

            int rndBase = this.myRandom.Next(65536) * 256;
            int rndOffset = this.myRandom.Next(Math.Abs((int)DateTime.UtcNow.Ticks % 65536)) + 256;
            for (int i = 0; i < rndOffset; i++)
                this.myRandom.Next(rndBase);

            this.computeThread = new Thread(this.Compute);
            computeThread.Start(null);

            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().Location);

            bool startup = Properties.Settings.Default.startup;
            if (!startup)
                if (MessageBox.Show("This program should run at startup, would yoou like to enable this?", "Run at start", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    startup = true;
            
            if(this.SetStartup(startup))
                this.cbStartup.IsChecked = startup;

            Properties.Settings.Default.startup = startup;
            Properties.Settings.Default.Save();

            this.myGlobalData = new List<SQLData>();
            this.myLocalData = new List<SQLData>();

            this.prBusy.IsActive = true;
            this.IsEnabled = false;

            this.myService.BeginGetUserData(this.myUser.user, TimeSpan.FromHours(24), this.GetUserDataCallBack, null);

            for (int i = -12; i <= 12; i++)
                this.cbTimeZoneGlobal.Items.Add($"UTC{(i >= 0 ? "+" : "")}{i}");

            this.cbTimeZoneGlobal.SelectedIndex = this.myUser.time_zone + 12;
        }

        private bool SetStartup(bool onOff)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (onOff)
                    rk.SetValue("random_experiment", System.Reflection.Assembly.GetExecutingAssembly().Location);
                else
                    rk.DeleteValue("random_experiment", false);

                return true;
            }
            catch 
            {
                MessageBox.Show("Couldn't set program to run at  startup. You can try starting the program with administrator rights.", "Error setting startup.");
                return false;
            }
        }
        bool test = true;
        private void Compute(object data)
        {
            Stopwatch time = new Stopwatch();
            time.Start();

            double s0 = 0, s1 = 0, s2 = 0;
            List<double> values = new List<double>();

            long last_tick = 0;
            while(true)
            {
                //Console.WriteLine(GetLastUserInput.GetIdleTickCount());

                //long tick = DateTime.UtcNow.Ticks;
                //Console.WriteLine($"{tick - last_tick}|{tick}");
                //last_tick = tick;
                for (int i = 0; i < DateTime.Now.Ticks % 65536; i++)
                    this.myRandom.NextDouble();

                float primary = (float)this.myRandom.NextDouble();
                values.Add(primary);
                
                Thread.Sleep(100);

                if(this.StopThread)
                {
                    this.StopThread = false;
                    return;
                }

                if (time.Elapsed.Minutes >= 5)
                {
                    List<double> submit = new List<double>(values);
                    Dispatcher.Invoke(() => this.SubmitData(submit));
                    values.Clear();
                    time.Restart();
                }

                //if (test) return;
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

            bool idle = GetLastUserInput.GetIdleTickCount() < 1000 * 60 * 5;

            SQLData submitData = new SQLData();
            submitData.Mean = mean;
            submitData.Median = median;
            submitData.StdDev = std_dev;
            submitData.Active = idle;
            submitData.Count = values.Count;
            submitData.Time = DateTime.UtcNow;
            submitData.TimeZone = this.myUser.time_zone;
            submitData.User = this.myUser.user;

            this.prBusy.IsActive = true;
            this.IsEnabled = false;

            this.lbLastSubmit.Content = DateTime.Now.ToString("HH:mm dd/MM/yyyy");

            try
            {
                this.myService.BeginGetToken(new AsyncCallback(GetTokenCallBack), submitData);
            }
            catch { this.InitializeServer(); }
        }
        private void GetTimeZoneDataCallBack(IAsyncResult result)
        {
            try
            {
                this.myGlobalData = this.myService.EndGetTimeZoneData(result);
                Dispatcher.Invoke(() => this.PopulateGlobalGraph());
            }
            catch {}

            Dispatcher.Invoke(() => this.prBusy.IsActive = false);
            Dispatcher.Invoke(() => this.IsEnabled = true);
        }
        private void GetUserDataCallBack(IAsyncResult result)
        {
            try
            {
                this.myLocalData = this.myService.EndGetUserData(result);
                Dispatcher.Invoke(() => this.PopulateLocalGraph());
            }
            catch (Exception e) {}

            Dispatcher.Invoke(() => this.prBusy.IsActive = false);
            Dispatcher.Invoke(() => this.IsEnabled = true);
        }
        private void GetTokenCallBack(IAsyncResult result)
        {
            try
            {
                string token = this.myService.EndGetToken(result).ToString();

                this.myService.BeginSubmitStatus(token, result.AsyncState as SQLData, this.GetSubmitCallBack, null);
            }
            catch
            {
                Dispatcher.Invoke(() => this.lbLastSubmit.Content = "[error]");
                Dispatcher.Invoke(() => this.prBusy.IsActive = false);
                Dispatcher.Invoke(() => this.IsEnabled = true);

                this.InitializeServer();
            }
        }
        private void GetSubmitCallBack(IAsyncResult result)
        {
            try
            {
                string incoming = this.myService.EndSubmitStatus(result).ToString();
                if (incoming.IndexOf("error") != -1)
                    Dispatcher.Invoke(() => this.lbLastSubmit.Content = "[error]");
            }
            catch
            {
                Dispatcher.Invoke(() => this.lbLastSubmit.Content = "[error]");
            }
            Dispatcher.Invoke(() => this.prBusy.IsActive = false);
            Dispatcher.Invoke(() => this.IsEnabled = true);
        }
        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                if(Properties.Settings.Default.first)
                {
                    this.tbiTaskBar.ShowBalloonTip("Still running.", "The program is still running in the background.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                    Properties.Settings.Default.first = false;
                    Properties.Settings.Default.Save();
                }
            }
            else
                this.ShowInTaskbar = true;
        }
        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
                this.cmTaskBarShow_Click(sender, e);
            else
                this.cmTaskBarHide_Click(sender, e);
        }
        private void cmTaskBarShow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            this.Activate();
        }
        private void cmTaskBarHide_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void cmQuit_Click(object sender, RoutedEventArgs e)
        {
            this.StopThread = true;
            while (!this.StopThread)
                Thread.Sleep(100);

            this.Close();
        }

        public void PopulateLocalGraph()
        {
            if (this.myLocalData == null || this.myLocalData.Count == 0) return;

            ChartValues<double> mean = new ChartValues<double>();
            ChartValues<double> median = new ChartValues<double>();
            ChartValues<double> stddev = new ChartValues<double>();
            List<string> labels = new List<string>();

            foreach(SQLData data in this.myLocalData)
            {
                mean.Add(data.Mean);
                median.Add(data.Median);
                stddev.Add(data.StdDev);
                labels.Add(data.Time.ToString("HH:mm dd/MM"));
            }

            DataContext = null;

            LocalCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Median",
                    Values = median,
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Mean",
                    Values = mean,
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "STD Dev",
                    Values = stddev,
                    PointGeometry = null
                }
            };

            LabelsLocal = labels.ToArray();
            YFormatterA = value => value.ToString("N4");

            DataContext = this;
        }

        public void PopulateGlobalGraph()
        {
            if (this.myGlobalData == null || this.myGlobalData.Count == 0) return;

            ChartValues<double> mean = new ChartValues<double>();
            ChartValues<double> median = new ChartValues<double>();
            ChartValues<double> stddev = new ChartValues<double>();
            List<string> labels = new List<string>();

            foreach (SQLData data in this.myGlobalData)
            {
                mean.Add(data.Mean);
                median.Add(data.Median);
                stddev.Add(data.StdDev);
                labels.Add(data.Time.ToString("HH:mm dd/MM"));
            }

            DataContext = null;

            GlobalCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Median",
                    Values = median,
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "Mean",
                    Values = mean,
                    PointGeometry = null
                },
                new LineSeries
                {
                    Title = "STD Dev",
                    Values = stddev,
                    PointGeometry = null
                }
            };

            LabelsGlobal = labels.ToArray();
            YFormatterB = value => value.ToString("N4");

            DataContext = this;
        }

        public void PopulateGraph()
        {

            /*SeriesCollection = new SeriesCollection
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

                        /*AxisYCollection = new AxesCollection
            {
                new Axis { Title = "Value", Foreground = Brushes.White, LabelFormatter = YFormatter },
                new Axis { Title = "Value", Foreground = Brushes.White, LabelFormatter = YFormatter }
            };*/


            //LabelsLocal = new[] { "Jan", "Feb", "Mar", "Apr", "May" };
            //YFormatter = value => value.ToString("C");

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

            //DataContext = this;
        }
        private void cbStartup_Checked(object sender, RoutedEventArgs e)
        {
            if (this.SetStartup(true))
            {
                Properties.Settings.Default.startup = true;
                Properties.Settings.Default.Save();
            }
        }
        private void cbStartup_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.SetStartup(false))
            {
                Properties.Settings.Default.startup = false;
                Properties.Settings.Default.Save();
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.StopThread = true;
            while (!this.StopThread)
                Thread.Sleep(100);
        }

        private void cbThisTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.first) return;

            TimeSpan range = TimeSpan.FromDays(1);
            if (this.cbThisTime.SelectedIndex == 0)
                range = TimeSpan.FromHours(24);
            else if (this.cbThisTime.SelectedIndex == 1)
                range = TimeSpan.FromDays(7);
            else if (this.cbThisTime.SelectedIndex == 2)
                range = TimeSpan.FromDays(14);

            try
            {
                this.prBusy.IsActive = true;
                this.IsEnabled = false;

                this.myService.BeginGetUserData(this.myUser.user, range, this.GetUserDataCallBack, null);
            }
            catch { this.InitializeServer(); }
        }

        private void cbTimeZoneGlobal_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.first) return;

            TimeSpan range = TimeSpan.FromDays(1);
            if (this.cbGlobalTimeZone.SelectedIndex == 0)
                range = TimeSpan.FromHours(24);
            else if (this.cbGlobalTimeZone.SelectedIndex == 1)
                range = TimeSpan.FromDays(7);
            else if (this.cbGlobalTimeZone.SelectedIndex == 2)
                range = TimeSpan.FromDays(14);

            try
            {
                this.prBusy.IsActive = true;
                this.IsEnabled = false;

                this.myService.BeginGetTimeZoneData(this.cbTimeZoneGlobal.SelectedIndex - 12, range, this.GetTimeZoneDataCallBack, null);
            }
            catch { this.InitializeServer(); }
        }

        private int oldTab = 0;
        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.first) return;

            if (oldTab == this.tabMain.SelectedIndex) return;

            if (this.tabMain.SelectedIndex == 0)
                this.cbThisTime_SelectionChanged(null, null);
            else if (this.tabMain.SelectedIndex == 1)
                this.cbGlobalTimeZone_SelectionChanged(null, null);

            this.oldTab = this.tabMain.SelectedIndex;

        }

        private void cbGlobalTimeZone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.first) return;

            TimeSpan range = TimeSpan.FromDays(1);
            if (this.cbGlobalTimeZone.SelectedIndex == 0)
                range = TimeSpan.FromHours(24);
            else if (this.cbGlobalTimeZone.SelectedIndex == 1)
                range = TimeSpan.FromDays(7);
            else if (this.cbGlobalTimeZone.SelectedIndex == 2)
                range = TimeSpan.FromDays(14);

            this.prBusy.IsActive = true;
            this.IsEnabled = false;

            try
            {
                this.myService.BeginGetTimeZoneData(this.cbTimeZoneGlobal.SelectedIndex - 12, range, this.GetTimeZoneDataCallBack, null);
            }
            catch { this.InitializeServer(); }
        }

        private void InitializeServer()
        {
            myService = new RandomServerClient();
            myService.Endpoint.Address = new EndpointAddress(string.Format("http://{0}:3030", "http://server.pypem.com"));//server.pypem.com//127.0.0.1
            myService.InnerChannel.OperationTimeout = new TimeSpan(0, 2, 30);

            Dispatcher.Invoke(() => this.prBusy.IsActive = false);
            Dispatcher.Invoke(() => this.IsEnabled = true);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.first = false;
        }

        private void btRefreshLocal_Click(object sender, RoutedEventArgs e)
        {
            this.cbThisTime_SelectionChanged(null, null);
        }
    }
}
