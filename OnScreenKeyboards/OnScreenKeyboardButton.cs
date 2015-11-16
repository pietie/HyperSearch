using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using HyperSearch.Classes;

namespace HyperSearch
{
    public class OnScreenKeyboardButton : Decorator
    {
        // TODO: Add LED (PacLed) button DepProperty here. This could allow us to light up the relevant buttons in the orb, how cool would that be?!!

        private Border _border = new Border();
        private TextBlock _textBlock = new TextBlock();

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(int), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(0, PositionProperty_Changed));

        public int Position { get { return (int)this.GetValue(PositionProperty); } set { this.SetValue(PositionProperty, value); } }

        private static void PositionProperty_Changed(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (Util.IsInDesignMode)
            {
                ((OnScreenKeyboardButton)source).Text = ((int)e.NewValue).ToString();
            }
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(null, Border_PropertyChanged));
        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(new Thickness(0), Border_PropertyChanged));
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(null, Border_PropertyChanged));
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(new CornerRadius(0), Border_PropertyChanged));
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(new Thickness(0), Border_PropertyChanged));

        public Brush BorderBrush { get { return (Brush)this.GetValue(BorderBrushProperty); } set { this.SetValue(BorderBrushProperty, value); } }
        public Thickness BorderThickness { get { return (Thickness)this.GetValue(BorderThicknessProperty); } set { this.SetValue(BorderThicknessProperty, value); } }
        public Brush Background { get { return (Brush)this.GetValue(BackgroundProperty); } set { this.SetValue(BackgroundProperty, value); } }
        public CornerRadius CornerRadius { get { return (CornerRadius)this.GetValue(CornerRadiusProperty); } set { this.SetValue(CornerRadiusProperty, value); } }
        public Thickness Padding { get { return (Thickness)this.GetValue(PaddingProperty); } set { this.SetValue(PaddingProperty, value); } }


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(null, TextBlock_PropertyChanged));
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(new FontFamily("Arial"), TextBlock_PropertyChanged));
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(FontWeights.Normal, TextBlock_PropertyChanged));
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(12d, TextBlock_PropertyChanged));
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(OnScreenKeyboardButton), new UIPropertyMetadata(null, TextBlock_PropertyChanged));

        public string Text { get { return (string)this.GetValue(TextProperty); } set { this.SetValue(TextProperty, value); } }
        public FontFamily FontFamily { get { return (FontFamily)this.GetValue(FontFamilyProperty); } set { this.SetValue(FontFamilyProperty, value); } }
        public FontWeight FontWeight { get { return (FontWeight)this.GetValue(FontWeightProperty); } set { this.SetValue(FontWeightProperty, value); } }
        public double FontSize { get { return (double)this.GetValue(FontSizeProperty); } set { this.SetValue(FontSizeProperty, value); } }
        public Brush Foreground { get { return (Brush)this.GetValue(ForegroundProperty); } set { this.SetValue(ForegroundProperty, value); } }

        private static void Border_PropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var border = ((OnScreenKeyboardButton)source)._border;

            if (e.Property == BorderBrushProperty) { border.BorderBrush = (Brush)e.NewValue; }
            else if (e.Property == BorderThicknessProperty) { border.BorderThickness = (Thickness)e.NewValue; }
            else if (e.Property == BackgroundProperty) { border.Background = (Brush)e.NewValue; }
            else if (e.Property == CornerRadiusProperty) { border.CornerRadius = (CornerRadius)e.NewValue; }
            else if (e.Property == PaddingProperty) { border.Padding = (Thickness)e.NewValue; }
        }

        private static void TextBlock_PropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var tb = ((OnScreenKeyboardButton)source)._textBlock;

            if (e.Property == TextProperty) { tb.Text = (string)e.NewValue; }
            else if (e.Property == FontFamilyProperty) { tb.FontFamily = (FontFamily)e.NewValue; }
            else if (e.Property == FontWeightProperty) { tb.FontWeight = (FontWeight)e.NewValue; }
            else if (e.Property == FontSizeProperty) { tb.FontSize = (double)e.NewValue; }
            else if (e.Property == ForegroundProperty) { tb.Foreground = (Brush)e.NewValue; }
        }

        public string Key { get; set; }

        public OnScreenKeyboardButton()
        {
            //_border.BorderThickness = new Thickness(5);
            //! _border.BorderBrush = new SolidColorBrush(Colors.Red);
            this.Child = _border;

            _border.Child = _textBlock;

            //_border.Width = 45;

            _textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            _textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            _textBlock.FontFamily = new FontFamily("Arial");
            _textBlock.FontWeight = FontWeights.Bold;
            _textBlock.FontSize = 16;
        }

        public void AnimateClick()
        {
            this.RenderTransformOrigin = new Point(0.5, 0.5);
            this.RenderTransform = new ScaleTransform();

            var sb = this.ScaleUniformAnimation(0, 0.1, 1.0, 1.2, autoReverse: true);

            sb.Begin(this);
        }

    }
}
