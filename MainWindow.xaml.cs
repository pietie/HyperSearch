using HyperSearch.Classes;
using HyperSearch.Windows.Settings;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public static Windows.GameSearchWindow _lastSearchWindow = null;
        private static HyperSearch.Windows.GameSearchWindow.SearchList? _lastSearchList = null;

        public static int? _lastHyperspinHwnd = null;
        public static RECT? _lastHyperspinRect = null;

        private CommandLineOptions CliOptions { get; set;  }

        private class CommandLineOptions
        {
            public bool StartupFullSearch { get; set; }
            public bool StartupGenre { get; set; }
            public bool StartupFavourites { get; set; }
            public bool StartupSettings { get; set; }

        }
        
        private void ParseCommandLineArgs()
        {
            // very very simple parsing for now

            this.CliOptions = new MainWindow.CommandLineOptions();

            var args = Environment.GetCommandLineArgs();
            
            if (args.Length != 2) return;

            switch (args[1].ToLower())
            {
                case "-search":
                    this.CliOptions.StartupFullSearch = true;
                    break;
                case "-genre":
                    this.CliOptions.StartupGenre = true;
                    break;
                case "-fav":
                    this.CliOptions.StartupFavourites = true;
                    break;
                case "-settings":
                    this.CliOptions.StartupSettings = true;
                    break;
            }
        }

        private static void StartCliPipeServer(MainWindow main)
        {
            // http://stackoverflow.com/a/16302188
            Task.Factory.StartNew(() =>
            {
                var server = new NamedPipeServerStream("CliPipe", PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                var reader = new StreamReader(server);
                var connectedOrWaiting = false;

                while (true)
                {
                    if (!connectedOrWaiting)
                    {
                        server.BeginWaitForConnection((a) => { server.EndWaitForConnection(a); }, null);
                        connectedOrWaiting = true;
                    }

                    if (server.IsConnected)
                    {
                        var line = reader.ReadLine();

                        if (line != null)
                        {
                            switch (line.ToLower())
                            {
                                case "-search":
                                    main.OnTriggerKeyHit(HyperSearchSettings.Instance().Input.Triggers.Search.FirstKey, null);
                                    break;
                                case "-genre":
                                    main.OnTriggerKeyHit(HyperSearchSettings.Instance().Input.Triggers.Genre.FirstKey, null);
                                    break;
                                case "-fav":
                                    main.OnTriggerKeyHit(HyperSearchSettings.Instance().Input.Triggers.Favourites.FirstKey, null);
                                    break;
                                case "-settings":
                                    main.OnTriggerKeyHit(HyperSearchSettings.Instance().Input.Triggers.Settings.FirstKey, null);
                                    break;
                            }
                        }
                          
                        server.Disconnect();
                        connectedOrWaiting = false;
                    }
                }
            });
        }

        private void SendPipeCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd)) return;

            try
            {
                using (var client = new NamedPipeClientStream(".", "CliPipe", PipeDirection.Out))
                {
                    client.Connect(1000 * 5);
                    //using (StreamReader reader = new StreamReader(client))
                    {
                        using (StreamWriter writer = new StreamWriter(client))
                        {
                            // currently this is a once off connection and message as that is all we need
                            //while(true)
                            {
                                writer.WriteLine(cmd);
                                writer.Flush();
                            }
                        }
                    }
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Log("ERROR! Failed to send pipe command: {0}", cmd);
                ErrorHandler.HandleException(ex);
            }
        }

      
        
        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                var Settings = HyperSearchSettings.Instance();

                ParseCommandLineArgs();
                
                bool createdNew;

                // allow only one instance to run at a time
                {
                    SingleInstance = new Mutex(true, "HyperSearch_SingleInstance", out createdNew);

                    if (!createdNew)
                    {
                        var cmdLineArgs = Environment.GetCommandLineArgs();

                        // if cmd line options were specified, pass it on to running instance
                        if (cmdLineArgs.Length > 1)
                        {
                            SendPipeCommand(cmdLineArgs[1]);
                        }
                        else
                        {
                            MessageBox.Show("There is already another instance of HyperSearch running. Only one instance is allowed.", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }

                        Application.Current.Shutdown();
                        return;
                    }
                }

                ErrorHandler.ClearSessionFile();

                StartCliPipeServer(this);

                var stylesUri = new Uri("pack://siteoforigin:,,,/Styles.xaml", UriKind.RelativeOrAbsolute);

                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = stylesUri });

                string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                this.Title = "HyperSearch - " + version + " - " + this.Title ?? "";

                App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
               
                InitSysTray();

                var triggerKeyConfig = new Dictionary<KeyList, int?>();

                // Trigger Keys
                {
                    triggerKeyConfig.Add(HyperSearchSettings.Instance().Input.Triggers.Search, HyperSearchSettings.Instance().Input.Triggers.SearchTriggerDelayInMilliseconds);
                    triggerKeyConfig.Add(HyperSearchSettings.Instance().Input.Triggers.Favourites, HyperSearchSettings.Instance().Input.Triggers.FavouritesTriggerDelayInMilliseconds);
                    triggerKeyConfig.Add(HyperSearchSettings.Instance().Input.Triggers.Genre, HyperSearchSettings.Instance().Input.Triggers.GenreTriggerDelayInMilliseconds);
                    triggerKeyConfig.Add(HyperSearchSettings.Instance().Input.Triggers.Settings, HyperSearchSettings.Instance().Input.Triggers.SettingsTriggerDelayInMilliseconds);
                }

                KbHook.TriggerKeyConfig = triggerKeyConfig;

                KbHook.OnTriggerKeyHit += OnTriggerKeyHit;
                KbHook.InstallHook();

                Log("Hook installed succesfully");


                var hsPath = Settings.General.HyperSpinPath;
                Log("Hyperspin path: {0}", hsPath);
                bool isRelative;

                try
                {
                    hsPath = Util.AbsolutePath(hsPath, out isRelative);

                    if (isRelative) Log("Specified HyperSpin path is a relative path, translating to absolute path: {0}", hsPath);

                    HyperSearchSettings.Instance().General.HyperSpinPathAbsolute = hsPath;
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                    Log("ERROR: The specified HyperSpin path is not a valid URI!");
                }

                var launcherPath = Settings.General.RocketLauncherExePath;

                if (string.IsNullOrEmpty(launcherPath))
                {
                    Log("General.RocketLauncherExePath not set. Defaulting to HyperSpin config and HyperLaunch.exe");
                }
                else
                {

                    try
                    {
                        launcherPath = Util.AbsolutePath(launcherPath, out isRelative);

                        if (isRelative) Log("Specified Launcher path is a relative path, translating to absolute path: {0}", launcherPath);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogException(ex);
                        Log("ERROR: The specified Launcher path is not a valid URI!");
                    }

                    Log("LauncherPath: {0}", launcherPath);
                }

                var settingsPath = Global.BuildFilePathInHyperspinDir("Settings\\Settings.ini");
                HqSettings = new HyperHQSettings();

                if (File.Exists(settingsPath))
                {
                    HqSettings.InitFromIniFile(settingsPath);
                    Log("Hyperspin settings loaded from: {0}", settingsPath);
                    Log("HyperLaunch path: {0}", HqSettings.MainSection.HyperlaunchPath);
                    Log("LEDBlinky path: {0}", HqSettings.LEDBlinkySection.Path);

                    if (HqSettings != null && HqSettings.LEDBlinkySection != null && HqSettings.LEDBlinkySection.IsActive && !File.Exists(HqSettings.LEDBlinkySection.Path))
                    {
                        Log("ERROR! LED blinky not found at configured location.");
                    }
                }
                else
                {
                    Log("WARNING! HQ Settings file does not exist at: {0}", settingsPath);
                }

                Log("CabMode: {0}", Settings.General.CabMode ?? false);


                var altSystemWheelSourceFolder = HyperSearchSettings.Instance().Misc.AlternativeSystemWheelImagePath;

                if (!string.IsNullOrEmpty(altSystemWheelSourceFolder))
                {
                    altSystemWheelSourceFolder = Util.AbsolutePath(altSystemWheelSourceFolder, out isRelative);

                    try
                    {
                        if (isRelative) Log("Specified AlternativeSystemWheelImagePath path is a relative path, translating to absolute path: {0}", altSystemWheelSourceFolder);
                        else Log("AlternativeSystemWheelSourceFolder: {0}", altSystemWheelSourceFolder);

                        Global.AlternativeSystemWheelSourceFolder = altSystemWheelSourceFolder;
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogException(ex);
                        Log("ERROR: The specified AlternativeSystemWheelImagePath is not a valid URI!");
                    }
                }
                else
                {
                    Log("AlternativeSystemWheelImagePath not specified. Defaulting to 'Wheel'.");
                }

                var altGameWheelSourceFolder = HyperSearchSettings.Instance().Misc.AlternativeGameWheelSourceFolder;

                if (!string.IsNullOrEmpty(altGameWheelSourceFolder))
                {
                    Log("AlternativeGameWheelSourceFolder: {0}", altGameWheelSourceFolder);
                    Global.AlternativeGameWheelSourceFolder = altGameWheelSourceFolder;
                }
                else
                {
                    Log("AlternativeGameWheelSourceFolder not specified. Defaulting to 'Wheel'.");
                }


                // kick-off background initialising work (e.g. preparing the full search index)
                new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((new BackgroundThreadInitialiser()).Run)).Start(this); // wow, one line..this is a horrible idea.


                this.WindowState = WindowState.Minimized;
                this.Hide();
                
                if (this.CliOptions.StartupFullSearch)
                {
                    OnTriggerKeyHit(HyperSearchSettings.Instance().Input.Triggers.Search.FirstKey, null);
                }
                else if (this.CliOptions.StartupGenre)
                {
                    OnTriggerKeyHit(HyperSearchSettings.Instance().Input.Triggers.Genre.FirstKey, null);
                }
                else if (this.CliOptions.StartupFavourites)
                {
                    OnTriggerKeyHit(HyperSearchSettings.Instance().Input.Triggers.Favourites.FirstKey, null);
                }
                else if (this.CliOptions.StartupSettings)
                {
                    OnTriggerKeyHit(HyperSearchSettings.Instance().Input.Triggers.Settings.FirstKey, null);
                }
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

            int balloonToolTipTimeoutInsMS = HyperSearchSettings.Instance().General.BalloonToolTipTimeOutInMilliseconds;

            if (balloonToolTipTimeoutInsMS > 0)
            {
                ni.ShowBalloonTip(balloonToolTipTimeoutInsMS, "HyperSearch started", "Double click tray icon to bring up log window.", System.Windows.Forms.ToolTipIcon.Info);
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
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // triggers are disabled while the settings window is open
                if (Windows.Settings.MainSettings.IsSettingsOpen) return; 

                try
                {
                    var triggerKey = (Key)sender;

                    if (HyperSearchSettings.Instance().Input.Triggers.Settings != null && HyperSearchSettings.Instance().Input.Triggers.Settings.Is(triggerKey))
                    {
                        this.Show();
                        settingsButton_Click(sender, null);

                        return;
                    }

                    RECT? hsWinRect = null;
                    IntPtr? hsWinHwnd = null;

                    var standaloneMode = HyperSearchSettings.Instance().General.StandAloneMode ?? false;

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
                        int w = HyperSearchSettings.Instance().General.StandaloneWidth;
                        int h = HyperSearchSettings.Instance().General.StandaloneHeight;

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
                    catch (Exception ee)
                    {
                        Log(ee.ToString());
                    }

                    HyperSearch.Windows.GameSearchWindow.SearchList searchList = Windows.GameSearchWindow.SearchList.Normal;

                    if (HyperSearchSettings.Instance().Input.Triggers.Search != null && HyperSearchSettings.Instance().Input.Triggers.Search.Is(triggerKey)) searchList = Windows.GameSearchWindow.SearchList.Normal;
                    else if (HyperSearchSettings.Instance().Input.Triggers.Favourites != null && HyperSearchSettings.Instance().Input.Triggers.Favourites.Is(triggerKey)) searchList = Windows.GameSearchWindow.SearchList.Favourites;
                    else if (HyperSearchSettings.Instance().Input.Triggers.Genre != null && HyperSearchSettings.Instance().Input.Triggers.Genre.Is(triggerKey)) searchList = Windows.GameSearchWindow.SearchList.Genre;

                    Log("Using list: {0}", searchList);

                    ShowGameSearchWindow(hsWinRect, searchList);
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }
            }));

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

                if (_lastSearchWindow != null && !_lastSearchWindow.IsLoaded || (_lastSearchList.HasValue && _lastSearchList != searchList))
                {
                    if (_lastSearchWindow != null && _lastSearchWindow.IsLoaded) _lastSearchWindow.Close();

                    _lastSearchWindow = null;
                }

                var settings = HyperSearchSettings.Instance().Input;

                if (settings.Minimize == null|| _lastSearchWindow == null)
                { // if no minimizeKey was configured we do not reuse the prev window
                    if (_lastSearchWindow != null)
                    {
                        if (_lastSearchWindow.IsLoaded) _lastSearchWindow.Close();

                        _lastSearchWindow = null;
                    }

                    win = new Windows.GameSearchWindow(searchList);
                }
                else
                {
                    win = _lastSearchWindow;
                }

                PositionWindowOnRect(win, hsWinRect.Value, 15);

                _lastSearchWindow = win;
                _lastSearchList = searchList;

                win.ShowSystemImagesOnFilter = HyperSearchSettings.Instance().Misc.ShowSystemImagesOnFilter;
                win.ShowSystemImagesOnResults = HyperSearchSettings.Instance().Misc.ShowSystemImagesOnResults;
                win.ShowWheelImagesOnResults = HyperSearchSettings.Instance().Misc.ShowWheelImagesOnResults;

                win.Show();
                win.Activate();
            }
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
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
                Util.StartProcess(LEDBlinkyPath(), string.Format("7 \"{0}\"", systemName));
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
                Util.StartProcess(LEDBlinkyPath(), string.Format("3 \"{0}\" \"{1}\"", gameName, systemName));
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
                Util.StartProcess(LEDBlinkyPath(), string.Format("9 \"{0}\"", gameName));
            }
            catch (Exception ex)
            {
                LogStatic(ex.ToString());
            }
        }

        public static void LogStaticVerbose(string line, params object[] args)
        {
            if (!HyperSearchSettings.Instance().General.VerboseLogging) return;
            LogStatic(line, args);
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

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Win.Modal(new Windows.Settings.MainSettings(), this);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }
    }

   

}
