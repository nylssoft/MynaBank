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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank
{
    public class Account : IDisposable
    {
        private SQLiteConnection con;

        public void Dispose()
        {
            con?.Dispose();
        }

        public void Open(string filename)
        {
            var sb = new SQLiteConnectionStringBuilder();
            sb.DataSource = filename;
            con = new SQLiteConnection(sb.ToString());
            con.Open();
        }

        public void CreateTables()
        {
            using (var cmd = new SQLiteCommand(con))
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS test (name TEXT);";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
