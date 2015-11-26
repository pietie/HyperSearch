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

        public class SectionWithSettingsEntry
        {
            public PropertyInfo Property { get; set; }
            public object ParentObject { get; set; }
            public SettingSectionAttribute Section { get; set; }
            public SettingTypeAttribute SettingsType { get; set; }
        }

        private IEnumerable<dynamic> FindAllChildSettings(Type t, object parentObj)
        {
            var props = t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).ToList();

            foreach (var p in props)
            {
                yield return new { Property = p, Parent = parentObj };

                if (!p.IsDefined(typeof(SettingCompositeAttribute), false)) continue;

                var rt = p.GetGetMethod().ReturnType;

                var parentObjChild = p.GetValue(parentObj, null);

                foreach (var c in FindAllChildSettings(rt, parentObjChild))
                {
                    yield return new { Property = c.Property, Parent = parentObjChild };
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IsSettingsOpen = true;

                var settings = HyperSearchSettings.Instance();


                var highLevelSections = (from p in typeof(HyperSearchSettings).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                         select new
                                         {
                                             Property = p,
                                             SectionAttribute = p.GetGetMethod().ReturnType.GetCustomAttributes(typeof(SettingSectionAttribute), false).FirstOrDefault() as SettingSectionAttribute
                                         }).Where(h => h.SectionAttribute != null).ToList();

                var allSettings = new List<SectionWithSettingsEntry>();

                foreach (var section in highLevelSections)
                {
                    var propVal = section.Property.GetValue(settings, null);
                    var sectionSettings = FindAllChildSettings(section.Property.GetGetMethod().ReturnType, propVal).Where(c => (c.Property as PropertyInfo).IsDefined(typeof(SettingTypeAttribute), false));

                    allSettings.AddRange(from s in sectionSettings
                                         select new SectionWithSettingsEntry()
                                         {
                                             Property = s.Property,
                                             ParentObject = s.Parent,
                                             Section = section.SectionAttribute,
                                             SettingsType = (s.Property as PropertyInfo).GetCustomAttributes(typeof(SettingTypeAttribute), false).FirstOrDefault() as SettingTypeAttribute
                                         });
                }


                _sectionsWithSettingsDict = (
                      from setting in allSettings
                      group setting by setting.Section.Name into grp
                      select grp
                      ).ToDictionary(g => g.Key, g => g.ToList());

                //var highLevelProperties = from p in settings.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                //                          select new
                //                          {
                //                              Property = p,
                //                              Children = (from c in p.GetGetMethod().ReturnType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                //                                          select new
                //                                          {
                //                                              Property = c,
                //                                              SettingsTypeAttribute = c.GetCustomAttributes(typeof(SettingTypeAttribute), false).FirstOrDefault() as SettingTypeAttribute,
                //                                              SectionAttribute = c.GetCustomAttributes(typeof(SettingSectionAttribute), false).FirstOrDefault() as SettingSectionAttribute,
                //                                              Parent = p
                //                                          }),
                //                              SectionAttribute = p.GetGetMethod().ReturnType.GetCustomAttributes(typeof(SettingSectionAttribute), false).FirstOrDefault() as SettingSectionAttribute
                //                          };


                //_sectionsWithSettingsDict = ((from setting in
                //                                  (from h in highLevelProperties
                //                                   from c in h.Children
                //                                   where c.Parent == h.Property && h.SectionAttribute != null
                //                                   select new SectionWithSettingsEntry()
                //                                   {
                //                                       Property = c.Property,
                //                                       ParentObject = c.Parent.GetGetMethod().Invoke(settings, null),
                //                                       Section = c.SectionAttribute == null ? h.SectionAttribute : c.SectionAttribute, // inherit if necessary
                //                                       SettingsType = c.SettingsTypeAttribute
                //                                   })
                //                              group setting by setting.Section.Name into grp
                //                              select grp).ToDictionary(g => g.Key, g => g.ToList()));

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

            if (HyperSearchSettings.Instance().Input.Action == null || HyperSearchSettings.Instance().Input.Action.Keys.Count == 0)
            {
                Alert.ShowExclamation("No Action key configured, defaulting to ENTER.", App.Current.MainWindow);

                HyperSearchSettings.Instance().Input.Action = new KeyList(Key.Enter);
            }

            HyperSearchSettings.Instance().Save();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = true;

                ListView focussedLV = null;

                if (Keyboard.FocusedElement is ListViewItem) focussedLV = ((FrameworkElement)Keyboard.FocusedElement).GetAncestorOfType<ListView>();

                var settings = HyperSearchSettings.Instance().Input;
                var elementWithFocus = Keyboard.FocusedElement as UIElement;

                DependencyObject listViewItem = null;

                if (listView.SelectedIndex >= 0)
                    listViewItem = listView.ItemContainerGenerator.ContainerFromIndex(listView.SelectedIndex);

                var ctrl = FindISettingsControls(listViewItem).FirstOrDefault();

                SettingsListViewItem item = listView.SelectedItem as SettingsListViewItem;
                SettingTypeAttribute attrib = null;

                if (item != null) attrib = item.SettingType;

                var actionKey = new KeyList(Key.Enter);

                if (settings.Action != null && settings.Action.Keys.Count > 0) actionKey = settings.Action;


                if (actionKey.Is(e.Key) && attrib != null)
                {
                    if (ctrl != null)
                    {
                        if (ctrl is TrueFalse)
                        {
                            ((TrueFalse)ctrl).IsChecked = !((TrueFalse)ctrl).IsChecked;
                            item.Property.SetValue(item.ParentObject, ((TrueFalse)ctrl).IsChecked, null);
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
                                    if (attrib.EnumSource != null)
                                    {
                                        //var items = attrib.MutliValueCsv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        var items = Enum.GetNames(attrib.EnumSource);

                                        MultiOptionSelector win = new MultiOptionSelector();

                                        win.SetItemSource(items);

                                        if (Win.Modal(win, this))
                                        {
                                            if (win.SelectedItem != null)
                                            {
                                                var o = (TextInputType)Enum.Parse(attrib.EnumSource, win.SelectedItem.ToString());

                                                if (o == TextInputType.Orb)
                                                {
                                                    Alert.ShowExclamation("Orb type not yet supported", secondsBeforeWeMayClose:0);
                                                    return;
                                                }

                                                item.Value = win.SelectedItem.ToString();
                                                item.Property.SetValue(item.ParentObject, o, null);
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
                else if (settings.Clear.Is(e.Key))
                {
                    if (ctrl == null) return;

                    ctrl.ResetValue();
                    item.Property.SetValue(item.ParentObject, null, null);
                }
                else if (settings.Exit.Is(e.Key))
                {
                    this.Close();
                }
                else if (settings.Down.Is(e.Key))
                {
                    if (focussedLV == sectionListView)
                    {
                        listView.SelectAndFocusItem();
                    }
                    else
                    {
                        if (listView.SelectedIndex >= listView.Items.Count - 1)
                        {
                            listView.SelectedIndex = 0;
                            listView.ScrollIntoView(listView.SelectedItem);
                            listView.SelectAndFocusItem(0);
                        }
                        else
                        {
                            elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        }
                    }
                }
                else if (settings.Up.Is(e.Key))
                {
                    if (listView.SelectedIndex == 0)
                    {
                        listView.SelectedIndex = listView.Items.Count - 1;
                        listView.ScrollIntoView(listView.SelectedItem);
                        listView.SelectAndFocusItem(listView.Items.Count - 1);
                    }
                    else
                    {
                        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    }
                }
                else if (settings.Left.Is(e.Key))
                {
                    if (sectionListView.SelectedIndex >= 1)
                    {
                        sectionListView.SelectedIndex = sectionListView.SelectedIndex - 1;
                        sectionListView.ScrollIntoView(sectionListView.SelectedItem);
                    }
                    else
                    {
                        sectionListView.SelectAndFocusItem(sectionListView.Items.Count - 1);
                    }
                }
                else if (settings.Right.Is(e.Key))
                {
                    if (sectionListView.SelectedIndex < sectionListView.Items.Count - 1)
                    {
                        sectionListView.SelectedIndex = sectionListView.SelectedIndex + 1;
                        sectionListView.ScrollIntoView(sectionListView.SelectedItem);
                    }
                    else
                    {
                        sectionListView.SelectAndFocusItem(0);
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
                Alert.ShowExclamation("Not yet implemented.", this);
                return;
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
                    }
                }
            }
        }


        //private void listView_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Key == Key.Up)
        //        {
        //            if (listView.SelectedIndex == 0)
        //            {
        //                var listViewItem = listView.ItemContainerGenerator.ContainerFromIndex(listView.SelectedIndex);

        //                var ctrl = FindISettingsControls(listViewItem).FirstOrDefault();

        //                // prevent moving focus up to section listview if the first item is a slider in Active mode
        //                if (ctrl != null && ctrl is Slider && ctrl.IsActive)
        //                {
        //                    return;
        //                }

        //                listView.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
        //                e.Handled = true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorHandler.HandleException(ex);
        //    }
        //}


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
            listView.SelectAndFocusItem();
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
            //sectionListView.Opacity = 0.7;
        }

        private void sectionListView_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            sectionListView.Opacity = 1.0;
        }


    }
}
