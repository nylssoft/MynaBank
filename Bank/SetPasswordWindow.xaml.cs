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
using System.Security;
using System.Windows;

namespace Bank
{
    public partial class SetPasswordWindow : Window
    {
        private Database database;
        public SecureString Password { get; private set; }

        public SetPasswordWindow(Window owner, string title, Database database, SecureString password)
        {
            Owner = owner;
            Title = title;
            Password = password;
            this.database = database;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            if (password != null)
            {
                passwordBoxOld.Focus();
            }
            else
            {
                passwordBoxOld.Visibility = Visibility.Hidden;
                labelPasswordBoxOld.Visibility = Visibility.Hidden;
                grid.RowDefinitions[0].Height = new GridLength(0.0);
                passwordBoxNew.Focus();
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Password != null && !Password.IsEqualTo(passwordBoxOld.SecurePassword))
                {
                    MessageBox.Show(Properties.Resources.ERROR_WRONG_PASSWORD, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!passwordBoxNew.SecurePassword.IsEqualTo(passwordBoxNewConfirm.SecurePassword))
                {
                    MessageBox.Show($"{Properties.Resources.ERROR_PASSWORD_DOES_NOT_MATCH}", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                database.ChangePassword(passwordBoxNew.SecurePassword);
                Password = passwordBoxNew.SecurePassword.Length > 0 ? passwordBoxNew.SecurePassword : null;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PasswordBoxNew_PasswordChanged(object sender, RoutedEventArgs e)
        {
            passwordBoxNewConfirm.Password = string.Empty;
        }
    }
}
