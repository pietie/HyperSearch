using HyperSearch.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace HyperSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Mutex SingleInstance;

        public static HyperHQSettings HqSettings;
        private static bool ShowSystemImagesOnFilter { get; set; }
        private static bool ShowSystemImagesOnResults { get; set; }
        private static bool ShowWheelImagesOnResults { get; set; }

        public static bool IsCabModeEnabled { get; private set; }

        public static Windows.GameSearchWindow _lastSearchWindow = null;
        public static int? _lastHyperspinHwnd = null;
        public static RECT? _lastHyperspinRect = null;

        private KeyList _searchTriggerKey = null;
        private KeyList _favouritesTriggerKey = null;
        private KeyList _genreTriggerKey = null;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                bool createdNew;

                // allow only one instance to run at a time
                {
                    SingleInstance = new Mutex(true, "HyperSearch_SingleInstance", out createdNew);

                    if (!createdNew)
                    {
                        MessageBox.Show("There is already another instance of HyperSearch running. Only one instance is allowed.", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        Application.Current.Shutdown();
                        return;
                    }
                }

                ErrorHandler.ClearSessionFile();

                var stylesUri = new Uri("pack://siteoforigin:,,,/Styles.xaml", UriKind.RelativeOrAbsolute);

                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = stylesUri });

                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                this.Title = "HyperSearch - " + version + " - " + this.Title ?? "";

                App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

                
                InitSysTray();

                var triggerKeyConfig = new Dictionary<KeyList, int?>();

                // Trigger Keys
                {
                    _searchTriggerKey = GetKeyFromAppSetting("Keys.Trigger.Search", Key.F3);
                    _favouritesTriggerKey = GetKeyFromAppSetting("Keys.Trigger.Favourites", Key.F4);
                    _genreTriggerKey = GetKeyFromAppSetting("Keys.Trigger.Genre", Key.F5);

                    triggerKeyConfig.Add(_searchTriggerKey, ConfigurationManager.AppSettings["SearchTriggerDelayInMilliseconds"].ToIntNullable(0));
                    triggerKeyConfig.Add(_favouritesTriggerKey, ConfigurationManager.AppSettings["FavouritesTriggerDelayInMilliseconds"].ToIntNullable(0));
                    triggerKeyConfig.Add(_genreTriggerKey, ConfigurationManager.AppSettings["GenreTriggerDelayInMilliseconds"].ToIntNullable(0));

                }

                KbHook.TriggerKeyConfig = triggerKeyConfig;

                KbHook.OnTriggerKeyHit += OnTriggerKeyHit;
                KbHook.InstallHook();

                Log("Hook installed succesfully");


                var hsPath = ConfigurationManager.AppSettings["HyperspinPath"];
                Log("Hyperspin path: {0}", hsPath);

                var launcherPath = ConfigurationManager.AppSettings["LauncherPath"];

                if (string.IsNullOrEmpty(launcherPath))
                {
                    Log("LauncherPath not set. Defaulting to HyperSpin config and HyperLaunch.exe");
                }
                else
                {
                    Uri uriTester;

                    if (!Uri.TryCreate(launcherPath, UriKind.RelativeOrAbsolute, out uriTester))
                    {
                        Log("ERROR: The specified Launcher path is not a valid URI!");
                    }
                    else if (!uriTester.IsAbsoluteUri)
                    {// convert relative Uri to absolute
                        launcherPath = new Uri(new Uri(System.AppDomain.CurrentDomain.BaseDirectory, UriKind.Absolute), uriTester.ToString()).LocalPath;
                        Log("Specified Launcher path is a relative path, translating to absolute path: {0}", launcherPath);
                    }


                    Log("LauncherPath: {0}", launcherPath);
                    Global.LauncherFullPath = launcherPath;
                }

                Uri hsPathUriTester;

                if (!Uri.TryCreate(hsPath, UriKind.RelativeOrAbsolute, out hsPathUriTester))
                {
                    Log("ERROR: The specified HyperSpin path is not a valid URI!");
                }
                else if (!hsPathUriTester.IsAbsoluteUri)
                {// convert relative Uri to absolute
                    hsPath = new Uri(new Uri(System.AppDomain.CurrentDomain.BaseDirectory, UriKind.Absolute), hsPathUriTester.ToString()).LocalPath;
                    Log("Specified HyperSpin path is a relative path, translating to absolute path: {0}", hsPath);
                }

                Global.HsPath = hsPath;


                MainWindow.ShowSystemImagesOnFilter = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowSystemImagesOnFilter"]);
                MainWindow.ShowSystemImagesOnResults = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowSystemImagesOnResults"]);
                MainWindow.ShowWheelImagesOnResults = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowWheelImagesOnResults"]);

               
                var settingsPath = Global.BuildFilePathInHyperspinDir("Settings\\Settings.ini");
                HqSettings = new HyperHQSettings();
                HqSettings.InitFromIniFile(settingsPath);

                Log("Hyperspin settings loaded from: {0}", settingsPath);
                Log("HyperLaunch path: {0}", HqSettings.MainSection.HyperlaunchPath);
                Log("LEDBlinky path: {0}", HqSettings.LEDBlinkySection.Path);

                if (HqSettings != null && HqSettings.LEDBlinkySection != null && HqSettings.LEDBlinkySection.IsActive && !File.Exists(HqSettings.LEDBlinkySection.Path))
                {
                    Log("ERROR! LED blinky not found at configured location.");
                }


                Uri hlPathUriTester;

                if (!Uri.TryCreate(HqSettings.MainSection.HyperlaunchPath, UriKind.RelativeOrAbsolute, out hlPathUriTester))
                {
                    Log("ERROR: The specified HyperLaunch path is not a valid URI!");
                }
                else if (!hlPathUriTester.IsAbsoluteUri)
                {// convert relative Uri to absolute
                    HqSettings.MainSection.HyperlaunchPath = new Uri(new Uri(System.AppDomain.CurrentDomain.BaseDirectory, UriKind.Absolute), hlPathUriTester.ToString()).LocalPath;
                    Log("Specified HyperLaunch path is a relative path, translating to absolute path: {0}", HqSettings.MainSection.HyperlaunchPath);
                }

                IsCabModeEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["CabMode"]);

                Log("CabMode: {0}", IsCabModeEnabled);

                var minimizeKeyStr = ConfigurationManager.AppSettings["MinimizeKey"];

                if (!string.IsNullOrEmpty(minimizeKeyStr))
                {
                    System.Windows.Input.Key minKey;

                    if (Enum.TryParse<System.Windows.Input.Key>(minimizeKeyStr, out minKey))
                    {
                        Log("MinimizeKey: Enabled for {0}", minKey);

                        Global.MinimizeKeyOld = minKey;
                    }
                    else
                    {
                        Log("MinimizeKey: Invalid value specified: {0}", minimizeKeyStr);
                        Log("For valid values see https://msdn.microsoft.com/en-us/library/system.windows.input.key%28v=vs.110%29.aspx");
                    }
                }
                else
                {
                    Log("MinimizeKey: Not configured.");
                }


                var altGameWheelSourceFolder = ConfigurationManager.AppSettings["AlternativeGameWheelSourceFolder"];

                if (!string.IsNullOrEmpty(altGameWheelSourceFolder))
                {
                    Global.AlternativeGameWheelSourceFolder = altGameWheelSourceFolder;
                    Log("AlternativeGameWheelSourceFolder: {0}", altGameWheelSourceFolder);
                }
                else
                {
                    Log("AlternativeGameWheelSourceFolder not specified. Defaulting to 'Wheel'.");
                }


                Global.ActionKey = GetKeyFromAppSetting("Keys.Action");
                Global.BackKey = GetKeyFromAppSetting("Keys.Back");
                Global.ExitKey = GetKeyFromAppSetting("Keys.Exit");
                Global.MinimizeKey = GetKeyFromAppSetting("Keys.Minimize");

                Global.UpKey = GetKeyFromAppSetting("Keys.Up");
                Global.RightKey = GetKeyFromAppSetting("Keys.Right");
                Global.DownKey = GetKeyFromAppSetting("Keys.Down");
                Global.LeftKey = GetKeyFromAppSetting("Keys.Left");

                /*
                if (!actionKey.HasValue)
                {
                    Log("Keys.Action: Not configured.");
                }
                else if (actionKey == System.Windows.Input.Key.None)
                {
                    Log("Keys.Action: Invalid value specified.");
                    Log("For valid values see https://msdn.microsoft.com/en-us/library/system.windows.input.key%28v=vs.110%29.aspx");
                }
                else
                {
                    Log("Keys.Action: {0}", actionKey);
                    Global.ActionKey = actionKey;
                }*/


                // kick-off background initialising work (e.g. preparing the full search index)
                new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((new BackgroundThreadInitialiser()).Run)).Start(this); // wow, one line..this is a horrible idea.


                this.WindowState = WindowState.Minimized;
                this.Hide();
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void InitSysTray()
        {
            var rs = Application.GetResourceStream(new Uri("pack://application:,,,/search.ico"));
            var ni = new System.Windows.Forms.NotifyIcon();

            ni.Icon = new System.Drawing.Icon(rs.Stream);

            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            ni.Text = "HyperSearch - Double click to open log";

            var ctx = new System.Windows.Forms.ContextMenu();

            ctx.MenuItems.Add(new System.Windows.Forms.MenuItem("View log", (ss, ee) => { this.Show(); this.WindowState = System.Windows.WindowState.Normal; }));
            ctx.MenuItems.Add(new System.Windows.Forms.MenuItem("Exit", (ss, ee) => { Application.Current.Shutdown(); }));

            ni.ContextMenu = ctx;

            int balloonToolTipTimeoutInsMS = Convert.ToInt32(ConfigurationManager.AppSettings["BalloonToolTipTimeOutInMilliseconds"]);

            if (balloonToolTipTimeoutInsMS > 0)
            {
                ni.ShowBalloonTip(balloonToolTipTimeoutInsMS, "HyperSearch started", "Double click tray icon to bring up log window.", System.Windows.Forms.ToolTipIcon.Info);
            }
        }

        private KeyList GetKeyFromAppSetting(string appKey, Key defaultVal = Key.None)
        {
            var str = ConfigurationManager.AppSettings[appKey];

            if (!string.IsNullOrEmpty(str))
            {
                KeyList kl = new KeyList();

                var entries = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var e in entries)
                {
                    System.Windows.Input.Key key;

                    if (Enum.TryParse<System.Windows.Input.Key>(e, out key))
                    {
                        kl.Add(key);
                    }
                }

                return kl;
            }
            else
            {
                var def= new KeyList();
                def.Add(defaultVal);
                return def;
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
                Log("Unhandled exception: {0}", e.Exception.ToString());
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                KbHook.Unhook();

                if (_lastSearchWindow != null) _lastSearchWindow.Close();
                _lastSearchWindow = null;
            }
            catch (Exception)
            {

            }
        }

        private void OnTriggerKeyHit(object sender, EventArgs e)
        {
            try
            {
                //var triggerKey = (System.Windows.Forms.Keys)sender;
                var triggerKey = (Key)sender;

                RECT? hsWinRect = null;
                IntPtr? hsWinHwnd = null;

                var standaloneMode = Convert.ToBoolean(ConfigurationManager.AppSettings["StandaloneMode"]);

                if (!standaloneMode)
                {
                    var procs = Process.GetProcessesByName("HyperSpin");

                    if (procs.Length == 0)
                    {
                        Log("ERROR! Hyperspin process not found!");
                        return;
                    }

                    if (procs.Length > 1)
                    {
                        Log("WARNING! More than one Hyperspin process found. Make sure there is only one instance at a time.");
                    }

                    var hsProcess = procs[0];
                    var allWindows = GetWindows();


                    int pid;

                    // attempt to find the HS window
                    foreach (var c in allWindows)
                    {
                        IntPtr hwnd = (IntPtr)c;

                        GetWindowThreadProcessId(hwnd, out pid);

                        if (pid == hsProcess.Id)
                        {
                            RECT winRC;
                            GetWindowRect(hwnd, out winRC);

                            StringBuilder title = new StringBuilder(50);
                            GetWindowText(hwnd, title, 50);

                            if (title.ToString().ToLower() == "hyperspin" && winRC.Right - winRC.Left > 0)
                            {
                                hsWinRect = winRC;
                                hsWinHwnd = hwnd;
                                _lastHyperspinRect = winRC;
                                _lastHyperspinHwnd = hwnd.ToInt32();
                                break;// looks like we found the HS window
                            }
                        }
                    }

                    if (!hsWinRect.HasValue)
                    {
                        Log("ERROR! Failed to find Hyperspin window.");
                        return;
                    }

                    var fgw = GetForegroundWindow();

                    if (fgw != hsWinHwnd.Value)
                    {
                        Log("TriggerKey fired but HS is not the active window.");
                        return;
                    }
                }
                else
                {
                    int w = Convert.ToInt32(ConfigurationManager.AppSettings["StandaloneWidth"]);
                    int h = Convert.ToInt32(ConfigurationManager.AppSettings["StandaloneHeight"]);

                    int x = (int)((System.Windows.SystemParameters.PrimaryScreenWidth / 2.0) - ((double)w / 2.0));
                    int y = (int)((System.Windows.SystemParameters.PrimaryScreenHeight / 2.0) - ((double)h / 2.0));

                    hsWinRect = new RECT() { Left = x, Top = y, Right = x + w, Bottom = y + h };
                }

                try
                {
                    if (System.IO.File.Exists("notify.wav"))
                    {
                        System.Media.SoundPlayer sp = new System.Media.SoundPlayer("notify.wav");

                        sp.Play();
                    }
                }
                catch(Exception ee)
                {
                    Log(ee.ToString());
                }

                HyperSearch.Windows.GameSearchWindow.SearchList searchList = Windows.GameSearchWindow.SearchList.Normal;

                if (_searchTriggerKey != null && _searchTriggerKey.Is(triggerKey)) searchList = Windows.GameSearchWindow.SearchList.Normal;
                else if (_favouritesTriggerKey != null && _favouritesTriggerKey.Is(triggerKey)) searchList = Windows.GameSearchWindow.SearchList.Favourites;
                else if (_genreTriggerKey != null && _genreTriggerKey.Is(triggerKey)) searchList = Windows.GameSearchWindow.SearchList.Genre;

                Log("Using list: {0}", searchList);

                ShowGameSearchWindow(hsWinRect, searchList);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        public static void PositionWindowOnRect(Window win, RECT rc, int margin = 0)
        {
            double w = rc.Right - rc.Left;
            double h = rc.Bottom - rc.Top;

            win.Left = rc.Left + margin;
            win.Top = rc.Top + margin;
            win.Width = w - (2 * margin);
            win.Height = h - (2 * margin);
        }

        private static void ShowGameSearchWindow(RECT? hsWinRect, HyperSearch.Windows.GameSearchWindow.SearchList searchList)
        {
            try
            {
                Windows.GameSearchWindow win;

                if (!Global.MinimizeKeyOld.HasValue || _lastSearchWindow == null)
                { // if no minimizeKey was configured we do not reuse the prev window (current, original behaviour)
                    if (_lastSearchWindow != null)
                    {
                        //??! _lastSearchWindow.Close();
                        _lastSearchWindow = null;
                    }

                    win = new Windows.GameSearchWindow(searchList);
                }
                else
                {
                    win = _lastSearchWindow;

                    //win.WindowState = WindowState.Normal;
                }

                PositionWindowOnRect(win, hsWinRect.Value, 15);

                _lastSearchWindow = win;

                win.ShowSystemImagesOnFilter = MainWindow.ShowSystemImagesOnFilter;
                win.ShowSystemImagesOnResults = MainWindow.ShowSystemImagesOnResults;
                win.ShowWheelImagesOnResults = MainWindow.ShowWheelImagesOnResults;

                win.Show();
                win.Activate();
                /*
                var bmp = KbHook.GetWindowCaptureAsBitmap(hsWinHwnd.Value.ToInt32());

                var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                         bmp.GetHbitmap(),
                         IntPtr.Zero,
                         Int32Rect.Empty,
                         BitmapSizeOptions.FromEmptyOptions());


                Image img = new Image();

                img.Source = bmpSrc;

                ((win.Content as Border).Child as Grid).Children.Add(img);
                 **/
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // TODO: Think this might need to move to the game search window?
                SystemSoundPlayer.Init(mainGrid, @"C:\Hyperspin\Media\Frontend\Sounds");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private static bool MayCallLEDBlinky()
        {
            return HqSettings != null && HqSettings.LEDBlinkySection != null && HqSettings.LEDBlinkySection.IsActive && File.Exists(LEDBlinkyPath());
        }

        private static string LEDBlinkyPath()
        {
            return Path.Combine(HqSettings.LEDBlinkySection.Path, "LEDBlinky.exe");
        }


        public static void LEDBlinkSystemSelected(string systemName)
        {
            try
            {
                if (!MayCallLEDBlinky()) return;
                Util.StartProcess(LEDBlinkyPath(), string.Format("7 \"{0}\"", systemName), false);
            }
            catch (Exception ex)
            {
                LogStatic(ex.ToString());
            }
        }

        public static void LEDBlinkGameLaunch(string systemName, string gameName)
        {
            try
            {
                if (!MayCallLEDBlinky()) return;
                Util.StartProcess(LEDBlinkyPath(), string.Format("3 \"{0}\" \"{1}\"", gameName, systemName), false);
            }
            catch(Exception ex)
            {
                LogStatic(ex.ToString());
            }
        }

        public static void LEDBlinkGameSelected(string gameName)
        {
            try
            {
                if (!MayCallLEDBlinky()) return;
                Util.StartProcess(LEDBlinkyPath(), string.Format("9 \"{0}\"", gameName), false);
            }
            catch (Exception ex)
            {
                LogStatic(ex.ToString());
            }
        }



        public static void LogStatic(string line, params object[] args)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                MainWindow mw = Application.Current.MainWindow as MainWindow;
                mw.Log(line, args);
            }));
        }

        public void Log(string line, params object[] args)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                string[] lines = null;

                if (log.Text != null) lines = log.Text.Split('\n');

                if (lines != null && lines.Length > 250)
                {
                    log.Text = string.Join("\n", lines.Skip(10).ToArray());
                }

                var newLogLine = string.Format("{0:HH:mm:ss}  {1}\n", DateTime.Now, string.Format(line, args));

                log.AppendText(newLogLine);
                ErrorHandler.LogRawLineToSessionFile(newLogLine);

                log.CaretIndex = log.Text.Length;
                log.ScrollToEnd();
            }));
        }
    }

    public class KeyList
    {
        private List<System.Windows.Input.Key> Keys;

        public KeyList(params Key[] keys)
        {
            Keys = new List<System.Windows.Input.Key>();

            if (keys != null) Keys.AddRange(keys);
        }

        public bool Is(System.Windows.Input.Key key)
        {
            if (this.Keys == null) return false;

            return Keys.Contains(key);
        }

        public void Add(Key key)
        {
            if (!this.Keys.Contains(key))
                this.Keys.Add(key);
        }
    }

}
