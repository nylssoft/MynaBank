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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Bank
{
    public partial class MainWindow : Window
    {
        private Database database = new Database();
        private ObservableCollection<Account> accounts = new ObservableCollection<Account>();
        private List<Balance> balances = new List<Balance>();
        private ObservableCollection<Booking> bookings = new ObservableCollection<Booking>();

        public MainWindow()
        {
            InitializeComponent();
            listView.ItemsSource = bookings;
            comboBox.ItemsSource = accounts;
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
            accounts.Clear();
            foreach (var account in database.GetAccounts())
            {
                accounts.Add(account);
            }
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

        private void comboBoxAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var account = comboBox.SelectedItem as Account;
                if (account == null) return;
                balances.Clear();
                foreach (var balance in database.GetBalances(account))
                {
                    balances.Add(balance);
                }
                if (balances.Count > 0)
                {
                    var minidx = 0;
                    var maxidx = balances.Count - 1;
                    DateTime dtFirst = new DateTime(balances[minidx].Year, balances[maxidx].Month, 1);
                    DateTime dtLast = new DateTime(balances[maxidx].Year, balances[maxidx].Month, 1);
                    textBlockFirst.Text = $"{dtFirst:y}";
                    textBlockLast.Text = $"{dtLast:y}"; ;
                    slider.Minimum = 0;
                    slider.Maximum = maxidx;
                    slider.TickFrequency = 1;
                    slider.Value = 0;
                    slider.IsSnapToTickEnabled = true;
                    slider.Visibility = Visibility.Visible;
                    slider_ValueChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int cnt = (int)slider.Value;
            if (cnt < 0 || cnt >= balances.Count) return;
            var balance = balances[balances.Count - cnt - 1];
            DateTime dt = new DateTime(balance.Year, balance.Month, 1);
            textBlockCurrent.Text = string.Format(Properties.Resources.TEXT_CURRENT_BALANCE_0, $"{dt:y}");
            try
            {
                bookings.Clear();
                long currentBalance = balance.First;
                foreach (var booking in database.GetBookings(balance))
                {
                    currentBalance += booking.Amount;
                    booking.CurrentBalance = currentBalance;
                    bookings.Add(booking);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }
}
