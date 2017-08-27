/*
    Myna Bank
    Copyright (C) 2017 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Bank
{
    public partial class StatisticsWindow : Window
    {
        private Matrix WtoDMatrix;
        private Matrix DtoWMatrix;

        private bool init = false;

        private Dictionary<string, List<Point>> dataDict = new Dictionary<string, List<Point>>();
        private Dictionary<string, List<string>> infoDict = new Dictionary<string, List<string>>();

        private DateTime refdate;
        private long totalMinY = long.MaxValue;
        private long totalMaxY = long.MinValue;

        private Brush [] brushes = new[] { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Black, Brushes.DarkBlue, Brushes.DarkGray, Brushes.DarkGreen };

        private TextBlock infoTextBlock = new TextBlock { Background = Brushes.Yellow, TextAlignment = TextAlignment.Left, VerticalAlignment = VerticalAlignment.Top };

        public StatisticsWindow(Window owner, string title)
        {
            init = true;
            InitializeComponent();
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            IsClosed = false;
            init = false;
        }

        public void Update(Database database)
        {
            init = true;
            try
            {
                Init(database);
                DrawGraph();
            }
            finally
            {
                init = false;
            }
        }

        private void Init(Database database)
        {
            dataDict.Clear();
            infoDict.Clear();
            totalMinY = long.MaxValue;
            totalMaxY = long.MinValue;
            stackPanelAccounts.Children.Clear();
            var nw = DateTime.Now;
            refdate = new DateTime(nw.Year, nw.Month, nw.Day);
            var last = new DateTime(refdate.Year - 1, 1, 1);
            var lastdays = (int)(refdate - last).TotalDays;
            datePickerFrom.SelectedDate = refdate;
            datePickerTo.SelectedDate = last;
            int brushidx = 0;
            long allMinY = long.MaxValue;
            long allMaxY = long.MinValue;
            foreach (var account in database.GetAccounts())
            {
                var cb = new CheckBox();
                cb.Content = new TextBlock()
                {
                    Text = account.Name,
                    Background = brushes[brushidx++],
                    Foreground = Brushes.White
                };
                if (brushidx >= brushes.Length)
                {
                    brushidx = 0;
                }
                cb.Tag = account.Name;
                cb.IsChecked = false;
                cb.Margin = new Thickness(10,5,10,5);
                cb.Checked += CheckBox_Changed;
                cb.Unchecked += CheckBox_Changed;
                stackPanelAccounts.Children.Add(cb);
                long minY = long.MaxValue;
                long maxY = long.MinValue;
                var points = new List<Point>();
                var info = new List<string>();
                foreach (var balance in database.GetBalances(account))
                {
                    long y = balance.First;
                    foreach (var booking in database.GetBookings(balance))
                    {
                        y += booking.Amount;
                        var dt = new DateTime(balance.Year, balance.Month, booking.Day);
                        var x = (refdate - new DateTime(balance.Year, balance.Month, booking.Day)).TotalDays;
                        if (x <= lastdays)
                        {
                            if (cb.IsChecked == false)
                            {
                                cb.IsChecked = true;
                            }
                            minY = Math.Min(y, minY);
                            maxY = Math.Max(y, maxY);
                        }
                        totalMinY = Math.Min(y, totalMinY);
                        totalMaxY = Math.Max(y, totalMaxY);
                        points.Add(new Point(x, y));
                        info.Add($"{dt:d} {CurrencyConverter.ConvertToCurrencyString(y)}\r\n{booking.Text} {CurrencyConverter.ConvertToCurrencyString(booking.Amount)}");
                    }
                }
                points.Reverse();
                info.Reverse();
                dataDict[account.Name] = points;
                infoDict[account.Name] = info;
                allMinY = Math.Min(allMinY, minY);
                allMaxY = Math.Max(allMaxY, maxY);
            }
            textBoxYMin.Text = CurrencyConverter.ConvertToInputString(allMinY);
            textBoxYMax.Text = CurrencyConverter.ConvertToInputString(allMaxY);
        }

        private void PrepareTransformations(
            double wxmin, double wxmax, double wymin, double wymax,
            double dxmin, double dxmax, double dymin, double dymax)
        {
            WtoDMatrix = Matrix.Identity;
            WtoDMatrix.Translate(-wxmin, -wymin);
            double xscale = (dxmax - dxmin) / (wxmax - wxmin);
            double yscale = (dymax - dymin) / (wymax - wymin);
            WtoDMatrix.Scale(xscale, yscale);
            WtoDMatrix.Translate(dxmin, dymin);
            DtoWMatrix = WtoDMatrix;
            DtoWMatrix.Invert();
        }

        private Point WtoD(Point point)
        {
            return WtoDMatrix.Transform(point);
        }

        private Point DtoW(Point point)
        {
            return DtoWMatrix.Transform(point);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (init) return;
            if (e.HeightChanged)
            {
                canGraph.Height = Math.Max(e.NewSize.Height - 110, 110);
            }
            if (e.WidthChanged)
            {
                canGraph.Width = Math.Max(e.NewSize.Width - 10, 10);
            }
            DrawGraph();
        }
        
        private void DrawGraph()
        {
            canGraph.Children.Clear();
            init = true;
            try
            {
                infoTextBlock.Visibility = Visibility.Hidden;
                var ymin = CurrencyConverter.ParseCurrency(textBoxYMin.Text);
                if (ymin < totalMinY)
                {
                    ymin = totalMinY;
                    textBoxYMin.Text = CurrencyConverter.ConvertToInputString(totalMinY);
                }
                var ymax = CurrencyConverter.ParseCurrency(textBoxYMax.Text);
                if (ymax > totalMaxY)
                {
                    ymax = totalMaxY;
                    textBoxYMax.Text = CurrencyConverter.ConvertToInputString(totalMaxY);
                }
                if (!datePickerFrom.SelectedDate.HasValue ||
                    !datePickerTo.SelectedDate.HasValue ||
                    ymin >= ymax)
                {
                    return;
                }
                var fromdays = Math.Max((int)((refdate - datePickerFrom.SelectedDate.Value).TotalDays), 0);
                var todays = Math.Max((int)(refdate - datePickerTo.SelectedDate.Value).TotalDays + 1, 0);
                if (todays < fromdays)
                {
                    return;
                }
                double
                    dxmin = 60,
                    dxmax = Math.Max(canGraph.Width - 30, 30),
                    dymin = 30,
                    dymax = Math.Max(canGraph.Height - 70, 70);
                double
                    wxmin = fromdays,
                    wxmax = todays,
                    wymin = ymin,
                    wymax = ymax;
                PrepareTransformations(
                    wxmin, wxmax, wymin, wymax,
                    dxmin, dxmax, dymax, dymin);
                DrawAxis(wxmin, wxmax, wymin, wymax);
                foreach (CheckBox cb in stackPanelAccounts.Children)
                {
                    if (cb.IsChecked == true && cb.Tag is string name)
                    {
                        if (cb.Content is TextBlock tb)
                        {
                            DrawAccount(name, tb.Background, wxmin, wxmax);
                        }
                    }
                }
                canGraph.Children.Add(infoTextBlock);
            }
            catch
            {
                // ignored
            }
            finally
            {
                init = false;
            }
        }

        private void DrawAxis(double wxmin, double wxmax, double wymin, double wymax)
        {
            var axis = new GeometryGroup();
            var wys = Math.Round(wymin / 100000.0) * 100000.0;
            var wye = Math.Round(wymax / 100000.0) * 100000.0;
            var ps = WtoD(new Point(wxmin, wys));
            var pe = WtoD(new Point(wxmin, wye));
            axis.Children.Add(new LineGeometry
            {
                StartPoint = new Point(ps.X, ps.Y + 8),
                EndPoint = new Point(pe.X, pe.Y - 8)
            });
            pe = WtoD(new Point(wxmax, wys));
            axis.Children.Add(new LineGeometry
            {
                StartPoint = new Point(ps.X - 8, ps.Y),
                EndPoint = new Point(pe.X + 8, pe.Y)
            });
            var ticks = new GeometryGroup();
            double? lasty = null;
            for (var ticy = wys; ticy <= wye; ticy += 100000)
            {
                var p = WtoD(new Point(wxmin, ticy));
                if (!lasty.HasValue || Math.Abs(p.Y - lasty.Value) > 15)
                {
                    var t = new TextBlock
                    {
                        Text = Convert.ToString((long)(ticy / 100.0)),
                        TextAlignment = TextAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    Canvas.SetLeft(t, p.X - 50);
                    Canvas.SetTop(t, p.Y - 8);
                    canGraph.Children.Add(t);
                    ticks.Children.Add(new LineGeometry
                    {
                        StartPoint = new Point { X = p.X - 8, Y = p.Y },
                        EndPoint = WtoD(new Point(wxmax, ticy))
                    });
                    lasty = p.Y;
                }
            }
            double? lastxDay = null;
            double? lastxYear = null;
            for (int ticx = (int)wxmin; ticx <= (int)wxmax; ticx++)
            {
                var dt = refdate.AddDays(-(ticx - (int)wxmin));
                if (dt.Day == 1)
                {
                    var p = WtoD(new Point(ticx, wys));
                    if (!lastxDay.HasValue || Math.Abs(p.X - lastxDay.Value) > 15)
                    {
                        lastxDay = p.X;
                        var t = new TextBlock
                        {
                            Text = $"{dt.Month}",
                            TextAlignment = TextAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top
                        };
                        Canvas.SetLeft(t, p.X - 4);
                        Canvas.SetTop(t, p.Y + 8);
                        canGraph.Children.Add(t);
                        ticks.Children.Add(new LineGeometry
                        {
                            StartPoint = new Point { X = p.X, Y = p.Y + 8 },
                            EndPoint = WtoD(new Point(ticx, wye))
                        });
                    }
                    if (dt.Month == 1)
                    {
                        if (!lastxYear.HasValue || Math.Abs(p.X - lastxYear.Value) > 30)
                        {
                            lastxYear = p.X;
                            var ty = new TextBlock
                            {
                                Text = $"{dt.Year}",
                                TextAlignment = TextAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top
                            };
                            Canvas.SetLeft(ty, p.X - 15);
                            Canvas.SetTop(ty, p.Y + 28);
                            canGraph.Children.Add(ty);
                        }
                    }
                }
            }
            canGraph.Children.Add(new Path { StrokeThickness = 1, Stroke = Brushes.LightGray, Data = ticks });
            canGraph.Children.Add(new Path { StrokeThickness = 1, Stroke = Brushes.Black, Data = axis });
        }

        private void DrawAccount(string name, Brush brush, double wxmin, double wxmax)
        {
            if (!dataDict.ContainsKey(name)) return;
            var points = dataDict[name];
            if (points.Count == 0) return;
            var geometryGroup = new GeometryGroup();
            var pointCollection = new PointCollection();
            foreach (var wp in points)
            {
                if (wp.X >= wxmin && wp.X <= wxmax)
                {
                    var p = WtoD(wp);
                    pointCollection.Add(p);
                    geometryGroup.Children.Add(new EllipseGeometry(p, 5, 5));
                }
            }
            canGraph.Children.Add(new Path { StrokeThickness = 1, Stroke = brush, Data = geometryGroup });
            canGraph.Children.Add(new Polyline { StrokeThickness = 1, Stroke = brush, Points = pointCollection });
        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            DrawGraph();
        }

        private void DatePickerTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            DrawGraph();
        }

        private void TextBoxY_LostFocus(object sender, RoutedEventArgs e)
        {
            if (init) return;
            DrawGraph();
        }

        private void TextBoxY_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                e.Handled = true;
                if (init) return;
                DrawGraph();
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (init) return;
            DrawGraph();
        }

        public bool IsClosed { get; set; } = false;

        private void Window_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Point? found = null;
                string info = "";
                var mp = e.GetPosition(canGraph);
                foreach (CheckBox cb in stackPanelAccounts.Children)
                {
                    if (cb.IsChecked == true && cb.Tag is string name)
                    {
                        var infoIdx = 0;
                        foreach (var wp in dataDict[name])
                        {
                            var p = WtoD(wp);
                            var vector = p - mp;
                            if (vector.Length < 5)
                            {
                                found = wp;
                                info = infoDict[name][infoIdx];
                                break;
                            }
                            infoIdx++;
                        }
                    }
                    if (found.HasValue)
                    {
                        break;
                    }
                }                
                if (found.HasValue)
                {
                    infoTextBlock.Visibility = Visibility.Visible;
                    infoTextBlock.Text = info;
                    var p = WtoD(found.Value);
                    Canvas.SetLeft(infoTextBlock, p.X - 10);
                    Canvas.SetTop(infoTextBlock, p.Y - 10);
                }
                else
                {
                    infoTextBlock.Visibility = Visibility.Hidden;
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}
