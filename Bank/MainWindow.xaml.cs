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
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace Bank
{
    public partial class MainWindow : Window
    {
        private Database database = new Database();
        private ObservableCollection<Account> accounts = new ObservableCollection<Account>();
        private List<Balance> balances = new List<Balance>();
        private ObservableCollection<Booking> bookings = new ObservableCollection<Booking>();
        private SecureString Password;
        private StatisticsWindow statisticsWindow = null;

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
                if (statisticsWindow != null && !statisticsWindow.IsClosed)
                {
                    statisticsWindow.Close();
                    statisticsWindow = null;
                }
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
            bool next = false;
            bool prev = false;
            if (account != null && balances.Count > 0)
            {
                int sv = (int)slider.Value;
                int sm = (int)slider.Maximum;
                updatebalance = true;
                if (balances.Count > 1)
                {
                    next = sv < sm;
                    prev = sv > 0;
                }
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
                case "ConfigureDefaultText":
                case "ConfigureDefaultBooking":
                case "SetPassword":
                    e.CanExecute = account != null;
                    break;
                case "ShowGraph":
                    e.CanExecute = account != null && (statisticsWindow == null || statisticsWindow.IsClosed);
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
                case "Next":
                case "Last":
                    e.CanExecute = next;
                    break;
                case "Previous":
                case "First":
                    e.CanExecute = prev;
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
                case "ConfigureDefaultText":
                    ConfigureDefaultText();
                    break;
                case "ConfigureDefaultBooking":
                    ConfigureDefaultBooking();
                    break;
                case "SetPassword":
                    SetPassword();
                    break;
                case "Next":
                    Next();
                    break;
                case "Previous":
                    Previous();
                    break;
                case "First":
                    First();
                    break;
                case "Last":
                    Last();
                    break;
                case "ShowGraph":
                    ShowGraph();
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
                if (CurrentBalance != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() => { CreateBalance(); }));
                }
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
            var di = new FileInfo(filename).Directory;
            if (!di.Exists)
            {
                Directory.CreateDirectory(di.FullName);
            }
            bool enterpwd = Properties.Settings.Default.HasPassword;
            if (!enterpwd)
            {
                try
                {
                    database.Open(filename, null);
                    Password = null;
                }
                catch
                {
                    enterpwd = true;
                }
            }
            if (enterpwd)
            {
                EnterPasswordWindow w = new EnterPasswordWindow(this, Properties.Resources.TITLE_ENTER_PASSWORD, database, filename);
                if (w.ShowDialog() == false)
                {
                    Close();
                    return;
                }
                Password = w.Password;
                Properties.Settings.Default.HasPassword = Password != null;
            }
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
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int selected = listView.SelectedItems.Count;
            int total = listView.Items.Count;
            string status = string.Empty;
            if (selected > 1)
            {
                status = string.Format(Properties.Resources.SELECTED_0_OF_1, selected, total);
                long sum = 0;
                foreach (Booking b in listView.SelectedItems)
                {
                    sum += b.Amount;
                }
                long avg = sum / listView.SelectedItems.Count;
                status += " ";
                status += string.Format(Properties.Resources.STATUS_AMOUNT_0_1,
                    CurrencyConverter.ConvertToCurrencyString(sum),
                    CurrencyConverter.ConvertToCurrencyString(avg));
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
                var cur = CurrentBalance;
                if (cur != null)
                {
                    status += " ";
                    status += string.Format(Properties.Resources.MONTH_BALANCE_0, CurrencyConverter.ConvertToCurrencyString(cur.Last - cur.First));
                }
            }
            textBlockStatus.Text = status;
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
                slider.Visibility = Visibility.Visible;
            }
            else
            {
                slider.Maximum = 0;
                slider.Visibility = Visibility.Hidden;
                textBlockCurrent.Text = "";
            }
            CurrentBalance = current;
            if (current == null && balances.Count > 0)
            {
                CurrentBalance = balances[balances.Count - 1];
            }
            ShowBalance(CurrentBalance);
        }

        private void CreateBalance()
        {
            try
            {
                if (comboBox.SelectedItem is Account account && CurrentBalance != null)
                {
                    DateTime now = DateTime.Now;
                    if (CurrentBalance.Year == now.Year && CurrentBalance.Month == now.Month - 1 ||
                        CurrentBalance.Year == now.Year - 1 && CurrentBalance.Month == 12 && now.Month == 1)
                    {
                        if (MessageBox.Show(
                            this,
                            string.Format(Properties.Resources.QUESTION_CREATE_SHEET_0, $"{now:y}"),
                            Title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            var balance = database.GetBalance(account, now.Month, now.Year, true /* create */);
                            ShowAccount(balance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
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
            UpdateStatus();
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
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes)
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
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes)
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
                    var newbooking = database.CreateBooking(balance, wnd.Day, wnd.Text, wnd.Amount);
                    ShowAccount(balance);
                    SelectBooking(newbooking);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void SelectBooking(Booking booking)
        {
            for (int idx = 0; idx < listView.Items.Count; idx++)
            {
                if (listView.Items[idx] is Booking b)
                {
                    if (b.Id == booking.Id)
                    {
                        listView.ScrollIntoView(b);
                        listView.SelectedIndex = idx;
                        listView.FocusItem(idx);
                        break;
                    }
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
                    SelectBooking(booking);
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
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    var idx = listView.SelectedIndex;
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
                    idx = Math.Min(idx, listView.Items.Count - 1);
                    if (idx >= 0)
                    {
                        listView.SelectedIndex = idx;
                        listView.FocusItem(idx);
                    }
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

        private void ConfigureDefaultText()
        {
            try
            {
                var account = comboBox.SelectedItem as Account;
                if (account == null) return;
                var defaultTexts = database.GetDefaultTexts(account);
                var dlg = new ConfigureDefaultTextWindow(this, Properties.Resources.TITLE_CONFIGURE_DEFAULT_TEXT, defaultTexts);
                if (dlg.ShowDialog() == true)
                {
                    database.SetDefaultTexts(account, dlg.DefaultTexts);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ConfigureDefaultBooking()
        {
            try
            {
                var account = comboBox.SelectedItem as Account;
                if (account == null) return;
                var defaultBookings = database.GetDefaultBookings(account);
                var dlg = new ConfigureDefaultBookingWindow(
                    this,
                    Properties.Resources.TITLE_CONFIGURE_DEFAULT_BOOKING,
                    account,
                    defaultBookings);
                if (dlg.ShowDialog() == true)
                {
                    database.SetDefaultBookings(account, dlg.DefaultBookings);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void SetPassword()
        {
            try
            {
                var wnd = new SetPasswordWindow(this, Properties.Resources.TITLE_SET_PASSWORD, database, Password);
                if (wnd.ShowDialog() == true)
                {
                    Password = wnd.Password;
                    Properties.Settings.Default.HasPassword = Password != null;
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Next()
        {
            try
            {
                slider.Value += 1;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Previous()
        {
            try
            {
                slider.Value -= 1;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void First()
        {
            try
            {
                slider.Value = 0;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void Last()
        {
            try
            {
                slider.Value = slider.Maximum;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        private void ShowGraph()
        {
            try
            {
                if (statisticsWindow == null || statisticsWindow.IsClosed)
                {
                    statisticsWindow = new StatisticsWindow(null, Properties.Resources.TITLE_SHOW_GRAPH);
                    statisticsWindow.Show();
                }
                statisticsWindow.Update(database);
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
