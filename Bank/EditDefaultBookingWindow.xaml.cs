using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bank
{
    public partial class EditDefaultBookingWindow : Window
    {
        private CheckBox[] checkBoxes;
        private bool changed = false;

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
                if (changed &&
                    !string.IsNullOrEmpty(textBoxDay.Text) &&
                    !string.IsNullOrEmpty(textBoxText.Text) &&
                    !string.IsNullOrEmpty(textBoxAmount.Text) &&
                    Convert.ToInt32(textBoxDay.Text) <= 28)
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
    }
}
