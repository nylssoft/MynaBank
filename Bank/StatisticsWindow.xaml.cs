﻿/*
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
        private Dictionary<string, Point> minDict = new Dictionary<string, Point>();
        private Dictionary<string, Point> maxDict = new Dictionary<string, Point>();

        private Brush [] brushes = new[] { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Black, Brushes.DarkBlue, Brushes.DarkGray, Brushes.DarkGreen };

        public StatisticsWindow(Window owner, string title, Database database)
        {
            init = true;
            InitializeComponent();
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Init(database);
            init = false;
        }

        private void Init(Database database)
        {
            var now = DateTime.Now;
            var totalMinY = Double.MaxValue;
            var totalMaxY = Double.MinValue;
            var totalMinX = Double.MaxValue;
            var totalMaxX = Double.MinValue;
            int brushidx = 0;
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
                cb.IsChecked = true;
                cb.Margin = new Thickness(10,5,10,5);
                cb.Checked += CheckBox_Changed;
                cb.Unchecked += CheckBox_Changed;
                stackPanelAccounts.Children.Add(cb);
                var minY = Double.MaxValue;
                var maxY = Double.MinValue;
                var minX = Double.MaxValue;
                var maxX = Double.MinValue;
                var points = new List<Point>();
                foreach (var balance in database.GetBalances(account))
                {
                    long y = balance.First;
                    foreach (var booking in database.GetBookings(balance))
                    {
                        y += booking.Amount;
                        var x = (now - new DateTime(balance.Year, balance.Month, booking.Day)).TotalDays;
                        points.Add(new Point(x, y));
                        maxX = Math.Max(x, maxX);
                        maxY = Math.Max(y, maxY);
                        minX = Math.Min(x, minX);
                        minY = Math.Min(y, minY);
                    }
                }
                points.Reverse();
                dataDict[account.Name] = points;
                minDict[account.Name] = new Point(minX, minY);
                maxDict[account.Name] = new Point(maxX, maxY);
                totalMaxX = Math.Max(totalMaxX, maxX);
                totalMaxY = Math.Max(totalMaxY, maxY);
                totalMinX = Math.Min(totalMinX, minX);
                totalMinY = Math.Min(totalMinY, minY);
            }
            datePickerFrom.SelectedDate = now.Subtract(new TimeSpan((int)totalMinX, 0, 0, 0));
            datePickerTo.SelectedDate = new DateTime(datePickerFrom.SelectedDate.Value.Year - 1, 1, 1);
            textBoxYMin.Text = CurrencyConverter.ConvertToInputString((long)totalMinY);
            textBoxYMax.Text = CurrencyConverter.ConvertToInputString((long)totalMaxY);
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
                canGraph.Height = Math.Max(e.NewSize.Height - 160, 160);
            }
            if (e.WidthChanged)
            {
                canGraph.Width = Math.Max(e.NewSize.Width - 20, 20);
            }
            DrawGraph();
        }

        
        private void DrawGraph()
        {
            canGraph.Children.Clear();
            try
            {
                var now = DateTime.Now;
                var ymin = CurrencyConverter.ParseCurrency(textBoxYMin.Text);
                var ymax = CurrencyConverter.ParseCurrency(textBoxYMax.Text);
                if (!datePickerFrom.SelectedDate.HasValue ||
                    !datePickerTo.SelectedDate.HasValue)
                {
                    return;
                }
                var fromdays = Math.Max((int)((now - datePickerFrom.SelectedDate.Value).TotalDays), 0);
                var todays = Math.Max((int)(now - datePickerTo.SelectedDate.Value).TotalDays, 0);
                if (todays < fromdays)
                {
                    return;
                }
                double
                    dxmin = 60,
                    dxmax = Math.Max(canGraph.Width - 60, 60),
                    dymin = 60,
                    dymax = Math.Max(canGraph.Height - 60, 60);
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
            }
            catch
            {
                // ignored
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
            for (var ticy = wys; ticy <= wye; ticy += 100000)
            {
                var p = WtoD(new Point(wxmin, ticy));
                var t = new TextBlock {
                    Text = Convert.ToString((long)(ticy / 100.0)),
                    TextAlignment = TextAlignment.Right,                    
                    VerticalAlignment = VerticalAlignment.Top};
                Canvas.SetLeft(t, p.X - 50);
                Canvas.SetTop(t, p.Y - 8);
                canGraph.Children.Add(t);
                ticks.Children.Add(new LineGeometry
                {
                    StartPoint = new Point { X = p.X - 8, Y = p.Y },
                    EndPoint = WtoD(new Point(wxmax, ticy))
                });
            }
            var now = DateTime.Now;
            for (int ticx = (int)wxmin; ticx <= (int)wxmax; ticx++)
            {
                var dt = now.Subtract(new TimeSpan(ticx, 0, 0, 0));
                if (dt.Day == 1)
                {
                    var p = WtoD(new Point(ticx, wys));
                    var t = new TextBlock
                    {
                        Text = $"{dt.Month}",
                        TextAlignment = TextAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    Canvas.SetLeft(t, p.X - 4);
                    Canvas.SetTop(t, p.Y + 8);
                    canGraph.Children.Add(t);
                    if (dt.Month == 1)
                    {
                        var ty = new TextBlock
                        {
                            Text = $"{dt.Year}",
                            TextAlignment = TextAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top
                        };
                        Canvas.SetLeft(ty, p.X - 10);
                        Canvas.SetTop(ty, p.Y + 28);
                        canGraph.Children.Add(ty);
                    }
                    ticks.Children.Add(new LineGeometry
                    {
                        StartPoint = new Point { X = p.X, Y = p.Y + 8 },
                        EndPoint = WtoD(new Point(ticx, wye))
                    });
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

        private void TextBoxYMin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (init) return;
            DrawGraph();
        }

        private void TextBoxYMax_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (init) return;
            DrawGraph();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (init) return;
            UpdateMinMax();
            DrawGraph();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void UpdateMinMax()
        {
            var now = DateTime.Now;
            var totalMinY = Double.MaxValue;
            var totalMaxY = Double.MinValue;
            var found = false;
            foreach (CheckBox cb in stackPanelAccounts.Children)
            {
                if (cb.IsChecked == true && cb.Tag is string name)
                {
                    var minp = minDict[name];
                    var maxp = maxDict[name];
                    totalMaxY = Math.Max(totalMaxY, maxp.Y);
                    totalMinY = Math.Min(totalMinY, minp.Y);
                    found = true;
                }
            }
            if (found)
            {
                init = true;
                textBoxYMin.Text = CurrencyConverter.ConvertToInputString((long)totalMinY);
                textBoxYMax.Text = CurrencyConverter.ConvertToInputString((long)totalMaxY);
                init = false;
            }
        }
    }
}
