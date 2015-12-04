using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Animation;
using System.Threading;
using HyperSpinClone.Classes;
using System.Diagnostics;
using Gajatko.IniFiles;
using System.Threading.Tasks;
using System.Configuration;
using HyperSearch.Classes;

namespace HyperSearch.Windows
{
    /// <summary>
    /// Interaction logic for GameSearchWindow.xaml
    /// </summary>
    public partial class GameSearchWindow : Window
    {
        private const string SystemSummaryAllCollectionName = "(All)";

        public bool ShowSystemImagesOnFilter { get; set; }
        public bool ShowSystemImagesOnResults { get; set; }
        public bool ShowWheelImagesOnResults { get; set; }

        public bool ShowGameVideos { get; private set; }

        private enum ViewState
        {
            SetupSearch,
            Searching,
            Results
        }

  

        public enum SearchList
        {
            Normal,
            Favourites,
            Genre
        }


        private Thread _searchThread;
        private ViewState CurrentViewState { get; set; }
        private FrameworkElement _currentViewElement;

        private DispatcherTimerContainingAction _videoPopupTimer;

        private int VideoPopupTimeoutInMilliseconds { get; set; }

        public TextInputType InputType { get; set; }

        public int ResultListViewItemResDependentHeight
        {
            get {
                //System.Windows.SystemParameters.PrimaryScreenHeight
                var screenH = this.Height;
                if (screenH <= 480) // 640x480
                {
                    return 40;
                }
                else if (screenH <= 600) // 800x600
                {
                    return 60;
                }
                else
                {
                    return 80;
                }
            }
        }

        public int ResultListViewItemResDependentHeightSelected
        {
            get
            {
                var screenH = this.Height;

                if (screenH <= 480) // 640x480
                {
                    return 60;
                }
                else if (screenH <= 600) // 800x600
                {
                    return 80;
                }
                else
                {
                    return 120;
                }
            }
        }

        public int ResultListViewItemGameImageResDependentMargin
        {
            get
            {
                var screenH = this.Height;
                if (screenH <= 480) // 640x480
                {
                    return 4;
                }
                else if (screenH <= 600) // 800x600
                {
                    return 5;
                }
                else
                {
                    return 10;
                }
            }
        }

        public int ResultListViewItemFontSize
        {
            get
            {
                var screenH = this.Height;

                if (screenH <= 480) // 640x480
                {
                    return 14;
                }
                else if (screenH <= 600) // 800x600
                {
                    return 16;
                }
                else
                {
                    return 22;
                }
            }
        }

        private SearchList SearchListToUse { get; set; }

        public GameSearchWindow(SearchList searchList)
         {
            InitializeComponent();
            
            if (searchList == SearchList.Normal) searchListIndicator.Text = "(Full search)";
            else if (searchList == SearchList.Favourites) searchListIndicator.Text = "(Favourites search)";
            else if (searchList == SearchList.Genre) searchListIndicator.Text = "(Genre search)";

            CurrentViewState = ViewState.SetupSearch;
            _currentViewElement = this.searchSetupGrid;

            searchSetupGrid.Visibility = System.Windows.Visibility.Visible;
            searchingMsg.Visibility = System.Windows.Visibility.Collapsed;
            resultsGrid.Visibility = System.Windows.Visibility.Collapsed;

            this.SearchListToUse = searchList;
            this.InputType = (HyperSearchSettings.Instance().General.KeyboardType ?? TextInputType.AtoZ);
            bool hideCursor = HyperSearchSettings.Instance().General.HideMouseCursor ?? false;

            if (hideCursor)
            {
                this.Cursor = Cursors.None;
            }
        }


