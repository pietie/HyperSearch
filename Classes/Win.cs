using HscLib.ShaderEffects;
using System.Windows;

namespace HyperSearch.Classes
{
    public class Win
    {
        public static bool Modal(Window win, Window parent, bool dimParent = true)
        {
            win.Owner = parent;

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
