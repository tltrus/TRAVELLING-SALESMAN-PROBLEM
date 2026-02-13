using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace _Chart2D
{
    internal class CitiesData
    {
        public int[] cities;
        public List<Point> positions;
        Random rnd;

        public CitiesData(int numberCities, Random rnd)
        {
            this.rnd = rnd;
            positions = new List<Point>();

            cities = new int[numberCities];
            for (int i = 0; i < cities.Length; ++i)
            {
                cities[i] = i;
                //get random position to add to list
                int x = rnd.Next(MainWindow.width);
                int y = rnd.Next(MainWindow.height);
                Point pos = new Point(x, y);
                positions.Add(pos);
            }

            // for debugging
            //positions = new List<Point>()
            //{
            //    new Point(185, 505),
            //    new Point(199, 698),
            //    new Point(335, 372),
            //    new Point(314, 260),
            //    new Point(625, 474),
            //    new Point(126, 590),
            //    new Point(55,  525),
            //    new Point(500, 173),
            //    new Point(612, 18),
            //    new Point(353, 253),
            //    new Point(296, 689),
            //    new Point(325, 475),
            //    new Point(542, 321),
            //    new Point(109, 90),
            //    new Point(393, 107),
            //    new Point(329, 111),
            //    new Point(220, 612),
            //    new Point(295, 626),
            //    new Point(479, 61),
            //    new Point(346, 218)
            //};
        }
        public double Distance(int firstCity, int secondCity)
        {
            double x1 = positions[firstCity].X;
            double y1 = positions[firstCity].Y;

            double x2 = positions[secondCity].X;
            double y2 = positions[secondCity].Y;

            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        public override string ToString()
        {
            string s = "";
            s += "Cities: ";
            for (int i = 0; i < this.cities.Length; ++i)
                s += this.cities[i] + " ";
            return s;
        }
        public void Drawing(DrawingContext dc)
        {
            foreach (var p in positions)
            {
                dc.DrawEllipse(Brushes.DeepPink, null, p, 5, 5);

                // Draw labeling
                FormattedText formattedText = new FormattedText(positions.IndexOf(p).ToString(), CultureInfo.GetCultureInfo("en-us"),
                                                                FlowDirection.LeftToRight, new Typeface("Verdana"), 9, Brushes.Black,
                                                                VisualTreeHelper.GetDpi(MainWindow.visual).PixelsPerDip);
                dc.DrawText(formattedText, new Point(p.X + 5, p.Y - 15));
            }
        }
    }
}
