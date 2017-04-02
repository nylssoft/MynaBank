using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank
{
    public class Booking : INotifyPropertyChanged
    {
        private int day;
        private string text;
        private long amount;
        private long currentBalance;

        public long Id { get; set; }

        public Balance Balance { get; set; }

        public int Day
        {
            get
            {
                return day;
            }
            set
            {
                day = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Day"));
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public long Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
            }
        }

        public long CurrentBalance
        {
            get
            {
                return currentBalance;
            }
            set
            {
                currentBalance = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentBalance"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
