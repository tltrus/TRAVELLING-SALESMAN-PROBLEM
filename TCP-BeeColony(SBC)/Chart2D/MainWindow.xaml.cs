using System.Windows;
using System.Windows.Media;


namespace _Chart2D
{
    // Natural Algorithms - Use Bee Colony Algorithms to Solve Impossible Problems
    // https://learn.microsoft.com/en-us/archive/msdn-magazine/2011/april/msdn-magazine-natural-algorithms-use-bee-colony-algorithms-to-solve-impossible-problems

    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        Random rnd = new Random();

        public static DrawingVisual visual = new DrawingVisual();
        public static int width, height;

        DrawingContext dc;
        Hive hive;
        CitiesData citiesData;

        public MainWindow()
        {
            InitializeComponent();

            width = (int)g.Width;
            height = (int)g.Height;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerMainTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 50);

            InitCities();
            Drawing();
        }

        void InitCities() => citiesData = new CitiesData(30, rnd);
        void InitHive()
        {
            int totalNumberBees = 250; // 150;
            int numberInactive = 40; // 10;
            int numberActive = 120; // 50;
            int numberScout = 90; // 30;

            int maxNumberVisits = 30; // 100;
            int maxNumberCycles = 2000; // 6000;

            rtbConsole.Clear();

            rtbConsole.AppendText("Begin Simulated Bee Colony algorithm demo");
            rtbConsole.AppendText("\nNumber of cities = " + citiesData.cities.Length);

            hive = new Hive(totalNumberBees, numberInactive, numberActive, numberScout, maxNumberVisits, maxNumberCycles, citiesData);
            hive.MessageNotify += (msg) => { rtbConsole.AppendText(msg); };
            hive.TimerNotify += () => {
                timer.Stop();
                btnStart.Content = "Start timer";
                rtbConsole.AppendText("\nEnd Simulated Bee Colony demo");
            };

            rtbConsole.AppendText("\n\nInitial random hive\n");
            rtbConsole.AppendText(hive.ToString());
        }

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                citiesData.Drawing(dc);
                if (hive is not null) hive.Drawing(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                InitHive();
                timer.Start();
                btnStart.Content = "Stop timer";
            }
            else
            {
                timer.Stop();
                btnStart.Content = "Start timer";
                InitCities();
            }
        }

        private void timerMainTick(object sender, EventArgs e)
        {
            hive.Solve();
            lbTime.Content = hive.GetCycleStr();
            Drawing();
        }
    }
}