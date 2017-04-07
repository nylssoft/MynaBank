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
            bool updatebalance = false;
            if (account != null && balances.Count > 0)
            {
                updatebalance = (int)slider.Value == (int)slider.Maximum;
            }
            switch (r.Name)
            {
                case "CreateAccount":
                case "ImportAccount":
                case "About":
                case "Exit":
                    e.CanExecute = true;
                    break;
                case "UpdateBalance":
                    e.CanExecute = updatebalance;
                    break;
                case "Add":
                case "DeleteAccount":
                case "RenameAccount":
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
                case "ImportAccount":
                    ImportAccount();
                    break;
                case "RenameAccount":
                    RenameAccount();
                    break;
                case "DeleteAccount":
                    DeleteAccount();
                    break;
                case "DeleteSheet":
                    DeleteSheet();
                    break;
                case "UpdateBalance":
                    UpdateBalance();
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
                    About();
                    break;
                default:
                    break;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(listView);
            var lvitem = listView.GetItemAt(mousePosition);
            if (lvitem != null)
            {
                UpdateBooking(lvitem.Content as Booking);
            }
        }

        private void ComboBoxAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (comboBox.SelectedIndex >= 0)
                {
                    Properties.Settings.Default.LastUsedAccount = comboBox.SelectedIndex;
                }
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
            var viewcombo = (CollectionView)CollectionViewSource.GetDefaultView(comboBox.ItemsSource);
            viewcombo.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            viewlist.SortDescriptions.Add(new SortDescription("Day", ListSortDirection.Ascending));
            viewlist.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
            if (accounts.Count > 0)
            {
                Properties.Settings.Default.LastUsedAccount = Math.Min(Properties.Settings.Default.LastUsedAccount, accounts.Count - 1);
                comboBox.SelectedIndex = Properties.Settings.Default.LastUsedAccount;
            }
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
                var wnd = new PrepareWindow(this, Properties.Resources.TITLE_NEW, null);
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

        private void ImportAccount()
        {
            try
            {
                var dlg = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = Properties.Resources.TEXT_SELECT_IMPORT_DIRECTORY,
                };
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var account = database.Migrate(dlg.SelectedPath);
                    accounts.Add(account);
                    comboBox.SelectedItem = account;
                    UpdateStatus();
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void RenameAccount()
        {
            try
            {
                var account = comboBox.SelectedItem as Account;
                if (account == null) return;
                var wnd = new PrepareWindow(this, Properties.Resources.TITLE_RENAME_ACCOUNT, account.Name);
                if (wnd.ShowDialog() == true)
                {
                    account.Name = wnd.AccountName;
                    database.UpdateAccount(account);
                    UpdateStatus();
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
                Properties.Settings.Default.LastUsedAccount = comboBox.SelectedIndex;
                var account = comboBox.SelectedItem as Account;
                if (account == null) return;
                if (MessageBox.Show(
                    string.Format(Properties.Resources.QUESTION_DELETE_ACCOUNT_0, account.Name),
                    Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    database.DeleteAccount(account);
                    accounts.Remove(account);
                    if (accounts.Count > 0)
                    {
                        Properties.Settings.Default.LastUsedAccount = Math.Min(Properties.Settings.Default.LastUsedAccount, accounts.Count - 1);
                        comboBox.SelectedIndex = Properties.Settings.Default.LastUsedAccount;
                    }
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
                    string.Format(Properties.Resources.QUESTION_DELETE_SHEET_0, $"{dt:y}"),
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

        private void UpdateBalance()
        {
            try
            {
                var current = CurrentBalance;
                if (current == null) return;
                long old = current.First;
                var wnd = new UpdateBalanceWindow(this, Properties.Resources.TITLE_UPDATE_BALANCE, current);
                if (wnd.ShowDialog() == true)
                {
                    long change = wnd.First - old;
                    if (change != 0)
                    {
                        current.First += change;
                        current.Last += change;
                        database.UpdateBalance(current, change);
                        ShowAccount(current);
                    }
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
                var defaultTexts = database.GetDefaultTexts(account);
                var wnd = new EditWindow(this, Properties.Resources.TITLE_ADD, dt, defaultTexts, null);
                if (wnd.ShowDialog() == true)
                {
                    var balance = database.GetBalance(account, wnd.Month, wnd.Year, true /* create */);
                    database.CreateBooking(balance, wnd.Day, wnd.Text, wnd.Amount);
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
            if (comboBox.SelectedItem is Account account)
            {
                foreach (var balance in database.GetBalances(account))
                {
                    balances.Add(balance);
                }
            }
            if (balances.Count > 1)
            {
                slider.Minimum = 0;
                slider.Maximum = balances.Count - 1;
                slider.TickFrequency = 1;
                slider.IsSnapToTickEnabled = true;
                slider.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                slider.Visibility = Visibility.Visible;
                slider.Width = 100;
            }
            else
            {
                slider.Maximum = 0;
                slider.Visibility = Visibility.Hidden;
                slider.Width = 0;
                textBlockCurrent.Text = "";
                textBlockCurrent.Width = 0;
            }
            CurrentBalance = current;
            if (current == null && balances.Count > 0)
            {
                textBlockCurrent.Width = 100;
                CurrentBalance = balances[balances.Count - 1];
            }
            ShowBalance(CurrentBalance);
        }

        private void ShowBalance(Balance balance)
        {
            bookings.Clear();
            if (balance != null)
            {
                DateTime dt = new DateTime(balance.Year, balance.Month, 1);
                textBlockCurrent.Text = $"{dt:y}";
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
            UpdateBooking(listView.SelectedItem as Booking);
        }

        private void UpdateBooking(Booking booking)
        {
            try
            {
                if (booking == null) return;
                var current = CurrentBalance;
                var defaultTexts = database.GetDefaultTexts(current.Account);
                var wnd = new EditWindow(this, Properties.Resources.TITLE_EDIT, new DateTime(current.Year, current.Month, 1), defaultTexts, booking);
                if (wnd.ShowDialog() == true)
                {
                    booking.Day = wnd.Day;
                    booking.Text = wnd.Text;
                    booking.Amount = wnd.Amount;
                    database.UpdateBooking(booking);
                    ShowAccount(current);
                }
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
                    Properties.Resources.QUESTION_DELETE_ITEMS,
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

        private void About()
        {
            try
            {
                var dlg = new AboutWindow(this);
                dlg.ShowDialog();
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
