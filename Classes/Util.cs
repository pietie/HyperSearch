using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Reflection;
using HyperSearch;

namespace HyperSearch
{
    public class DispatcherTimerContainingAction : System.Windows.Threading.DispatcherTimer
    {// http://blog.bodurov.com/How-to-Create-setTimeout-Function-in-Silverlight/
        public Action Action { get; set; }
    }

    public static class Util
    {
        public static T GetAncestorOfType<T>(this FrameworkElement child) where T : FrameworkElement
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent != null && !(parent is T))
                return (T)GetAncestorOfType<T>((FrameworkElement)parent);
            return (T)parent;
        }

        public static T FindParent<T>(this FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = LogicalTreeHelper.GetParent(element) as FrameworkElement;

            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                    return correctlyTyped;
                else
                    return FindParent<T>(parent);
            }

            return null;
        }

        public static string AbsolutePath(string path, out bool isRelative)
        {
            Uri uriTester;
            isRelative = false;

            if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uriTester))
            {
                throw new Exception("Invalid URI specified: " + path ?? "(null)");
            }
            else if (!uriTester.IsAbsoluteUri)
            {// convert relative Uri to absolute
                isRelative = true;
                return new Uri(new Uri(System.AppDomain.CurrentDomain.BaseDirectory, UriKind.Absolute), uriTester.ToString()).LocalPath;
            }
            else
            {
                return uriTester.LocalPath;
            }
        }

        public static bool EqAny(this Key key, params object[] list)
        {
            if (list == null || list.Length == 0) return false;

            for (int i = 0; i < list.Length;i++)
            {
                if (list[i] is Key)
                {
                    if ((Key)list[i] == key) return true;
                }
                else if (list[i] is HyperSearch.HyperHQSettings.KeyCombo)
                {
                    if (((HyperSearch.HyperHQSettings.KeyCombo)list[i]).Equals(key)) return true;
                }
                //else if (list[i] is HyperSearch.HyperHQSettings.KeyWrapper)
                //{
                //    if (((HyperSearch.HyperHQSettings.KeyWrapper)list[i]).Equals(key)) return true;
                //}

            }

                //


                //return list.Contains(key);
                return false;
        }

        public static void StartProcess(string process, string args, bool log = false)
        {
            if (log)
            {
                MainWindow.LogStatic("Launching: {0}", string.Format("{0} {1}", process, args));
            }

            var hl = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.FileName = process;
            startInfo.Arguments = args;

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            hl.StartInfo = startInfo;
            
            hl.Start();
        }

        public static void SelectAndFocusItem(this ListView lv, int ix = 0, EventHandler callback = null)
        {
            if (lv.Items.Count == 0) return;

            ListViewItem lvi = lv.ItemContainerGenerator.ContainerFromIndex(ix) as ListViewItem;

            if (lvi != null)
            {
                lvi.Dispatcher.BeginInvoke(new Action(() =>
                {
                    lv.SelectedIndex = ix;
                    lv.ScrollIntoView(lv.SelectedItem);
                }), System.Windows.Threading.DispatcherPriority.Render);
                lvi.DelayedFocus(callback);
               
            }
        }
        public static void DelayedFocus(this UIElement uiElement, EventHandler callback = null)
        {
            uiElement.Dispatcher.BeginInvoke(
            new Action(delegate
            {
                uiElement.Focusable = true;
                uiElement.Focus();
                System.Windows.Input.Keyboard.Focus(uiElement);
                if (callback != null) callback(uiElement, null);
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        public static bool IsInDesignMode { get { return System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()); } }
        public static DispatcherTimerContainingAction SetTimeout(int milliseconds, Action func)
        {
            var timer = new DispatcherTimerContainingAction
            {
                Interval = new TimeSpan(0, 0, 0, 0, milliseconds),
                Action = func
            };
            timer.Tick += OnTimeoutTimerTick;
            timer.Start();

            return timer;
        }

        private static void OnTimeoutTimerTick(object sender, EventArgs arg)
        {
            var t = sender as DispatcherTimerContainingAction;
            t.Stop();
            t.Action();
            t.Tick -= OnTimeoutTimerTick;
        }

     

        // Modified from http://stackoverflow.com/questions/974598/find-all-controls-in-wpf-window-by-type
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in child.FindVisualChildren<T>())
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static bool Like(this string str, string match)
        {
            if (str == null) return false;
            if (match == null) return false;

            return str.IndexOf(match, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool StartsWithSafe(this string str, string match)
        {
            if (str == null) return false;
            if (match == null) return false;
            return str.StartsWith(match, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EqualsCI(this string s, string eq)
        {
            if (s == null || eq == null) return false;

            return s.Equals(eq, StringComparison.OrdinalIgnoreCase);
        }

        public static int? ToIntNullable(this string s, int? treatAsNullValue = null)
        {
            if (s == null) return null;
            int t;
            if (int.TryParse(s, out t))
            {
                if (treatAsNullValue.HasValue && t == treatAsNullValue.Value) return null;

                return t;
            }

            return null;
        }

        public static Point CalcPointOnCircle(Point center, double radius, double angle)
        {
            double radians = angle * (Math.PI / 180.0);

            Point result = new Point(Math.Cos(radians) * radius + center.X, Math.Sin(radians) * radius + center.Y);

            return result;
        }

        public static DoubleAnimation CreateDoubleAnimation(double beginTimeInSeconds, double durationInSeconds, double from, double to, RepeatBehavior? repeatBehavior = null)
        {
            DoubleAnimation da = new DoubleAnimation();

            da.BeginTime = TimeSpan.FromSeconds(beginTimeInSeconds);
            da.Duration = TimeSpan.FromSeconds(durationInSeconds);
            da.From = from;
            da.To = to;

            if (repeatBehavior.HasValue) da.RepeatBehavior = repeatBehavior.Value;

            return da;
        }

        public static DoubleAnimationUsingKeyFrames CreateDoubleAnimationUsingKeyFrames(object parameter, double beginTimeInSeconds, params AnimationKeyFrame[] keyFrames)
        {
            return CreateDoubleAnimationUsingKeyFrames(new PropertyPath(parameter), beginTimeInSeconds, keyFrames);
        }

        public static DoubleAnimationUsingKeyFrames CreateDoubleAnimationUsingKeyFrames(string propertyPath, double beginTimeInSeconds, params AnimationKeyFrame[] keyFrames)
        {
            return CreateDoubleAnimationUsingKeyFrames(new PropertyPath(propertyPath), beginTimeInSeconds, keyFrames);
        }

        public static DoubleAnimationUsingKeyFrames CreateDoubleAnimationUsingKeyFrames(PropertyPath propertyPath, double beginTimeInSeconds, params AnimationKeyFrame[] keyFrames)
        {
            DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();

            Storyboard.SetTargetProperty(anim, propertyPath);

            anim.BeginTime = TimeSpan.FromSeconds(beginTimeInSeconds);

            if (keyFrames != null)
            {
                foreach (var kf in keyFrames)
                {
                    anim.KeyFrames.Add(kf);
                }
            }

            return anim;
        }

        public static T FindTransform<T>(this UIElement element, out int? transformGroupIndex) where T : Transform
        {
            transformGroupIndex = null;

            if (element == null) return default(T);

            if (element is UIElement)
            {
                if (element.RenderTransform is T)
                {
                    return (T)element.RenderTransform;
                }
                else if (element.RenderTransform is TransformGroup)
                {
                    TransformGroup tg = (TransformGroup)element.RenderTransform;

                    T transform = (T)tg.Children.FirstOrDefault(t => t is T);

                    transformGroupIndex = tg.Children.IndexOf(transform);

                    return transform;
                }
            }

            return null;
        }

        public static string GetFilenameWithoutExtension(this System.IO.FileInfo fi)
        {
            return fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
        }

        public static void AddChildCentered(this Canvas panel, FrameworkElement child, double w, double h, int? zIndex = null)
        {
            child.Width = w;
            child.Height = h;

            double x = panel.ActualWidth / 2.0 - w / 2.0;
            double y = panel.ActualHeight / 2.0 - h / 2.0;

            panel.AddChild(child, x, y, zIndex);
        }
        public static void AddChild(this Canvas panel, UIElement child, double x, double y, int? zIndex = null)
        {
            panel.AddChild(child, new Point(x, y), zIndex);
        }
        public static void AddChild(this Canvas panel, UIElement child, Point? pos = null, int? zIndex = null)
        {
            panel.Children.Add(child);

            if (pos.HasValue)
            {
                Canvas.SetLeft(child, pos.Value.X);
                Canvas.SetTop(child, pos.Value.Y);
            }

            if (zIndex.HasValue) Panel.SetZIndex(child, zIndex.Value);
        }

        public static string ToStringSafe(this object o)
        {
            if (o == null) return null;

            return o.ToString();
        }

        public static bool Is(this Classes.KeyList kl,  System.Windows.Input.Key key)
        {
            if (kl == null) return false;
            if (kl.Keys == null) return false;

            return kl.Keys.Contains(key);
        }
    }

    public class AnimationKeyFrame : EasingDoubleKeyFrame
    {
        public AnimationKeyFrame(double value, double keyTimeInSeconds, EasingFunctionBase easingFunction = null)
            : base(value, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(keyTimeInSeconds)))
        {
            this.EasingFunction = easingFunction;
        }
    }

    public class SerializeWithJsonAttributesContractResolver : DefaultContractResolver 
    {
        //private List<Type> _allowedTypes = new List<Type>();

        public SerializeWithJsonAttributesContractResolver()
        {
        }
      
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            var attributes = member.GetCustomAttributes(typeof(JsonPropertyAttribute),false).Select(a => (Attribute)a).ToList();

            var allAttribs = member.GetCustomAttributes(false).Select(a => a.GetType()).ToList();

            var b = allAttribs.Count(a => a == typeof(JsonObjectAttribute) || a == typeof(JsonPropertyAttribute)) > 0;

            property.ShouldSerialize = instance => { return b; };
            

            return property;
        }

     

    }
}
