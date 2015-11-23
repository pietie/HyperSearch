using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

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
            AttachConsole(-1);
            
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e); 
        }
    }
}
