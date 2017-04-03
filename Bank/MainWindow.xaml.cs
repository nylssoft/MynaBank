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
using System.Windows.Data;
using System.Windows.Input;

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

        private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            RoutedUICommand r = e.Command as RoutedUICommand;
            if (r == null) return;
            Account account = comboBox?.SelectedItem as Account;
            int selcount = (listView != null ? listView.SelectedItems.Count : 0);
            switch (r.Name)
            {
                case "CreateAccount":
                case "About":
                case "ShowSettings":
                case "Exit":
                    e.CanExecute = true;
                    break;
                case "Add":
                case "DeleteAccount":
                    e.CanExecute = account != null;
                    break;
                case "DeleteSheet":
                    e.CanExecute = balances.Count > 0;
                    break;
                case "Edit":
                    e.CanExecute = selcount == 1;
                    break;
                case "Remove":
                    e.CanExecute = selcount >= 1;
                    break;
                default:
                    break;
            }
        }

        private void Command_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RoutedUICommand r = e.Command as RoutedUICommand;
            if (r == null) return;
            switch (r.Name)
            {
                case "Exit":
                    Close();
                    break;
                case "CreateAccount":
                    CreateAccount();
                    break;
                case "DeleteAccount":
                    DeleteAccount();
                    break;
                case "DeleteSheet":
                    DeleteSheet();
                    break;
                case "Add":
                    InsertBooking();
                    break;
                case "Edit":
                    UpdateBooking();
                    break;
                case "Remove":
                    DeleteBooking();
                    break;
                case "About":
                    //About();
                    break;
                case "ShowSettings":
                    //ShowSettings();
                    break;
                default:
                    break;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void ComboBoxAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ShowAccount(null);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void SliderValue_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                ShowBalance(CurrentBalance);
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
            var view = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Day", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
        }

        private Balance CurrentBalance
        {
            get
            {
                int cnt = (int)slider.Value;
                if (cnt < 0 || cnt >= balances.Count) return null;
                var balance = balances[balances.Count - cnt - 1];
                return balance;
            }
            set
            {
                if (value == null)
                {
                    slider.Value = 0;
                }
                else
                {
                    int idx = 0;
                    foreach (var b in balances)
                    {
                        if (b.Year == value.Year && b.Month == value.Month)
                        {
                            break;
                        }
                        idx++;
                    }
                    slider.Value = balances.Count - idx - 1;
                }
            }
        }

        private void CreateAccount()
        {
            try
            {
                var wnd = new PrepareWindow();
                if (wnd.ShowDialog() == true)
                {
                    var account = database.CreateAccount(wnd.AccountName);
                    accounts.Add(account);
                    comboBox.SelectedItem = account;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void DeleteAccount()
        {
            try
            {
                var account = comboBox.SelectedItem as Account;
                if (account == null) return;
                if (MessageBox.Show(
                    $"Wollen Sie das Konto '{account.Name}' wirklich löschen?",
                    Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    database.DeleteAccount(account);
                    accounts.Remove(account);
                    ShowAccount(null);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void DeleteSheet()
        {
            try
            {
                var current = CurrentBalance;
                if (current == null) return;
                DateTime dt = new DateTime(current.Year, current.Month, 1);
                if (MessageBox.Show(
                    $"Wollen Sie den Kontoauszug für {dt:y} wirklich löschen?",
                    Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    database.DeleteBalance(current);
                    ShowAccount(null);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void InsertBooking()
        {
            try
            {
                var account = comboBox.SelectedItem as Account;
                if (account == null) return;
                var current = CurrentBalance;
                DateTime dt = DateTime.Now;
                if (current != null)
                {
                    dt = new DateTime(current.Year, current.Month, 1);
                }
                var wnd = new EditWindow(dt);
                if (wnd.ShowDialog() == true)
                {
                    var balance = database.GetBalance(account, wnd.Month, wnd.Year, true /* create */);
                    database.CreateBooking(balance, wnd.Booking.Day, wnd.Booking.Text, wnd.Booking.Amount);
                    ShowAccount(balance);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ShowAccount(Balance current)
        {
            balances.Clear();
            var account = comboBox.SelectedItem as Account;
            if (account != null)
            {
                foreach (var balance in database.GetBalances(account))
                {
                    balances.Add(balance);
                }
            }
            if (balances.Count > 0)
            {
                var minidx = 0;
                var maxidx = balances.Count - 1;
                DateTime dtFirst = new DateTime(balances[minidx].Year, balances[minidx].Month, 1);
                DateTime dtLast = new DateTime(balances[maxidx].Year, balances[maxidx].Month, 1);
                textBlockFirst.Text = $"{dtFirst:y}";
                textBlockLast.Text = $"{dtLast:y}"; ;
                slider.Minimum = 0;
                slider.Maximum = maxidx;
                slider.TickFrequency = 1;
                slider.IsSnapToTickEnabled = true;
                slider.Visibility = Visibility.Visible;
                if (current == null)
                {
                    current = balances[balances.Count - 1];
                }
                CurrentBalance = current;
            }
            else
            {
                slider.Maximum = 0;
                slider.Visibility = Visibility.Hidden;
                textBlockCurrent.Text = "";
                textBlockFirst.Text = "";
                textBlockLast.Text = "";
            }
            ShowBalance(CurrentBalance);
        }

        private void ShowBalance(Balance balance)
        {
            bookings.Clear();
            if (balance != null)
            {
                DateTime dt = new DateTime(balance.Year, balance.Month, 1);
                textBlockCurrent.Text = string.Format(Properties.Resources.TEXT_CURRENT_BALANCE_0, $"{dt:y}");
                long currentBalance = balance.First;
                foreach (var booking in database.GetBookings(balance))
                {
                    currentBalance += booking.Amount;
                    booking.CurrentBalance = currentBalance;
                    bookings.Add(booking);
                }
            }
        }

        private void UpdateBooking()
        {
            try
            {
                var booking = listView.SelectedItem as Booking;
                if (booking == null) return;
                booking.Text = "Miete2";
                booking.Amount = 100000;
                database.UpdateBooking(booking);
                ShowAccount(CurrentBalance);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void DeleteBooking()
        {
            try
            {
                if (MessageBox.Show(
                    $"Wollen Sie die ausgewählten Buchungen wirklich löschen?",
                    Title, MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var del = new List<Booking>();
                    foreach (Booking booking in listView.SelectedItems)
                    {
                        del.Add(booking);
                    }
                    foreach (var d in del)
                    {
                        database.DeleteBooking(d);
                    }
                    ShowAccount(CurrentBalance);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
        private void UpdateStatus()
        {
            int selected = listView.SelectedItems.Count;
            int total = listView.Items.Count;
            string status = string.Empty;
            if (selected > 0)
            {
                if (total == 1)
                {
                    status = Properties.Resources.SELECTED_ONE;
                }
                else
                {
                    status = string.Format(Properties.Resources.SELECTED_0_OF_1, selected, total);
                }
            }
            else if (total > 0)
            {
                if (total == 1)
                {
                    status = Properties.Resources.TOTAL_ONE;
                }
                else
                {
                    status = string.Format(Properties.Resources.TOTAL_0, total);
                }
            }
            textBlockStatus.Text = status;
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

    }
}
