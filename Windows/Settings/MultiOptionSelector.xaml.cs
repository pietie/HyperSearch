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

namespace HyperSearch.Windows.Settings
{
    public partial class MultiOptionSelector : Window
    {
        public MultiOptionSelector()
        {
            InitializeComponent();
        }

        public MultiOptionSelector(DataTemplate customListViewItemContentTemplate)
        {
            InitializeComponent();

            listview.ItemContainerStyle.Setters.Add(new Setter(ContentTemplateProperty, customListViewItemContentTemplate));
        }

        private void listview_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void listview_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            // TODO: use controller settings!
            try
            {
                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    this.DialogResult = true;
                    this.Close();
                }
                else if (e.Key == Key.Back)
                {
                    e.Handled = true;
                    this.DialogResult = false;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }

        }

        public object SelectedItem
        {
            get { return listview.SelectedItem; }
        }

        public void SetItemSource(object[] collection)
        {
            listview.ItemsSource = collection;

            if (listview.Items.Count > 0) listview.SelectedIndex = 0;
        }

        private void win_Loaded(object sender, RoutedEventArgs e)
        {
            listview.Focus();
        }


    }
}
