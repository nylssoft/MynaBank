/*
    Myna Bank
    Copyright (C) 2018 Niels Stockfleth

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Bank
{
    public partial class SearchWindow : Window
    {
        private bool init = false;
        private DateTime refdate;
        private ObservableCollection<SearchBooking> result = new ObservableCollection<SearchBooking>();
        private List<SearchBooking> all = new List<SearchBooking>();
        private SortDecorator sortDecorator = null;
        private DateTime? lastFromDate = null;
        private DateTime? lastToDate = null;
        private string lastSearchText = string.Empty;
        private ISet<string> lastAccountNames = new HashSet<string>();

        public SearchWindow(Window owner, string title)
        {
            init = true;
            InitializeComponent();
            listView.ItemsSource = result;
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            IsClosed = false;
            textBoxSearch.Focus();
            init = false;
        }

        public void Update(Database database)
        {
            init = true;
            try
            {
                Init(database);
            }
            finally
            {
                init = false;
            }
        }

        private void Init(Database database)
        {
            stackPanelAccounts.Children.Clear();
            var nw = DateTime.Now;
            refdate = new DateTime(nw.Year, nw.Month, nw.Day);
            var last = new DateTime(refdate.Year - 1, 1, 1);
            var lastdays = (int)(refdate - last).TotalDays;
            datePickerFrom.SelectedDate = refdate;
            datePickerTo.SelectedDate = last;
            foreach (var account in database.GetAccounts())
            {
                var cb = new CheckBox();
                cb.Content = new TextBlock() { Text = account.Name };
                cb.Tag = account;
                cb.IsChecked = false;
                cb.Margin = new Thickness(0,5,10,5);
                cb.Checked += CheckBox_Changed;
                cb.Unchecked += CheckBox_Changed;
                stackPanelAccounts.Children.Add(cb);
                foreach (var balance in database.GetBalances(account))
                {
                    long y = balance.First;
                    foreach (var booking in database.GetBookings(balance))
                    {
                        y += booking.Amount;
                        var dt = new DateTime(balance.Year, balance.Month, booking.Day);
                        var x = (refdate - dt).TotalDays;
                        if (x <= lastdays)
                        {
                            if (cb.IsChecked == false)
                            {
                                cb.IsChecked = true;
                            }
                        }
                        var s = new SearchBooking() {
                            Date = dt,
                            DateString = $"{dt:d}",
                            Text = booking.Text,
                            Amount = booking.Amount,
                            AmountString = CurrencyConverter.ConvertToCurrencyString(booking.Amount),
                            AccountName = account.Name};
                        all.Add(s);
                    }
                }
            }
            if (stackPanelAccounts.Children.Count > 0)
            {
                labelAccount.Target = stackPanelAccounts.Children[0];
            }
        }

        private void DatePickerFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            Search();
        }

        private void DatePickerTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            Search();
        }

        private void TextBoxSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (init) return;
            Search();
        }

        private void TextBoxSearch_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                e.Handled = true;
                if (init) return;
                Search();
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (init) return;
            Search();
        }

        public bool IsClosed { get; set; } = false;

        private void Window_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void ListView_ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            var column = (sender as GridViewColumnHeader);
            if (column == null || column.Tag == null || sortDecorator == null) return;
            sortDecorator.Click(column);
            string sortBy = column.Tag.ToString();
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            viewlist.SortDescriptions.Clear();
            viewlist.SortDescriptions.Add(new SortDescription(sortBy, sortDecorator.Direction));
            if (sortBy != "Date")
            {
                viewlist.SortDescriptions.Add(new SortDescription("Date", sortDecorator.Direction));
            }
        }

        private void Search()
        {
            var search = textBoxSearch.Text.ToLower();
            if (string.IsNullOrEmpty(search) ||
                !datePickerFrom.SelectedDate.HasValue ||
                !datePickerTo.SelectedDate.HasValue)
            {
                return;
            }
            var fromdays = Math.Max((int)((refdate - datePickerFrom.SelectedDate.Value).TotalDays), 0);
            var todays = Math.Max((int)(refdate - datePickerTo.SelectedDate.Value).TotalDays + 1, 0);
            if (todays < fromdays)
            {
                return;
            }
            ISet<string> accountNames = new HashSet<string>();
            foreach (CheckBox cb in stackPanelAccounts.Children)
            {
                if (cb.IsChecked == true && cb.Tag is Account account)
                {
                    accountNames.Add(account.Name);
                }
            }
            if (lastFromDate == datePickerFrom.SelectedDate &&
                lastToDate == datePickerTo.SelectedDate && 
                lastSearchText == textBoxSearch.Text &&
                lastAccountNames.SetEquals(accountNames))
            {
                return;
            }
            lastFromDate = datePickerFrom.SelectedDate;
            lastToDate = datePickerTo.SelectedDate;
            lastSearchText = textBoxSearch.Text;
            lastAccountNames = accountNames;
            result.Clear();
            foreach (var sb in all)
            {
                if (accountNames.Contains(sb.AccountName))
                {
                    if (sb.Text.ToLower().Contains(search))
                    {
                        var x = (refdate - sb.Date).TotalDays;
                        if (x >= fromdays && x <= todays)
                        {
                            result.Add(sb);
                        }
                    }
                }
            }
            if (sortDecorator == null)
            {
                sortDecorator = new SortDecorator(ListSortDirection.Descending);
                sortDecorator.Click(gridViewColumHeaderDate);
                var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
                viewlist.SortDescriptions.Clear();
                viewlist.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
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
                foreach (SearchBooking b in listView.SelectedItems)
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
            }
            textBlockInfo.Text = status;
        }

        private class SearchBooking
        {
            public DateTime Date { get; set; }
            public string Text { get; set; }
            public long Amount { get; set; }
            public string AccountName { get; set; }
            public string DateString { get; set; }
            public string AmountString { get; set; }
        }
    }
}
