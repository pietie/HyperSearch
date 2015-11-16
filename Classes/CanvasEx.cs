using System.Windows;
using System.Windows.Controls;

namespace HyperSearch.Classes
{
    public class CanvasEx
    {
        public static void SetPosition(UIElement el, Point pos, int? zIndex = null)
        {
            SetPosition(el, pos.X, pos.Y, zIndex);
        }

        public static void SetPosition(UIElement el, double x, double y, int? zIndex = null)
        {
            Canvas.SetTop(el, y);
            Canvas.SetLeft(el, x);

            if (zIndex.HasValue) Canvas.SetZIndex(el, zIndex.Value);
        }

        public static Point GetPosition(UIElement el)
        {
            var x = Canvas.GetLeft(el);
            var y = Canvas.GetTop(el);

            return new Point(x, y);
        }
    }
}
