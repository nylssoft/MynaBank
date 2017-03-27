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
using System.ComponentModel;
using System.Windows;

namespace Bank
{
    public partial class MainWindow : Window
    {
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
        }

        // actions

        private void Init()
        {
            using (var db = new Database())
            {
                db.Open(@"%MyDocuments%\bank.bdb".ReplaceSpecialFolder());
                foreach (var account in db.GetAccounts())
                {
                    MessageBox.Show(account.Name);
                    DateTime from = db.GetFirstDate(account).Value;
                    from = from.AddDays(-(from.Day - 1));
                    var to = from.AddMonths(1);
                    MessageBox.Show($"Get bookings from [{from}...{to}[.");
                    foreach (var booking in db.GetBookings(account, from, to))
                    {
                        MessageBox.Show($"{booking.Date}|{booking.Text}|{booking.Amount}|{booking.Balance}");
                    }
                }
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
