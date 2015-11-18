using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace HyperSearch.Windows.Settings
{
    public class SettingsListViewItem : DependencyObject
    {
        public object ParentObject { get; set; }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(SettingsListViewItem), new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsListViewItem item = (SettingsListViewItem)d;

            if (item.Property != null && item.ParentObject != null)
            {
                if (item.Property.PropertyType == typeof(string))
                {
                    var setMethod = item.Property.GetSetMethod();

                    setMethod.Invoke(item.ParentObject, new object[] { e.NewValue as string });


                }
            }
        }


        public System.Reflection.PropertyInfo Property { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        //public string Value { get; set; }

        public SettingTypeAttribute SettingType { get; set; }

    }
}
