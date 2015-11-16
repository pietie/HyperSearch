using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HyperSearch.Attached
{
    public static class ControllerLayout
    {
        public static int? GetPosition(DependencyObject obj)
        {
            return (int?)obj.GetValue(PositionProperty);
        }

        public static void SetPosition(DependencyObject obj, int? value)
        {
            obj.SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.RegisterAttached("Position", typeof(int?), typeof(ControllerLayout), new PropertyMetadata(null));


        public static string GetName(DependencyObject obj)
        {
            return (string)obj.GetValue(NameProperty);
        }

        public static void SetName(DependencyObject obj, string value)
        {
            obj.SetValue(NameProperty, value);
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.RegisterAttached("Name", typeof(string), typeof(ControllerLayout), new PropertyMetadata(null));


        public static string GetDescription(DependencyObject obj)
        {
            return (string)obj.GetValue(DescriptionProperty);
        }

        public static void SetDescription(DependencyObject obj, string value)
        {
            obj.SetValue(DescriptionProperty, value);
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.RegisterAttached("Description", typeof(string), typeof(ControllerLayout), new PropertyMetadata(null));


    }
}
