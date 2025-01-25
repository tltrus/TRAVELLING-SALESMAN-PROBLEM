using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Media;


namespace WpfApp
{
    // The idea is taken from here https://habr.com/ru/post/308960/

    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer timer;
        Random rnd = new Random();
        DrawingVisual visual;
        DrawingContext dc;
        int width, height;


        Vector2[] cities;
        double[,] M;            // Матрица смежности (расстояний)
        int n = 9;              // Количество вершин

        int m = 5000;         // Количество итераций
        double Tstart = 10000;  // Начальная температура
        double Tend = 0.1;      // Конечная температура
        double T = 0;           // Температура для вычислений

        double p = 0;
        double S = 0;
        int[] PATH; // маршрут
        int pathLength;

        int i = 0;

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
            T = Tstart;
            cities = new Vector2[n];
            S = 0;
            p = 0;
            i = 0;

            // СОЗДАНИЕ ГОРОДОВ
            for (int i = 0; i < cities.Length; i++)
            {
                int x = rnd.Next(10, width - 10);
                int y = rnd.Next(10, height - 10);
                cities[i] = new Vector2(x, y);
            }

            // СОЗДАЕМ И ТАСУЕМ МАРШРУТ
            PATH = SattoloShuffle(n);
            pathLength = PATH.Length;

            // СОЗДАЕМ МАТРИЦУ РАССТОЯНИЙ
            rtbConsole.AppendText("Adjacency matrix:\r\r");

            M = new double[n, n];
            double s = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    s = Math.Sqrt((cities[j].X - cities[i].X) * (cities[j].X - cities[i].X) + (cities[j].Y - cities[i].Y) * (cities[j].Y - cities[i].Y));
                    s = Math.Round(s);
                    M[i, j] = s;
                    M[j, i] = s;

                    S += s;
                }
            }

            // Draw Adjacency matrix
            for (int i = 0; i < n; i++)
            {
                var value = "";

                for (int j = 0; j < n; j++)
                {
                    value += M[i, j] + "\t";
                }

                rtbConsole.AppendText(value + "\r");
            }
        }

        private void Drawing()
        {
            g.RemoveVisual(visual);
            using (dc = visual.RenderOpen())
            {
                DrawCities(dc);

                dc.Close();
                g.AddVisual(visual);
            }
        }

        private void DrawCities(DrawingContext dc)
        {
            Point[] points = new Point[cities.Length];

            for (int i = 0; i < cities.Length; i++)
            {
                int x = (int)cities[i].X;
                int y = (int)cities[i].Y;

                // Buildings
                dc.DrawEllipse(Brushes.White, null, new Point(x, y), 4, 4);

                // Labeling
                FormattedText formattedText = new FormattedText(i.ToString(), CultureInfo.GetCultureInfo("en-us"),
                                                                FlowDirection.LeftToRight, new Typeface("Verdana"), 14, Brushes.White,
                                                                VisualTreeHelper.GetDpi(visual).PixelsPerDip);
                Point textPos = new Point(x, y - 20);
                dc.DrawText(formattedText, textPos);

                points[i] = new Point(cities[PATH[i]].X, cities[PATH[i]].Y);
            }

            // Connections
            DrawConnections(dc, points);
        }
        private void DrawConnections(DrawingContext dc, Point[] points)
        {
            Point p0 = new Point();
            Point p1 = new Point();

            // City connections
            for (int i = 1; i < points.Length; i++)
            {
                p0.X = points[i - 1].X;
                p0.Y = points[i-1].Y;

                p1.X = points[i].X;
                p1.Y = points[i].Y;
                dc.DrawLine(new Pen(Brushes.White, 0.5), p0, p1);
            }

            // connect start and end points
            p0.X = points[0].X;
            p0.Y = points[0].Y;

            p1.X = points[points.Length - 1].X;
            p1.Y = points[points.Length - 1].Y;
            dc.DrawLine(new Pen(Brushes.White, 0.5), p0, p1);
        }

        private void timerTick(object? sender, EventArgs e)
        {
            Annealing();
            Drawing();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Init();
            Drawing();
            timer.Start();
        }

        // Алгоритм создания массива и тасования Саттоло
        private int[] SattoloShuffle(int n)
        {
            int[] a = Enumerable.Range(0, n).ToArray();
            int i = n;
            while (i > 1)
            {
                i--;
                int r = rnd.Next(i, a.Length);

                int tmp = a[i];
                a[i] = a[r];
                a[r] = tmp;
            }
            return a;
        }

        // Функция отжига
        private void Annealing()
        {
            // количество итераций
            if (i < m)
            {
                double Sp = 0;

                // Потенциальный маршрут
                int[] ROUTEp = new int[pathLength];
                Array.Copy(PATH, ROUTEp, pathLength); // ROUTEp <-- PATH;

                // Два случайных индекса города
                int transp1 = rnd.Next(0, pathLength / 2);
                int transp2 = rnd.Next(pathLength / 2, pathLength);

                // переворот вектора
                /*
                 * взяли два сгенерированных числа transp и перевернули маршрут между ними.
                 * Например, у нас был маршрут (1,2,3,4,5,6,7). Генератор случайных чисел выбрал города 2 и 7, 
                 * мы выполнили процедуру и получили (1,7,6,5,4,3,2)
                */

                for (int h = transp1; h <= transp2; h++)
                {
                    int last_indx = transp2 - h;
                    if (last_indx <= 1) break;

                    int first = ROUTEp[h];
                    int last = ROUTEp[last_indx];

                    ROUTEp[h] = last;
                    ROUTEp[last_indx] = first;

                }

                // Вычисляем энергию (расстояние) потенциального маршрута
                for (int j = 0; j < n - 1; j++)
                {
                    Sp += M[ROUTEp[j], ROUTEp[j + 1]];
                }
                Sp += M[ROUTEp[0], ROUTEp.Last()];

                // если она меньше то, потенциальный маршрут становится основным ...
                // если нет, смотрим, осуществляется ли переход
                if (Sp < S)
                {
                    S = Sp;
                    Array.Copy(ROUTEp, PATH, pathLength); // PATH <-- ROUTEp;
                }
                else
                {
                    // вычисляем вероятность перехода
                    p = Math.Exp((-(Sp - S)) / T);

                    if (rnd.NextDouble() <= p)
                    {
                        S = Sp;
                        Array.Copy(ROUTEp, PATH, pathLength);  // PATH <-- ROUTEp;
                    }
                }

                // уменьшаем температуру
                T = Tstart / i;

                // проверяем условие выхода
                if (T < Tend)
                {
                    i = m;
                    timer.Stop();
                }

                i++;
            }

            lbS.Content = S.ToString();
            lbM.Content = i.ToString();
            lbT.Content = T.ToString("F3");

            string text = "[";
            Array.ForEach(PATH, x => { text += " " + x; });
            text += " ]";
            lbRoute.Content = text;
        }
    }
}