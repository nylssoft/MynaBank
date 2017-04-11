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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Bank
{
    public partial class ConfigureDefaultBookingWindow : Window
    {
        private ObservableCollection<DefaultBooking> defaultBookings = new ObservableCollection<DefaultBooking>();
        private CheckBox[] checkBoxes;
        private bool changed = false;
        private Account account;

        public List<DefaultBooking> DefaultBookings { get; private set; }

        public ConfigureDefaultBookingWindow(Window owner, string title, Account account, List<DefaultBooking> defaultBookings)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.account = account;
            InitializeComponent();
            checkBoxes = new CheckBox[] {
                checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6,
                checkBox7, checkBox8, checkBox9, checkBox10, checkBox11, checkBox12,
            };
            for (int idx = 0; idx < checkBoxes.Length; idx++)
            {
                checkBoxes[idx].IsEnabled = false;
                checkBoxes[idx].Visibility = Visibility.Hidden;
                checkBoxes[idx].Content = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(idx + 1);
            }
            foreach (var item in defaultBookings)
            {
                this.defaultBookings.Add(item);
            }
            listView.ItemsSource = this.defaultBookings;
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource);
            viewlist.SortDescriptions.Add(new SortDescription("Day", ListSortDirection.Ascending));
            viewlist.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
            changed = false;
            UpdateControls();
        }

        private void UpdateControls()
        {
            var selcnt = listView.SelectedItems.Count;
            buttonEdit.IsEnabled = selcnt == 1;
            buttonRemove.IsEnabled = selcnt > 0;
            buttonOK.IsEnabled = changed;
        }

        private void UpdateMonthMask(int mask)
        {
            foreach (var c in checkBoxes)
            {
                c.IsChecked = false;
                c.Visibility = Visibility.Visible;
            }
            for (int idx=0; idx<12; idx++)
            {
                int val = 1 << idx;
                if ((val & mask) == val)
                {
                    checkBoxes[idx].IsChecked = true;
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cnt = listView.SelectedItems.Count;
            if (cnt == 1)
            {
                var item = listView.SelectedItem as DefaultBooking;
                UpdateMonthMask(item.Monthmask);
            }
            else
            {
                foreach (var c in checkBoxes)
                {
                    c.Visibility = Visibility.Hidden;
                }
            }
            UpdateControls();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new EditDefaultBookingWindow(this, Properties.Resources.TITLE_CONFIGURE_DEFAULT_BOOKING, null);
            if (wnd.ShowDialog() == true)
            {
                defaultBookings.Add(wnd.DefaultBooking);
                changed = true;
                UpdateControls();
                UpdateMonthMask(wnd.DefaultBooking.Monthmask);
                listView.SelectedItem = wnd.DefaultBooking;
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Edit(listView.SelectedItem as DefaultBooking);
        }

        private void Edit(DefaultBooking item)
        {
            if (item == null)
            {
                return;
            }
            var wnd = new EditDefaultBookingWindow(this, Properties.Resources.TITLE_CONFIGURE_DEFAULT_BOOKING, item);
            if (wnd.ShowDialog() == true)
            {
                item.Day = wnd.DefaultBooking.Day;
                item.Text = wnd.DefaultBooking.Text;
                item.Amount = wnd.DefaultBooking.Amount;
                item.Monthmask = wnd.DefaultBooking.Monthmask;
                changed = true;
                UpdateControls();
                var selitem = listView.SelectedItem as DefaultBooking;
                if (selitem != null)
                {
                    UpdateMonthMask(selitem.Monthmask);
                }
            }
        }

        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            var idx = listView.SelectedIndex;
            if (idx < 0)
            {
                return;
            }
            var del = new List<DefaultBooking>();
            foreach (DefaultBooking item in listView.SelectedItems)
            {
                del.Add(item);
            }
            foreach (var d in del)
            {
                defaultBookings.Remove(d);
            }
            idx = Math.Min(idx, defaultBookings.Count - 1);
            if (idx > 0)
            {
                listView.SelectedIndex = idx;
                listView.FocusItem(idx);
            }
            changed = true;
            UpdateControls();
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(listView);
            var lvitem = listView.GetItemAt(mousePosition);
            if (lvitem != null)
            {
                Edit(lvitem.Content as DefaultBooking);
            }
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ButtonRemove_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                ButtonEdit_Click(null, null);
                e.Handled = true;
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DefaultBookings = new List<DefaultBooking>();
            foreach (var item in defaultBookings)
            {
                item.Account = account;
                DefaultBookings.Add(item);
            }
            DialogResult = true;
            Close();
        }
    }
}
