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
using System.Globalization;
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

        public EditWindow(Window owner, string title, DateTime dt, List<string> defaultTexts, Booking booking)
        {
            Owner = owner;
            Title = title;
            InitializeComponent();
            var monthnames = new List<string>();
            foreach (var m in DateTimeFormatInfo.CurrentInfo.MonthNames)
            {
                if (!string.IsNullOrEmpty(m))
                {
                    monthnames.Add(m);
                }
            }
            comboBoxMonth.ItemsSource = monthnames;
            comboBoxMonth.SelectedIndex = dt.Month - 1;
            textBoxYear.Text = Convert.ToString(dt.Year);
            comboBoxText.ItemsSource = defaultTexts;
            if (booking != null)
            {
                comboBoxMonth.IsEnabled = false;
                textBoxYear.IsEnabled = false;
                textBoxDay.Text = Convert.ToString(booking.Day);
                comboBoxText.Text = booking.Text;
                textBoxAmount.Text = CurrencyConverter.ConvertToInputString(booking.Amount);
            }
            textBoxDay.Focus();
            UpdateControls();
            // set max length of editable combobox
            comboBoxText.Loaded += delegate
            {
                if (comboBoxText.Template.FindName("PART_EditableTextBox", comboBoxText) is TextBox textBox)
                {
                    textBox.MaxLength = 260;
                }
            };
        }

        private void UpdateControls()
        {
            bool ok = false;
            try
            {
                if (!string.IsNullOrEmpty(textBoxYear.Text) &&
                    !string.IsNullOrEmpty(textBoxDay.Text) &&
                    !string.IsNullOrEmpty(comboBoxText.Text) &&
                    !string.IsNullOrEmpty(textBoxAmount.Text))
                {
                    var month = comboBoxMonth.SelectedIndex + 1;
                    new DateTime(Convert.ToInt32(textBoxYear.Text), month, Convert.ToInt32(textBoxDay.Text));
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
        private void ComboBoxMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControls();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Year = Convert.ToInt32(textBoxYear.Text);
                Month = Convert.ToInt32(comboBoxMonth.SelectedIndex + 1);
                Day = Convert.ToInt32(textBoxDay.Text);
                Text = comboBoxText.Text;
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.CaretIndex = tb.Text.Length;
            }
        }
    }
}
