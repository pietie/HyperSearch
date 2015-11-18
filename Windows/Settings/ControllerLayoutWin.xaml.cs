using HyperSearch.Attached;
using HyperSearch.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HyperSearch.Windows.Settings
{
    /// <summary>
    /// Interaction logic for ControllerLayoutWin.xaml
    /// </summary>
    public partial class ControllerLayoutWin : Window
    {
        private List<ControllerLayoutDefinition> _controllerLayoutTemplates = new List<ControllerLayoutDefinition>();



        public ControllerLayoutDefinition SelectedLayoutDefinition
        {
            get { return (ControllerLayoutDefinition)GetValue(SelectedLayoutDefinitionProperty); }
            set { SetValue(SelectedLayoutDefinitionProperty, value); }
        }


        public static readonly DependencyProperty SelectedLayoutDefinitionProperty =
            DependencyProperty.Register("SelectedLayoutDefinition", typeof(ControllerLayoutDefinition), typeof(ControllerLayoutWin), new PropertyMetadata(null, new PropertyChangedCallback(OnSelectedLayoutDefinitionChanged)));

        public static void OnSelectedLayoutDefinitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = (ControllerLayoutWin)d;

 
        }

        public ControllerLayoutWin()
        {
            InitializeComponent();
        }

        private void LoadTemplates()
        {
            try
            {// TODO: Log each failure and continue with next file
                // TODO: ALso apply some validation rules. Like missing Position defs , not sequential positions, duplicate posiitions, etc...
                var xamlPath = Global.BuildFilePathInAppDir("Resources\\ControllerLayouts");
                var xamlFiles = Directory.EnumerateFiles(xamlPath, "*.xaml");

                System.Windows.Markup.ParserContext parserContext = new System.Windows.Markup.ParserContext();

                parserContext.XmlnsDictionary.Add("ex", "clr-namespace:HyperSearch.Attached;assembly=HyperSearch");
                parserContext.XmlnsDictionary.Add("hsc", "clr-namespace:HyperSearch.Attached;assembly=HyperSearch");

                MainWindow.LogStatic("Found {0} controller template(s) in {1}", xamlFiles.Count(), xamlPath);

                // build a list of all available controller layout templates
                foreach (var file in xamlFiles)
                {
                    try
                    {
                        ControllerLayoutDefinition def = new ControllerLayoutDefinition();

                        var xaml = File.ReadAllText(file);

                        var topLevelPanel = (FrameworkElement)System.Windows.Markup.XamlReader.Parse(xaml, parserContext);

                        def.RawXaml = xaml;
                        def.TopLevelPanel = topLevelPanel;

                        var name = ControllerLayout.GetName(topLevelPanel);
                        var description = ControllerLayout.GetDescription(topLevelPanel);

                        if (!string.IsNullOrEmpty(name)) def.Name = name;
                        else def.Name = new FileInfo(file).GetFilenameWithoutExtension();

                        def.Description = description;

                        var allChildControls = topLevelPanel.FindVisualChildren<Control>().ToList();

                        foreach (var c in allChildControls)
                        {
                            var pos = ControllerLayout.GetPosition(c);

                            if (!pos.HasValue) continue;

                            if (def.ButtonPositionMapping.ContainsKey(pos.Value))
                            {
                                throw new InvalidDataException(string.Format("Duplicate position detected for position {0}. Make sure that Positions are only allocated once.", pos.Value));
                            }

                            def.ButtonPositionMapping[pos.Value] = c;
                        }

                        _controllerLayoutTemplates.Add(def);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.HandleException(ex);
                    }
                }



                // TODO: Remove commented out code - think this was sample code to animated every button across each selection
                return;

                // TODO: Generate Preview
                //{
                //    var panelTopPreview = _controllerLayoutTemplates[1].TopLevelPanel;

                //    panelTopPreview.ClipToBounds = false;
                //    //_controllerLayoutTemplates[0].TopLevelPanel.Background = Brushes.AntiqueWhite;
                //    panelTopPreview.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;


                //    previewBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                //    previewBox.InvalidateVisual();
                //    //previewBox.AddChildCentered(panelTopPreview, 400, 300);
                //    panelTopPreview.RenderTransformOrigin = new Point(0.5, 0.5);

                //    var widthToHeightRatio = panelTopPreview.Width / panelTopPreview.Height;

                //    panelTopPreview.Height = 180;
                //    panelTopPreview.Width = panelTopPreview.Height * widthToHeightRatio;

                //    cp.Content = panelTopPreview;
                //}

                //var el = _controllerLayoutTemplates[1].ButtonPositionMapping[2];

                //el.RenderTransform = new ScaleTransform() { };
                //el.RenderTransformOrigin = new Point(0.5, 0.5);

                //var sb = el.ScaleUniformAnimation(1, 1.2, 1, 1.2, repeatBehavior: RepeatBehavior.Forever, autoReverse: true);

                //sb.Begin();

                //var testXaml = File.ReadAllText(@"D:\00-Work\Projects\HyperSpinClone\HyperSearch\bin\Debug\Resources\ControllerLayouts\6 button simple.xaml");

                //for (int i = 0; i < _controllerLayoutTemplates[0].ButtonPositionMapping.Count; i++)
                //{
                //    var topLevelPanel = (FrameworkElement)System.Windows.Markup.XamlReader.Parse(testXaml, parserContext);

                //    ControllerLayoutDefinition def = new ControllerLayoutDefinition();
                //    def.TopLevelPanel = topLevelPanel;

                //    //var name = ControllerLayoutButton.GetName(topLevelPanel);

                //    //if (!string.IsNullOrEmpty(name)) def.Name = name;
                //    //else def.Name = new FileInfo(file).GetFilenameWithoutExtension();

                //    var allChildControls = topLevelPanel.FindVisualChildren<Control>().ToList();

                //    foreach (var c in allChildControls)
                //    {// TODO: Check for duplicate positions
                //        var pos = ControllerLayout.GetPosition(c);

                //        if (!pos.HasValue) continue;

                //        def.ButtonPositionMapping[pos.Value] = c;
                //    }

                //    listView.Items.Add(topLevelPanel);
                //    var el2 = def.ButtonPositionMapping[i];

                //    el2.RenderTransform = new ScaleTransform() { };
                //    el2.RenderTransformOrigin = new Point(0.5, 0.5);

                //    var sb2 = el2.ScaleUniformAnimation(1, 1.2, 1, 1.2, repeatBehavior: RepeatBehavior.Forever, autoReverse: true);

                //    sb2.Begin();
                //}
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                listView.SelectedIndex = 0;
                listView.Focus();
                Keyboard.Focus(listView);
                LoadTemplates();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void listView_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (Global.ActionKey.Is(e.Key))
                {
                    if (listView.SelectedIndex == 0/*Layout*/)
                    {
                        OnSelectLayoutTemplate();
                    }
                    else if(listView.SelectedIndex == 1/*Bindings*/)
                    {
                        //OnSelectLayoutTemplate();
                    }
                    else if (listView.SelectedIndex == 2/*Styles*/)
                    {
                        OnSetStyles();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void OnSelectLayoutTemplate()
        {
            var dataTemplate = this.Resources["ButtonLayoutSelectorListViewItemDataTemplate"] as DataTemplate;

            MultiOptionSelector sel = new MultiOptionSelector(dataTemplate);

            sel.SetItemSource(_controllerLayoutTemplates.ToArray());

            sel.Width = 600;
            sel.Height = 600;

            if (Win.Modal(sel, this))
            {
                ControllerLayoutDefinition def = (ControllerLayoutDefinition)sel.SelectedItem;

                this.SelectedLayoutDefinition = def;
                                
                SettingsListViewItem item = (listView.Items[0] as ContentControl).DataContext as SettingsListViewItem;
                item.Value = def.Name;

                ////ControllerLayoutDefinition def = (ControllerLayoutDefinition)sel.SelectedItem;
                //////!HSCSettings.Instance().P1KeyboardControlsSection.ButtonLayoutTemplateFilename = def.Name; // TODO: Filename!!!!


            }
        }

        private void OnSetStyles()
        {
            if (this.SelectedLayoutDefinition == null)
            {
                MessageBox.Show("Please select a layout first.");
                return;       
            }

            var win = new ButtonStyleConfig();

            win.SetLayoutDefinition(this.SelectedLayoutDefinition);

            win.Width = 700;
            win.Height = 600;

            if (Win.Modal(win, this))
            {

            }
        }
    }

    // TODO: Move somewhere appropriate
    public class ControllerLayoutDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string RawXaml { get; set; }

        public FrameworkElement TopLevelPanel { get; set; }

        public Dictionary<int/*Position*/, Control> ButtonPositionMapping = new Dictionary<int, Control>();
    }
}
