using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static readonly RoutedUICommand About =
            new RoutedUICommand(
            Properties.Resources.CMD_ABOUT,
            "About",
            typeof(CustomCommands));

        public static readonly RoutedUICommand ShowSettings =
            new RoutedUICommand(
            Properties.Resources.CMD_SETTINGS,
            "ShowSettings",
            typeof(CustomCommands));
    }
}
