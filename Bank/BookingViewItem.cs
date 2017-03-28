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
using System.Globalization;

namespace Bank
{
    public class BookingViewItem : INotifyPropertyChanged
    {
        public BookingViewItem(Database.Booking booking)
        {
            Booking = booking;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Database.Booking booking;

	    public Database.Booking Booking
        {
            get
            {
                return booking;
            }
            set
            {
                booking = value;
                Date = booking.Date.ToShortDateString();
                Text = booking.Text;
                // @TODO: use current culture? number format configurable?
                Amount = booking.Amount.ToString("c", euroCulture);
                Balance = booking.Amount.ToString("c", euroCulture);
            }
        }

        private static CultureInfo euroCulture = new CultureInfo("de-DE");

        public string Date { get; private set; }

        public string Text { get; private set; }

        public string Amount { get; private set; }

        public string Balance { get; private set; }
    }
}
