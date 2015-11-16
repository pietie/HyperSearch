using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;


namespace HyperSearch.Classes
{
    public static class AnimationExtensions
    {
        public static Storyboard XYAnimation(this UIElement element, double beginTimeInSeconds, double durationInSeconds, double? toX = null, double? toY = null
            , EasingFunctionBase easingFunction = null, Storyboard sb = null, RepeatBehavior? repeatBehavior = null, bool autoReverse = false)
        {
            if (sb == null) sb = new Storyboard();

            Storyboard.SetTarget(sb, element);

            int? transformGroupIndex;
            string xProp, yProp;

            var translateTransform = element.FindTransform<TranslateTransform>(out transformGroupIndex);

            if (translateTransform == null) throw new Exception("No translate transformation found on target element.");

            if (transformGroupIndex.HasValue)
            {
                xProp = "(UIElement.RenderTransform).(TransformGroup.Children)[" + transformGroupIndex + "].(TranslateTransform.X)";
                yProp = "(UIElement.RenderTransform).(TransformGroup.Children)[" + transformGroupIndex + "].(TranslateTransform.Y)";
            }
            else
            {
                xProp = "(UIElement.RenderTransform).(TransformGroup.X)";
                yProp = "(UIElement.RenderTransform).(TransformGroup.Y)";
            }

            if (toX.HasValue)
            {
                DoubleAnimation xAnim = new DoubleAnimation();
                sb.Children.Add(xAnim);

                xAnim.BeginTime = TimeSpan.FromSeconds(beginTimeInSeconds);
                xAnim.Duration = TimeSpan.FromSeconds(durationInSeconds);
                xAnim.To = toX.Value;

                Storyboard.SetTargetProperty(xAnim, new PropertyPath(xProp));
                Storyboard.SetTarget(xAnim, element);

                if (repeatBehavior.HasValue)
                {
                    xAnim.RepeatBehavior = repeatBehavior.Value;
                }
            }

            if (toY.HasValue)
            {
                DoubleAnimation yAnim = new DoubleAnimation();
                sb.Children.Add(yAnim);

                yAnim.BeginTime = TimeSpan.FromSeconds(beginTimeInSeconds);
                yAnim.Duration = TimeSpan.FromSeconds(durationInSeconds);
                yAnim.To = toY.Value;

                Storyboard.SetTargetProperty(yAnim, new PropertyPath(yProp));
                Storyboard.SetTarget(yAnim, element);

                if (repeatBehavior.HasValue)
                {
                    yAnim.RepeatBehavior = repeatBehavior.Value;
                }
            }

            sb.AutoReverse = autoReverse;

            return sb;
        }

        public static Storyboard OpacityAnimation(this UIElement element, double beginTimeInSeconds, double durationInSeconds, double fromOpacity, double toOpacity
           , EasingFunctionBase easingFunction = null, Storyboard sb = null, RepeatBehavior? repeatBehavior = null, bool autoReverse = false)
        {
            if (sb == null) sb = new Storyboard();

            Storyboard.SetTarget(sb, element);

            DoubleAnimation opacityAnim = new DoubleAnimation();

            opacityAnim.BeginTime = TimeSpan.FromSeconds(beginTimeInSeconds);
            opacityAnim.From = fromOpacity;
            opacityAnim.To = toOpacity;
            opacityAnim.EasingFunction = easingFunction;

            Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(opacityAnim, element);

            if (repeatBehavior.HasValue)
            {
                opacityAnim.RepeatBehavior = repeatBehavior.Value;
            }

            sb.AutoReverse = autoReverse;
            sb.Children.Add(opacityAnim);

            return sb;
        }

        public static Storyboard ScaleUniformAnimation(this UIElement element, double beginTimeInSeconds, double durationInSeconds, double fromScale, double toScale, EasingFunctionBase easingFunction = null, Storyboard sb = null, RepeatBehavior? repeatBehavior = null, bool autoReverse = false)
        {
            return element.ScaleAnimation(beginTimeInSeconds, durationInSeconds, fromScale, fromScale, toScale, toScale, easingFunction, sb, repeatBehavior, autoReverse);
        }

