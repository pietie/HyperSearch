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
            var positions = layoutDef.ButtonPositionMapping.Keys.OrderBy(k => k);


            var lst = (from p in positions
                       select new ButtonStyleListViewItem()
                       {
                           Button = layoutDef.ButtonPositionMapping[p],
                           Position = p
                       }).ToList();


            foreach (var item in lst) listview.Items.Add(item);

            if (listview.Items.Count > 0) listview.SelectedIndex = 0;

            listview.Focus();
           
            cp.Content = layoutDef.TopLevelPanel;
        }

        private void listview_PreviewKeyDown(object sender, KeyEventArgs e)
        {
//?            e.Handled = true;

            if (Global.BackKey.Is(e.Key))
            {
                this.Close();
            }


        }

        private void listview_KeyUp(object sender, KeyEventArgs e)
        {
            if (Global.ActionKey.Is(e.Key))
            {
                if (listview.SelectedItem == null) return;

                var listViewItem = listview.ItemContainerGenerator.ContainerFromIndex(listview.SelectedIndex);

                var ctrl = FindISettingsControls(listViewItem).FirstOrDefault();
                
                if (ctrl != null)
                {
                    ctrl.IsActive = !ctrl.IsActive;
                }
                
                //SettingsListViewItem item = (SettingsListViewItem)listView.SelectedItem;
                //SettingTypeAttribute attrib = null;


                //attrib = item.SettingType;
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
                var ctrl = slider.Tag as Control;

                if (ctrl != null) ctrl.Effect = new HscLib.ShaderEffects.HueShiftEffect() { HueShift = slider.Value };
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void win_Loaded(object sender, RoutedEventArgs e)
        {
            listview.Focus();
        }
    }

    public class ButtonStyleListViewItem
    {
        public Control Button { get; set; }
        public int Position { get; set; }
    }
    
}
