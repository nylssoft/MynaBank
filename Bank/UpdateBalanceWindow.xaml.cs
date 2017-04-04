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
    public partial class UpdateBalanceWindow : Window
    {
        public long First { get; set; }

        public UpdateBalanceWindow(Balance balance)
        {
            InitializeComponent();
            DateTime dt = new DateTime(balance.Year, balance.Month, 1);
            textBlockInfo.Text = $"Geben Sie den Kontostand für {dt:y} zum Monatsanfang an.";
            textBoxFirst.Text = CurrencyConverter.ConvertToInputString(balance.First);
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
                MessageBox.Show(ex.Message);
            }
        }
    }
}
