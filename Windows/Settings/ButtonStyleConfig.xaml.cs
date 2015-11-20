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
    /// <summary>
    /// Interaction logic for ButtonStyleConfig.xaml
    /// </summary>
    public partial class ButtonStyleConfig : Window
    {
        public ButtonStyleConfig()
        {
            InitializeComponent();
        }

        public void SetLayoutDefinition(ControllerLayoutDefinition layoutDef)
        {
            var lst = (from b in layoutDef.Buttons
                       orderby b.Position
                       select new ButtonStyleListViewItem()
                       {
                           Button = b
                       }).ToList();


            foreach (var item in lst)
            {
                item.Button.ButtonControl.Tag = item; // this is so backwards :(
                listview.Items.Add(item);
            }

            layoutDef.TopLevelPanel.Focusable = false;
            layoutDef.TopLevelPanel.IsHitTestVisible = false;
            cp.Content = layoutDef.TopLevelPanel;
        }

        private void listview_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var elementWithFocus = Keyboard.FocusedElement as UIElement;
            DependencyObject listViewItem = null;
            ISettingsControl ctrl = null;
            
            if (listview.SelectedIndex >= 0)
            {
                listViewItem = listview.ItemContainerGenerator.ContainerFromIndex(listview.SelectedIndex);
                ctrl = FindISettingsControls(listViewItem).FirstOrDefault();
            }

            if (HyperSearchSettings.Instance().Input.Action.Is(e.Key) || HyperSearchSettings.Instance().Input.Exit.Is(e.Key) || HyperSearchSettings.Instance().Input.Back.Is(e.Key))
            {
                if (ctrl != null)
                {
                    // if the current item is not active and one of the exit keys were pressed
                    if (!ctrl.IsActive && (HyperSearchSettings.Instance().Input.Exit.Is(e.Key) || HyperSearchSettings.Instance().Input.Back.Is(e.Key)))
                    {
                        this.Close();
                        return;
                    }

                    ctrl.IsActive = !ctrl.IsActive;
                }
            }
            else if (HyperSearchSettings.Instance().Input.Up.Is(e.Key) && !ctrl.IsActive)
            {
                elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
            }
            else if (HyperSearchSettings.Instance().Input.Down.Is(e.Key) && !ctrl.IsActive)
            {
                elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
            }

        }

        private void listview_KeyUp(object sender, KeyEventArgs e)
        {
            if (HyperSearchSettings.Instance().Input.Action.Is(e.Key) || HyperSearchSettings.Instance().Input.Exit.Is(e.Key) || HyperSearchSettings.Instance().Input.Back.Is(e.Key))
            {
                if (listview.SelectedItem == null) return;

                var listViewItem = listview.ItemContainerGenerator.ContainerFromIndex(listview.SelectedIndex);

                var ctrl = FindISettingsControls(listViewItem).FirstOrDefault();
                
                if (ctrl != null)
                {
                    // if the current item is not active and one of the exit keys were pressed
                    if (!ctrl.IsActive && (HyperSearchSettings.Instance().Input.Exit.Is(e.Key) || HyperSearchSettings.Instance().Input.Back.Is(e.Key)))
                    {
                        this.Close();
                        return;
                    }

                    ctrl.IsActive = !ctrl.IsActive;
                }
            }
        }

        public static IEnumerable<ISettingsControl> FindISettingsControls(DependencyObject depObj)
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is ISettingsControl)
                    {
                        yield return (ISettingsControl)child;
                    }

                    foreach (ISettingsControl childOfChild in FindISettingsControls(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void hueshift_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                var slider = (Slider)sender;
                var buttonConfig = slider.DataContext as ButtonConfig;

                if (buttonConfig != null)
                {
                    buttonConfig.ButtonControl.Effect = new HscLib.ShaderEffects.HueShiftEffect() { HueShift = slider.Value };
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void win_Loaded(object sender, RoutedEventArgs e)
        {
            if (listview.Items.Count > 0)
            {
                listview.SelectedIndex = 0;
                Util.DelayedFocus(listview);
            }
        }
    }

    public class ButtonStyleListViewItem
    {
        public ButtonConfig Button { get; set; }
    }
    
}
