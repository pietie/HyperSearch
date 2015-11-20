using HyperSearch.Attached;
using HyperSearch.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                            
                            if (def.Buttons.Count(b => b.Position == pos.Value) > 0)
                            {
                                throw new InvalidDataException(string.Format("Duplicate position detected for position {0}. Make sure that Positions are only allocated once.", pos.Value));
                            }

                            def.Buttons.Add(new ButtonConfig() { ButtonControl = c, Position = pos.Value });
                        }

                        _controllerLayoutTemplates.Add(def);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.HandleException(ex);
                    }
                }
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
                LoadTemplates();

                listView.SelectedIndex = 0;
                Keyboard.Focus((listView.SelectedItem as ContentControl));
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
                var settings = HyperSearchSettings.Instance().Input;
                if (settings.Action.Is(e.Key))
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

            Win.Modal(sel, this);

            //if (Win.Modal(sel, this))
            {
                ControllerLayoutDefinition def = (ControllerLayoutDefinition)sel.SelectedItem;

                this.SelectedLayoutDefinition = def;
                                
                layoutTemplate.Value = def.Name;

                //var layoutConfig = new LayoutConfig();

                //layoutConfig.Name = def.Name;
                //layoutConfig.Description = def.Description;
                //layoutConfig.Buttons = new List<ButtonConfig>();

                //foreach(var p in def.ButtonPositionMapping)
                //{
                //    layoutConfig.Buttons.Add(new ButtonConfig() { Position = p.Key, HueOffset = def.ButtonPositionMapping[p.Key].HueOffset });
                //}


                HyperSearchSettings.Instance().Input.LayoutConfig = def;

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
    /// <summary>
    /// Defines the *possible* layout definitions to choose from.
    /// </summary>
    public class ControllerLayoutDefinition : LayoutConfig
    {
        public string RawXaml { get; set; }

        public FrameworkElement TopLevelPanel { get; set; }
    }

    public class LayoutConfig
    {
        public LayoutConfig()
        {
            this.Buttons = new List<ButtonConfig>();
        }

        [JsonProperty]
        [DefaultValue("Default")]
        public string Name { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public List<ButtonConfig> Buttons { get; set; }
    }

    public class ButtonConfig : DependencyObject
    {
        [JsonIgnore]
        public Control ButtonControl { get; set; }

        [JsonProperty]
        public int Position { get; set; }

        [JsonProperty]
        public double HueOffset
        {
            get { return (double)GetValue(HueOffsetProperty); }
            set { SetValue(HueOffsetProperty, value); }
        }

        [JsonIgnore]
        public static readonly DependencyProperty HueOffsetProperty = DependencyProperty.Register("HueOffset", typeof(double), typeof(ButtonConfig), new PropertyMetadata(0d));
    }

}
