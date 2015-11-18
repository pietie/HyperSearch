using HscLib.ShaderEffects;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace HyperSearch.Classes
{
    public class Win
    {
        private const int GWL_STYLE = -16; //WPF's Message code for Title Bar's Style 
        private const int WS_SYSMENU = 0x80000; //WPF's Message code for System Menu
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        public static bool Modal(Window win, Window parent, bool dimParent = true, bool removeTitlebarMenu = true)
        {
            win.Owner = parent;

            if (removeTitlebarMenu)
            {
                win.Loaded += (s, e) =>
                {
                    var hwnd = new WindowInteropHelper(win).Handle;
                    SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
                };
            }

            if (dimParent && parent != null)
            {
                DimAllTheThings(parent);
            }

            var ret = win.ShowDialog() ?? false;

            if (dimParent && parent != null)
            {
                UnDimAllTheThings(parent);
            }

            return ret;
        }

        private static void DimAllTheThings(Window target)
        {
            var satEff = new HscLib.ShaderEffects.Saturation() { SaturationValue = 1.0 };

            target.Tag = target.Effect;
            target.Effect = satEff;

            satEff.BeginAnimation(Saturation.SaturationValueProperty, Util.CreateDoubleAnimation(0, 0.4, 1, 0));
        }

        private static void UnDimAllTheThings(Window target)
        {
            var satEff = target.Effect as Saturation;

            if (satEff != null)
            {
                var daSatBack = Util.CreateDoubleAnimation(0, 0.3, 0, 1.0);

                daSatBack.Completed += (s, e) => {

                    if (target.Tag is System.Windows.Media.Effects.Effect)
                    {
                        // restore pre dim effect
                        target.Effect = (System.Windows.Media.Effects.Effect)target.Tag;
                    }
                    else
                    {
                        target.Effect = null;
                    }

                };

                satEff.BeginAnimation(Saturation.SaturationValueProperty, daSatBack);
            }
        }

    }
}
