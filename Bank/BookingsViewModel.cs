using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank
{
    public class BookingsViewModel : INotifyPropertyChanged
    {
        private DateTime firstDate;

        private DateTime lastDate;

        private int filteredCount = 0;

        public List<BookingViewItem> BookingItems { get; set; } = new List<BookingViewItem>();

        public BookingsViewModel()
        {
        }

        public BookingsViewModel(List<Database.Booking> bookings)
        {
            foreach (var booking in bookings)
            {
                BookingItems.Add(new BookingViewItem(booking));
            }
        }

        public int TotalCount
        {
            get
            {
                return BookingItems.Count;
            }
        }

        public DateTime FirstDate
        {
            get
            {
                return firstDate;
            }

            set
            {
                firstDate = value;
                NotifyPropertyChanged("FilteredBookings");
            }
        }

        public DateTime LastDate
        {
            get
            {
                return lastDate;
            }

            set
            {
                lastDate = value;
                NotifyPropertyChanged("FilteredBookings");
            }
        }

        public int FilteredCount
        {
            get
            {
                return filteredCount;
            }
            set
            {
                filteredCount = value;
                NotifyPropertyChanged("FilteredCount");
            }
        }

        public List<BookingViewItem> FilteredBookings
        {
            get
            {
                List<BookingViewItem> ret = new List<BookingViewItem>();
                foreach (var bvi in BookingItems)
                {
                    if (bvi.DateTime >= firstDate && bvi.DateTime <= lastDate)
                    {
                        ret.Add(bvi);
                    }
                }
                FilteredCount = ret.Count;
                ret.Reverse();
                return ret;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
