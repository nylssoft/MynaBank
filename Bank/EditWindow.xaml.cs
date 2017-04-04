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
    public partial class EditWindow : Window
    {
        public int Year { get; private set; }
        public int Month { get; private set; }
        public int Day { get; private set; }
        public string Text { get; private set; }
        public long Amount { get; private set; }

        public EditWindow(DateTime dt, Booking booking)
        {
            InitializeComponent();
            textBoxMonth.Text = Convert.ToString(dt.Month);
            textBoxYear.Text = Convert.ToString(dt.Year);
            textBoxDay.Text = Convert.ToString(DateTime.Now.Day);
            textBoxAmount.Text = CurrencyConverter.ConvertToInputString(0);
            if (booking != null)
            {
                textBoxMonth.IsEnabled = false;
                textBoxYear.IsEnabled = false;
                textBoxDay.Text = Convert.ToString(booking.Day);
                textBoxText.Text = booking.Text;
                textBoxAmount.Text = CurrencyConverter.ConvertToInputString(booking.Amount);
            }
            textBoxDay.Focus();
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
                MessageBox.Show(ex.Message);
            }
        }

    }
}
