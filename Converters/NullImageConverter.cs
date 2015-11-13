using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace HyperSpinClone.Classes.Converters
{
    //http://stackoverflow.com/questions/5399601/imagesourceconverter-error-for-source-null
    public class NullImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            if (value is Uri)
            {
                if (!System.IO.File.Exists(((Uri)value).LocalPath))
                {
                    return DependencyProperty.UnsetValue;
                }

            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

   
}