        public static Storyboard ScaleAnimation(this UIElement element, double beginTimeInSeconds, double durationInSeconds, double fromScaleX, double fromScaleY, double toScaleX, double toScaleY
            , EasingFunctionBase easingFunction = null, Storyboard sb = null, RepeatBehavior? repeatBehavior = null, bool autoReverse = false)
        {
            if (sb == null) sb = new Storyboard();

            Storyboard.SetTarget(sb, element);

            int? transformGroupIndex;
            string scaleXProp, scaleYProp;

            var scaleTransform = element.FindTransform<ScaleTransform>(out transformGroupIndex);

            if (scaleTransform == null) throw new Exception("No scale transformation found on target element.");

            if (transformGroupIndex.HasValue)
            {
                scaleXProp = "(UIElement.RenderTransform).(TransformGroup.Children)[" + transformGroupIndex + "].(ScaleTransform.ScaleX)";
                scaleYProp = "(UIElement.RenderTransform).(TransformGroup.Children)[" + transformGroupIndex + "].(ScaleTransform.ScaleY)";
            }
            else
            {
                scaleXProp = "(UIElement.RenderTransform).(ScaleTransform.ScaleX)";
                scaleYProp = "(UIElement.RenderTransform).(ScaleTransform.ScaleY)";
            }

            DoubleAnimationUsingKeyFrames scaleXAnim = Util.CreateDoubleAnimationUsingKeyFrames(scaleXProp, beginTimeInSeconds, new AnimationKeyFrame(fromScaleX, 0, easingFunction), new AnimationKeyFrame(toScaleX, durationInSeconds, easingFunction));
            DoubleAnimationUsingKeyFrames scaleYAnim = Util.CreateDoubleAnimationUsingKeyFrames(scaleYProp, beginTimeInSeconds, new AnimationKeyFrame(fromScaleY, 0, easingFunction), new AnimationKeyFrame(toScaleY, durationInSeconds, easingFunction));

            if (repeatBehavior.HasValue)
            {
                scaleXAnim.RepeatBehavior = scaleYAnim.RepeatBehavior = repeatBehavior.Value;
            }
            sb.AutoReverse = autoReverse;

            sb.Children.Add(scaleXAnim);
            sb.Children.Add(scaleYAnim);

            return sb;
        }

        public static Storyboard WidthHeightAnimation(this FrameworkElement element, double beginTimeInSeconds, double durationInSeconds, double fromWidth, double fromHeight, double toWidth, double toHeight, EasingFunctionBase easingFunction = null)
        {
            Storyboard sb = new Storyboard();

            Storyboard.SetTarget(sb, element);

            DoubleAnimationUsingKeyFrames widthAnim = new DoubleAnimationUsingKeyFrames();
            DoubleAnimationUsingKeyFrames heightAnim = new DoubleAnimationUsingKeyFrames();

            Storyboard.SetTargetProperty(widthAnim, new PropertyPath(FrameworkElement.WidthProperty));
            Storyboard.SetTargetProperty(heightAnim, new PropertyPath(FrameworkElement.HeightProperty));

            widthAnim.BeginTime = heightAnim.BeginTime = TimeSpan.FromSeconds(beginTimeInSeconds);

            widthAnim.KeyFrames.Add(new AnimationKeyFrame(fromWidth, 0, easingFunction));
            widthAnim.KeyFrames.Add(new AnimationKeyFrame(toWidth, durationInSeconds, easingFunction));

            heightAnim.KeyFrames.Add(new AnimationKeyFrame(fromHeight, 0, easingFunction));
            heightAnim.KeyFrames.Add(new AnimationKeyFrame(toHeight, durationInSeconds, easingFunction));

            sb.Children.Add(widthAnim);
            sb.Children.Add(heightAnim);

            return sb;
        }

    }
}
