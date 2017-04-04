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
using System.Windows;
using System.Windows.Controls;

namespace Bank
{
    public partial class EditWindow : Window
    {
        public int Year { get; private set; }
        public int Month { get; private set; }
        public int Day { get; private set; }
        public string Text { get; private set; }
        public long Amount { get; private set; }

        public EditWindow(Window owner, string title, DateTime dt, Booking booking)
        {
            Owner = owner;
            Title = title;
            InitializeComponent();
            textBoxMonth.Text = Convert.ToString(dt.Month);
            textBoxYear.Text = Convert.ToString(dt.Year);
            if (booking != null)
            {
                textBoxMonth.IsEnabled = false;
                textBoxYear.IsEnabled = false;
                textBoxDay.Text = Convert.ToString(booking.Day);
                textBoxText.Text = booking.Text;
                textBoxAmount.Text = CurrencyConverter.ConvertToInputString(booking.Amount);
            }
            textBoxDay.Focus();
            UpdateControls();
        }

        private void UpdateControls()
        {
            bool ok = false;
            try
            {
                if (!string.IsNullOrEmpty(textBoxYear.Text) &&
                    !string.IsNullOrEmpty(textBoxMonth.Text) &&
                    !string.IsNullOrEmpty(textBoxDay.Text) &&
                    !string.IsNullOrEmpty(textBoxText.Text) &&
                    !string.IsNullOrEmpty(textBoxAmount.Text))
                {
                    new DateTime(Convert.ToInt32(textBoxYear.Text), Convert.ToInt32(textBoxMonth.Text), Convert.ToInt32(textBoxDay.Text));
                    CurrencyConverter.ParseCurrency(textBoxAmount.Text);
                    ok = true;
                }
            }
            catch
            {
                // ignored
            }
            buttonOK.IsEnabled = ok;
        }

        private void TextBox_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateControls();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Year = Convert.ToInt32(textBoxYear.Text);
                Month = Convert.ToInt32(textBoxMonth.Text);
                Day = Convert.ToInt32(textBoxDay.Text);
                Text = textBoxText.Text;
                Amount = CurrencyConverter.ParseCurrency(textBoxAmount.Text);
                new DateTime(Year, Month, Day); // test valid date
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