        private void RunSelectedResult()
        {
            try
            {
                if (resultListView.SelectedItem != null)
                {
                    dynamic item = resultListView.SelectedItem;

                    /****
                    LoadingHsWindow loadingHsProcessWin = new LoadingHsWindow();

                    // take a screenshot of the HS window
                    var bmp = KbHook.GetWindowCaptureAsBitmap(MainWindow._lastHyperspinHwnd.Value);

                    var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                             bmp.GetHbitmap(),
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());


                    MainWindow.PositionWindowOnRect(loadingHsProcessWin, MainWindow._lastHyperspinRect.Value);

                    loadingHsProcessWin.ImageSource = bmpSrc;

                    loadingHsProcessWin.Show();
               
                    var procs = Process.GetProcessesByName("HyperSpin");

                    Console.WriteLine("Killing {0}", procs[0].Id);

                    procs[0].Kill();
                    procs[0].WaitForExit();

                    var mainSettingsIniPath = Global.BuildFilePathInHyperspinDir("Settings\\Settings.ini");
                    var systemSettingsIniPath = Global.BuildFilePathInHyperspinDir("Settings\\{0}.ini", item.SystemName);

                    var iniMainSettings = IniFile.FromFile(mainSettingsIniPath);
                    var iniSystemSettings = IniFile.FromFile(mainSettingsIniPath);

                    iniMainSettings["Main"]["Single_Mode_Name"] = item.SystemName;
                    iniMainSettings["Main"]["Last_System"] = item.SystemName;
                    iniMainSettings.Save(mainSettingsIniPath);

                    iniSystemSettings["navigation"]["use_last_game"] = "true";
                    iniSystemSettings["navigation"]["last_game"] = item.name;
                    iniSystemSettings.Save(systemSettingsIniPath);
                    
                    //new System.Threading.Thread(new ThreadStart(()=>Util.StartProcess(hsExe, null))).Start();
                    ThreadPool.QueueUserWorkItem(delegate
                    {

                        var hsExe = Global.BuildFilePathInHyperspinDir("Hyperspin.exe");
                        Process.Start(hsExe);
                        //Util.StartProcess(hsExe, null);
                    });
                    //return; // dont need any of the rest if in single wheel mode! Well...we can use it to wait and to know when to kill our window?
                    ThreadPool.QueueUserWorkItem(delegate
                   {
                       // wait for HS.exe to startup again
                       do
                       {
                           System.Threading.Thread.Sleep(100);
                       }
                       while (Process.GetProcessesByName("HyperSpin").Length == 0);

                       procs = Process.GetProcessesByName("HyperSpin");

                       var hsProcess = procs[0];

                       Console.WriteLine("New HS {0}", hsProcess.Id);

                       int pid;

                       IntPtr? hsWinHwnd = null;
                       HyperSearch.MainWindow.RECT? hsWinRect = null;

                       // wait for the main HS window to be present
                       do
                       {
                           System.Threading.Thread.Sleep(100);
                           var allWindows = MainWindow.GetWindows();


                           // attempt to find the HS window
                           foreach (var c in allWindows)
                           {
                               IntPtr hwnd = (IntPtr)c;

                               MainWindow.GetWindowThreadProcessId(hwnd, out pid);

                               if (pid == hsProcess.Id)
                               {
                                   HyperSearch.MainWindow.RECT winRC;
                                   MainWindow.GetWindowRect(hwnd, out winRC);

                                   StringBuilder title = new StringBuilder(50);
                                   MainWindow.GetWindowText(hwnd, title, 50);

                                   if (title.ToString().ToLower() == "hyperspin" && winRC.Right - winRC.Left > 0)
                                   {
                                       hsWinRect = winRC;
                                       hsWinHwnd = hwnd;
                                       break;// looks like we found the HS window
                                   }
                               }
                           }

                       }
                       while (!hsWinHwnd.HasValue);


                       System.Threading.Thread.Sleep(2000);
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            loadingHsProcessWin.Close();

                        }));
                    
                       Console.WriteLine("hsWinHwnd:{0}", hsWinHwnd.Value.ToInt32());

                       //System.Threading.Thread.Sleep(800);

                       KbHook.SetForegroundWindow(hsWinHwnd.Value.ToInt32());
                       KbHook.SendKeyDownAndUp(System.Windows.Forms.Keys.Enter);



                   });

                    if (MainWindow._lastSearchWindow != null)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MainWindow._lastSearchWindow.Close();
                            MainWindow._lastSearchWindow = null;

                        }));
                    }

                    return;
                     * ***/


                    var hlExe = HyperSearchSettings.Instance().General.RocketLauncherExePath;
                    var hlArgs = string.Format("\"{0}\" \"{1}\"", item.SystemName, item.name);

                    MainWindow.LogStatic("Launching: {0} {1}", hlExe, hlArgs);
                    Util.StartProcess(hlExe, hlArgs);

                    MainWindow.LEDBlinkGameLaunch(item.SystemName, item.name);

                    if (MainWindow._lastSearchWindow != null)
                    {
                        MainWindow._lastSearchWindow.Close();
                        MainWindow._lastSearchWindow = null;
                    }

                }
            }
            catch(Exception ex)
            {
                MainWindow.LogStatic(ex.ToString());
            }
        }

        private void MoveToNewView(FrameworkElement toViewElement, EventHandler outCompleted, EventHandler inCompleted)
        {
            if (_searchThread != null && _searchThread.IsAlive)
            {
                _searchThread.Abort();
            }

            var h = this.Height;
            var durationOut = 0.3;
            var durationIn = 0.2;

            ThicknessAnimation animMoveOut = new ThicknessAnimation();

            animMoveOut.BeginTime = TimeSpan.FromSeconds(0);
            animMoveOut.Duration = TimeSpan.FromSeconds(durationOut);
            animMoveOut.From = new Thickness(0, 0, 0, 0);
            animMoveOut.To = new Thickness(0, h, 0, -h);
            animMoveOut.FillBehavior = FillBehavior.Stop;
            animMoveOut.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseIn, Exponent = 2 };

            animMoveOut.Completed += (s, e) =>
            {
                _currentViewElement.Visibility = System.Windows.Visibility.Collapsed;
                if (outCompleted != null) outCompleted(s, e);
            };

            ThicknessAnimation animMoveIn = new ThicknessAnimation();

            animMoveIn.BeginTime = TimeSpan.FromSeconds(durationOut + 0.05);
            animMoveIn.Duration = TimeSpan.FromSeconds(durationIn);
            animMoveIn.From = new Thickness(0, h, 0, -h);
            animMoveIn.To = new Thickness(0, 0, 0, 0);
            animMoveIn.FillBehavior = FillBehavior.Stop;
            animMoveIn.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut, Exponent = 2 };

            animMoveIn.Completed += (s, e) =>
            {
                _currentViewElement = toViewElement;
                toViewElement.Margin = new Thickness(0, 0, 0, 0);

                if (inCompleted != null) inCompleted(s, e);
            };

            toViewElement.Margin = new Thickness(0, h, 0, -h);
            toViewElement.Visibility = System.Windows.Visibility.Visible;

