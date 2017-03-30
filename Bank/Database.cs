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
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;

namespace Bank
{
    public class Database : IDisposable
    {
        private SQLiteConnection con;

        public class Booking
        {
            public long Id { get; set; }

            public long AccountId { get; set; }

            public DateTime Date { get; set; }

            public string Text { get; set; }

            public long Amount { get; set; }

            public long Balance { get; set; }
        }

        public class Account
        {
            public long Id { get; set; }

            public string Guid { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }
        }

        public void Dispose()
        {
            con?.Dispose();
        }

        public void Open(string filename)
        {
            var sb = new SQLiteConnectionStringBuilder() { DataSource = filename };
            con = new SQLiteConnection(sb.ToString());
            con.Open();
            CreateTables(con);
        }

        private void CreateTables(SQLiteConnection con)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS account " +
                    "(guid TEXT, name TEXT, description TEXT);";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS booking " +
                    "(accountid INTEGER, date INTEGER, text TEXT, amount INTEGER, balance INTEGER);";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS defaulttext "+
                    "(accountid INTEGER, text TEXT);";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS defaultbooking " +
                    "(accountid INTEGER, monthmask INTEGER, day INTEGER, text TEXT, amount INTEGER);";
                cmd.ExecuteNonQuery();
            }
        }

        public List<Account> GetAccounts()
        {
            var ret = new List<Account>();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "SELECT rowid, guid, name, description FROM account";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ret.Add(new Account()
                            {
                                Id = reader.GetInt64(0),
                                Guid = reader.GetString(1),
                                Name = reader.GetString(2),
                                Description = reader.GetString(3)
                            });
                        }
                    }
                }
            }
            return ret;
        }

        public void CreateAccount(Account account)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO account VALUES(@p1,@p2,@p3)";
                cmd.Parameters.Add(new SQLiteParameter("@p1", Guid.NewGuid().ToString()));
                cmd.Parameters.Add(new SQLiteParameter("@p2", account.Name));
                cmd.Parameters.Add(new SQLiteParameter("@p3", account.Description));
                cmd.ExecuteNonQuery();
                account.Id = con.LastInsertRowId;
            }
        }

        public void UpdateAccount(Account account)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "UPDATE account SET name=@p2, description=@p3 WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p2", account.Name));
                cmd.Parameters.Add(new SQLiteParameter("@p3", account.Description));
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteAccount(Account account)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "DELETE FROM account WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                cmd.ExecuteNonQuery();
            }
        }

        public DateTime? GetFirstDate(Account account)
        {
            return GetMinMaxDate(account, true);
        }

        public DateTime? GetLastDate(Account account)
        {
            return GetMinMaxDate(account, false);
        }

        private DateTime? GetMinMaxDate(Account account, bool mindate)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                string op = mindate ? "MIN" : "MAX";
                cmd.CommandText = $"SELECT {op}(date) FROM booking WHERE accountid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        var dto = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(0));
                        return dto.DateTime.ToLocalTime();
                    }
                }
            }
            return null;
        }

        public void AddBooking(Account account, Booking booking)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                var dto = new DateTimeOffset(booking.Date.ToUniversalTime());
                cmd.CommandText = "INSERT INTO booking VALUES(@p1,@p2,@p3,@p4,@p5)";
                cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p2", dto.ToUnixTimeMilliseconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p3", booking.Text));
                cmd.Parameters.Add(new SQLiteParameter("@p4", booking.Amount));
                cmd.Parameters.Add(new SQLiteParameter("@p5", booking.Balance));
                cmd.ExecuteNonQuery();
                booking.Id = con.LastInsertRowId;
            }
        }

        public void UpdateBooking(Booking booking, long oldBalance)
        {
            using (var trans = con.BeginTransaction())
            {
                using (var cmd = new SQLiteCommand(con))
                {
                    var dto = new DateTimeOffset(booking.Date.ToUniversalTime());
                    cmd.CommandText = "UPDATE booking SET date=@p2, text=@p3, amount=@p4, balance=@p5 WHERE rowid=@p1";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", booking.Id));
                    cmd.Parameters.Add(new SQLiteParameter("@p2", dto));
                    cmd.Parameters.Add(new SQLiteParameter("@p3", booking.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@p4", booking.Amount));
                    cmd.Parameters.Add(new SQLiteParameter("@p5", booking.Balance));
                    cmd.ExecuteNonQuery();
                }
                long diff = booking.Balance - oldBalance;
                // @TODO: more complicated if date is changed...
                AdjustBooking(booking.AccountId, booking.Date, diff);
                trans.Commit();
            }
        }

        public void DeleteBooking(Booking booking)
        {
            using (var trans = con.BeginTransaction())
            {
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "DELETE FROM booking WHERE rowid=@p1";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", booking.Id));
                    cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
        }

        private void AdjustBooking(long accountid, DateTime date, long diffBalance)
        {
            if (diffBalance != 0)
            {
                string op = diffBalance > 0 ? $"+{diffBalance}" : $"-{diffBalance}";
                var dto = new DateTimeOffset(date.ToUniversalTime());
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText =
                        $"UPDATE booking SET balance=balance{op}" +
                         " WHERE accountid=@p1 AND date>@p2";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", accountid));
                    cmd.Parameters.Add(new SQLiteParameter("@p2", dto.ToUnixTimeMilliseconds()));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Booking> GetBookings(Account account, DateTime from, DateTime to)
        {
            var ret = new List<Booking>();
            using (var cmd = new SQLiteCommand(con))
            {
                var dtofrom = new DateTimeOffset(from.ToUniversalTime());
                var dtoto = new DateTimeOffset(to.ToUniversalTime());
                cmd.CommandText =
                    "SELECT rowid, date, text, amount, balance FROM booking"+
                    " WHERE accountid=@p1 AND date>=@p2 AND date<@p3 ORDER BY date";
                cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p2", dtofrom.ToUnixTimeMilliseconds()));
                cmd.Parameters.Add(new SQLiteParameter("@p3", dtoto.ToUnixTimeMilliseconds()));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            long rowid = reader.GetInt64(0);
                            var dto = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64(1));
                            var booking = new Booking()
                            {
                                Id = rowid,
                                Date = dto.DateTime.ToLocalTime(),
                                Text = reader.GetString(2),
                                Amount = reader.GetInt64(3),
                                Balance = reader.GetInt64(4)
                            };
                            ret.Add(booking);
                        }
                    }
                }
            }
            return ret;
        }

        public void Migrate(string directory)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(directory);
            string accountname = dirinfo.Name;
            string guid = Guid.NewGuid().ToString();
            long accountid = 0;
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO account VALUES(@p1, @p2, @p3)";
                cmd.Parameters.Add(new SQLiteParameter("@p1", guid));
                cmd.Parameters.Add(new SQLiteParameter("@p2", accountname));
                cmd.Parameters.Add(new SQLiteParameter("@p3", $"Migrated on {DateTime.Now.ToString()}"));
                cmd.ExecuteNonQuery();
                accountid = con.LastInsertRowId;
            }
            foreach (var direntry in Directory.EnumerateFiles(directory))
            {
                DirectoryInfo di = new DirectoryInfo(direntry);
                if (string.Equals(di.Extension, ".txt", StringComparison.InvariantCultureIgnoreCase))
                {
                    MigrateDefaultText(accountid, di.FullName);
                }
                else if (string.Equals(di.Extension, ".dft", StringComparison.InvariantCultureIgnoreCase))
                {
                    MigrateDefaultBooking(accountid, di.FullName);
                }
                else if (string.Equals(di.Extension, ".bhg", StringComparison.InvariantCultureIgnoreCase))
                {
                    int year = Convert.ToInt32(di.Name.Substring(1, 4));
                    int month = Convert.ToInt32(di.Name.Substring(5, 2));
                    MigrateBooking(accountid, di.FullName, year, month);
                }
            }
        }

        private void MigrateDefaultText(long accountid, string txtfile)
        {
            var allines = File.ReadAllLines(txtfile, Encoding.GetEncoding("ISO-8859-1"));
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO defaulttext VALUES(@p1, @p2)";
                foreach (var line in allines)
                {
                    var s = line.Trim();
                    if (s.Length > 0)
                    {
                        cmd.Parameters.Add(new SQLiteParameter("@p1", accountid));
                        cmd.Parameters.Add(new SQLiteParameter("@p2", ExtractString(line, out string rest)));
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                }
            }
        }

        private void MigrateDefaultBooking(long accountid, string dftfile)
        {
            CultureInfo ci = new CultureInfo("en-US");
            var allines = File.ReadAllLines(dftfile, Encoding.GetEncoding("iso-8859-1"));
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO defaultbooking VALUES(@p1, @p2, @p3, @p4, @p5)";
                foreach (var line in allines)
                {
                    int mask = 0;
                    int day = 0;
                    long balance = 0;
                    string txt = ExtractString(line, out string rest);
                    var arr = rest.Split(' ');
                    int idx = 0;
                    foreach (var elem in arr)
                    {
                        string t = elem.Trim();
                        if (elem.Length > 0)
                        {
                            switch (idx)
                            {
                                case 0:
                                    mask = Convert.ToInt32(elem);
                                    break;
                                case 1:
                                    day = Convert.ToInt32(elem);
                                    break;
                                case 2:
                                    balance = (long)(Double.Parse(elem, ci.NumberFormat) * 100.0);
                                    break;
                                default:
                                    break;
                            }
                            idx++;
                        }
                    }
                    cmd.Parameters.Add(new SQLiteParameter("@p1", accountid));
                    cmd.Parameters.Add(new SQLiteParameter("@p2", mask));
                    cmd.Parameters.Add(new SQLiteParameter("@p3", day));
                    cmd.Parameters.Add(new SQLiteParameter("@p4", txt));
                    cmd.Parameters.Add(new SQLiteParameter("@p5", balance));
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }
        }

        private void MigrateBooking(long accountid, string txtfile, int year, int month)
        {
            CultureInfo ci = new CultureInfo("en-US");
            var allines = File.ReadAllLines(txtfile, Encoding.GetEncoding("ISO-8859-1"));
            using (var cmd = new SQLiteCommand(con))
            {
                long money = (long)(Double.Parse(allines[0], ci.NumberFormat) * 100.0);
                cmd.CommandText = "INSERT INTO booking VALUES(@p1, @p2, @p3, @p4, @p5)";
                using (var transaction = con.BeginTransaction())
                {
                    for (int cnt = 2; cnt < allines.Length; cnt++)
                    {
                        int day = 0;
                        long amount = 0;
                        string text = ExtractString(allines[cnt], out string rest);
                        var arr = rest.Split(' ');
                        int idx = 0;
                        foreach (var elem in arr)
                        {
                            string t = elem.Trim();
                            if (elem.Length > 0)
                            {
                                switch (idx)
                                {
                                    case 0:
                                        day = Convert.ToInt32(elem);
                                        break;
                                    case 1:
                                        amount = (long)(Double.Parse(elem, ci.NumberFormat) * 100.0);
                                        break;
                                    default:
                                        break;
                                }
                                idx++;
                            }
                        }
                        DateTime dt = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
                        DateTimeOffset dto = new DateTimeOffset(dt.ToUniversalTime());
                        long datems = dto.ToUnixTimeMilliseconds();
                        money += amount;
                        cmd.Parameters.Add(new SQLiteParameter("@p1", accountid));
                        cmd.Parameters.Add(new SQLiteParameter("@p2", datems));
                        cmd.Parameters.Add(new SQLiteParameter("@p3", text));
                        cmd.Parameters.Add(new SQLiteParameter("@p4", amount));
                        cmd.Parameters.Add(new SQLiteParameter("@p5", money));
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    transaction.Commit();
                }
            }
        }

        private string ExtractString(string line, out string rest)
        {
            var begidx = line.IndexOf('\"');
            var endidx = line.LastIndexOf('\"');
            if (begidx >= 0 && endidx > begidx)
            {
                rest = line.Substring(0, begidx) + line.Substring(endidx + 1);
                return line.Substring(begidx + 1, endidx - begidx - 1);
            }
            rest = line;
            return "";
        }
    }
}
