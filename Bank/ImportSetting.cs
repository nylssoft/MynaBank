/*
    Myna Bank
    Copyright (C) 2018 Niels Stockfleth

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
namespace Bank
{
    public class ImportSetting
    {
        public long Id { get; set; }

        public Account Account { get; set; }

        public string Encoding { get; set; } = "ISO-8859-1";

        public string Separator { get; set; } = ";";

        public int DateColumn { get; set; } = -1;

        public int Text1Column { get; set; } = -1;

        public int Text2Column { get; set; } = -1;

        public int Text3Column { get; set; } = -1;

        public int AmountColumn { get; set; } = -1;

        public string DateFormat { get; set; } = "dd.MM.yyyy";

        public string CurrencyLanguage { get; set; } = "de";

        public string Text1Start { get; set; } = "";

        public string Text1End { get; set; } = "";

        public string Text2Start { get; set; } = "";

        public string Text2End { get; set; } = "";

        public string Text3Start { get; set; } = "";

        public string Text3End { get; set; } = "";
    }
}
