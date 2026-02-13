using System.Windows;
using System.Windows.Media;


namespace WpfApp
{
    // Code based on "Test Run - Ant Colony Optimization"
    // https://learn.microsoft.com/en-us/archive/msdn-magazine/2012/february/test-run-ant-colony-optimization
    // quote from atricle:
    // "...With 60 cities, assuming you can start at any city and go either forward or backward, and that all cities are connected,
    // there are a total of (60 - 1)! / 2 = 69,341,559,272,844,917,868,969,509,860,194,703,172,951,438,386,343,716,270,410,647,470,080,000,000,000,000 possible solutions.
    // Even if you could evaluate 1 billion possible solutions per second, it would take about 2.2 * 1063 years to check them all, which is many times longer than the estimated age of the universe..."
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        Random rnd = new Random();
        public static DrawingVisual visual;
        DrawingContext dc;
        public static int width, height;
        AntColony antColony;

        int numCities, numAnts, maxTime;

        public MainWindow()
        {
            InitializeComponent();

            width = (int)g.Width;
            height = (int)g.Height;

            visual = new DrawingVisual();

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
        }

        void Init()
        {
            rtbConsole.Document.Blocks.Clear();

            rtbConsole.AppendText("\nBegin Ant Colony Optimization demo\n");

            numCities = 100;
            numAnts = 10;
            maxTime = 1000;

            rtbConsole.AppendText("\n\nNumber cities in problem = " + numCities);

            rtbConsole.AppendText("\nNumber ants = " + numAnts);
            rtbConsole.AppendText("\nMaximum time = " + maxTime);

            antColony = new AntColony(rnd, numAnts, numCities);
            antColony.BestLengthNotify += (s) => 
            { 
                rtbConsole.AppendText(s); 
            };

            rtbConsole.AppendText("\n\nAlpha (pheromone influence) = " + antColony.alpha);
            rtbConsole.AppendText("\nBeta (local node influence) = " + antColony.beta);
            rtbConsole.AppendText("\nRho (pheromone evaporation coefficient) = " + antColony.rho.ToString("F2"));
            rtbConsole.AppendText("\nQ (pheromone deposit factor) = " + antColony.Q.ToString("F2") + "\n");

            rtbConsole.AppendText(antColony.ShowAnts());

            // the length of the best trail
            double bestLength = antColony.BestLength;
            rtbConsole.AppendText("\nBest initial trail length: " + bestLength.ToString("F1") + "\n");
        }

        private void Control()
        {
            lbT.Content = "Time: " + antColony.time + " / " + maxTime;

            antColony.Calculate(maxTime);

            // if DONE
            if (antColony.isCalculationDone)
            {
                timer.Stop();

                rtbConsole.AppendText("\n\nTime complete");

                rtbConsole.AppendText("\n\nBest trail found:\n");
                double bestLength = antColony.BestLength;
                rtbConsole.AppendText(antColony.DisplayBestTrail());

                rtbConsole.AppendText("\nLength of best trail found: " + bestLength.ToString("F1"));

                rtbConsole.AppendText("\n\nEnd Ant Colony Optimization demo\n");
            }
        }

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                antColony.Drawing(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void timerTick(object? sender, EventArgs e)
        {
            Control();
            Drawing();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Init();
            timer.Start();
        }
    }
}