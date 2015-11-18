using System.Windows;
using System.Windows.Input;

namespace HyperSearch.Windows.Common
{
    public partial class PromptWin : Window
    {
        public string Text { get { return txt.Text; } set { txt.Text = value; } }

        public PromptWin()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listView.SelectedIndex = 1;
            listView.Focus();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Get from config
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                this.DialogResult = listView.SelectedIndex == 0;
                this.Close();
            }
            else if (e.Key == Key.Back)
            {
                e.Handled = true;
                this.DialogResult = false;
                this.Close();
            }

        }
    }
}
