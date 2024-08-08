using HscLib.Attached;
using HscLib.HyperTheme;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace HyperSearch.Controls
{
    public class SwfXamlHost : Canvas, INotifyPropertyChanged
    {
        public static readonly RoutedEvent StartSwfArtworkAnimationsEvent = EventManager.RegisterRoutedEvent("StartSwfArtworkAnimations", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SwfXamlHost));

        public event RoutedEventHandler StartSwfArtworkAnimations
        {
            add { AddHandler(StartSwfArtworkAnimationsEvent, value); }
            remove { RemoveHandler(StartSwfArtworkAnimationsEvent, value); }
        }

        public SwfXamlHost()
        {
            this.StartSwfArtworkAnimations += SwfXamlHost_StartSwfArtworkAnimations;
        }


        public string XamlPath
        {
            get
            {
                return (string)GetValue(XamlPathProperty);
            }
            set
            {
                SetValue(XamlPathProperty, value);
            }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            if (args.Property == XamlPathProperty)
            {
                ((SwfXamlHost)dependencyObject).InitFromXaml();
            }
        }

        public static readonly DependencyProperty XamlPathProperty = DependencyProperty.Register("XamlPath", typeof(string), typeof(SwfXamlHost), new PropertyMetadata(null, PropertyChangedCallback));


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //  this.InitFromXaml();
            this.Loaded += SwfXamlHost_Loaded;
        }

        private void SwfXamlHost_Loaded(object sender, RoutedEventArgs e)
        {
            // InitFromXaml();
        }

        private void InitFromXaml()
        {
            try
            {
                if (this.XamlPath == null || !File.Exists(this.XamlPath))
                {
                    this.Children.Clear();
                    return;
                }

                Panel xamlTopContainer;

                System.Windows.Markup.ParserContext parserContext = new System.Windows.Markup.ParserContext();

                parserContext.XmlnsDictionary.Add("ex", "clr-namespace:HscLib.Attached;assembly=HscLib");
                parserContext.XmlnsDictionary.Add("hsc", "clr-namespace:HscLib.Attached;assembly=HscLib");

                var xaml = File.ReadAllText(this.XamlPath);
                var fiXaml = new FileInfo(this.XamlPath);

                xamlTopContainer = (Panel)System.Windows.Markup.XamlReader.Parse(xaml, parserContext);

                var root = xamlTopContainer.FindName("_root") as FrameworkElement;

                MediaTimeline mtl = null;
                List<MediaTimeline> mediaTimeLineList = new List<MediaTimeline>();
                int mediaTimeLineIx = 0;

                //this.Dispatcher.Invoke(new Action(() => {
                xamlTopContainer = (Panel)System.Windows.Markup.XamlReader.Parse(xaml, parserContext);

                mtl = (MediaTimeline)xamlTopContainer.FindName("mediaTimeLine_" + mediaTimeLineIx);

                while (mtl != null)
                {
                    mediaTimeLineList.Add(mtl);

                    mediaTimeLineIx++;
                    mtl = (MediaTimeline)xamlTopContainer.FindName("mediaTimeLine_" + mediaTimeLineIx);
                }
                //}));


                // find all image resource keys so we can tie them up with their actual images (from the extended zip file)
                var imgKeys = xamlTopContainer.Resources.Keys.Cast<string>().Where(key => key.StartsWith("img_", StringComparison.CurrentCultureIgnoreCase)).ToList();

                foreach (var key in imgKeys)
                {
                    var resource = xamlTopContainer.Resources[key];

                    // the zip filename ref
                    string fn = null;

                    fn = ZipResource.GetFilename((DependencyObject)resource);

                    if (fn != null)
                    {
                        if (resource is ImageBrush)
                        {
                            string imgPath = System.IO.Path.Combine(fiXaml.DirectoryName, fn);

                            ((ImageBrush)resource).ImageSource = new BitmapImage(new Uri(imgPath));
                        }
                        else
                        {
                            throw new Exception("Unsupported type: " + resource.GetType().FullName);
                        }
                    }
                }


                //TODO: SOUNDS!!!
                //foreach (var el in mediaTimeLineList)
                //{
                //    this.Dispatcher.BeginInvoke(new Action(() => el.Source = new Uri(@"C:\Hyperspin\HS Downloads\Main Menu\Themes\MAME_HyperSpinClone\Resources_Background\snd_4.mp3")));
                //}

                this.Children.Add(xamlTopContainer);

                //Canvas.SetTop(root,25);
                //Canvas.SetLeft(root,75);


                xamlTopContainer.Clip = null;
                xamlTopContainer.ClipToBounds = false;
                xamlTopContainer.Background = Brushes.Transparent;


                //                if (this.XamlPath.EndsWith("Atari 7800.xaml"))
                //if (this.XamlPath.EndsWith("Aladdin (Europe).xaml"))
                {

                    xamlTopContainer.Loaded += (ss, ee) =>
                    {
                        var bounds = VisualTreeHelper.GetDescendantBounds(xamlTopContainer);

                        this.Width = xamlTopContainer.Width = bounds.Width;
                        this.Height = xamlTopContainer.Height = bounds.Height;



                        //!Canvas.SetTop(xamlTopContainer, (bounds.Height / 2.0));
                        //!Canvas.SetLeft(xamlTopContainer, (bounds.Width / 2.0));

                        Canvas.SetTop(xamlTopContainer, 0);
                        Canvas.SetLeft(xamlTopContainer, 0);

                        // this.Dispatcher.BeginInvoke(new Action(() =>
                        // {
                        var root2 = xamlTopContainer.FindName("_root") as FrameworkElement;

                        //root2.Resources

                        dynamic offsets = null;

                        foreach (var trig in root2.Triggers)
                        {
                            if (trig is EventTrigger)
                            {
                                var action = ((EventTrigger)trig).Actions.FirstOrDefault() as System.Windows.Media.Animation.BeginStoryboard;


                                if (action != null)
                                {
                                    var sb = action.Storyboard;

                                    offsets = sb.Children.Where(c => c is MatrixAnimationUsingKeyFrames)
                                                                .Select(c => (MatrixAnimationUsingKeyFrames)c)
                                                                .Where(ma => ma.KeyFrames.Count > 0)
                                                                .Select(ma => new { ma.KeyFrames[0].Value.OffsetX, ma.KeyFrames[0].Value.OffsetY })
                                                                .GroupBy(ma => 1)
                                                                .Select(g => new { MinOffsetX = g.Min(i => i.OffsetX), MinOffsetY = g.Min(i => i.OffsetY) })
                                                                .FirstOrDefault();

                                    if (offsets != null)
                                    {
                                        Canvas.SetTop(root2, Math.Abs(offsets.MinOffsetY));
                                        Canvas.SetLeft(root2, Math.Abs(offsets.MinOffsetX));
                                    }



                                }

                            }

                        }


                         root2.RaiseEvent(new RoutedEventArgs(HyperThemeHost.StartSwfArtworkAnimationsEvent));


                        //this.MouseLeftButtonDown += (sender, e) =>
                        //{
                        //    this.RaiseEvent(new RoutedEventArgs(StartSwfArtworkAnimationsEvent, new object[] { xamlTopContainer, offsets }));
                        //  //this.RaiseEvent(new RoutedEventArgs(HyperThemeHost.StartSwfArtworkAnimationsEvent));

                        //    //object[] args = (object[])sender;

                        //    //var xamlTopContainer = (FrameworkElement)args[0];
                        //    //var root3 = xamlTopContainer.FindName("_root") as FrameworkElement;

                        //    //if (args.Length == 2 && args[1] != null)
                        //    //{
                        //    //    //dynamic offsets = args[1];
                        //    //    Canvas.SetTop(root2, Math.Abs(offsets.MinOffsetY));
                        //    //    Canvas.SetLeft(root2, Math.Abs(offsets.MinOffsetX));
                        //    //}

                        //    //root2.RaiseEvent(new RoutedEventArgs(HyperThemeHost.StartSwfArtworkAnimationsEvent));

                        //};

                        //}));
                    };
                }
            }
            catch (Exception ex)
            {
                this.Children.Clear();
                this.Children.Add(new TextBlock() { Text = ex.Message, FontSize = 11 });
            }
        }

        private void SwfXamlHost_StartSwfArtworkAnimations(object sender, RoutedEventArgs e)
        {
            object[] args = (object[])e.OriginalSource;

            var xamlTopContainer = (FrameworkElement)args[0];
            var root = xamlTopContainer.FindName("_root") as FrameworkElement;

            if (args.Length == 2 && args[1] != null)
            {
                dynamic offsets = args[1];
                Canvas.SetTop(root, Math.Abs(offsets.MinOffsetY));
                Canvas.SetLeft(root, Math.Abs(offsets.MinOffsetX));
            }

            root.RaiseEvent(new RoutedEventArgs(HyperThemeHost.StartSwfArtworkAnimationsEvent));
        }

    }
}
