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
using System.ComponentModel;

namespace Bank
{
    public class DefaultBooking : INotifyPropertyChanged
    {
        private int day;
        private string text;
        private long amount;
        private int monthmask;

        public long Id { get; set; }

        public Account Account { get; set; }

        public int Monthmask
        {
            get
            {
                return monthmask;
            }
            set
            {
                monthmask = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Monthmask"));
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
