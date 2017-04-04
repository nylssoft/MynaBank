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

namespace Bank
{
    public partial class UpdateBalanceWindow : Window
    {
        public long First { get; private set; }

        public UpdateBalanceWindow(Window owner, string title, Balance balance)
        {
            Owner = owner;
            Title = title;
            InitializeComponent();
            DateTime dt = new DateTime(balance.Year, balance.Month, 1);
            textBlockInfo.Text = string.Format(Properties.Resources.TEXT_ENTER_ACCOUNT_BALANCE_0, $"{dt:y}");
            textBoxFirst.Text = CurrencyConverter.ConvertToInputString(balance.First);
            textBoxFirst.Focus();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                First = CurrencyConverter.ParseCurrency(textBoxFirst.Text);
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
