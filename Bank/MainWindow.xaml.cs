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
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Bank
{
    public partial class MainWindow : Window
    {
        private Database database = new Database();

        public BookingsViewModel BookingsModel { get; set; } = new BookingsViewModel();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                HandleError(ex);
                Close();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (WindowState == WindowState.Normal)
                {
                    Properties.Settings.Default.Left = Left;
                    Properties.Settings.Default.Top = Top;
                    Properties.Settings.Default.Width = Width;
                    Properties.Settings.Default.Height = Height;
                }
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        // actions

        private void Init()
        {
            Title = Properties.Resources.TITLE_BANK;
            this.RestorePosition(
                Properties.Settings.Default.Left,
                Properties.Settings.Default.Top,
                Properties.Settings.Default.Width,
                Properties.Settings.Default.Height);
            string filename = Properties.Settings.Default.DatabaseFile.ReplaceSpecialFolder();
            FileInfo fi = new FileInfo(filename);
            if (!fi.Directory.Exists)
            {
                PrepareDirectory(fi.Directory.FullName);
            }
            database.Open(filename);
            var accounts = database.GetAccounts();
            foreach (var acc in accounts)
            {
                comboBoxAccounts.Items.Add(new ComboBoxItem() { Content = acc.Name, Tag = acc });
            }
            if (comboBoxAccounts.Items.Count > 0)
            {
                comboBoxAccounts.SelectedIndex = 0;
            }
            UpdateControls();
        }

        private void UpdateControls()
        {
        }

        private void SortListView()
        {
            //listView.Items.SortDescriptions.Clear();
            //listView.Items.SortDescriptions.Add(new SortDescription("DateTime", ListSortDirection.Descending));
            //listView.Items.Refresh();
        }

        private void PrepareDirectory(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void HandleError(Exception ex)
        {
            MessageBox.Show(
                this,
                string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message),
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        // private DateTime? lastBookingDate = null;
        // private List<Database.Booking> bookings = new List<Database.Booking>();

        private void comboBoxAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var cbi = comboBoxAccounts.SelectedItem as ComboBoxItem;
                if (cbi == null) return;
                var account = cbi.Tag as Database.Account;
                if (account == null) return;
                DateTime? first = database.GetFirstDate(account);
                DateTime? last = database.GetLastDate(account);
                if (first.HasValue && last.HasValue)
                {
                    var bookings = database.GetBookings(account, first.Value, last.Value);
                    BookingsModel = new BookingsViewModel(bookings) { LastDate = last.Value, FirstDate = last.Value.AddDays(-30.0) };
                    TimeSpan ts = last.Value - first.Value;
                    slider.Minimum = 30.0;
                    slider.Maximum = ts.TotalDays;
                    slider.TickFrequency = 30.0;
                    slider.Value = 30.0;
                    slider.IsSnapToTickEnabled = true;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int days = (int)slider.Value;
            textBlockTimeSpan.Text = $"{days} Tage";
            try
            {
                var cbi = comboBoxAccounts.SelectedItem as ComboBoxItem;
                if (cbi == null) return;
                var account = cbi.Tag as Database.Account;
                if (account == null) return;
                BookingsModel.FirstDate = BookingsModel.LastDate.AddDays(-slider.Value);                
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
