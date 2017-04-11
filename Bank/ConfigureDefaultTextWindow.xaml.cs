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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bank
{
    public partial class ConfigureDefaultTextWindow : Window
    {
        private bool changed = false;

        public List<string> DefaultTexts { get; private set; }

        public ConfigureDefaultTextWindow(Window owner, string title, List<string> defaultTexts)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            foreach (string txt in defaultTexts)
            {
                listBoxDefaultText.Items.Add(txt);
            }
            textBoxDefaultText.Focus();
            changed = false;
            UpdateControls();
        }

        private void UpdateControls()
        {
            buttonOK.IsEnabled = changed;
            buttonAddDefaultText.IsEnabled = textBoxDefaultText.Text.Length > 0;
            buttonRemoveDefaultText.IsEnabled = listBoxDefaultText.SelectedItems.Count > 0;
            buttonEditDefaultText.IsEnabled = listBoxDefaultText.SelectedItems.Count == 1;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DefaultTexts = new List<string>();
            foreach (string txt in listBoxDefaultText.Items)
            {
                DefaultTexts.Add(txt);
            }
            DialogResult = true;
            Close();
        }

        private void ListBoxDefaultText_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateControls();
        }

        private void TextBoxDefaultText_TextChanged(object sender, TextChangedEventArgs e)
        {
            changed = true;
            UpdateControls();
        }

        private void ButtonAddDefaultText_Click(object sender, RoutedEventArgs e)
        {
            string txt = textBoxDefaultText.Text.Trim();
            listBoxDefaultText.Items.Add(txt);
            listBoxDefaultText.ScrollIntoView(txt);
            textBoxDefaultText.Text = "";
            textBoxDefaultText.Focus();
            changed = true;
            UpdateControls();
        }

        private void ButtonEditDefaultText_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDefaultText.SelectedItems.Count != 1) return;
            string txt = listBoxDefaultText.SelectedItem as string;
            if (txt == null) return;
            var w = new EditDefaultTextWindow(this, Properties.Resources.TITLE_EDIT_DEFAULT_TEXT, txt);
            if (w.ShowDialog() == true)
            {
                int selidx = listBoxDefaultText.SelectedIndex;
                listBoxDefaultText.Items[selidx] = w.DefaultText;
                listBoxDefaultText.SelectedIndex = selidx;
                changed = true;
                UpdateControls();
            }
        }

        private void ButtonRemoveDefaultText_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxDefaultText.SelectedItems.Count > 0)
            {
                int idx = listBoxDefaultText.SelectedIndex;
                List<string> selected = new List<string>();
                foreach (string txt in listBoxDefaultText.SelectedItems)
                {
                    selected.Add(txt);
                }
                foreach (string txt in selected)
                {
                    listBoxDefaultText.Items.Remove(txt);
                }
                idx = Math.Min(idx, listBoxDefaultText.Items.Count - 1);
                if (idx >= 0)
                {
                    listBoxDefaultText.SelectedIndex = idx;
                }
                changed = true;
                UpdateControls();
            }
        }

        private void TextBoxDefaultText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ButtonAddDefaultText_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ListBoxDefaultText_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ButtonEditDefaultText_Click(sender, null);
        }
    }
}
