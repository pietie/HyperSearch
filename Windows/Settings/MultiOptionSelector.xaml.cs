using HyperSearch.Classes;
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

        private void win_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            // TODO: use controller settings!
            try
            {
                e.Handled = true;
                var settings = HyperSearchSettings.Instance().Input;
                var elementWithFocus = Keyboard.FocusedElement as UIElement;

                if (settings.Action.Is(e.Key))
                {
                    this.DialogResult = true;
                    this.Close();
                }
                else if (settings.Back.Is(e.Key) || settings.Exit.Is(e.Key))
                {
                    e.Handled = true;
                    this.DialogResult = false;
                    this.Close();
                }
                else if (settings.Down.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                }
                else if (settings.Up.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                }
                else if (settings.Left.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                }
                else if (settings.Right.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                }

            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }
    }
}