            _currentViewElement.BeginAnimation(FrameworkElement.MarginProperty, animMoveOut);
            toViewElement.BeginAnimation(FrameworkElement.MarginProperty, animMoveIn);
        }

        private void PerformSearch()
        {
            CurrentViewState = ViewState.Searching;

            CloseAndStopVideoWin();

            MoveToNewView(searchingMsg, null, (sender, e) =>
            {
                if (_searchThread != null && _searchThread.IsAlive)
                {
                    _searchThread.Abort();
                }

                // start off actual search on a background thread
                PerformSearchTask task = new PerformSearchTask(this, this.SearchListToUse);

                task.ShowSystemImagesOnFilter = this.ShowSystemImagesOnFilter;
                task.ShowSystemImagesOnResults = this.ShowSystemImagesOnResults;
                task.ShowWheelImagesOnResults = this.ShowWheelImagesOnResults;

                _searchThread = new Thread(new ParameterizedThreadStart(task.Run));

                _searchThread.Start(searchTerm.Text);
            });
        }

        private void resultListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeResultListGridViewColumns((ListView)sender);
        }

        private void ResizeResultListGridViewColumns(ListView listView)
        {
            try
            {              
                // http://stackoverflow.com/questions/8331940/how-can-i-get-a-listview-gridviewcolumn-to-fill-the-remaining-space-in-my-grid/10526024#10526024
                GridView gView = listView.View as GridView;

                var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth; // take into account vertical scrollbar

                if (workingWidth == 0 || Double.IsNaN(workingWidth) || workingWidth < 0) return;

                SearchResultPerSystem systemFilter = systemSummaryListView.SelectedItem as SearchResultPerSystem;

                double col1 = 0.15;
                double col2 = 0.50;
                double col3 = 0.25;
                double col4 = 0.10;

                if (systemFilter != null && systemFilter.SystemName != SystemSummaryAllCollectionName && this.SearchListToUse != SearchList.Genre)
                {
                    col1 = 0;
                    col2 = 0.65;
                }

                gView.Columns[0].Width = workingWidth * col1;
                gView.Columns[1].Width = workingWidth * col2;
                gView.Columns[2].Width = workingWidth * col3;
                gView.Columns[3].Width = workingWidth * col4;
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }


        private void resultListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (resultListView.SelectedItem == null)
            {
                selectedItemText.Text = "(no game selected)";
                return;
            }

            SearchResult selectedItem = resultListView.SelectedItem as SearchResult;

            int ix = resultListView.SelectedIndex + 1;

            selectedItemText.Text = string.Format("{4}. {0} - {1}, {2}, {3}", selectedItem.SystemName, selectedItem.description, selectedItem.manufacturer, selectedItem.year, ix).Replace(", ,", ",").TrimEnd(' ', ',');

            MainWindow.LEDBlinkGameSelected(selectedItem.name);

            CloseAndStopVideoWin();

            // only attempt to video popup if result view has focus
            if (ShowGameVideos && Keyboard.FocusedElement == resultListView.ItemContainerGenerator.ContainerFromItem(selectedItem))
            {
                var timeout = VideoPopupTimeoutInMilliseconds;

                if (videoBorder.Visibility == System.Windows.Visibility.Visible) timeout = 0;

                _videoPopupTimer = Util.SetTimeout(timeout, new Action(() =>
                    {
                        try
                        {
                            if (CurrentViewState != ViewState.Results) return;

                            videoBorder.Width = this.ActualWidth * 0.4;
                            videoBorder.Height = this.ActualHeight * 0.4;
                            videoBorder.Visibility = System.Windows.Visibility.Visible;

                            LoadVideo(selectedItem.SystemName, selectedItem.name);
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.HandleException(ex);
                        }
                    }));
            }
        }

        private void video_MediaOpened(object sender, RoutedEventArgs e)
        {
            
        }

        public void LoadVideo(string systemName, string gameName)
        {
            try
            {
                var settingsFilePath = Global.BuildFilePathInHyperspinDir("Settings\\{0}.ini", systemName);
                string customVideoPath = null;

                if (File.Exists(settingsFilePath))
                {
                    try
                    {
                        WheelSettings wheelSettings = new WheelSettings();

                        wheelSettings.InitFromIniFile(settingsFilePath);

                        customVideoPath = wheelSettings.VideoDefaultsSection.CustomVideoPath;

                        Uri uriTester;

                        if (Uri.TryCreate(customVideoPath, UriKind.RelativeOrAbsolute, out uriTester))
                        {
                            // relative path
                            if (!uriTester.IsAbsoluteUri)
                            {
                                customVideoPath = new Uri(new Uri(HyperSearchSettings.Instance().General.HyperSpinPathAbsolute, UriKind.Absolute), uriTester.ToString()).LocalPath;
                            }
                        }

                        if (!string.IsNullOrEmpty(customVideoPath) && !Directory.Exists(customVideoPath))
                        {
                            MainWindow.LogStatic("{0} has a custom video path defined ({1}) but that path does not exist, so reverting back to default.", systemName, customVideoPath);
                            customVideoPath = null;
                        }
                    }
                    catch(Exception ex) 
                    {
                        MainWindow.LogStatic("Failed to read settings file: {0}", settingsFilePath);
                        ErrorHandler.HandleException(ex);
                    }
                }

                var baseVideoPath = Global.BuildFilePathInHyperspinDir("Media\\{0}\\Video", systemName);

                if (!string.IsNullOrEmpty(customVideoPath))
                {
                    baseVideoPath = customVideoPath.TrimEnd('\\');
                    
                }

                MainWindow.LogStaticVerbose("Base video path={0}", customVideoPath);

                var mp4Path = string.Format("{0}\\{1}.mp4", baseVideoPath, gameName);
                var flvPath = string.Format("{0}\\{1}.flv", baseVideoPath, gameName);
                var pngPath = string.Format("{0}\\{1}.png", baseVideoPath, gameName);

                if (File.Exists(mp4Path))
                {
                    MainWindow.LogStaticVerbose("Video.Source={0}", mp4Path);
                    video.Source = new Uri(mp4Path, UriKind.Absolute);
                }
                else if (File.Exists(flvPath))
                {
                    MainWindow.LogStaticVerbose("Video.Source={0}", flvPath);
                    video.Source = new Uri(flvPath, UriKind.Absolute);
                }
                else if (File.Exists(pngPath))
                {
                    MainWindow.LogStaticVerbose("Video.Source={0}", pngPath);
                    video.Source = new Uri(pngPath, UriKind.Absolute);
                }
                else
                {
                    MainWindow.LogStaticVerbose("No video found for {0}/{1}. Looking for placeholder.",systemName, gameName);

                    var mp4NoVideo = Global.BuildFilePathInHyperspinDir("Media\\Frontend\\Video\\No Video.mp4");
                    var flvNoVideo = Global.BuildFilePathInHyperspinDir("Media\\Frontend\\Video\\No Video.flv");

                    if (File.Exists(mp4NoVideo))
                    {
                        MainWindow.LogStaticVerbose("Video.Source={0}", mp4NoVideo);
                        video.Source = new Uri(mp4NoVideo, UriKind.Absolute);
                    }
                    else if (File.Exists(flvNoVideo))
                    {
                        MainWindow.LogStaticVerbose("Video.Source={0}", flvNoVideo);
                        video.Source = new Uri(flvNoVideo, UriKind.Absolute);
                    }
                    else
                    {
                        MainWindow.LogStaticVerbose("No placeholder video found!");
                    }
                }

                if (video.Source != null && this.CurrentViewState == ViewState.Results) video.Play();
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void CloseAndStopVideoWin()
        {
            if (_videoPopupTimer != null)
            {
                _videoPopupTimer.Stop();
                _videoPopupTimer = null;
            }
            if (video.Source != null) video.Stop();
            video.Source = null;
            videoBorder.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //            keyboard.AnimateOpen();
            try
            {
                this.ShowGameVideos = HyperSearchSettings.Instance().Misc.EnableGameVideos;
                this.VideoPopupTimeoutInMilliseconds = HyperSearchSettings.Instance().Misc.GameVideoPopupTimeoutInMilliseconds;

                if (this.InputType == TextInputType.AtoZ)
                {
                    LoadAtoZControl();
                }
                else if (this.InputType == TextInputType.Qwerty)
                {
                    LoadQwertyControl();
                }
                else if (this.InputType == TextInputType.Azerty)
                {
                    LoadAzertyControl();
                }
                else if (this.InputType == TextInputType.Orb)
                {
                    LoadOrbControl();
                }

                var soundsPath = Global.BuildFilePathInHyperspinDir(@"Media\Frontend\Sounds");
                
                SystemSoundPlayer.Init(mainGrid, soundsPath);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void LoadOrbControl()
        {
            var kb = new OrbKeyboard() { Width = 800, Height = 600 };

            RegisterName("orb", kb);

            var buttonTemplateXamlFilePath = Global.BuildFilePathInResourceDir("OnScreenKeyboardButtonTemplate.xaml");

            if (!File.Exists(buttonTemplateXamlFilePath)) throw new FileNotFoundException("Failed to find the on-screen keyboard's button template XAML file.", "Resources\\OnScreenKeyboardButtonTemplate.xaml");

            kb.InitFromButtonTemplate(File.ReadAllText(buttonTemplateXamlFilePath));

            Util.SetTimeout(80, new Action(() => kb.AnimateOpen()));
            Util.SetTimeout(90, new Action(() => kb.Focus()));

            kb.AttachedTextBox = searchTerm;
            kb.OnOskKeyPressed += OnOskKeyPressed;
            keyboardContainer.Child = new Viewbox() { Child = kb };
        }

        private void LoadAtoZControl()
        {
            var kb = new AtoZKeyboard();

            RegisterName("a2z", kb);
            
            kb.AttachedTextBox = searchTerm;
            kb.OnOskKeyPressed += OnOskKeyPressed;
            keyboardContainer.Child = new Viewbox() { Child = kb };
        }

        private void LoadQwertyControl()
        {
            var kb = new QwertyKeyboard();

            RegisterName("qwerty", kb);

            kb.AttachedTextBox = searchTerm;
            kb.OnOskKeyPressed += OnOskKeyPressed;
            keyboardContainer.Child = new Viewbox() { Child = kb };
        }

        private void LoadAzertyControl()
        {
            var kb = new AzertyKeyboard();

            RegisterName("a2z", kb);

            kb.AttachedTextBox = searchTerm;
            kb.OnOskKeyPressed += OnOskKeyPressed;
            keyboardContainer.Child = new Viewbox() { Child = kb };
        }

        private void OnOskKeyPressed(string charRepresentation, OskSpecialKey specialKey)
        {
            try
            {
                if (specialKey == OskSpecialKey.Done && CurrentViewState == ViewState.SetupSearch)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        PerformSearch();
                    }));
                }
                else if (specialKey == OskSpecialKey.Exit)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void HandlePreviousScreenButtonPressed()
        {
            if (CurrentViewState == ViewState.SetupSearch)
            {
                this.Close();
            }
            else // move back to prev view
            {
                var fe = Keyboard.FocusedElement;
                if (fe != null && fe is ListViewItem)
                {
                    if ((fe as ListViewItem).Content is SearchResult && videoBorder.Visibility != Visibility.Hidden)
                    {
                        //resultListView.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));

                        //if (systemSummaryListView.SelectedItem != null)
                        {
                            CloseAndStopVideoWin();
                            //!systemSummaryListView.SelectAndFocusItem(systemSummaryListView.SelectedIndex);
                        }
                        return;
                    }

                }

                CurrentViewState = ViewState.SetupSearch;

                MoveToNewView(searchSetupGrid, null, (ss, ee) => { ((keyboardContainer.Child as Viewbox).Child as OskBaseControl).FocusInput(); });
            }
        }

        private void resultListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //?RunSelectedResult();
        }

        private void systemSummaryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SearchResultPerSystem item = (SearchResultPerSystem)systemSummaryListView.SelectedItem;

                if (item == null)
                {
                    return;
                }

                // do not send the LED Blinky command for the "(All)" collection
                if (!item.SystemName.EqualsCI(SystemSummaryAllCollectionName))
                {
                    MainWindow.LEDBlinkSystemSelected(item.SystemName);
                }

                ResizeResultListGridViewColumns(resultListView);

                resultListView.ItemsSource = item.GameList;
                
                if (resultListView.Items.Count > 0)
                {

                    //resultListView.SelectAndFocusItem(callback: (ss, ee) => resultListView_SelectionChanged(null, null));
                    Util.SetTimeout(0, new Action(() =>
                    {
                        resultListView.SelectedIndex = 0;
                        resultListView.ScrollIntoView(resultListView.SelectedItem);
                        resultListView.SelectAndFocusItem(callback: (ss, ee) => resultListView_SelectionChanged(null, null));
                    }));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {


            try
            {
                e.Handled = true;

                var elementWithFocus = Keyboard.FocusedElement as UIElement;
                var settings = HyperSearchSettings.Instance().Input;
                ListView focussedLV = null;

                if (Keyboard.FocusedElement == resultListView && resultListView.Items.Count > 0)
                {
                    resultListView.SelectAndFocusItem();
                }

                if (Keyboard.FocusedElement is ListViewItem) focussedLV = ((FrameworkElement)Keyboard.FocusedElement).GetAncestorOfType<ListView>();


                if (focussedLV == systemSummaryListView)
                {
                    resultListView.SelectAndFocusItem();
                    focussedLV = resultListView;
                }

                if (HyperSearchSettings.Instance().Input.Minimize.Is(e.Key))
                {
                    this.Hide();
                    return;
                }
                else if (settings.Action.Is(e.Key))
                {
                    if (focussedLV == resultListView)
                    {
                        RunSelectedResult();
                    }
                    else
                    {
                        e.Handled = false;
                        return;
                    }

                }
                else if (settings.Up.Is(e.Key))
                {
                    //if (resultListView.SelectedIndex == 0)
                    //{
                    //    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    //    if (systemSummaryListView.SelectedItem != null)
                    //    {
                    //        CloseAndStopVideoWin();
                    //        systemSummaryListView.SelectAndFocusItem(systemSummaryListView.SelectedIndex);
                    //    }

                    //    return;
                    //}

                    
                    

                    if (focussedLV == resultListView )
                    {
                        if (resultListView.SelectedIndex > 0)
                        {
                            elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                        }
                        else
                        {
                            resultListView.SelectedIndex = resultListView.Items.Count-1;
                            resultListView.ScrollIntoView(resultListView.SelectedItem);
                            resultListView.SelectAndFocusItem(resultListView.Items.Count - 1);
                        }

                    }
                    else
                    {
                        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    }
                }
                else if (settings.Right.Is(e.Key))
                {
                    if (focussedLV == resultListView)
                    {
                        if (systemSummaryListView.SelectedIndex < systemSummaryListView.Items.Count - 1)
                        {
                            systemSummaryListView.SelectedIndex = systemSummaryListView.SelectedIndex + 1;
                            systemSummaryListView.ScrollIntoView(systemSummaryListView.SelectedItem);
                        }
                        else
                        {
                            systemSummaryListView.SelectAndFocusItem(0);
                        }

                    }
                    else
                    {
                        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    }
                }
                else if (settings.Down.Is(e.Key))
                {
                    if (focussedLV == resultListView)
                    {
                        if (resultListView.SelectedIndex < resultListView.Items.Count - 1)
                        {
                            resultListView.SelectAndFocusItem(resultListView.SelectedIndex + 1, (ss, ee) => { resultListView_SelectionChanged(this, null); });
                        }
                        else
                        {
                            resultListView.SelectedIndex = 0;
                            resultListView.ScrollIntoView(resultListView.SelectedItem);
                        }

                    }
                    else
                    {
                        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    }
                }
                else if (settings.Left.Is(e.Key))
                {
                    if (focussedLV == resultListView)
                    {
                        if (systemSummaryListView.SelectedIndex >= 1)
                        {
                            systemSummaryListView.SelectedIndex = systemSummaryListView.SelectedIndex - 1;
                            systemSummaryListView.ScrollIntoView(systemSummaryListView.SelectedItem);
                        }
                        else
                        {
                            systemSummaryListView.SelectAndFocusItem(systemSummaryListView.Items.Count - 1);
                        }
                    }
                    else
                    {
                        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    }
                }
                else if (settings.Back.Is(e.Key))
                {
                    HandlePreviousScreenButtonPressed();
                }
                else if (settings.Exit.Is(e.Key))
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }

        }

/*        private void systemSummaryListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                e.Handled = true;
                var settings = HyperSearchSettings.Instance().Input;
                var elementWithFocus = Keyboard.FocusedElement as UIElement;

                if (settings.Down.Is(e.Key))
                {
                    if (resultListView.Items.Count > 0)
                    {
                        elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        resultListView.SelectAndFocusItem();
                        resultListView_SelectionChanged(null, null);
                    }
                }
                else if (settings.Up.Is(e.Key))// block UP as it makes the listview behave stupid
                {
                    return;
                }
                else if (settings.Left.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    return;
                }
                else if (settings.Right.Is(e.Key))
                {
                    elementWithFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    return;
                }
                else if (settings.Back.Is(e.Key))
                {
                    HandlePreviousScreenButtonPressed();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }*/

        /*
        private void gameSearchWindow_KeyDown(object sender, KeyEventArgs e)
        {
            var hqSettings = MainWindow.HqSettings;
            var cabMode = MainWindow.IsCabModeEnabled;

            var p1Action = HyperHQSettings.ActionScriptKeyCodeToVK(hqSettings.P1ControlsSection.Start);
            var p1Up = HyperHQSettings.ActionScriptKeyCodeToVK(hqSettings.P1ControlsSection.Up);
            var p1Down = HyperHQSettings.ActionScriptKeyCodeToVK(hqSettings.P1ControlsSection.Down);
            var p1Exit = HyperHQSettings.ActionScriptKeyCodeToVK(hqSettings.P1ControlsSection.Exit);

            var p2Action = HyperHQSettings.ActionScriptKeyCodeToVK(hqSettings.P2ControlsSection.Start);
            var p2Up = HyperHQSettings.ActionScriptKeyCodeToVK(hqSettings.P2ControlsSection.Up);
            var p2Down = HyperHQSettings.ActionScriptKeyCodeToVK(hqSettings.P2ControlsSection.Down);
            var p2Exit = HyperHQSettings.ActionScriptKeyCodeToVK(hqSettings.P2ControlsSection.Exit);

            if (cabMode)
            {// prefer HyperSpin config
                if (e.Key.EqAny(p1Exit, p2Exit))
                {
                    HandlePreviousScreenButtonPressed();
                }
                else if (e.Key.EqAny(p1Action, p2Action))
                {
                    if (CurrentViewState == ViewState.Results)
                    {
                        RunSelectedResult();
                    }
                }
            }
            else
            {
                if (e.Key.EqAny(p1Exit, p2Exit, Key.Back/*,Key.Escape* /))
                {
                    HandlePreviousScreenButtonPressed();
                }
                else if (e.Key.EqAny(p1Action, p2Action, Key.Enter))
                {
                    if (CurrentViewState == ViewState.Results)
                    {
                        RunSelectedResult();
                    }
                }
            }


        }
        */

        private void HandleSearchComplete(int totalCount, List<SearchResultPerSystem> perSystemResults, int timeTakenInMS)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                systemSummaryListView.ItemsSource = perSystemResults;

                if (systemSummaryListView.Items.Count > 0)
                {
                    systemSummaryListView.SelectedIndex = 0;
                }

                CurrentViewState = ViewState.Results;

                noResultsMsg.Visibility = (totalCount == 0) ? Visibility.Visible : Visibility.Collapsed;
                resultListView.Visibility = noResultsMsg.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
                selectedItemText.Visibility = resultListView.Visibility;

                MoveToNewView(resultsGrid, null, (s, e) =>
                {
                    if (totalCount > 0)
                    {
                        if (systemSummaryListView.Items.Count > 0)
                        {
                            systemSummaryListView.SelectedIndex = 0;
                            systemSummaryListView.ScrollIntoView(systemSummaryListView.SelectedItem);
                        }
                        resultListView.SelectAndFocusItem(callback: (ss,ee)=>resultListView_SelectionChanged(null, null));
                    }
                });
            }));
        }

        public class SearchResult
        {
            public SearchResult()
            {

            }

            public string name { get; set; }
            public string description { get; set; }
            public string manufacturer { get; set; }
            public string year { get; set; }
            public string genre { get; set; }
            public SortedSet<string> AllGenres { get; set; }
            public string SystemName { get; set; }
            public Uri SystemImageSourceUri { get; set; }
            public Uri GameImageSourceUri { get; set; }

            public override string ToString()
            {
                return string.Format("{0}\\{1}", SystemName??"(null)", name??"(null)");
            }
        }

      


        public class PerformSearchTask
        {
            private GameSearchWindow _gameSearchWindow;
            private static List<GameXmlDatabase> _allGamesList = new List<GameXmlDatabase>();
            private static List<GameXmlDatabase> _allFavouritesList = new List<GameXmlDatabase>();
            private static SortedSet<string> _genreLookupSet = new SortedSet<string>();

            private static List<string> _genreImageLocations;

            private SearchMode SearchMode { get; set; }

            //private static Dictionary<string/*SystemName*/, SortedList<string, string/*RomName*/>> _allFavouritesList = new Dictionary<string, SortedList<string, string>>();
            

            public bool ShowSystemImagesOnFilter { get; set; }
            public bool ShowSystemImagesOnResults { get;set; }
            public bool ShowWheelImagesOnResults { get; set; }

            private SearchList SearchList { get; set; }
            

            public PerformSearchTask(GameSearchWindow gameSearchWin, SearchList searchList)
            {
                _gameSearchWindow = gameSearchWin;
                this.SearchList = searchList;

                this.SearchMode = HyperSearchSettings.Instance().General.SearchMode;
                MainWindow.LogStatic("SearchMode: {0}", HyperSearchSettings.Instance().General.SearchMode);
            }

            public static void LoadGenreImageLocations()
            {
                try
                {
                    _genreImageLocations = new List<string>();

                    var locations = HyperSearchSettings.Instance().Misc.GenreWheelImageLocations;

                    if (locations != null && locations.Count > 0)
                    {
                        bool isRelative;

                        foreach(var loc in locations)
                        {
                            _genreImageLocations.Add(Util.AbsolutePath(loc, out isRelative));
                        }
                    }

                }
                catch(Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }

            public static int LoadFullGameList()
            {
                lock (_allGamesList)
                {
                    // TODO: Consider caching DB List and loading that from a single file on startup. But then we have need some way of clearing the cache again and on my machine and config that will only save 2 seconds on intial startup.
                    List<MenuXmlDatabase> dbList = new List<MenuXmlDatabase>();

                    var preloadAllXmlDbsPerf = PerformanceTracker.Begin("GAME SEARCH: PRELOAD");

                    List<GameXmlDatabase> gameList = null;
                    
                    if (MainWindow.HqSettings != null && MainWindow.HqSettings.MainSection != null && MainWindow.HqSettings.MainSection.MenuMode.EqualsCI("single"))
                    {
                        MainWindow.LogStatic("Single mode: {0}", MainWindow.HqSettings.MainSection.SingleModeName);
                        gameList = new List<GameXmlDatabase>() { new GameXmlDatabase() { name = MainWindow.HqSettings.MainSection.SingleModeName } };
                    }
                    else
                    {
                        var mainMenuDbPath = Global.BuildFilePathInHyperspinDir("Databases\\Main Menu\\Main Menu.xml");

                        MainWindow.LogStatic("\tSearchDB: Parsing {0}...", mainMenuDbPath);

                        if (!File.Exists(mainMenuDbPath))
                        {
                            MainWindow.LogStatic("\tWARNING! [Main Menu.xml] not found at: {0}\r\nMake sure your HyperSpin path is setup correctly.", mainMenuDbPath);
                            return 0;
                        }

                        var mainMenuDb = MenuXmlDatabase.LoadFromFile(mainMenuDbPath);

                        MainWindow.LogStatic("\tSearchDB: Found {0} entries in [Main Menu.xml]", mainMenuDb.GameList.Count);

                        gameList = mainMenuDb.GameList;
                    }
                    
                    var loadXmlDbsPerf = PerformanceTracker.Begin("Load ALL xml databases");

                    foreach (var system in gameList)
                    {
                        try
                        {
                            var dbFilePath = Global.BuildFilePathInHyperspinDir("Databases\\{0}\\{0}.xml", system.name);
                            MainWindow.LogStatic("\tSearchDB: Processing {0}", dbFilePath);

                            Console.WriteLine("----------- Loading: {0}", dbFilePath);
                            var db = MenuXmlDatabase.LoadFromFile(dbFilePath);

                            MainWindow.LogStatic("\t\tFound {0} entries", db.GameList.Count);

                            var settingsFilePath = Global.BuildFilePathInHyperspinDir("Settings\\{0}.ini", system.name);

                            if (File.Exists(settingsFilePath))
                            {
                                WheelSettings wheelSettings = new WheelSettings();

                                wheelSettings.InitFromIniFile(settingsFilePath);

                                if (wheelSettings.FilterSection.ParentsOnly)
                                {// remove all clones
                                    db.GameList.RemoveAll(g => !string.IsNullOrEmpty(g.cloneof));
                                }

                                int wheelRemoveCount = 0;
                                int themeRemoveCount = 0;

                                if (wheelSettings.FilterSection.WheelsOnly)
                                {
                                    for (int i = 0 ;i< db.GameList.Count;i++)
                                    {
                                        var expectedWheelImagePath = Global.BuildFilePathInHyperspinDir("Media\\{0}\\Images\\Wheel\\{1}.png", system.name, db.GameList[i].name);
                                        if (!File.Exists(expectedWheelImagePath))
                                        {
                                            db.GameList.Remove(db.GameList[i]);
                                            i--;
                                            wheelRemoveCount++;
                                        }
                                    }
                                }

                                if (wheelSettings.FilterSection.ThemesOnly)
                                {
                                    for (int i = 0; i < db.GameList.Count; i++)
                                    {
                                        var expectedThemePath = Global.BuildFilePathInHyperspinDir("Media\\{0}\\Themes\\{1}.zip", system.name, db.GameList[i].name);
                                        if (!File.Exists(expectedThemePath))
                                        {
                                            db.GameList.Remove(db.GameList[i]);
                                            i--;
                                            themeRemoveCount++;
                                        }
                                    }
                                }

                                MainWindow.LogStatic("\t\tRemoved: {0} (wheel filter), {1} (theme filter)", wheelRemoveCount, themeRemoveCount);
                            }

                            dbList.Add(db);
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.LogException(ex);
                        }
                    }

                    loadXmlDbsPerf.End();

                    var allGamesArray = (from d in dbList
                                         select d.GameList).ToArray();

                    List<GameXmlDatabase> fullGameList = new List<GameXmlDatabase>();

                    foreach (var a in allGamesArray)
                    {
                        fullGameList.AddRange(a);
                    }

                    _allGamesList = (from game in fullGameList
                                     where !string.IsNullOrEmpty(game.description)
                                     select game).ToList();

                    var p2 = PerformanceTracker.Begin("Game search: Initialise ALL urls");

                    foreach (var g in _allGamesList)
                    {
                        g.InitUris();
                    }

                    p2.End();

                    preloadAllXmlDbsPerf.End();
                }

                return _allGamesList.Count;
            }

            public static int LoadFavourites()
            {
                lock (_allFavouritesList)
                {
                    var dbPath = Global.BuildFilePathInHyperspinDir("Databases");

                    if (!Directory.Exists(dbPath))
                    {
                        MainWindow.LogStatic("WARNING! Databases folder does not exists at: {0}\r\nMake sure your HyperSpin path is setup correctly.", dbPath);
                        return 0; 
                    }


                    var favoriteFiles = Directory.EnumerateFiles(dbPath, "favorites.txt", SearchOption.AllDirectories).ToList();

                    MainWindow.LogStatic("Favourites: Found {0} file(s).", favoriteFiles.Count);


                    foreach (var fav in favoriteFiles)
                    {
                        var systemName = new FileInfo(fav).Directory.Name;

                        MainWindow.LogStatic("\tProcessing {0}...", fav);

                        var allLines = File.ReadAllLines(fav);
                        // for each valid entry, we need to match it up with the full game list to get to all the goodness...so this is really just an index into the full game list? yeah makes sense
                        MainWindow.LogStatic("\tFound {0} item(s)", allLines.Length);

                        var systemSpecificRomsOnly = from g in _allGamesList where g.ParentMenu.SystemName.EqualsCI(systemName) select g;

                        foreach (var romName in allLines)
                        {
                            var favRom = (from g in systemSpecificRomsOnly where g.name.EqualsCI(romName) select g).FirstOrDefault();

                            if (favRom == null)
                            {
                                MainWindow.LogStatic("\tWARNING: FAV failed to match up: {0}. System: {1}", romName ?? "(null)", systemName ?? "(null)");
                                continue; 
                            }

                            if (!_allFavouritesList.Contains(favRom))
                            {
                                _allFavouritesList.Add(favRom);
                            }
                        }
                    }

                    return _allFavouritesList.Count;
                }
            }

            public static int LoadGenres()
            {
                // run through each game in the full list and look at its genres
                Parallel.ForEach<GameXmlDatabase>(_allGamesList, new Action<GameXmlDatabase>(g =>
                    {
                        if (!string.IsNullOrEmpty(g.genre))
                        {
                            var genres = g.genre.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                            for (int i = 0; i < genres.Length; i++)
                            {
                                var genreKey = genres[i].ToUpper();

                                lock(_genreLookupSet)
                                {
                                    if (!_genreLookupSet.Contains(genreKey, StringComparer.OrdinalIgnoreCase))
                                    {
                                        _genreLookupSet.Add(genreKey);
                                    }                                 
                                }

                                if (g.AllGenres == null) g.AllGenres = new SortedSet<string>();

                                if (!g.AllGenres.Contains(genreKey, StringComparer.OrdinalIgnoreCase))
                                {
                                    g.AllGenres.Add(genreKey);
                                }
                            }

                           
                        }
                    }));

                
                return _genreLookupSet.Count();
            }

            public void Run(object data)
            {
                try
                {
                    System.Threading.Thread.CurrentThread.Name = "PerformGameSearchThread";

                    string searchTerm = (string)data;

                    List<GameXmlDatabase> sourceDB = null;

                    var searchPerf = PerformanceTracker.Begin("Search");
                    IOrderedEnumerable<GameXmlDatabase> matches;

                    switch (this.SearchList)
                    {
                        case SearchList.Normal:
                            sourceDB = _allGamesList;
                            break;
                        case SearchList.Favourites:
                            sourceDB = _allFavouritesList;
                            break;
                        case SearchList.Genre:
                            sourceDB = _allGamesList;
                            break;
                        default:
                            throw new Exception("Unsupported SearchList type: " + this.SearchList.ToString());
                    }

                    if (this.SearchMode == SearchMode.Contains)
                    {
                        matches = (from g in sourceDB
                                   where g != null
                                    && (g.description.Like(searchTerm)
                                    || g.manufacturer.Like(searchTerm)
                                       //|| g.name.Like(term)
                                    || (!string.IsNullOrEmpty(g.year) && g.year.Equals(searchTerm, StringComparison.OrdinalIgnoreCase)))
                                   orderby g.ParentMenu.SystemName, g.description
                                   select g);
                    }
                    else
                    {
                        matches = (from g in sourceDB
                                   where g != null &&
                                   (g.description.StartsWithSafe(searchTerm)
                                    || g.manufacturer.StartsWithSafe(searchTerm)
                                       //|| g.name.StartsWithSafe(term)
                                    || (!string.IsNullOrEmpty(g.year) && g.year.Equals(searchTerm, StringComparison.OrdinalIgnoreCase)))
                                   orderby g.ParentMenu.SystemName, g.description
                                   select g);
                    }

                    var results = (from m in matches
                                   select new SearchResult()
                                   {
                                       name = m.name,
                                       description = m.description,
                                       manufacturer = m.manufacturer,
                                       year = m.year,
                                       AllGenres = m.AllGenres,
                                       SystemName = m.ParentMenu.SystemName,
                                       SystemImageSourceUri = this.ShowSystemImagesOnResults? m.SystemImageSourceUri : null,
                                       GameImageSourceUri = this.ShowWheelImagesOnResults? m.GameImageSourceUri : null
                                   }).Distinct().ToList(); 
                    
                    searchPerf.End();

                    var systemWheelImageBasePath = Global.BuildFilePathInHyperspinDir(@"Media\Main Menu\Images\Wheel");
                    var altPath = Global.AlternativeSystemWheelSourceFolder;

                    if (!string.IsNullOrEmpty(altPath) && Directory.Exists(altPath))
                    {
                        foreach(var r  in results)
                        {
                            r.SystemImageSourceUri = new Uri(System.IO.Path.Combine(altPath, string.Format("{0}.png", r.SystemName)));
                        }
                    }

                    var p = PerformanceTracker.Begin("Search results - compile per System");

                    List<SearchResultPerSystem> perSystemResults = null;

                    if (this.SearchList != GameSearchWindow.SearchList.Genre)
                    {

                        if (!string.IsNullOrEmpty(altPath) && Directory.Exists(altPath))
                        {
                            systemWheelImageBasePath = altPath;
                        }

                        perSystemResults = (from r in results
                                                group r by r.SystemName into g
                                                orderby g.Key
                                                select new SearchResultPerSystem()
                                                {
                                                    SystemName = g.Key,
                                                    GameList = g.ToList(),
                                                    SystemImageSourceUri = ShowSystemImagesOnFilter ? new Uri(System.IO.Path.Combine(systemWheelImageBasePath, string.Format("{0}.png", g.Key))) : null
                                                }).ToList();
                    }
                    else // handle Genre list differently
                    {
                        var resultPerGenre = (from genre in _genreLookupSet
                                              from res in results
                                              where res.AllGenres != null && res.AllGenres.Contains(genre)
                                             // select res).ToList();
                                              select new SearchResult()
                                              {
                                                  name = res.name,
                                                  description = res.description,
                                                  manufacturer = res.manufacturer,
                                                  year = res.year,
                                                  genre = genre,
                                                  SystemName = res.SystemName,
                                                  GameImageSourceUri = res.GameImageSourceUri,
                                                  SystemImageSourceUri = res.SystemImageSourceUri
                                              }).ToList();

                        perSystemResults = (from r in resultPerGenre
                                            group r by r.genre into g
                                            orderby g.Key
                                            select new SearchResultPerSystem()
                                            {
                                                SystemName = g.Key,
                                                GameList = g.ToList(),
                                                SystemImageSourceUri = ShowSystemImagesOnFilter ? GetGenreImageUri(g.Key) : null
                                            }).ToList();                  

                        
                    }


                    // TODO: Order by some setting? Like SysName or GameCount asc/desc?

                    // we don't need an "(All)"
                    if (perSystemResults.Count > 1)
                    {
                        perSystemResults.Insert(0, new SearchResultPerSystem { SystemName = SystemSummaryAllCollectionName, GameList = results, SystemImageSourceUri = (Uri)null });
                    }

                    p.End();

                    _gameSearchWindow.HandleSearchComplete(results.Count, perSystemResults, (int)searchPerf.DurationInMilliseconds.Value);
                }
                catch (ThreadAbortException)
                {
                    // ignore TAEs
                }
                catch (Exception ex)
                {
                    ErrorHandler.HandleException(ex);
                }

            }


            private Uri GetGenreImageUri(string genre)
            {
                if (_genreImageLocations == null) return null;
                string path;
                foreach (var loc in _genreImageLocations)
                {
                    path = string.Format("{0}\\{1}.png", loc.TrimEnd('\\'), genre);

                    if (!File.Exists(path))
                    {
                        // try adding " Games" as this seems to be the HyperSpin convention 
                        path = string.Format("{0}\\{1} Games.png", loc.TrimEnd('\\'), genre);
                    }

                    if (File.Exists(path)) return new Uri(path, UriKind.Absolute);
                }

                return null;
            }
        }

        public class SearchResultPerSystem
        {
            public string SystemName { get; set; }
            public List<SearchResult> GameList { get; set; }
            public Uri SystemImageSourceUri { get; set; }
        }

        private void video_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MainWindow.LogStatic("Video failed: {0}, with error: {1}", (e.Source as MediaElement).Source.ToString(), e.ErrorException.ToString());
        }

       
    }
}
