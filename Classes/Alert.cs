using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HyperSearch.Classes
{
    public static class Alert
    {
        public static void ShowExclamation(string msg, Window parent = null, int secondsBeforeWeMayClose = 2)
        {
            var win = new Windows.Common.AlertWin() { SecondsBeforeWeMayClose = secondsBeforeWeMayClose };

            win.Owner = parent;
            win.Text = msg;

            Win.Modal(win, parent);
        }

        public static bool PromptYesNo(string msg, Window parent = null)
        {
            var win = new Windows.Common.PromptWin();
            win.Text = msg;
            win.Owner = parent;

            return Win.Modal(win, parent);
        }
    }
}
