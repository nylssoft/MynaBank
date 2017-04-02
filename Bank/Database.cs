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

        public void Dispose()
        {

            con?.Dispose();
        }

        public void Open(string filename)
        {
            var sb = new SQLiteConnectionStringBuilder() { DataSource = filename };
            con = new SQLiteConnection(sb.ToString());
            con.Open();
            Init(con);
        }

        private void Init(SQLiteConnection con)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS account " +
                    "(name TEXT);";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS balance " +
                    "(accountid INTEGER, month INTEGER, year INTEGER, first INTEGER, last INTEGER);";
                cmd.ExecuteNonQuery();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS booking " +
                    "(balanceid INTEGER, day INTEGER, text TEXT, amount INTEGER);";
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
                cmd.CommandText = "SELECT rowid, name FROM account";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ret.Add(new Account() { Id = reader.GetInt64(0), Name = reader.GetString(1) });
                        }
                    }
                }
            }
            return ret;
        }

        public Account CreateAccount(string name)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO account VALUES(@p1)";
                cmd.Parameters.Add(new SQLiteParameter("@p1", name));
                cmd.ExecuteNonQuery();
                return new Account { Id = con.LastInsertRowId, Name = name };
            }
        }

        public void UpdateAccount(Account account)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "UPDATE account SET name=@p2 WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p2", account.Name));
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteAccount(Account account)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                using (var trans = con.BeginTransaction())
                {
                    cmd.CommandText = "DELETE FROM booking WHERE balanceid=@p1";
                    foreach (var balance in GetBalances(account))
                    {
                        cmd.Parameters.Add(new SQLiteParameter("@p1", balance.Id));
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    cmd.CommandText = "DELETE FROM balance WHERE accountid=@p1";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    cmd.CommandText = "DELETE FROM defaultbooking WHERE accountid=@p1";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    cmd.CommandText = "DELETE FROM defaulttext WHERE accountid=@p1";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    cmd.CommandText = "DELETE FROM account WHERE rowid=@p1";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
        }

        public Balance CreateBalance(Account account, int month, int year, long first, long last)
        {
            var defaultbookings = GetDefaultBookings(account);
            using (var trans = con.BeginTransaction())
            {
                Balance ret = null;
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "INSERT INTO balance VALUES(@p1,@p2,@p3,@p4,@p5)";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                    cmd.Parameters.Add(new SQLiteParameter("@p2", month));
                    cmd.Parameters.Add(new SQLiteParameter("@p3", year));
                    cmd.Parameters.Add(new SQLiteParameter("@p4", first));
                    cmd.Parameters.Add(new SQLiteParameter("@p5", last));
                    cmd.ExecuteNonQuery();
                    ret = new Balance()
                    {
                        Id = con.LastInsertRowId,
                        Account = account,
                        Month = month,
                        Year = year,
                        First = first,
                        Last = last
                    };
                    foreach (var defaultbooking in defaultbookings)
                    {
                        // @TODO: month mask...
                        CreateBooking(ret, defaultbooking.Day, defaultbooking.Text, defaultbooking.Amount);
                    }
                }
                trans.Commit();
                return ret;
            }
        }

        public List<Balance> GetBalances(Account account)
        {
            var ret = new List<Balance>();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText =
                    "SELECT rowid, month, year, first, last FROM balance"+
                    " WHERE accountid=@p1 ORDER BY year, month ASC";
                cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ret.Add(new Balance()
                            {
                                Id = reader.GetInt64(0),
                                Account = account,
                                Month = reader.GetInt32(1),
                                Year = reader.GetInt32(2),
                                First = reader.GetInt64(3),
                                Last = reader.GetInt64(4)
                            });
                        }
                    }
                }
            }
            return ret;
        }

        public Balance GetBalanceById(long rowid)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "SELECT accountid, month, year, first, last FROM balance WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", rowid));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return new Balance()
                        {
                            Id = rowid,
                            Account = new Account() { Id = reader.GetInt64(0) },
                            Month = reader.GetInt32(1),
                            Year = reader.GetInt32(2),
                            First = reader.GetInt64(3),
                            Last = reader.GetInt64(4)
                        };
                    }
                }
                return null;
            }
        }

        public void UpdateBalance(Balance balance)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "UPDATE balance SET first=@p2, last=@p3 WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", balance.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p2", balance.First));
                cmd.Parameters.Add(new SQLiteParameter("@p3", balance.Last));
                cmd.ExecuteNonQuery();
            }
        }

        public Booking CreateBooking(Balance balance, int day, string text, long amount)
        {
            using (var trans = con.BeginTransaction())
            {
                Booking ret = new Booking();
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "INSERT INTO booking VALUES(@p1,@p2,@p3,@p4)";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", balance.Id));
                    cmd.Parameters.Add(new SQLiteParameter("@p2", day));
                    cmd.Parameters.Add(new SQLiteParameter("@p3", text));
                    cmd.Parameters.Add(new SQLiteParameter("@p4", amount));
                    cmd.ExecuteNonQuery();
                    ret.Id = con.LastInsertRowId;
                    balance.Last += amount;
                    UpdateBalance(balance);
                    ret.Balance = balance;
                    ret.Day = day;
                    ret.Text = text;
                    ret.Amount = amount;
                }
                trans.Commit();
                return ret;
            }
        }

        private Booking GetBookingById(long rowid)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "SELECT balanceid, day, text, amount FROM booking WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", rowid));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return new Booking()
                        {
                            Id = rowid,
                            Balance = new Balance() { Id = reader.GetInt64(0) },
                            Day = reader.GetInt32(1),
                            Text = reader.GetString(2),
                            Amount = reader.GetInt64(3)
                        };
                    }
                }
            }
            return null;
        }

        public void UpdateBooking(Booking booking)
        {
            using (var trans = con.BeginTransaction())
            {
                var oldBooking = GetBookingById(booking.Id);
                using (var cmd = new SQLiteCommand(con))
                {
                    cmd.CommandText = "UPDATE booking SET day=@p2, text=@p3, amount=@p4 WHERE rowid=@p1";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", booking.Id));
                    cmd.Parameters.Add(new SQLiteParameter("@p2", booking.Day));
                    cmd.Parameters.Add(new SQLiteParameter("@p3", booking.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@p4", booking.Amount));
                    cmd.ExecuteNonQuery();
                }
                if (oldBooking.Amount != booking.Amount)
                {                    
                    booking.Balance.Last += (booking.Amount - oldBooking.Amount);
                    UpdateBalance(booking.Balance);
                }
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
                var balance = booking.Balance;
                balance.Last -= booking.Amount;
                UpdateBalance(balance);
                trans.Commit();
            }
        }

        public List<Booking> GetBookings(Balance balance)
        {
            var ret = new List<Booking>();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText =
                    "SELECT rowid, day, text, amount FROM booking"+
                    " WHERE balanceid=@p1 ORDER BY day ASC";
                cmd.Parameters.Add(new SQLiteParameter("@p1", balance.Id));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var booking = new Booking()
                            {
                                Id = reader.GetInt64(0),
                                Balance = balance,
                                Day = reader.GetInt32(1),
                                Text = reader.GetString(2),
                                Amount = reader.GetInt64(3)
                            };
                            ret.Add(booking);
                        }
                    }
                }
            }
            return ret;
        }

        public void InsertDefaultBooking(DefaultBooking defaultBooking)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO defaultbooking VALUES(@p1,@p2,@p3,@p4,@p5)";
                cmd.Parameters.Add(new SQLiteParameter("@p1", defaultBooking.Account.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p2", defaultBooking.Monthmask));
                cmd.Parameters.Add(new SQLiteParameter("@p3", defaultBooking.Day));
                cmd.Parameters.Add(new SQLiteParameter("@p4", defaultBooking.Text));
                cmd.Parameters.Add(new SQLiteParameter("@p5", defaultBooking.Amount));
                cmd.ExecuteNonQuery();
                defaultBooking.Id = con.LastInsertRowId;
            }
        }

        public void UpdateDefaultBooking(DefaultBooking defaultBooking)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "UPDATE defaultbooking SET monthmask=@p2,day=@p3,text=@p4,amount=@p5 WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", defaultBooking.Id));
                cmd.Parameters.Add(new SQLiteParameter("@p2", defaultBooking.Monthmask));
                cmd.Parameters.Add(new SQLiteParameter("@p3", defaultBooking.Day));
                cmd.Parameters.Add(new SQLiteParameter("@p4", defaultBooking.Text));
                cmd.Parameters.Add(new SQLiteParameter("@p5", defaultBooking.Amount));
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteDefaultBooking(DefaultBooking defaultBooking)
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "DELETE FROM defaultbooking WHERE rowid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", defaultBooking.Id));
                cmd.ExecuteNonQuery();
            }
        }

        public List<DefaultBooking> GetDefaultBookings(Account account)
        {
            var ret = new List<DefaultBooking>();
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "SELECT monthmask, day, text, amount FROM defaultbooking WHERE accountid=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", account.Id));
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ret.Add(new DefaultBooking() {
                                Account = account,
                                Monthmask = reader.GetInt32(0),
                                Day = reader.GetInt32(1),
                                Text = reader.GetString(2),
                                Amount = reader.GetInt64(3)
                            });
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
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "SELECT COUNT(*) FROM account WHERE name=@p1";
                cmd.Parameters.Add(new SQLiteParameter("@p1", accountname));
                var obj = cmd.ExecuteScalar();
                if (obj is long && ((long)obj) > 0)
                {
                    return;
                }
            }
            string guid = Guid.NewGuid().ToString();
            long accountid = 0;
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "INSERT INTO account VALUES(@p1)";
                cmd.Parameters.Add(new SQLiteParameter("@p1", accountname));
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
                using (var transaction = con.BeginTransaction())
                {
                    long first = (long)(Double.Parse(allines[0], ci.NumberFormat) * 100.0);
                    long last = (long)(Double.Parse(allines[1], ci.NumberFormat) * 100.0);
                    cmd.CommandText = "INSERT INTO balance VALUES(@p1,@p2,@p3,@p4,@p5)";
                    cmd.Parameters.Add(new SQLiteParameter("@p1", accountid));
                    cmd.Parameters.Add(new SQLiteParameter("@p2", month));
                    cmd.Parameters.Add(new SQLiteParameter("@p3", year));
                    cmd.Parameters.Add(new SQLiteParameter("@p4", first));
                    cmd.Parameters.Add(new SQLiteParameter("@p5", last));
                    cmd.ExecuteNonQuery();
                    var balanceid = con.LastInsertRowId;
                    cmd.Parameters.Clear();
                    cmd.CommandText = "INSERT INTO booking VALUES(@p1, @p2, @p3, @p4)";
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
                        if (year <= 2001)
                        {
                            double am = Math.Round((amount / 100.0) * 0.511292 * 100.0);
                            amount = Convert.ToInt64(am);
                        }
                        cmd.Parameters.Add(new SQLiteParameter("@p1", balanceid));
                        cmd.Parameters.Add(new SQLiteParameter("@p2", day));
                        cmd.Parameters.Add(new SQLiteParameter("@p3", text));
                        cmd.Parameters.Add(new SQLiteParameter("@p4", amount));
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
