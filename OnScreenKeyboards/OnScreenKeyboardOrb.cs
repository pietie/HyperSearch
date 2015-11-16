using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HyperSearch
{
    public class OnScreenKeyboardOrb : Border
    {
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register("Selected", typeof(bool), typeof(OnScreenKeyboardOrb), new UIPropertyMetadata(false, null));

        public bool Selected { get { return (bool)this.GetValue(SelectedProperty); } set { this.SetValue(SelectedProperty, value); } }
    }

    public enum OrbIndex
    {
        None = -1,
        Top = 0,
        TopRight = 1,
        Right = 2,
        BottomRight = 3,
        Bottom = 4,
        BottomLeft = 5,
        Left = 6,
        TopLeft = 7
    }

}
