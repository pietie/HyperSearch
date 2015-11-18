using System;
using System.Collections.Generic;
using System.IO;
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

namespace HyperSearch.Windows.Common
{
    public enum TextInputType
    {
        Orbs,
        AtoZ,
        PhysicalKeyboard
    }

    public partial class TextInputWin : Window
    {
        public string Watermark
        {
            get { return txt.Tag as string; }
            set { txt.Tag = value; }
        }

        public TextInputType InputType { get; set; }

        public string Text { get { return txt.Text; } set { txt.Text = value; } }

        public TextInputWin()
        {
            InitializeComponent();

            this.InputType = TextInputType.PhysicalKeyboard;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.InputType == TextInputType.Orbs)
                {
                    LoadOrbControl();
                }
                else if (this.InputType == TextInputType.AtoZ)
                {
                    LoadAtoZControl();
                }
                else throw new NotImplementedException(string.Format("TextInputType {0} not yet supported", this.InputType.ToString()));

            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void OnOskKeyPressed(string charRepresentation, OskSpecialKey specialKey)
        {
            try
            {
                if (specialKey == OskSpecialKey.Done)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.DialogResult = true;
                        this.Close();
                    }));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void LoadOrbControl()
        {
            Viewbox viewBox = new Viewbox();

            var osk = new HyperSearch.OrbKeyboard() { Width = 800, Height = 600 };

            osk.AttachedTextBox = txt;
            osk.OnOskKeyPressed += OnOskKeyPressed;

            viewBox.Child = osk;

            var buttonTemplateXamlFilePath = Global.BuildFilePathInResourceDir("OnScreenKeyboardButtonTemplate.xaml");

            if (!File.Exists(buttonTemplateXamlFilePath)) throw new FileNotFoundException("Failed to find the on-screen keyboard's button template XAML file.", "Resources\\OnScreenKeyboardButtonTemplate.xaml");

            osk.InitFromButtonTemplate(File.ReadAllText(buttonTemplateXamlFilePath));

            container.Child = viewBox;

            Util.SetTimeout(80, new Action(() => osk.AnimateOpen()));

            Util.SetTimeout(90, new Action(() => osk.Focus()));
        }


        private void LoadAtoZControl()
        {
            var kb = new AtoZKeyboard();
            kb.AttachedTextBox = txt;
            kb.OnOskKeyPressed += OnOskKeyPressed;
            container.Child = kb;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Get key config from somewhere...static???
            if (e.Key == Key.Back)
            {
                this.DialogResult = false;
                this.Close();
            }

            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
