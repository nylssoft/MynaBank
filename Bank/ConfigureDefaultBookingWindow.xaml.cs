using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bank
{
    public partial class ConfigureDefaultBookingWindow : Window
    {
        private ObservableCollection<DefaultBooking> defaultBookings = new ObservableCollection<DefaultBooking>();
        private CheckBox[] checkBoxes;
        private bool changed = false;

        public ConfigureDefaultBookingWindow(Window owner, string title, List<DefaultBooking> defaultBookings)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            checkBoxes = new CheckBox[] {
                checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6,
                checkBox7, checkBox8, checkBox9, checkBox10, checkBox11, checkBox12,
            };
            foreach (var c in checkBoxes)
            {
                c.IsEnabled = false;
                c.Visibility = Visibility.Hidden;
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
                changed = true;
                UpdateControls();
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var item = listView.SelectedItem as DefaultBooking;
            if (item == null)
            {
                return;
            }
            var wnd = new EditDefaultBookingWindow(this, Properties.Resources.TITLE_CONFIGURE_DEFAULT_BOOKING, item);
            if (wnd.ShowDialog() == true)
            {
                changed = true;
                UpdateControls();
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
                var lvi = listView.ItemContainerGenerator.ContainerFromIndex(idx) as ListViewItem;
                if (lvi != null)
                {
                    lvi.Focus();
                }
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
                var wnd = new EditDefaultBookingWindow(
                    this,
                    Properties.Resources.TITLE_CONFIGURE_DEFAULT_BOOKING,
                    lvitem.Content as DefaultBooking);
                if (wnd.ShowDialog() == true)
                {
                    changed = true;
                    UpdateControls();
                }
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
    }
}
