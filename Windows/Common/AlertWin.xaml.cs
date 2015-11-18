using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HyperSearch.Windows.Common
{
    public partial class AlertWin : Window
    {
        public int SecondsBeforeWeMayClose
        {
            get { return (int)GetValue(SecondsBeforeWeMayCloseProperty); }
            set { SetValue(SecondsBeforeWeMayCloseProperty, value); }
        }

        public static readonly DependencyProperty SecondsBeforeWeMayCloseProperty = DependencyProperty.Register("SecondsBeforeWeMayClose", typeof(int), typeof(AlertWin), new PropertyMetadata(4));

        public AlertWin()
        {
            InitializeComponent();
            this.SecondsBeforeWeMayClose = 2;
        }


        public string Text { get { return txt.Text; } set { txt.Text = value; } }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (SecondsBeforeWeMayClose > 0) return;
            e.Handled = true;
            // TODO: Use config? For now any key press kills the alert window
            this.Close();
        }

        System.Timers.Timer timer = new System.Timers.Timer(1000/*1 second interval*/);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                secondsLeftRun.Text = this.SecondsBeforeWeMayClose.ToString();

                timer.Elapsed += (ss, ee) =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        secondsLeftRun.Text = this.SecondsBeforeWeMayClose.ToString();

                        if (this.SecondsBeforeWeMayClose <= 0)
                        {
                            toClose.Text = "Press any key to close";
                            timer.Stop();
                            return;
                        }

                        this.SecondsBeforeWeMayClose--;
                    }));
                };

                timer.Start();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }
    }
}
