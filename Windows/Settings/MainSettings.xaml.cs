using HyperSearch.Classes;
using HyperSearch.Windows.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HyperSearch.Windows.Settings
{
    /// <summary>
    /// Interaction logic for MainSettings.xaml
    /// </summary>
    public partial class MainSettings : Window
    {
        public static bool IsSettingsOpen = false;

        public MainSettings()
        {
            InitializeComponent();
        }

        // TODO: one day when I'm big and less lazy I'll create a proper typed class for this thing
        private Dictionary<string, List<SectionWithSettingsEntry>> _sectionsWithSettingsDict;

        class SectionWithSettingsEntry
        {
            public PropertyInfo Property { get; set; }
            public object ParentObject { get; set; }
            public SettingSectionAttribute Section { get; set; }
            public SettingTypeAttribute SettingsType { get; set; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IsSettingsOpen = true;

                var settings = HyperSearchSettings.Instance();
                

                var highLevelProperties = from p in settings.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                          select new
                                          {
                                              Property = p,
                                              Children = (from c in p.GetGetMethod().ReturnType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                                          select new
                                                          {
                                                              Property = c,
                                                              SettingsTypeAttribute = c.GetCustomAttributes(typeof(SettingTypeAttribute), false).FirstOrDefault() as SettingTypeAttribute,
                                                              SectionAttribute = c.GetCustomAttributes(typeof(SettingSectionAttribute), false).FirstOrDefault() as SettingSectionAttribute,
                                                              Parent = p
                                                          }),
                                              SectionAttribute = p.GetGetMethod().ReturnType.GetCustomAttributes(typeof(SettingSectionAttribute), false).FirstOrDefault() as SettingSectionAttribute
                                          };

                _sectionsWithSettingsDict = ((from setting in
                                                  (from h in highLevelProperties
                                                   from c in h.Children
                                                   where c.Parent == h.Property && h.SectionAttribute != null
                                                   select new SectionWithSettingsEntry()
                                                   {
                                                       Property = c.Property,
                                                       ParentObject = c.Parent.GetGetMethod().Invoke(settings, null),
                                                       Section = c.SectionAttribute == null ? h.SectionAttribute : c.SectionAttribute, // inherit if necessary
                                                       SettingsType = c.SettingsTypeAttribute
                                                   })
                                              group setting by setting.Section.Name into grp
                                              select grp).ToDictionary(g => g.Key, g => g.ToList()));

                sectionListView.ItemsSource = _sectionsWithSettingsDict.Keys;
                sectionListView.SelectedIndex = 0;
                sectionListView.Focus();

                listView.Opacity = 0.7;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IsSettingsOpen = false;
            HyperSearchSettings.Instance().Save();
        }

        private void listView_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // TODO: Map to key config!


                if (e.Key == Key.Enter)
                {
                    if (listView.SelectedItem == null) return;

                    var listViewItem = listView.ItemContainerGenerator.ContainerFromIndex(listView.SelectedIndex);

                    var ctrl = FindISettingsControls(listViewItem).FirstOrDefault();

                    SettingsListViewItem item = (SettingsListViewItem)listView.SelectedItem;
                    SettingTypeAttribute attrib = null;


                    attrib = item.SettingType;
                    //if (item.Property != null)
                    //{
                    //    attrib = item.Property.GetCustomAttribute(typeof(SettingTypeAttribute)) as SettingTypeAttribute;
                    //}

                    if (ctrl != null)
                    {
                        if (ctrl is TrueFalse)
                        {
                            ((TrueFalse)ctrl).IsChecked = !((TrueFalse)ctrl).IsChecked;
                        }
                        else if (ctrl is MultiOption)
                        {
                            if (attrib != null)
                            {
                                if (attrib.Type == SettingsType.FolderPath)
                                {
                                    var dlg = new System.Windows.Forms.FolderBrowserDialog();

                                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                    {
                                        item.Value = dlg.SelectedPath;
                                    }

                                }
                                else if (attrib.Type == SettingsType.FilePath)
                                {
                                    var dlg = new System.Windows.Forms.OpenFileDialog();

                                    dlg.Multiselect = false;
                                    dlg.Filter = "RocketLauncher|Rocketlauncher.exe|All filers (*.*)|*.*";

                                    ///ERROR: System.ArgumentException: Filter string you provided is not valid.The filter string must contain a description of the filter, followed by the vertical bar (|)
                                    //and the filter pattern.The strings for different filtering options must also be separated by the vertical bar.Example: "Text files (*.txt)|*.txt|All files (*.*)|*.*"
                                    //at System.Windows.Forms.FileDialog.set_Filter(String value)

                                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                    {
                                        item.Value = dlg.FileName;
                                    }
                                }
                                else if (attrib.ActionType != SettingActionType.None)
                                {
                                    HandleActionType(attrib);
                                }
                                else // default
                                {
                                    if (!string.IsNullOrEmpty(attrib.MutliValueCsv))
                                    {
                                        var items = attrib.MutliValueCsv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                                        MultiOptionSelector win = new MultiOptionSelector();

                                        win.SetItemSource(items);

                                        if (Win.Modal(win, this))
                                        {
                                            if (win.SelectedItem != null)
                                            {
                                                item.Value = win.SelectedItem.ToString();
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            ctrl.IsActive = !ctrl.IsActive;
                        }
                    }
                    else
                    {
                        if (attrib.Type == SettingsType.Action)
                        {
                            HandleActionType(attrib);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void HandleActionType(SettingTypeAttribute attrib)
        {
            SettingsListViewItem item = (SettingsListViewItem)listView.SelectedItem;
        
            if (attrib.ActionType == SettingActionType.ConfigureButtonLayout)
            {
                var controllerLayoutWin = new ControllerLayoutWin();

                if (Win.Modal(controllerLayoutWin, this))
                {
                    //controllerLayoutWin.SelectedLayoutDefinition
                }
            }
            else if (attrib.ActionType == SettingActionType.SetKeyMultiple)
            {
                var win = new CaptureKeyPressWin();

                if (Win.Modal(win, this))
                {
                    var keyList = item.Property.GetValue(item.ParentObject, null) as KeyList;

                    if (win.LastKeyPressed.HasValue)
                    {
                        if (keyList == null)
                        {
                            keyList = new KeyList();
                            item.Property.SetValue(item.ParentObject, keyList, null);
                        }

                        keyList.Add(win.LastKeyPressed.Value);

                        item.Value = keyList.ToString();
                        //RefreshSettingsListView();
                    }
                }
            }
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        { // TODO: hookup with Controller Settings
          // TODO: Need a way of looking for 'Up' command for example across all controller types!!!! ! ! ! ! !! 11 !!!



        }
        private void sectionListView_PreviewKeyUp(object sender, KeyEventArgs e)
        {

        }

        private void listView_PreviewKeyUp(object sender, KeyEventArgs e)
        {

        }

        private void sectionListView_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void sectionListView_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void listView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Up)
                {
                    if (listView.SelectedIndex == 0)
                    {
                        var listViewItem = listView.ItemContainerGenerator.ContainerFromIndex(listView.SelectedIndex);

                        var ctrl = FindISettingsControls(listViewItem).FirstOrDefault();

                        // prevent moving focus up to section listview if the first item is a slider in Active mode
                        if (ctrl != null && ctrl is Slider && ctrl.IsActive)
                        {
                            return;
                        }

                        listView.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }


        private void sectionListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Down)
                {
                    if (listView.Items.Count > 0)
                    {
                        sectionListView.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        listView.SelectedIndex = 0;
                        e.Handled = true;
                    }
                }


            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
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

        private void sectionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                RefreshSettingsListView();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void RefreshSettingsListView()
        {
            string sectionKey = (string)sectionListView.SelectedItem;

            var settingItems = (from p in _sectionsWithSettingsDict[sectionKey]
                                where p.SettingsType != null && p.SettingsType.Type != SettingsType.Ignore
                                select new SettingsListViewItem()
                                {
                                    Property = p.Property,
                                    Value = p.Property != null ? p.Property.GetValue(p.ParentObject, null).ToStringSafe() : (string)null,
                                    SettingType = p.SettingsType,
                                    ParentObject = p.ParentObject
                                }).ToList();

            listView.ItemsSource = settingItems;
       }

        private void listView_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void listView_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void listView_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            listView.Opacity = 0.7;
        }

        private void listView_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            listView.Opacity = 1.0;
        }

        private void sectionListView_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            sectionListView.Opacity = 0.7;
        }

        private void sectionListView_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            sectionListView.Opacity = 1.0;
        }

      
    }
}
