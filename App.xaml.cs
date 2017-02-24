using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

namespace HyperSearch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        [DllImport("Kernel32.dll")]
        public static extern bool AttachConsole(int processId);

        protected override void OnStartup(StartupEventArgs e)
        {
            //AttachConsole(-1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e); 
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            // TODO: Add a config option for this
            //if (this.Windows != null)
            //{
            //    var win = this.Windows.Cast<Window>().FirstOrDefault(w => w is Windows.GameSearchWindow);

            //    if (win != null)
            //    {
            //        this.Dispatcher.BeginInvoke(
            //               new Action(delegate
            //               {
            //                   win.Activate();
            //                   win.Focus();
            //                   System.Windows.Input.Keyboard.Focus(win);
            //               }), System.Windows.Threading.DispatcherPriority.Render);
            //    }

            //}

        }
    }
}
