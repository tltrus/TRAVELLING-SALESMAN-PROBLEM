using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace WpfApp
{
    internal class AntColony
    {
        public delegate void BestLengthHandler(string s);
        public event BestLengthHandler BestLengthNotify; 

        Random random;

        int[][] dists, ants;
        Point[] cities;
        int[] bestTrail;
        double[][] pheromones;
        private double bestLength;
        public double BestLength 
        { 
            get 
            {
                bestLength = GetBestTrailLength();
                return bestLength;
            } 
        }

        public int time = 1;
        public bool isCalculationDone;

        public int elitistAnts = 2;              // Количество элитных муравьев
        public double eliteWeight = 2.0;         // Вес элитного феромона (во сколько раз больше обычного)
        public List<int[]> eliteTrails;          // Список элитных маршрутов
        public List<double> eliteLengths;        // Длины элитных маршрутов

        // influence of pheromone on direction
        public int alpha = 2;//3;
        // influence of adjacent node distance
        public int beta = 4;//2;

        // pheromone decrease factor
        public double rho = 0.05;//0.01;
        // pheromone increase factor
        public double Q = 100;//2.0;

        public AntColony(Random random, int numAnts, int numCities)
        {
            this.random = random;
            isCalculationDone = false;

            cities = MakeCityPositions(numCities);
            dists = MakeGraphDistances(cities);
            ants = InitAnts(numAnts, numCities); // Initialing ants to random trails

            // determine the best initial trail
            bestTrail = BestTrail(ants, dists);

            // Initializing pheromones on trails
            pheromones = InitPheromones(numCities);

            eliteTrails = new List<int[]>();
            eliteLengths = new List<double>();
        }

        public void Calculate(int maxTime)
        {
            if (time < maxTime)
            {
                // Сначала строим новые маршруты
                UpdateAnts(ants, pheromones, dists);

                // Затем обновляем феромоны (включая элитных муравьев)
                UpdatePheromones(pheromones, ants, dists);

                // Проверяем, нашли ли новый лучший маршрут
                int[] currBestTrail = BestTrail(ants, dists);
                double currBestLength = Length(currBestTrail, dists);

                if (currBestLength < bestLength)
                {
                    bestLength = currBestLength;
                    bestTrail = currBestTrail;
                    BestLengthNotify?.Invoke($"\nNew best length of {bestLength:F1} found at time {time}");

                    // Можно также добавить в элиту сразу
                    if (eliteTrails.Count < elitistAnts)
                    {
                        int[] trailCopy = new int[bestTrail.Length];
                        bestTrail.CopyTo(trailCopy, 0);
                        eliteTrails.Add(trailCopy);
                        eliteLengths.Add(bestLength);
                    }
                }

                time += 1;

                // Периодически выводим информацию об элите
                if (time % 100 == 0)
                {
                    DisplayEliteInfo();
                }
            }
            else
            {
                isCalculationDone = true;
            }
        }

        private void DisplayEliteInfo()
        {
            string info = $"\n--- Elite info at time {time} ---";
            for (int i = 0; i < eliteTrails.Count; i++)
            {
                info += $"\nElite {i + 1}: length = {eliteLengths[i]:F1}";
            }
            BestLengthNotify?.Invoke(info);
        }
        // --------------------------------------------------------------------------------------------

        private int[][] InitAnts(int numAnts, int numCities)
        {
            int[][] ants = new int[numAnts][];
            for (int k = 0; k <= numAnts - 1; k++)
            {
                int start = random.Next(0, numCities);
                ants[k] = RandomTrail(start, numCities);
            }
            return ants;
        }
        private int[] RandomTrail(int start, int numCities)
        {
            // helper for InitAnts
            int[] trail = new int[numCities];

            // sequential
            for (int i = 0; i <= numCities - 1; i++)
            {
                trail[i] = i;
            }

            // Fisher-Yates shuffle
            for (int i = 0; i <= numCities - 1; i++)
            {
                int r = random.Next(i, numCities);
                int tmp = trail[r];
                trail[r] = trail[i];
                trail[i] = tmp;
            }

            int idx = IndexOfTarget(trail, start);
            // put start at [0]
            int temp = trail[0];
            trail[0] = trail[idx];
            trail[idx] = temp;

            return trail;
        }
        private int IndexOfTarget(int[] trail, int target)
        {
            // helper for RandomTrail
            for (int i = 0; i <= trail.Length - 1; i++)
            {
                if (trail[i] == target)
                {
                    return i;
                }
            }
            throw new Exception("Target not found in IndexOfTarget");
        }
        private double GetBestTrailLength()
        {
            double bestLength = Length(bestTrail, dists);
            return bestLength;
        }
        private double Length(int[] trail, int[][] dists)
        {
            // total length of a trail
            double result = 0.0;
            for (int i = 0; i <= trail.Length - 2; i++)
            {
                result += Distance(trail[i], trail[i + 1], dists);
            }
            return result;
        }

        // -------------------------------------------------------------------------------------------- 

        private int[] BestTrail(int[][] ants, int[][] dists)
        {
            // best trail has shortest total length
            double bestLength = Length(ants[0], dists);
            int idxBestLength = 0;
            for (int k = 1; k <= ants.Length - 1; k++)
            {
                double len = Length(ants[k], dists);
                if (len < bestLength)
                {
                    bestLength = len;
                    idxBestLength = k;
                }
            }
            int numCities = ants[0].Length;
            //INSTANT VB NOTE: The local variable bestTrail was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
            int[] bestTrail_Renamed = new int[numCities];
            ants[idxBestLength].CopyTo(bestTrail_Renamed, 0);
            return bestTrail_Renamed;
        }

        // --------------------------------------------------------------------------------------------

        private double[][] InitPheromones(int numCities)
        {
            double[][] pheromones = new double[numCities][];
            for (int i = 0; i <= numCities - 1; i++)
            {
                pheromones[i] = new double[numCities];
            }
            for (int i = 0; i <= pheromones.Length - 1; i++)
            {
                for (int j = 0; j <= pheromones[i].Length - 1; j++)
                {
                    pheromones[i][j] = 0.01;
                    // otherwise first call to UpdateAnts -> BuiuldTrail -> NextNode -> MoveProbs => all 0.0 => throws
                }
            }
            return pheromones;
        }

        // --------------------------------------------------------------------------------------------

        private void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists)
        {
            int numCities = pheromones.Length;
            for (int k = 0; k <= ants.Length - 1; k++)
            {
                int start = random.Next(0, numCities);
                int[] newTrail = BuildTrail(k, start, pheromones, dists);
                ants[k] = newTrail;
            }
        }

        private int[] BuildTrail(int k, int start, double[][] pheromones, int[][] dists)
        {
            int numCities = pheromones.Length;
            int[] trail = new int[numCities];
            bool[] visited = new bool[numCities];
            trail[0] = start;
            visited[start] = true;
            for (int i = 0; i <= numCities - 2; i++)
            {
                int cityX = trail[i];
                int next = NextCity(k, cityX, visited, pheromones, dists);
                trail[i + 1] = next;
                visited[next] = true;
            }
            return trail;
        }

        private int NextCity(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k (with visited[]), at nodeX, what is next node in trail?
            double[] probs = MoveProbs(k, cityX, visited, pheromones, dists);

            double[] cumul = new double[probs.Length + 1];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                cumul[i + 1] = cumul[i] + probs[i];
                // consider setting cumul[cuml.Length-1] to 1.00
            }

            double p = random.NextDouble();

            for (int i = 0; i <= cumul.Length - 2; i++)
            {
                if (p >= cumul[i] && p < cumul[i + 1])
                {
                    return i;
                }
            }
            throw new Exception("Failure to return valid city in NextCity");
        }

        private double[] MoveProbs(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists)
        {
            // for ant k, located at nodeX, with visited[], return the prob of moving to each city
            int numCities = pheromones.Length;
            double[] taueta = new double[numCities];
            // inclues cityX and visited cities
            double sum = 0.0;
            // sum of all tauetas
            // i is the adjacent city
            for (int i = 0; i <= taueta.Length - 1; i++)
            {
                if (i == cityX)
                {
                    taueta[i] = 0.0;
                    // prob of moving to self is 0
                }
                else if (visited[i] == true)
                {
                    taueta[i] = 0.0;
                    // prob of moving to a visited city is 0
                }
                else
                {
                    taueta[i] = Math.Pow(pheromones[cityX][i], alpha) * Math.Pow((1.0 / Distance(cityX, i, dists)), beta);
                    // could be huge when pheromone[][] is big
                    if (taueta[i] < 0.0001)
                    {
                        taueta[i] = 0.0001;
                    }
                    else if (taueta[i] > (double.MaxValue / (numCities * 100)))
                    {
                        taueta[i] = double.MaxValue / (numCities * 100);
                    }
                }
                sum += taueta[i];
            }

            double[] probs = new double[numCities];
            for (int i = 0; i <= probs.Length - 1; i++)
            {
                probs[i] = taueta[i] / sum;
                // big trouble if sum = 0.0
            }
            return probs;
        }

        private void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists)
        {
            // 1. Обновляем список элитных маршрутов
            UpdateEliteTrails(ants, dists);

            // 2. Испарение феромонов для всех ребер
            for (int i = 0; i < pheromones.Length; i++)
            {
                for (int j = i + 1; j < pheromones[i].Length; j++)
                {
                    pheromones[i][j] *= (1.0 - rho);
                    pheromones[j][i] = pheromones[i][j];
                }
            }

            // 3. Обычные муравьи откладывают феромоны
            for (int k = 0; k < ants.Length; k++)
            {
                UpdatePheromonesForAnt(ants[k], dists, Q, pheromones);
            }

            // 4. Элитные муравьи откладывают ДОПОЛНИТЕЛЬНЫЙ феромон
            for (int e = 0; e < eliteTrails.Count; e++)
            {
                // Элитные муравьи откладывают феромон с увеличенным весом
                double eliteDeposit = Q * eliteWeight / eliteLengths[e];
                UpdatePheromonesForAnt(eliteTrails[e], dists, eliteDeposit, pheromones);
            }

            // 5. Ограничение значений феромонов
            LimitPheromoneValues(pheromones);
        }

        private void UpdatePheromonesForAnt(int[] trail, int[][] dists, double depositAmount, double[][] pheromones)
        {
            // Для каждого ребра в маршруте добавляем феромон
            for (int i = 0; i < trail.Length - 1; i++)
            {
                int cityX = trail[i];
                int cityY = trail[i + 1];

                pheromones[cityX][cityY] += depositAmount;
                pheromones[cityY][cityX] = pheromones[cityX][cityY];
            }

            // Замыкаем маршрут (от последнего к первому городу)
            int firstCity = trail[0];
            int lastCity = trail[trail.Length - 1];
            pheromones[firstCity][lastCity] += depositAmount;
            pheromones[lastCity][firstCity] = pheromones[firstCity][lastCity];
        }

        private void LimitPheromoneValues(double[][] pheromones)
        {
            for (int i = 0; i < pheromones.Length; i++)
            {
                for (int j = 0; j < pheromones[i].Length; j++)
                {
                    if (pheromones[i][j] < 0.0001)
                    {
                        pheromones[i][j] = 0.0001;
                    }
                    else if (pheromones[i][j] > 100000.0)
                    {
                        pheromones[i][j] = 100000.0;
                    }
                }
            }
        }
        private bool EdgeInTrail(int cityX, int cityY, int[] trail)
        {
            // are cityX and cityY adjacent to each other in trail[]?
            int lastIndex = trail.Length - 1;
            int idx = IndexOfTarget(trail, cityX); // какой индекс городХ в списке trail

            if (idx == 0 && trail[1] == cityY)
            {
                return true;
            }
            else if (idx == 0 && trail[lastIndex] == cityY)
            {
                return true;
            }
            else if (idx == 0)
            {
                return false;
            }
            else if (idx == lastIndex && trail[lastIndex - 1] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex && trail[0] == cityY)
            {
                return true;
            }
            else if (idx == lastIndex)
            {
                return false;
            }
            else if (trail[idx - 1] == cityY)
            {
                return true;
            }
            else if (trail[idx + 1] == cityY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // --------------------------------------------------------------------------------------------

        private Point[] MakeCityPositions(int numCities)
        {
            Point[] positions = new Point[numCities];

            for (int i = 0; i < numCities; i++)
            {
                var x = random.Next(20, MainWindow.width);
                var y = random.Next(20, MainWindow.height);

                positions[i] = new Point(x, y);
            }

            return positions;
        }
        private int[][] MakeGraphDistances(Point[] cities)
        {
            int numCities = cities.Length;

            int[][] dists = new int[numCities][];
            for (int i = 0; i <= dists.Length - 1; i++)
            {
                dists[i] = new int[numCities];
            }
            for (int i = 0; i <= numCities - 1; i++)
            {
                for (int j = i + 1; j <= numCities - 1; j++)
                {
                    if (i == j) continue;

                    int d = (int)Math.Sqrt((cities[j].X - cities[i].X) * (cities[j].X - cities[i].X) + (cities[j].Y - cities[i].Y) * (cities[j].Y - cities[i].Y));

                    dists[i][j] = d;
                    dists[j][i] = d;
                }
            }
            return dists;
        }
        private double Distance(int cityX, int cityY, int[][] dists) => dists[cityX][cityY];

        // --------------------------------------------------------------------------------------------

        public string DisplayBestTrail()
        {
            string str = "";
            for (int i = 0; i <= bestTrail.Length - 1; i++)
            {
                str += bestTrail[i] + " ";
                if (i > 0 && i % 10 == 0)
                {
                    str += "\n";
                }
            }
            str += "\n";
            return str;
        }

        public string ShowAnts()
        {
            string str = "\n";
            for (int i = 0; i <= ants.Length - 1; i++)
            {
                str += i + ": [ ";

                for (int j = 0; j <= 3; j++)
                {
                    str += ants[i][j] + " ";
                }

                str += ". . . ";

                for (int j = ants[i].Length - 4; j <= ants[i].Length - 1; j++)
                {
                    str += ants[i][j] + " ";
                }

                str += "] len = ";
                double len = Length(ants[i], dists);
                str += len.ToString("F1");
                str += "\n";
            }
            return str;
        }

        public void Drawing(DrawingContext dc)
        {
            var n = cities.Length;

            // Draw path
            if (bestTrail != null)
            {
                for (int i = 0; i < bestTrail.Length - 1; i++)
                {
                    Pen pen = new Pen(Brushes.LimeGreen, 2);
                    Point p0 = cities[bestTrail[i]];
                    Point p1 = cities[bestTrail[i + 1]];
                    dc.DrawLine(pen, p0, p1);
                }
            }

            // Draw point + labeling
            for (int i = 0; i < n; ++i)
            {
                // Draw point
                double size = 5;

                var brush = Brushes.DarkGray;

                dc.DrawEllipse(Brushes.Black, new Pen(brush, 3), cities[i], size, size);

                // Draw labeling
                FormattedText formattedText = new FormattedText(i.ToString(), CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Verdana"), 11, Brushes.Black,
                                                                VisualTreeHelper.GetDpi(MainWindow.visual).PixelsPerDip);
                dc.DrawText(formattedText, new Point(cities[i].X, cities[i].Y - 20));
            }
        }

        private void UpdateEliteTrails(int[][] ants, int[][] dists)
        {
            // Собираем все маршруты и их длины
            var allTrails = new List<(int[] trail, double length)>();

            for (int k = 0; k < ants.Length; k++)
            {
                double len = Length(ants[k], dists);
                allTrails.Add((ants[k].ToArray(), len));
            }

            // Добавляем текущий лучший маршрут
            allTrails.Add((bestTrail.ToArray(), bestLength));

            // Сортируем по длине (от наименьшей)
            allTrails.Sort((a, b) => a.length.CompareTo(b.length));

            // Обновляем список элитных маршрутов
            eliteTrails.Clear();
            eliteLengths.Clear();

            int eliteCount = Math.Min(elitistAnts, allTrails.Count);
            for (int i = 0; i < eliteCount; i++)
            {
                // Копируем маршрут, чтобы избежать изменения ссылок
                int[] trailCopy = new int[allTrails[i].trail.Length];
                allTrails[i].trail.CopyTo(trailCopy, 0);

                eliteTrails.Add(trailCopy);
                eliteLengths.Add(allTrails[i].length);
            }
        }
    }
}
