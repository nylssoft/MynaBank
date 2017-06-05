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
    public partial class EnterPasswordWindow : Window
    {
        private Database database;
        private string fileName;

        public SecureString Password { get; private set; }

        public EnterPasswordWindow(Window owner, string title, Database database, string fileName)
        {
            Owner = owner;
            Title = title;
            this.database = database;
            this.fileName = fileName;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            passwordBox.Focus();
        }
 
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                database.Open(fileName, passwordBox.SecurePassword);
                Password = passwordBox.SecurePassword.Length > 0 ? passwordBox.SecurePassword : null;
                DialogResult = true;
                Close();
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.ERROR_WRONG_PASSWORD, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
