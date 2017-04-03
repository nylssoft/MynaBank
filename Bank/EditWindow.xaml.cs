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
        public int Year { get; set; }
        public int Month { get; set; }

        public Booking Booking { get; set; }

        public EditWindow(DateTime dt)
        {
            InitializeComponent();
            DataContext = this;
            textBoxMonth.Text = Convert.ToString(dt.Month);
            textBoxYear.Text = Convert.ToString(dt.Year);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Year = Convert.ToInt32(textBoxYear.Text);
                Month = Convert.ToInt32(textBoxMonth.Text);
                var b = new Booking()
                {
                    Day = Convert.ToInt32(textBoxDay.Text),
                    Text = textBoxText.Text,
                    Amount = Convert.ToInt64(textBoxAmount.Text)
                };
                Booking = b;
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
