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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Bank
{
    public partial class EditDefaultBookingWindow : Window
    {
        private CheckBox[] checkBoxes;
        private bool changed = false;

        public DefaultBooking DefaultBooking { get; private set; }

        public EditDefaultBookingWindow(Window owner, string title, DefaultBooking defaultBooking)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            checkBoxes = new CheckBox[] {
                checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6,
                checkBox7, checkBox8, checkBox9, checkBox10, checkBox11, checkBox12,
            };
            for (int idx = 0; idx < checkBoxes.Length; idx++)
            {
                checkBoxes[idx].Content = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(idx+1);
            }
            if (defaultBooking != null)
            {
                textBoxDay.Text = Convert.ToString(defaultBooking.Day);
                textBoxText.Text = defaultBooking.Text;
                textBoxAmount.Text = CurrencyConverter.ConvertToInputString(defaultBooking.Amount);
                UpdateMonthMask(defaultBooking.Monthmask);
            }
            textBoxDay.Focus();
            changed = false;
            UpdateControls();
        }

        private void UpdateControls()
        {
            bool ok = false;
            try
            {
                int day = Convert.ToInt32(textBoxDay.Text);
                if (changed &&
                    !string.IsNullOrEmpty(textBoxDay.Text) &&
                    !string.IsNullOrEmpty(textBoxText.Text) &&
                    !string.IsNullOrEmpty(textBoxAmount.Text) &&
                     day >= 1 && day <= 31)
                {
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

        private void UpdateMonthMask(int mask)
        {
            foreach (var c in checkBoxes)
            {
                c.IsChecked = false;
            }
            for (int idx = 0; idx < 12; idx++)
            {
                int val = 1 << idx;
                if ((val & mask) == val)
                {
                    checkBoxes[idx].IsChecked = true;
                }
            }
        }

        private int GetMonthMask()
        {
            int mask = 0;
            for (int idx=0; idx < checkBoxes.Length; idx++)
            {
                if (checkBoxes[idx].IsChecked == true)
                {
                    mask = mask | (1 << idx);
                }
            }
            return mask;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            changed = true;
            UpdateControls();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            changed = true;
            UpdateControls();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DefaultBooking = new DefaultBooking()
            {
                Day = Convert.ToInt32(textBoxDay.Text),
                Text = textBoxText.Text,
                Amount = CurrencyConverter.ParseCurrency(textBoxAmount.Text),
                Monthmask = GetMonthMask()                
            };
            DialogResult = true;
            Close();
        }
    }
}
