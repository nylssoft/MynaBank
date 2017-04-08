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
using System.Windows;
using System.Windows.Controls;

namespace Bank
{
    public partial class EditDefaultTextWindow : Window
    {
        public string DefaultText { get; private set; }

        public EditDefaultTextWindow(Window owner, string title, string defaultText)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            textBoxDefaultText.Text = defaultText;
            textBoxDefaultText.Focus();
            buttonOK.IsEnabled = false;
        }

        private void UpdateControls()
        {
            bool enabled = !string.IsNullOrEmpty(textBoxDefaultText.Text);
            buttonOK.IsEnabled = enabled;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DefaultText = textBoxDefaultText.Text;
            DialogResult = true;
            Close();
        }

        private void TextBoxDefaultText_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateControls();
        }
    }
}
