using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HyperSearch.Classes
{
    public static class Alert
    {
        public static void ShowExclamation(string msg, Window parent = null)
        {
            var win = new Windows.Common.AlertWin();

            win.Owner = parent;
            win.Text = msg;
            win.ShowDialog();
        }

        public static bool PromptYesNo(string msg, Window parent = null)
        {
            var win = new Windows.Common.PromptWin();
            win.Text = msg;
            win.Owner = parent;
            return win.ShowDialog() ?? false;
        }
    }
}
