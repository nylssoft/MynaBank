﻿/*
    Myna Bank
    Copyright (C) 2017-2024 Niels Stockfleth

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
using System.Windows.Input;

namespace Bank
{
    public class CustomCommands
    {
        public static readonly RoutedUICommand CreateAccount =
            new RoutedUICommand(
            Properties.Resources.CMD_CREATE_ACCOUNT,
            "CreateAccount",
            typeof(CustomCommands));

        public static readonly RoutedUICommand RenameAccount =
            new RoutedUICommand(
            Properties.Resources.CMD_RENAME_ACCOUNT,
            "RenameAccount",
            typeof(CustomCommands));

        public static readonly RoutedUICommand ImportAccount =
            new RoutedUICommand(
            Properties.Resources.CMD_IMPORT_ACCOUNT,
            "ImportAccount",
            typeof(CustomCommands));

        public static readonly RoutedUICommand DeleteAccount =
            new RoutedUICommand(
            Properties.Resources.CMD_DELETE_ACCOUNT,
            "DeleteAccount",
            typeof(CustomCommands));

        public static readonly RoutedUICommand UpdateBalance =
            new RoutedUICommand(
                Properties.Resources.CMD_UPDATE_BALANCE,
                "UpdateBalance",
                typeof(CustomCommands));

        public static readonly RoutedUICommand DeleteSheet =
            new RoutedUICommand(
            Properties.Resources.CMD_DELETE_SHEET,
            "DeleteSheet",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Exit =
            new RoutedUICommand(
            Properties.Resources.CMD_EXIT,
            "Exit",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) });
 
        public static readonly RoutedUICommand Add =
            new RoutedUICommand(
            Properties.Resources.CMD_ADD,
            "Add",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.I, ModifierKeys.Control) });

        public static readonly RoutedUICommand Remove =
            new RoutedUICommand(
            Properties.Resources.CMD_DELETE,
            "Remove",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.Delete) });

        public static readonly RoutedUICommand Edit =
            new RoutedUICommand(
            Properties.Resources.CMD_EDIT,
            "Edit",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.Enter) });

        public static readonly RoutedUICommand Import =
            new RoutedUICommand(
                Properties.Resources.CMD_IMPORT,
                "Import",
                typeof(CustomCommands));

        public static readonly RoutedUICommand Search =
            new RoutedUICommand(
                Properties.Resources.CMD_SEARCH,
                "Search",
                typeof(CustomCommands),
                new InputGestureCollection() { new KeyGesture(Key.F, ModifierKeys.Control) });

        public static readonly RoutedUICommand About =
            new RoutedUICommand(
            Properties.Resources.CMD_ABOUT,
            "About",
            typeof(CustomCommands));

        public static readonly RoutedUICommand ConfigureDefaultText =
            new RoutedUICommand(
            Properties.Resources.CMD_CONFIGURE_DEFAULT_TEXT,
            "ConfigureDefaultText",
            typeof(CustomCommands));

        public static readonly RoutedUICommand ConfigureDefaultBooking =
            new RoutedUICommand(
            Properties.Resources.CMD_CONFIGURE_DEFAULT_BOOKING,
            "ConfigureDefaultBooking",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Next =
            new RoutedUICommand(
            Properties.Resources.CMD_NEXT,
            "Next",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Previous =
            new RoutedUICommand(
            Properties.Resources.CMD_PREVIOUS,
            "Previous",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Last =
            new RoutedUICommand(
            Properties.Resources.CMD_LAST,
            "Last",
            typeof(CustomCommands));

        public static readonly RoutedUICommand First =
            new RoutedUICommand(
            Properties.Resources.CMD_FIRST,
            "First",
            typeof(CustomCommands));

        public static readonly RoutedUICommand ShowGraph =
            new RoutedUICommand(
                Properties.Resources.CMD_SHOW_GRAPH,
                "ShowGraph",
                typeof(CustomCommands));
    }
}
