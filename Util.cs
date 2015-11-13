using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HyperSearch
{
    public class DispatcherTimerContainingAction : System.Windows.Threading.DispatcherTimer
    {// http://blog.bodurov.com/How-to-Create-setTimeout-Function-in-Silverlight/
        public Action Action { get; set; }
    }

    public static class Util
    {
        public static string AbsolutePath(string path)
        {
            Uri uriTester;

            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uriTester))
            {
                throw new Exception("Invalid URI specified: " + path ?? "(null)");
            }
            else if (!uriTester.IsAbsoluteUri)
            {// convert relative Uri to absolute
                return new Uri(new Uri(System.AppDomain.CurrentDomain.BaseDirectory, UriKind.Absolute), uriTester.ToString()).LocalPath;
            }
            else
            {
                return uriTester.LocalPath;
            }
        }

        public static bool EqAny(this Key key, params object[] list)
        {
            if (list == null || list.Length == 0) return false;

            for (int i = 0; i < list.Length;i++)
            {
                if (list[i] is Key)
                {
                    if ((Key)list[i] == key) return true;
                }
                else if (list[i] is HyperSearch.HyperHQSettings.KeyCombo)
                {
                    if (((HyperSearch.HyperHQSettings.KeyCombo)list[i]).Equals(key)) return true;
                }
                //else if (list[i] is HyperSearch.HyperHQSettings.KeyWrapper)
                //{
                //    if (((HyperSearch.HyperHQSettings.KeyWrapper)list[i]).Equals(key)) return true;
                //}

            }

                //


                //return list.Contains(key);
                return false;
        }

        public static void StartProcess(string process, string args, bool log = false)
        {
            if (log)
            {
                MainWindow.LogStatic("Launching: {0}", string.Format("{0} {1}", process, args));
            }

            var hl = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.FileName = process;
            startInfo.Arguments = args;

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            hl.StartInfo = startInfo;
            
            hl.Start();
        }

        public static void SelectAndFocusItem(this ListView lv, int ix = 0, EventHandler callback = null)
        {
            if (lv.Items.Count == 0) return;

            ListViewItem lvi = lv.ItemContainerGenerator.ContainerFromIndex(ix) as ListViewItem;

            if (lvi != null)
            {
                lvi.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lv.SelectedIndex = ix;
                    lv.ScrollIntoView(lv.SelectedItem);
                }), System.Windows.Threading.DispatcherPriority.Render);
                lvi.DelayedFocus(callback);
               
            }
        }
        public static void DelayedFocus(this UIElement uiElement, EventHandler callback = null)
        {
            uiElement.Dispatcher.BeginInvoke(
            new Action(delegate
            {
                uiElement.Focusable = true;
                uiElement.Focus();
                System.Windows.Input.Keyboard.Focus(uiElement);
                if (callback != null) callback(uiElement, null);
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        public static bool IsInDesignMode { get { return System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()); } }
        public static DispatcherTimerContainingAction SetTimeout(int milliseconds, Action func)
        {
            var timer = new DispatcherTimerContainingAction
            {
                Interval = new TimeSpan(0, 0, 0, 0, milliseconds),
                Action = func
            };
            timer.Tick += OnTimeoutTimerTick;
            timer.Start();

            return timer;
        }

        private static void OnTimeoutTimerTick(object sender, EventArgs arg)
        {
            var t = sender as DispatcherTimerContainingAction;
            t.Stop();
            t.Action();
            t.Tick -= OnTimeoutTimerTick;
        }

        // Modified from http://stackoverflow.com/questions/974598/find-all-controls-in-wpf-window-by-type
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in child.FindVisualChildren<T>())
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static bool Like(this string str, string match)
        {
            if (str == null) return false;
            if (match == null) return false;

            return str.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool StartsWithSafe(this string str, string match)
        {
            if (str == null) return false;
            if (match == null) return false;
            return str.StartsWith(match, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsCI(this string s, string eq)
        {
            if (s == null || eq == null) return false;

            return s.Equals(eq, StringComparison.OrdinalIgnoreCase);
        }

        public static int? ToIntNullable(this string s, int? treatAsNullValue = null)
        {
            if (s == null) return null;
            int t;
            if (int.TryParse(s, out t))
            {
                if (treatAsNullValue.HasValue && t == treatAsNullValue.Value) return null;

                return t;
            }

            return null;
        }
    }
}
