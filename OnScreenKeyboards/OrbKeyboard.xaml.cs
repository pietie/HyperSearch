using HyperSearch.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HyperSearch.OnScreenKeyboards
{
   
    public partial class OrbKeyboard : OskBaseControl
    {
        public static class BackSpaceSpecialKey
        {
            public static string Value { get { return "<BACKSPACE>"; } }
        }

        public static class SpaceSpecialKey
        {
            public static string Value { get { return "<SPACE>"; } }
        }

        public static class DoneSpecialKey
        {
            public static string Value { get { return "<DONE>"; } }
        }

        public static class ClearSpecialKey
        {
            public static string Value { get { return "<CLEAR>"; } }
        }

        private List<string> ALPHABET = new List<string>() //TODO: Make this configurable
        {
            "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
            ,"0","1","2","3","4","5","6","7","8","9"
            ,"A","E","I","O","U","Y"
            , DoneSpecialKey.Value , BackSpaceSpecialKey.Value, SpaceSpecialKey.Value, ClearSpecialKey.Value
        };

        private List<string> NUMBERS = new List<string>() //TODO: Make this configurable
        {
            "0","1","2","3","4","5","6","7","8","9"
            //, "<<", "_", "CLR"
        };

        private OnScreenKeyboardOrb[] AllOrbs { get; set; }
        private Path[] AllArrows { get; set; }
        public OnScreenKeyboardOrb SelectedOrb { get; private set; }

        private Point[] _balltopPositions = new Point[8];

        public OrbKeyboard()
        {
            InitializeComponent();

            AllOrbs = new OnScreenKeyboardOrb[] { top, topRight, right, bottomRight, bottom, bottomLeft, left, topLeft };
            AllArrows = new Path[] { arrowT, arrowTR, arrowR, arrowBR, arrowB, arrowBL, arrowL, arrowTL };

            foreach (var o in AllOrbs) o.Visibility = System.Windows.Visibility.Hidden;
            balltop.Visibility = System.Windows.Visibility.Hidden;
            //!centerImg.Visibility = Visibility.Hidden;
        }

        public void InitFromButtonTemplate(string xaml)
        {
            var parserContext = new ParserContext();

            parserContext.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            parserContext.XmlnsDictionary.Add("hsc", "clr-namespace:HyperSpinClone.UserControls;assembly=HyperspinClone");
            parserContext.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");

            var topContainer = (DependencyObject)System.Windows.Markup.XamlReader.Parse(xaml, parserContext);

            var buttons = topContainer.FindVisualChildren<OnScreenKeyboardButton>().ToList();

            var duplicatePositionCounts = buttons.GroupBy<OnScreenKeyboardButton, int>((v) => v.Position).Where(g => g.Count() > 1).Count();

            if (duplicatePositionCounts > 0) throw new Exception("Position must be unqiue."); // TODO: Throw more descriptive error please thank you very much

            int orbsNeeded = (int)((ALPHABET.Count / (double)buttons.Count) + 0.5);

            if (orbsNeeded > 8) throw new Exception("Ran out of orbs :("); // TODO: Throw more descriptive error please thank you very much

            int alphaIndex = 0;


            // fill up the orbs with the ALPHABET, using the button template as a reference 
            for (int orbIndex = 0; orbIndex < orbsNeeded; orbIndex++)
            {
                // create a new instance of the button layout
                var buttonLayout = (UIElement)System.Windows.Markup.XamlReader.Parse(xaml, parserContext);
                var buts = buttonLayout.FindVisualChildren<OnScreenKeyboardButton>().OrderBy(b => b.Position).ToList();

                AllOrbs[orbIndex].Child = new Viewbox() { Child = buttonLayout, Width = AllOrbs[orbIndex].Width * 0.85 };

                for (int buttonIndex = 0; buttonIndex < buts.Count; buttonIndex++)
                {
                    if (alphaIndex < ALPHABET.Count)
                    {
                        buts[buttonIndex].Text = ALPHABET[alphaIndex];

                        if (ALPHABET[alphaIndex] == BackSpaceSpecialKey.Value)
                        {//backspaceStyle
                            var style = buts[buttonIndex].FindResource("backspaceStyle") as Style;

                            if (style != null) buts[buttonIndex].Style = style;

                            buts[buttonIndex].Text = "␈";
                            buts[buttonIndex].Tag = BackSpaceSpecialKey.Value;
                        }
                        else if (ALPHABET[alphaIndex] == SpaceSpecialKey.Value)
                        {
                            var style = buts[buttonIndex].FindResource("spaceStyle") as Style;

                            if (style != null) buts[buttonIndex].Style = style;
                            buts[buttonIndex].Text = "_";
                            buts[buttonIndex].Tag = SpaceSpecialKey.Value;
                        }
                        else if (ALPHABET[alphaIndex] == ClearSpecialKey.Value)
                        {
                            var style = buts[buttonIndex].FindResource("clearStyle") as Style;

                            if (style != null) buts[buttonIndex].Style = style;
                            buts[buttonIndex].Text = "C";
                            buts[buttonIndex].Tag = ClearSpecialKey.Value;
                        }
                        else if (ALPHABET[alphaIndex] == DoneSpecialKey.Value)
                        {
                            var style = buts[buttonIndex].FindResource("doneStyle") as Style;

                            if (style != null) buts[buttonIndex].Style = style;
                            buts[buttonIndex].Text = "D";
                            buts[buttonIndex].Tag = DoneSpecialKey.Value;
                        }
                    }
                    else
                    {
                        // hide buttons if we've reached the end of the alphabet
                        buts[buttonIndex].Visibility = System.Windows.Visibility.Collapsed;
                    }

                    alphaIndex++;
                }
            }
        }

        public void SelectOrb(OrbIndex orbIndex)
        {
            for (int i = 0; i < AllOrbs.Length; i++)
            {
                AllOrbs[i].Selected = false;
                AllArrows[i].Style = (Style)this.Resources["arrowPath"];
            }

            if (orbIndex != OrbIndex.None)
            {
                AllOrbs[(int)orbIndex].Selected = true;
                AllArrows[(int)orbIndex].Style = (Style)this.Resources["arrowPathSelected"];

                this.SelectedOrb = AllOrbs[(int)orbIndex];

                SystemSoundPlayer.Instance().PlaySound(Classes.SystemSound.ScreenClick); 
            }
            else
            {
                // TODO: Handle selection release
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void ResizeOrbs()
        {
            var orbRadius = Math.Min(this.ActualWidth, this.ActualHeight) / 3.8d;
            var cr = new CornerRadius(orbRadius / 2.0);

            foreach (var o in AllOrbs)
            {
                o.Width = o.Height = orbRadius;
                o.CornerRadius = cr;

                o.UpdateLayout();
            }
        }

        public void ReclalcFinalOrbPositions(bool actuallyMoveOrbToNewPos = false)
        {
            ResizeOrbs();

            var orbRadius = top.Width;
            var radius = (Math.Min(this.ActualWidth, this.ActualHeight) / 2.0) - (orbRadius / 2.0);

            var centerX = (this.ActualWidth / 2.0);
            var centerY = (this.ActualHeight / 2.0);

            Point center = new Point(centerX, centerY);

            balltop.Width = balltop.Height = this.ActualWidth * 0.08;//0.20;

            //!CanvasEx.SetPosition(centerImg, centerX - (centerImg.Width / 2.0), centerY - (centerImg.Height / 2.0));
            CanvasEx.SetPosition(balltop, centerX - (balltop.Width / 2.0), centerY - (balltop.Height / 2.0));

            PositionOrb(topLeft, center, 225, radius, actuallyMoveOrbToNewPos);
            PositionOrb(top, center, 270, radius, actuallyMoveOrbToNewPos);
            PositionOrb(topRight, center, 315, radius, actuallyMoveOrbToNewPos);

            PositionOrb(right, center, 0, radius, actuallyMoveOrbToNewPos);

            PositionOrb(bottomLeft, center, 135, radius, actuallyMoveOrbToNewPos);
            PositionOrb(bottom, center, 90, radius, actuallyMoveOrbToNewPos);
            PositionOrb(bottomRight, center, 45, radius, actuallyMoveOrbToNewPos);

            PositionOrb(left, center, 180, radius, actuallyMoveOrbToNewPos);

            Ellipse el = new Ellipse();

            el.Width = el.Height = radius;

            el.Fill = new SolidColorBrush(Colors.Yellow);
            el.Opacity = 0.3;

            CanvasEx.SetPosition(el, centerX - (radius / 2.0), centerY - (radius / 2.0), 9999);


            //mainContainer.Children.Add(el);

            //if (actuallyMoveOrbToNewPos)
            {
                mainContainer.InvalidateArrange();
                mainContainer.InvalidateMeasure();

                mainContainer.UpdateLayout();
                mainContainer.InvalidateVisual();
            }

            var arrowRadius = radius * 0.45;

            Ellipse elInner = new Ellipse();

            elInner.Width = elInner.Height = arrowRadius * 2;

            elInner.Fill = new SolidColorBrush(Colors.Yellow);
            elInner.Opacity = 0.3;

            CanvasEx.SetPosition(elInner, centerX - (elInner.Width / 2.0), centerY - (elInner.Width / 2.0), 9999);

            // mainContainer.Children.Add(elInner);


            PositionArrow(arrowTL, center, 225, arrowRadius);
            PositionArrow(arrowT, center, 270, arrowRadius);
            PositionArrow(arrowTR, center, 315, arrowRadius);

            PositionArrow(arrowR, center, 0, arrowRadius);

            PositionArrow(arrowBL, center, 135, arrowRadius);
            PositionArrow(arrowB, center, 90, arrowRadius);
            PositionArrow(arrowBR, center, 45, arrowRadius);

            PositionArrow(arrowL, center, 180, arrowRadius);
        }

        private void PositionArrow(Path arrow, Point center, double angle, double radius)
        {
            var pnt = Util.CalcPointOnCircle(center, radius, angle);

            pnt.X -= arrow.Width / 2.0;
            pnt.Y -= arrow.Height / 2.0;

            //orb.Tag = pnt;

            //if (actuallyMoveOrbToNewPos)
            {
                CanvasEx.SetPosition(arrow, pnt, 100); // TODO: zIndex!
            }
        }

        private void PositionOrb(OnScreenKeyboardOrb orb, Point center, double angle, double radius, bool actuallyMoveOrbToNewPos)
        {
            var pnt = Util.CalcPointOnCircle(center, radius, angle);

            pnt.X -= orb.Width / 2.0;
            pnt.Y -= orb.Height / 2.0;

            orb.Tag = pnt;

            if (actuallyMoveOrbToNewPos)
            {
                CanvasEx.SetPosition(orb, pnt, 100); // TODO: zIndex!
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReclalcFinalOrbPositions();

            if (!Double.IsNaN(this.Width))
            {
                mainContainer.Width = this.Width;
                mainContainer.Height = this.Height;
            }
            else
            {
                mainContainer.Width = this.ActualWidth;
                mainContainer.Height = this.ActualHeight;
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            ReclalcFinalOrbPositions(true);
            return base.ArrangeOverride(arrangeBounds);
        }

        private Thickness _preMinimizePadding;
        public void Minimize()
        {
            foreach (var o in AllOrbs)
            {
                _preMinimizePadding = o.Padding;
                o.Padding = new Thickness(0);
                if (o.Child != null) o.Child.Visibility = System.Windows.Visibility.Collapsed;
                balltop.Opacity = 0.2;
                //!centerImg.Opacity = 0.2;

            }
        }

        public void Maximize()
        {
            foreach (var o in AllOrbs)
            {
                if (_preMinimizePadding != null) o.Padding = _preMinimizePadding;

                if (o.Child != null) o.Child.Visibility = System.Windows.Visibility.Visible;

                balltop.Opacity = 1.0;
                //!centerImg.Opacity = 1.0;
            }
        }

        public void AnimateOpen()
        {
            ReclalcFinalOrbPositions(false);

            var animationDurationInSecs = 0.8;
            var scaleFrom = 0.2;

            var centerX = (this.ActualWidth / 2.0);
            var centerY = (this.ActualHeight / 2.0);

            Storyboard sb = new Storyboard();

            string scaleXProp = "(UIElement.RenderTransform).(ScaleTransform.ScaleX)";
            string scaleYProp = "(UIElement.RenderTransform).(ScaleTransform.ScaleY)";

            BounceEase ease = new BounceEase() { Bounces = 3, Bounciness = 2.5, EasingMode = EasingMode.EaseOut };

            // position all orbs in Center and create animations
            foreach (var o in AllOrbs)
            {
                o.RenderTransform = new ScaleTransform(1.0, 1.0);

                var fromX = centerX - ((o.Width * scaleFrom) / 2.0);
                var fromY = centerY - ((o.Height * scaleFrom) / 2.0);

                var finalPnt = new Point(0, 0);

                if (o.Tag != null) finalPnt = (Point)o.Tag;

                Canvas.SetZIndex(o, 999);
                CanvasEx.SetPosition(o, fromX, fromY);
                o.Visibility = System.Windows.Visibility.Visible;

                var animX = Util.CreateDoubleAnimationUsingKeyFrames(Canvas.LeftProperty, 0, new AnimationKeyFrame(fromX, 0), new AnimationKeyFrame(finalPnt.X, animationDurationInSecs, ease));
                var animY = Util.CreateDoubleAnimationUsingKeyFrames(Canvas.TopProperty, 0, new AnimationKeyFrame(fromY, 0), new AnimationKeyFrame(finalPnt.Y, animationDurationInSecs, ease));

                var scaleX = Util.CreateDoubleAnimationUsingKeyFrames(scaleXProp, 0, new AnimationKeyFrame(scaleFrom, 0), new AnimationKeyFrame(1.0, animationDurationInSecs, ease));
                var scaleY = Util.CreateDoubleAnimationUsingKeyFrames(scaleYProp, 0, new AnimationKeyFrame(scaleFrom, 0), new AnimationKeyFrame(1.0, animationDurationInSecs, ease));

                Storyboard.SetTarget(animX, o);
                Storyboard.SetTarget(animY, o);
                Storyboard.SetTarget(scaleX, o);
                Storyboard.SetTarget(scaleY, o);

                sb.Children.Add(animX);
                sb.Children.Add(animY);
                sb.Children.Add(scaleX);
                sb.Children.Add(scaleY);
            }

            sb.FillBehavior = FillBehavior.Stop;

            sb.Completed += (s, e) => { ReclalcFinalOrbPositions(true); };

            sb.Begin(this);

            balltop.Visibility = System.Windows.Visibility.Visible;
        }

        public OnScreenKeyboardButton ProcessKey(KeyList keypress)
        {
            if (this.SelectedOrb == null) return null;

            // check if an Orb button was pressed
            var matchingOrbButton = this.SelectedOrb.FindVisualChildren<OnScreenKeyboardButton>().FirstOrDefault(b =>
            {
                Key key;

                if (Enum.TryParse<Key>(b.Key, out key))
                {
                    return keypress.Is(key);
                }
                return false;
            });

            if (matchingOrbButton != null)
            {
                return matchingOrbButton;
            }

            return null;
        }

        private void RefreshSelectedOrbState()
        {
            // TODO: Work off settings
            if (Keyboard.IsKeyDown(Key.Up) && Keyboard.IsKeyDown(Key.Right))
            {
                SelectOrb(OrbIndex.TopRight);
            }
            else if (Keyboard.IsKeyDown(Key.Up) && Keyboard.IsKeyDown(Key.Left))
            {
                SelectOrb(OrbIndex.TopLeft);
            }
            else if (Keyboard.IsKeyDown(Key.Down) && Keyboard.IsKeyDown(Key.Right))
            {
                SelectOrb(OrbIndex.BottomRight);
            }
            else if (Keyboard.IsKeyDown(Key.Down) && Keyboard.IsKeyDown(Key.Left))
            {
                SelectOrb(OrbIndex.BottomLeft);
            }
            else if (Keyboard.IsKeyDown(Key.Up))
            {
                SelectOrb(OrbIndex.Top);
            }
            else if (Keyboard.IsKeyDown(Key.Right))
            {
                SelectOrb(OrbIndex.Right);
            }
            else if (Keyboard.IsKeyDown(Key.Down))
            {
                SelectOrb(OrbIndex.Bottom);
            }
            else if (Keyboard.IsKeyDown(Key.Left))
            {
                SelectOrb(OrbIndex.Left);
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!this.IsKeyboardFocused) return;

            RefreshSelectedOrbState();

            // see if one of the Orb buttons was pressed
            var button = this.ProcessKey(new KeyList(e.Key));

            if (button != null)
            {
                e.Handled = true;

                button.AnimateClick();

                if (button.Tag is string)
                {
                    string tag = (string)button.Tag;
                    
                    if (tag == BackSpaceSpecialKey.Value)
                    {
                        this.RaiseOskKeyPressedEvent(null, OskSpecialKey.Backspace);
                    }
                    else if (tag == SpaceSpecialKey.Value)
                    {
                        this.RaiseOskKeyPressedEvent(" ", OskSpecialKey.Space);
                    }
                    else if (tag == ClearSpecialKey.Value)
                    {
                        this.RaiseOskKeyPressedEvent(null, OskSpecialKey.Clear);
                    }
                    else if (tag == DoneSpecialKey.Value)
                    {
                        this.RaiseOskKeyPressedEvent(null, OskSpecialKey.Done);
                    }
                }
                else
                {
                    this.RaiseOskKeyPressedEvent(button.Text, OskSpecialKey.None);
                }
            }

            //// TODO: Get key config from somewhere...static???
            //if (e.Key == Key.Back)
            //{
            //    if (CurrentViewState == ViewState.SetupSearch)
            //    {
            //        this.Close();
            //    }
            //    else // move back to prev view
            //    {
            //        CurrentViewState = ViewState.SetupSearch;

            //        MoveToNewView(searchSetupGrid, null, null);
            //    }
            //}

            //if (e.Key == Key.Enter || e.Key == Key.Return)
            //{
            //    if (CurrentViewState == ViewState.SetupSearch)
            //    {
            //        PerformSearch();
            //    }
            //    else if (CurrentViewState == ViewState.Results)
            //    {
            //        RunSelectedResult();
            //    }
            //}
        }

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Util.SetTimeout(200, new Action(() => RefreshSelectedOrbState()));
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }
    }
}
