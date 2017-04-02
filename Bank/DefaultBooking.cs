using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank
{
    public class DefaultBooking
    {
        public long Id { get; set; }

        public Account Account { get; set; }

        public int Monthmask { get; set; }

        public int Day { get; set; }

        public string Text { get; set; }

        public long Amount { get; set; }
    }
}
