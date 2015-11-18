using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HyperSearch.Windows.Settings
{
    /// <summary>
    /// Interaction logic for Slider.xaml
    /// </summary>
    public partial class Slider : UserControl, ISettingsControl
    {
        public event RoutedPropertyChangedEventHandler<double> ValueChanged;

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(Slider), new PropertyMetadata(0d));



        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(Slider), new PropertyMetadata(0d));


        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(Slider), new PropertyMetadata(100d));



        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(Slider), new PropertyMetadata(1d));



        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register("LargeChange", typeof(double), typeof(Slider), new PropertyMetadata(10d));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(Slider), new PropertyMetadata(false, IsActive_PropertyChanged));

        private static void IsActive_PropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var src = (Slider)source;
                src.Focus();
            }
        }

        public Slider()
        {
            InitializeComponent();
            slider.ValueChanged += Slider_ValueChanged;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!IsActive) return;
                e.Handled = true;

                decimal newValue = (decimal)Value;
                decimal largeChange = (decimal)this.LargeChange;
                decimal smallChange = (decimal)this.SmallChange;

                if (Global.UpKey.Is(e.Key))
                {
                    newValue += largeChange;
                }
                else if (Global.DownKey.Is(e.Key))
                {
                    newValue -= largeChange;
                }
                else if (Global.LeftKey.Is(e.Key))
                {
                    newValue -= smallChange;
                }
                else if (Global.RightKey.Is(e.Key))
                {
                    newValue += smallChange;
                }

                if (newValue > (decimal)MaxValue) newValue = (decimal)MaxValue;
                if (newValue < (decimal)MinValue) newValue = (decimal)MinValue;

                slider.Value = (double)newValue;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

 

        private void UserControl_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }
    }
}
