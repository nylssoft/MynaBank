/*
    Myna Bank
    Copyright (C) 2018 Niels Stockfleth

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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Bank
{
    public partial class ImportWindow : Window
    {
        private string csvFile;
        private int page = 1;
        private bool page1Changed = false;
        private bool page2Changed = false;
        private bool init = true;
        private Encoding encoding = null;
        private ObservableCollection<ImportBooking> result = new ObservableCollection<ImportBooking>();
        private SortDecorator sortDecorator = null;

        public List<ImportBooking> Result { get; set; }

        public ImportSetting Setting { get; private set; }

        public ImportWindow(Window owner, string title, string csvFile, ImportSetting setting)
        {
            this.csvFile = csvFile;
            Setting = setting;
            InitializeComponent();
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            listViewPreview.ItemsSource = result;
            textBoxEncoding.Text = Setting.Encoding;
            textBoxLanguage.Text = Setting.CurrencyLanguage;
            textBoxDateFormat.Text = Setting.DateFormat;
            textBoxSeparator.Text = Setting.Separator;
            textBoxText1Start.Text = Setting.Text1Start;
            textBoxText1End.Text = Setting.Text1End;
            textBoxText2Start.Text = Setting.Text2Start;
            textBoxText2End.Text = Setting.Text2End;
            textBoxText3Start.Text = Setting.Text3Start;
            textBoxText3End.Text = Setting.Text3End;
            ReadLines();
            ShowPage();
        }

        private void ReadLines()
        {
            try
            {
                encoding = Encoding.GetEncoding(textBoxEncoding.Text);
                using (var sr = new StreamReader(csvFile, encoding))
                {
                    listViewImport.Items.Clear();
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        listViewImport.Items.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowPage()
        {
            init = true;
            buttonBack.IsEnabled = false;
            buttonNext.IsEnabled = false;
            buttonFinish.IsEnabled = false;
            gridWizard1.Visibility = Visibility.Hidden;
            gridWizard2.Visibility = Visibility.Hidden;
            gridWizard3.Visibility = Visibility.Hidden;
            switch (page)
            {
                case 1:
                    gridWizard1.Visibility = Visibility.Visible;
                    buttonNext.IsEnabled = listViewImport.SelectedItems.Count > 0;
                    break;
                case 2:
                    gridWizard2.Visibility = Visibility.Visible;
                    buttonBack.IsEnabled = true;
                    buttonNext.IsEnabled =
                        IsSelected(comboBoxColumnDate) &&
                        IsSelected(comboBoxColumnText1) &&
                        IsSelected(comboBoxColumnAmount);
                    break;
                case 3:
                    gridWizard3.Visibility = Visibility.Visible;
                    buttonBack.IsEnabled = true;
                    buttonFinish.IsEnabled = true;
                    break;
                default:
                    break;
            }
            init = false;
        }

        private static bool IsSelected(ComboBox cb)
        {
            var ci = cb.SelectedItem as ComboBoxItem;
            return ci != null && !string.IsNullOrEmpty(ci.Content as string);
        }

        private bool InitColumnMapping()
        {
            try
            {
                char delim = textBoxSeparator.Text.Length == 1  ? textBoxSeparator.Text[0] : ',';
                comboBoxColumnDate.Items.Clear();
                comboBoxColumnText1.Items.Clear();
                comboBoxColumnText2.Items.Clear();
                comboBoxColumnText2.Items.Add(string.Empty);
                comboBoxColumnText3.Items.Clear();
                comboBoxColumnText3.Items.Add(string.Empty);
                comboBoxColumnAmount.Items.Clear();
                result.Clear();
                int idx = listViewImport.SelectedIndex;
                if (idx >= 0)
                {
                    string line = listViewImport.Items[idx] as string;
                    if (line != null)
                    {
                        var cols = Split(line, delim);
                        int gridrow = 0;
                        foreach (var str in cols)
                        {
                            var s = string.Format(Properties.Resources.COLUMN_1_2, gridrow + 1, str.Trim());
                            if (s.Length > 40)
                            {
                                s = s.Substring(0, 40) + "...";
                            }
                            comboBoxColumnDate.Items.Add(new ComboBoxItem() { Content = s, Tag = gridrow });
                            comboBoxColumnText1.Items.Add(new ComboBoxItem() { Content = s, Tag = gridrow });
                            comboBoxColumnText2.Items.Add(new ComboBoxItem() { Content = s, Tag = gridrow });
                            comboBoxColumnText3.Items.Add(new ComboBoxItem() { Content = s, Tag = gridrow });
                            comboBoxColumnAmount.Items.Add(new ComboBoxItem() { Content = s, Tag = gridrow });
                            gridrow++;
                        }
                        if (Setting.DateColumn >= 0 && Setting.DateColumn < cols.Length)
                        {
                            comboBoxColumnDate.SelectedIndex = Setting.DateColumn;
                        }
                        if (Setting.Text1Column >= 0 && Setting.Text1Column < cols.Length)
                        {
                            comboBoxColumnText1.SelectedIndex = Setting.Text1Column;
                        }
                        if (Setting.Text2Column >= 0 && Setting.Text2Column < cols.Length)
                        {
                            comboBoxColumnText2.SelectedIndex = Setting.Text2Column;
                        }
                        if (Setting.Text3Column >= 0 && Setting.Text3Column < cols.Length)
                        {
                            comboBoxColumnText3.SelectedIndex = Setting.Text3Column;
                        }
                        if (Setting.AmountColumn >= 0 && Setting.AmountColumn < cols.Length)
                        {
                            comboBoxColumnAmount.SelectedIndex = Setting.AmountColumn;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool InitPreview()
        {
            try
            {
                char delim = textBoxSeparator.Text.Length == 1 ? textBoxSeparator.Text[0] : ',';
                var itemdate = comboBoxColumnDate.SelectedItem as ComboBoxItem;
                var itemtext1 = comboBoxColumnText1.SelectedItem as ComboBoxItem;
                var itemtext2 = comboBoxColumnText2.SelectedItem as ComboBoxItem;
                var itemtext3 = comboBoxColumnText3.SelectedItem as ComboBoxItem;
                var itemamount = comboBoxColumnAmount.SelectedItem as ComboBoxItem;
                if (itemdate != null && itemtext1 != null && itemamount != null)
                {
                    var idxdate = itemdate.Tag as int?;
                    int? idxtext2 = null;
                    if (itemtext2 != null)
                    {
                        idxtext2 = itemtext2.Tag as int?;
                    }
                    int? idxtext3 = null;
                    if (itemtext3 != null)
                    {
                        idxtext3 = itemtext3.Tag as int?;
                    }
                    var idxtext1 = itemtext1.Tag as int?;
                    var idxamount = itemamount.Tag as int?;
                    result.Clear();
                    var ci = CultureInfo.GetCultureInfo(textBoxLanguage.Text);
                    foreach (string str in listViewImport.SelectedItems)
                    {
                        var cols = Split(str, delim);
                        var dt = DateTime.ParseExact(cols[idxdate.Value], textBoxDateFormat.Text, CultureInfo.InvariantCulture);
                        long currency = (long)(Decimal.Parse(cols[idxamount.Value], NumberStyles.Currency, ci) * 100);
                        var b = new ImportBooking()
                        {
                            Date = dt,
                            Amount = currency,
                            AmountString = CurrencyConverter.ConvertToCurrencyString(currency),
                            DateString = $"{dt:d}"
                        };
                        b.Text = Cut(cols[idxtext1.Value].Trim(), textBoxText1Start.Text.Trim(), textBoxText1End.Text.Trim());
                        if (idxtext2.HasValue)
                        {
                            var text2 = Cut(cols[idxtext2.Value].Trim(), textBoxText2Start.Text.Trim(), textBoxText2End.Text.Trim());
                            if (text2.Length > 0)
                            {
                                b.Text += $", {text2}";
                            }
                        }
                        if (idxtext3.HasValue)
                        {
                            var text3 = Cut(cols[idxtext3.Value].Trim(), textBoxText3Start.Text.Trim(), textBoxText3End.Text.Trim());
                            if (text3.Length > 0)
                            {
                                b.Text += $", {text3}";
                            }
                        }
                        result.Add(b);
                    }
                    if (sortDecorator == null)
                    {
                        sortDecorator = new SortDecorator(ListSortDirection.Descending);
                        sortDecorator.Click(gridViewColumHeaderDate);
                        var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listViewPreview.ItemsSource);
                        viewlist.SortDescriptions.Clear();
                        viewlist.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private string Cut(string txt, string startpat, string endpat)
        {
            if (startpat.Length > 0)
            {
                var idx = txt.IndexOf(startpat);
                if (idx >= 0)
                {
                    txt = txt.Substring(idx + startpat.Length).Trim();
                }
            }
            if (endpat.Length > 0)
            {
                var idx = txt.IndexOf(endpat);
                if (idx >= 0)
                {
                    txt = txt.Substring(0, idx).Trim();
                }
            }
            return txt;
        }

        private void Page1SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            page1Changed = true;
            ShowPage();
        }

        private void Page1TextBoxEncoding_LostFocus(object sender, RoutedEventArgs e)
        {
            if (init) return;
            try
            {
                var enc = Encoding.GetEncoding(textBoxEncoding.Text);
                if (enc != encoding)
                {
                    ReadLines();
                    page1Changed = true;
                    ShowPage();
                }
            }
            catch
            {
                // ignored
            }
        }

        private void Page1TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (init) return;
            page1Changed = true;
            ShowPage();
        }

        private void Page2SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init) return;
            page2Changed = true;
            ShowPage();
        }

        private void Page2TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (init) return;
            page2Changed = true;
            ShowPage();
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if (page < 3)
            {
                if (page == 1 && page1Changed)
                {
                    if (!InitColumnMapping())
                    {
                        return;
                    }
                    page1Changed = false;
                }
                if (page == 2 && page2Changed)
                {
                    if (!InitPreview())
                    {
                        return;
                    }
                    page2Changed = false;
                }
                page++;
                ShowPage();
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (page > 1)
            {
                page--;
                ShowPage();
            }
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            Result = new List<ImportBooking>();
            foreach (ImportBooking ib in listViewPreview.Items)
            {
                Result.Add(ib);                
            }
            Result.Sort((ImportBooking c1, ImportBooking c2) => c1.Date.CompareTo(c2.Date) );
            Setting.Encoding = textBoxEncoding.Text;
            Setting.Separator = textBoxSeparator.Text;
            Setting.DateFormat = textBoxDateFormat.Text;
            Setting.CurrencyLanguage = textBoxLanguage.Text;
            Setting.DateColumn = comboBoxColumnDate.SelectedIndex;
            Setting.Text1Column = comboBoxColumnText1.SelectedIndex;
            Setting.Text2Column = comboBoxColumnText2.SelectedIndex;
            Setting.Text3Column = comboBoxColumnText3.SelectedIndex;
            Setting.AmountColumn = comboBoxColumnAmount.SelectedIndex;
            Setting.Text1Start = textBoxText1Start.Text;
            Setting.Text1End = textBoxText1End.Text;
            Setting.Text2Start = textBoxText2Start.Text;
            Setting.Text2End = textBoxText2End.Text;
            Setting.Text3Start = textBoxText3Start.Text;
            Setting.Text3End = textBoxText3End.Text;
            DialogResult = true;
            Close();
        }

        private string[] Split(string line, char delimiter)
        {
            var ret = new List<string>();
            var val = new StringBuilder();
            bool instring = false;
            var sr = new StringReader(line);
            int c = 0;
            while ((c = sr.Read()) >= 0)
            {
                if (c == '\"')
                {
                    instring = !instring;
                    continue;
                }
                if (!instring && c == delimiter)
                {
                    ret.Add(val.ToString());
                    val.Clear();
                    continue;
                }
                if (instring && c == '\\')
                {
                    c = sr.Read();
                    if (c < 0) break;
                }
                else if (instring && c == '\u0080' && encoding == Encoding.GetEncoding("ISO-8859-1"))
                {
                    c = '€';
                }
                val.Append((char)c);
            }
            ret.Add(val.ToString());
            return ret.ToArray();
        }

        private void ListView_ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            var column = (sender as GridViewColumnHeader);
            if (column == null || column.Tag == null || sortDecorator == null) return;
            sortDecorator.Click(column);
            string sortBy = column.Tag.ToString();
            var viewlist = (CollectionView)CollectionViewSource.GetDefaultView(listViewPreview.ItemsSource);
            viewlist.SortDescriptions.Clear();
            viewlist.SortDescriptions.Add(new SortDescription(sortBy, sortDecorator.Direction));
            if (sortBy != "Date")
            {
                viewlist.SortDescriptions.Add(new SortDescription("Date", sortDecorator.Direction));
            }
        }

        public class ImportBooking
        {
            public DateTime Date { get; set; }
            public string Text { get; set; }
            public long Amount { get; set; }
            public string DateString { get; set; }
            public string AmountString { get; set; }
        }
    }
}
