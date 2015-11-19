using System;
using System.Windows;
using System.Windows.Input;

namespace HyperSearch.Windows.Common
{
    public partial class CaptureKeyPressWin : Window
    {
        private int SecondsBeforeAutoClose { get; set; }
        public Key? LastKeyPressed { get; private set; }

        public CaptureKeyPressWin()
        {
            InitializeComponent();

            SecondsBeforeAutoClose = 6;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
           
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            this.LastKeyPressed = e.Key;
            this.DialogResult = true;
            timer.Stop();
            timer.Close();
            this.Close();
        }

        System.Timers.Timer timer = new System.Timers.Timer(1000/*1 second interval*/);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                secondsLeftRun.Text = this.SecondsBeforeAutoClose.ToString();

                timer.Elapsed += (ss, ee) =>
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        secondsLeftRun.Text = this.SecondsBeforeAutoClose.ToString();

                        if (this.SecondsBeforeAutoClose <= 0)
                        {
                            timer.Stop();
                            timer.Close();
                            this.DialogResult = false;
                            this.Close();

                            return;
                        }

                        this.SecondsBeforeAutoClose--;
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
