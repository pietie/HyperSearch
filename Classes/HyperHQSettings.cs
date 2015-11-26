using Gajatko.IniFiles;
using HyperSearch.Windows.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace HyperSearch
{

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class IniHeaderAttribute : Attribute
    {
        public string Name { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class IniFieldAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public class HyperHQSettings
    {
        public Main MainSection { get; set; }
        public Resolution ResolutionSection { get; set; }
        public IntroVideo IntroVideoSection { get; set; }
        public Sound SoundSection { get; set; }
        public AttractMode AttractModeSection { get; set; }
        public Keyboard KeyboardSection { get; set; }
        public LEDBlinky LEDBlinkySection { get; set; }
        public HiScore HiScoreSection { get; set; }
        public StartupProgram StartupProgramSection { get; set; }
        public ExitProgram ExitProgramSection { get; set; }
        public Optimizer OptimizerSection { get; set; }
        public P1Controls P1ControlsSection { get; set; }
        public P2Controls P2ControlsSection { get; set; }
        public P1Joystick P1JoystickSection { get; set; }
        public P2Joystick P2JoystickSection { get; set; }
        public Trackball TrackballSection { get; set; }
        public Spinner SpinnerSection { get; set; }

        public void InitFromIniFile(string iniFilepath)
        {
            var ini = IniFile.FromFile(iniFilepath);

            this.MainSection = new Main();
            this.ResolutionSection = new Resolution();
            this.IntroVideoSection = new IntroVideo();
            this.SoundSection = new Sound();
            this.AttractModeSection = new AttractMode();
            this.KeyboardSection = new Keyboard();
            this.LEDBlinkySection = new LEDBlinky();
            this.HiScoreSection = new HiScore();
            this.StartupProgramSection = new StartupProgram();
            this.ExitProgramSection = new ExitProgram();
            this.OptimizerSection = new Optimizer();
            this.P1ControlsSection = new P1Controls();
            this.P2ControlsSection = new P2Controls();
            this.P1JoystickSection = new P1Joystick();
            this.P2JoystickSection = new P2Joystick();
            this.TrackballSection = new Trackball();
            this.SpinnerSection = new Spinner();

            InitSections(ini, this.MainSection, this.ResolutionSection, this.IntroVideoSection, this.SoundSection, this.AttractModeSection, this.KeyboardSection
                , this.LEDBlinkySection, this.HiScoreSection, this.StartupProgramSection, this.ExitProgramSection, this.OptimizerSection, this.P1ControlsSection, this.P2ControlsSection
                , this.P1JoystickSection, this.P2JoystickSection, this.TrackballSection, this.SpinnerSection);
        }

        private static void InitSections(IniFile iniFile, params object[] sections)
        {
            foreach (var s in sections)
            {
                var attribs = s.GetType().GetCustomAttributes(typeof(IniHeaderAttribute), false);

                if (attribs.Length == 0) continue;

                var headerAttrib = (attribs[0] as IniHeaderAttribute);

                var sectionName = headerAttrib.Name;

                var properties = s.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                foreach (var prop in properties)
                {
                    var aa = prop.GetCustomAttributes(typeof(IniFieldAttribute), false);

                    if (aa.Length == 0) continue;

                    var iniField = (aa[0] as IniFieldAttribute);

                    var value = iniFile[sectionName][iniField.Name];
                    var rt = prop.GetGetMethod().ReturnType;
                    var isValNullOrEmpty = string.IsNullOrEmpty(value);

                    object newPropValue = null;

                    try
                    {
                        if (rt == typeof(bool))
                        {
                            newPropValue = value.EqualsCI("true") || value.EqualsCI("yes");
                        }
                        else if (rt == typeof(int?))
                        {
                            int n;
                            if (int.TryParse(value, out n))
                            {
                                newPropValue = n;
                            }
                        }
                        else if (rt == typeof(double?))
                        {
                            double d;
                            if (double.TryParse(value, out d))
                            {
                                newPropValue = d;
                            }
                        }
                        else// default
                        {
                            newPropValue = value;
                        }

                        prop.SetValue(s, newPropValue, null);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.HandleException(ex);
                    }

                    //iniField.Name
                    //var aa = prop.CustomAttributes.ToList();

                }
            }
        }

        [SettingSection("General")]
        [IniHeader(Name = "Main")]
        public class Main
        {
            //[SettingType(Type = SettingsType.MultiOption, Title = "Menu mode", Description = "Provides the ability to run only a single sub-wheel.", MutliValueCsv = "multi, single")]
            [IniField(Name = "Menu_Mode")]
            public string MenuMode { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Single mode name", Description = "If in single mode this is the one and only wheel that will be displayed.")]
            [IniField(Name = "Single_Mode_Name")]
            public string SingleModeName { get; set; }

            [IniField(Name = "Last_System")]
            public string LastSystem { get; set; }

            [SettingType(Type = SettingsType.TrueFalse, Title = "Use last system", Description = "If enabled the application will start off at the last system used before the last application use.")]
            [IniField(Name = "Use_Last_Game")]
            public bool UseLastGame { get; set; }

            [SettingSection("Startup/Exit")]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Exit menu", Description = "If enabled the user is presented with an exit menu before the application exits, otherwise it just exits without confirmation.")]
            [IniField(Name = "Enable_Exit_Menu")]
            public bool EnableExitMenu { get; set; }

            [SettingSection("Startup/Exit")]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Enable exit", Description = "If disabled the application cannot be closed.")]
            [IniField(Name = "Enable_Exit")]
            public bool EnableExit { get; set; }

            [SettingSection("Startup/Exit")]
            //[SettingType(Type = SettingsType.MultiOption, Title = "Exit default", Description = "The default selected option in the exit menu.", MutliValueCsv = "yes,no")]
            [IniField(Name = "Exit_Default")]
            public string ExitDefault { get; set; }

            [SettingSection("Startup/Exit")]
            //[SettingType(Type = SettingsType.MultiOption, Title = "Exit action", Description = "Controls whether the exit action just exits the application or alternatively shuts down the PC.", MutliValueCsv = "exit,shutdown")]
            [IniField(Name = "Exit_Action")]
            public string ExitAction { get; set; }

            [SettingSection("Paths")]
            [SettingType(Type = SettingsType.FolderPath, Title = "Hyperlaunch path", Description = "The path to the directory that contains the HyperLaunch executable.")]
            [IniField(Name = "Hyperlaunch_Path")]
            public string HyperlaunchPath { get; set; }

            [IniField(Name = "Version")]
            public string Version { get; set; }

        }

        [SettingSection("Video")]
        [IniHeader(Name = "Resolution")]
        public class Resolution
        {
            [SettingType(Type = SettingsType.TrueFalse, Title = "Fullscreen mode", Description = "Fullscreen mode uses your desktop resolution to fill the screen.")]
            [IniField(Name = "FullScreen")]
            public bool FullScreen { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Width", Description = "If in windowed mode, specifies the horizontal resolution of the window.")]
            [IniField(Name = "Width")]
            public int? Width { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Height", Description = "If in windowed mode, specifies the vertical resolution of the window.")]
            [IniField(Name = "Height")]
            public int? Height { get; set; }

            [SettingType(Type = SettingsType.TrueFalse, Title = "Scanlines", Description = "Provides and arcade monitor effect on LCD screen.")]
            [IniField(Name = "Scanlines_Active")]
            public bool IsScanlinesActive { get; set; }

            [SettingType(Type = SettingsType.FilePath, Title = "Scalines image", Description = "The scanlines image pattern to use if scanlines are enabled.")]
            [IniField(Name = "Scanlines_Image")]
            public string ScanlinesImage { get; set; }

            //[SettingType(Type = SettingsType.MultiOption, Title = "Scanline scale", Description = "A scale factor to apply to the scanline image.", MutliValueCsv = "1,2,3,4,5,6,7,8,9")]
            [IniField(Name = "Scanlines_Scale")]
            public int? ScanlinesScale { get; set; }

            [SettingType(Type = SettingsType.Slider, Title = "Scanlines alpha", Description = "Controls the scanlines opacity.")]
            [IniField(Name = "Scanlines_Alpha")]
            public double? ScanlinesAlpha { get; set; }

        }

        [SettingSection("Startup/Exit")]
        [IniHeader(Name = "IntroVideo")]
        public class IntroVideo
        {
            [SettingType(Type = SettingsType.TrueFalse, Title = "Intro video", Description = "Displays the configured intro video at startup if enabled.")]
            [IniField(Name = "Use_Intro")]
            public bool UseIntro { get; set; }

            [SettingType(Type = SettingsType.TrueFalse, Title = "Stop intro on keypress", Description = "Skips the intro video playback on any key press.")]
            [IniField(Name = "Stop_On_Keypress")]
            public bool StopOnKeypress { get; set; }
        }

        [SettingSection("Sound")]
        [IniHeader(Name = "Sound")]
        public class Sound
        {
            [SettingType(Type = SettingsType.Slider, Title = "Master volume", Description = "Controls the overall master volume.")]
            [IniField(Name = "Master_Volume")]
            public int? MasterVolume { get; set; }

            [SettingType(Type = SettingsType.Slider, Title = "Video volume", Description = "Controls game video sound volume.")]
            [IniField(Name = "Video_Volume")]
            public int? VideoVolume { get; set; }

            [SettingType(Type = SettingsType.Slider, Title = "Wheel volume", Description = "Controls wheel entry selection sounds volume.")]
            [IniField(Name = "Wheel_Volume")]
            public int? WheelVolume { get; set; }

            [SettingType(Type = SettingsType.Slider, Title = "Interface volume", Description = "Controls all other interface sound effects.")]
            [IniField(Name = "Interface_Volume")]
            public int? InterfaceVolume { get; set; }
        }

        [SettingSection("Attract")]
        [IniHeader(Name = "AttractMode")]
        public class AttractMode
        {
            [SettingType(Type = SettingsType.TrueFalse, Title = "Attract mode", Description = "If enabled, automatically spins the wheel if the frontend is left unattended.")]
            [IniField(Name = "Active")]
            public bool IsActive { get; set; }

            [IniField(Name = "Time")]
            public int? Time { get; set; }

            [IniField(Name = "MaxSpinTime")]
            public int? MaxSpinTime { get; set; }

            [SettingType(Type = SettingsType.TrueFalse, Title = "Hyperspin mode", Description = "Causes the wheel to spin faster if enabled.")]
            [IniField(Name = "HyperSpin")]
            public bool Hyperspin { get; set; }

            [SettingType(Type = SettingsType.TrueFalse, Title = "Wait for video", Description = "Forces attract mode to wait for the current game video to complete before activating.")]
            [IniField(Name = "Wait_For_Video")]
            public bool WaitForVideo { get; set; }
        }

        [SettingSection("Controls")]
        [IniHeader(Name = "Keyboard")]
        public class Keyboard
        {
            [SettingType(Type = SettingsType.TrueFalse, Title = "Keyboard delay", Description = "???I don't really know.")]
            [IniField(Name = "Key_Delay")]
            public bool KeyDelay { get; set; }
        }

        [SettingSection("Startup/Exit")]
        [IniHeader(Name = "Startup Program")]
        public class StartupProgram
        {
            [IniField(Name = "Executable")]
            public string Executable { get; set; }

            [IniField(Name = "Paramaters")]
            public string Paramaters { get; set; }

            [IniField(Name = "Working_Directory")]
            public string WorkingDirectory { get; set; }

            [IniField(Name = "WinState")]
            public string WinState { get; set; }
        }

        [SettingSection("Startup/Exit")]
        [IniHeader(Name = "Exit Program")]
        public class ExitProgram
        {
            [IniField(Name = "Executable")]
            public string Executable { get; set; }

            [IniField(Name = "Paramaters")]
            public string Paramaters { get; set; }

            [IniField(Name = "Working_Directory")]
            public string WorkingDirectory { get; set; }

            [IniField(Name = "WinState")]
            public string WinState { get; set; }
        }

        [SettingSection("Tools")]
        [IniHeader(Name = "LEDBlinky")]
        public class LEDBlinky
        {
            [IniField(Name = "Active")]
            public bool IsActive { get; set; }

            [IniField(Name = "Path")]
            public string Path { get; set; }
        }

        [SettingSection("Tools")]
        [IniHeader(Name = "HiScore")]
        public class HiScore
        {
            [IniField(Name = "Active")]
            public bool IsActive { get; set; }

            [IniField(Name = "Y")]
            public int? Y { get; set; }

            [IniField(Name = "Delay")]
            public int? Delay { get; set; }
        }

        [SettingSection("Optimizer")]
        [IniHeader(Name = "Optimizer")]
        public class Optimizer
        {
            [IniField(Name = "CPU_Priority")]
            public string CPUPriority { get; set; }

            [IniField(Name = "Quality")]
            public string Quality { get; set; }

            [IniField(Name = "Image_Smoothing")]
            public bool ImageSmoothing { get; set; }

            [SettingType(Type = SettingsType.TrueFalse, Title = "Animated backgrounds", Description = "????")]
            [IniField(Name = "Animated_Backgrounds")]
            public bool AnimatedBackgrounds { get; set; }

            [SettingType(Type = SettingsType.TrueFalse, Title = "Interstitial backgrounds", Description = "These are animations that play between wheel entry selections. They can have a huge impact on performance so turn it off if you experience poor performance.")]
            [IniField(Name = "Inter_Backgrounds")]
            public bool InterBackgrounds { get; set; }

            [IniField(Name = "Level1_Backgrounds")]
            public bool Level1Backgrounds { get; set; }

            [IniField(Name = "Level2_Backgrounds")]
            public bool Level2Backgrounds { get; set; }

            [IniField(Name = "Level3_Backgrounds")]
            public bool Level3Backgrounds { get; set; }

            [IniField(Name = "Level4_Backgrounds")]
            public bool Level4Backgrounds { get; set; }

            [IniField(Name = "Special_Backgrounds")]
            public bool SpecialBackgrounds { get; set; }

            [IniField(Name = "Wait_For_Special")]
            public bool WaitForSpecial { get; set; }

            [IniField(Name = "Animated_Artworks")]
            public bool AnimatedArtworks { get; set; }

            [IniField(Name = "Level1_Artworks")]
            public bool Level1Artworks { get; set; }

            [IniField(Name = "Level2_Artworks")]
            public bool Level2Artworks { get; set; }
        }


        [SettingSection("Controls")]
        [IniHeader(Name = "P1 Controls")]
        public class P1Controls 
        {
            [IniField(Name = "Start")]
            public int? Start { get; set; }

            [IniField(Name = "Exit")]
            public string Exit { get; set; }

            [IniField(Name = "Up")]
            public int? Up { get; set; }

            [IniField(Name = "Down")]
            public int? Down { get; set; }

            [IniField(Name = "SkipUp")]
            public int? SkipUp { get; set; }

            [IniField(Name = "SkipDown")]
            public int? SkipDown { get; set; }

            [IniField(Name = "SkipUpNumber")]
            public int? SkipUpNumber { get; set; }

            [IniField(Name = "SkipDownNumber")]
            public int? SkipDownNumber { get; set; }

            [IniField(Name = "HyperSpin")]
            public int? HyperSpin { get; set; }

            [IniField(Name = "Genre")]
            public int? Genre { get; set; }

            [IniField(Name = "Favorites")]
            public int? Favorites { get; set; }
        }

        [SettingSection("Controls")]
        [IniHeader(Name = "P2 Controls")]
        public class P2Controls 
        {
            [IniField(Name = "Start")]
            public int? Start { get; set; }

            [IniField(Name = "Exit")]
            public string Exit { get; set; }

            [IniField(Name = "Up")]
            public int? Up { get; set; }

            [IniField(Name = "Down")]
            public int? Down { get; set; }

            [IniField(Name = "SkipUp")]
            public int? SkipUp { get; set; }

            [IniField(Name = "SkipDown")]
            public int? SkipDown { get; set; }

            [IniField(Name = "SkipUpNumber")]
            public int? SkipUpNumber { get; set; }

            [IniField(Name = "SkipDownNumber")]
            public int? SkipDownNumber { get; set; }

            [IniField(Name = "HyperSpin")]
            public int? HyperSpin { get; set; }

            [IniField(Name = "Genre")]
            public int? Genre { get; set; }

            [IniField(Name = "Favorites")]
            public int? Favorites { get; set; }
        }

        [SettingSection("Controls")]
        [IniHeader(Name = "P1 Joystick")]
        public class P1Joystick
        {
            [IniField(Name = "Enabled")]
            public bool IsEnabled { get; set; }

            [IniField(Name = "Joy")]
            public int? Joy { get; set; }

            [IniField(Name = "Threshold")]
            public int? Threshold { get; set; }

            [IniField(Name = "Start")]
            public string Start { get; set; }

            [IniField(Name = "Exit")]
            public string Exit { get; set; }

            [IniField(Name = "Up")]
            public string Up { get; set; }

            [IniField(Name = "Down")]
            public string Down { get; set; }

            [IniField(Name = "SkipUp")]
            public string SkipUp { get; set; }

            [IniField(Name = "SkipDown")]
            public string SkipDown { get; set; }

            [IniField(Name = "SkipUpNumber")]
            public string SkipUpNumber { get; set; }

            [IniField(Name = "SkipDownNumber")]
            public string SkipDownNumber { get; set; }

            [IniField(Name = "HyperSpin")]
            public string HyperSpin { get; set; }

            [IniField(Name = "Genre")]
            public string Genre { get; set; }

            [IniField(Name = "Favorites")]
            public string Favorites { get; set; }
        }

        [SettingSection("Controls")]
        [IniHeader(Name = "P2 Joystick")]
        public class P2Joystick
        {
            [IniField(Name = "Enabled")]
            public bool IsEnabled { get; set; }

            [IniField(Name = "Joy")]
            public int? Joy { get; set; }

            [IniField(Name = "Threshold")]
            public int? Threshold { get; set; }

            [IniField(Name = "Start")]
            public string Start { get; set; }

            [IniField(Name = "Exit")]
            public string Exit { get; set; }

            [IniField(Name = "Up")]
            public string Up { get; set; }

            [IniField(Name = "Down")]
            public string Down { get; set; }

            [IniField(Name = "SkipUp")]
            public string SkipUp { get; set; }

            [IniField(Name = "SkipDown")]
            public string SkipDown { get; set; }

            [IniField(Name = "SkipUpNumber")]
            public string SkipUpNumber { get; set; }

            [IniField(Name = "SkipDownNumber")]
            public string SkipDownNumber { get; set; }

            [IniField(Name = "HyperSpin")]
            public string HyperSpin { get; set; }

            [IniField(Name = "Genre")]
            public string Genre { get; set; }

            [IniField(Name = "Favorites")]
            public string Favorites { get; set; }
        }

        [SettingSection("Controls")]
        [IniHeader(Name = "Trackball")]
        public class Trackball
        {
            [IniField(Name = "Enabled")]
            public bool IsEnabled { get; set; }

            [IniField(Name = "Sensitivity")]
            public int? Sensitivity { get; set; }
        }

        [SettingSection("Controls")]
        [IniHeader(Name = "Spinner")]
        public class Spinner
        {
            [IniField(Name = "Enabled")]
            public bool IsEnabled { get; set; }

            [IniField(Name = "Sensitivity")]
            public int? Sensitivity { get; set; }
        }

        private static Dictionary<int, Key> ActionScriptKeyCodeToVkLookup = new Dictionary<int, Key>()
        {
            {65, Key.A }
            ,{66, Key.B }
            ,{67, Key.C }
            ,{68, Key.D }
            ,{69, Key.E }
            ,{70, Key.F }
            ,{71, Key.G }
            ,{72, Key.H }
            ,{73, Key.I }
            ,{74, Key.J }
            ,{75, Key.K }
            ,{76, Key.L }
            ,{77, Key.M }
            ,{78, Key.N }
            ,{79, Key.O }
            ,{80, Key.P }
            ,{81, Key.Q }
            ,{82, Key.R }
            ,{83, Key.S }
            ,{84, Key.T }
            ,{85, Key.U }
            ,{86, Key.V }
            ,{87, Key.W }
            ,{88, Key.X }
            ,{89, Key.Y }
            ,{90, Key.Z }
            ,{48, Key.D0 }
            ,{49, Key.D1 }
            ,{50, Key.D2 }
            ,{51, Key.D3 }
            ,{52, Key.D4 }
            ,{53, Key.D5 }
            ,{54, Key.D6 }
            ,{55, Key.D7 }
            ,{56, Key.D8 }
            ,{57, Key.D9 }
       

            ,{96, Key.NumPad0 }
            ,{97, Key.NumPad1 }
            ,{98, Key.NumPad2 }
            ,{99, Key.NumPad3 }
            ,{100, Key.NumPad4 }
            ,{101, Key.NumPad5 }
            ,{102, Key.NumPad6 }
            ,{103, Key.NumPad7 }
            ,{104, Key.NumPad8 }
            ,{105, Key.NumPad9 }
            ,{106, Key.Multiply }
            ,{107, Key.Add }
            ,{13, Key.Enter }
            ,{109, Key.Subtract }
            ,{110, Key.Decimal }
            ,{111, Key.Divide }

            ,{112, Key.F1 }
            ,{113, Key.F2 }
            ,{114, Key.F3 }
            ,{115, Key.F4 }
            ,{116, Key.F5 }
            ,{117, Key.F6 }
            ,{118, Key.F7 }
            ,{119, Key.F8 }
            ,{120, Key.F9 }
            ,{122, Key.F11 }
            ,{123, Key.F12 }
            ,{124, Key.F13 }
            ,{125, Key.F14 }
            ,{126, Key.F15 }

            ,{8, Key.Back }
            ,{9, Key.Tab }
            ,{16, Key.LeftShift }
            ,{17, Key.LeftCtrl }
            ,{20, Key.CapsLock }
            ,{27, Key.Escape }
            ,{32, Key.Space }
            ,{33, Key.PageUp }
            ,{34, Key.PageDown }
            ,{35, Key.End }
            ,{36, Key.Home }
            ,{37, Key.Left }
            ,{38, Key.Up  }
            ,{39, Key.Right  }
            ,{40, Key.Down  }
            ,{45, Key.Insert }
            ,{46, Key.Delete }
            ,{144, Key.NumLock }
            ,{145, Key.Scroll }
            ,{19, Key.Pause }
            ,{186, Key.OemSemicolon }
            ,{187, Key.Add }
            ,{189, Key.OemMinus }
            ,{191, Key.OemQuestion }
            ,{192, Key.OemTilde }
            ,{219, Key.OemOpenBrackets }
            ,{220, Key.OemPipe }
            ,{221, Key.OemCloseBrackets }
            ,{222, Key.OemQuotes }
            ,{188, Key.OemComma }
            ,{190, Key.OemPeriod }


        };

        public static KeyCombo ActionScriptKeyCodeToVK(int? code)
        {
            if (!code.HasValue) return new KeyCombo(Key.None);
            if (ActionScriptKeyCodeToVkLookup.ContainsKey(code.Value)) return new KeyCombo(ActionScriptKeyCodeToVkLookup[code.Value]);
            return new KeyCombo(Key.None);
        }
        public static KeyCombo ActionScriptKeyCodeToVK(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;

            var codeElems = code.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            KeyCombo kc = new KeyCombo();

            foreach(var el in codeElems)
            {
                var c = el.ToIntNullable();
                if (!c.HasValue) continue; 
                if (ActionScriptKeyCodeToVkLookup.ContainsKey(c.Value))
                {
                    var k = ActionScriptKeyCodeToVkLookup[c.Value];
                    kc.AddKey(k);
                }
                else
                {
                //    MainWindow.LogStatic("WARNING!");
                }
            }

            //return Key.None;
            return kc;
        }

        public class KeyCombo 
        {
            private List<Key> _keyList = new List<Key>();

            public KeyCombo()  
            {

            }

            public KeyCombo(Key singleKey)
            {
                this.AddKey(singleKey);
            }

            public void AddKey(Key k)
            {
                _keyList.Add(k);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();  
            }

            public override bool Equals(object obj)
            {
                if (_keyList.Count == 1) return _keyList[0] == (Key)obj;

                foreach(var k in _keyList)// check that each key in the list is being held down
                {
                    if (!System.Windows.Input.Keyboard.IsKeyDown(k)) return false;
                }

                return true;
            }
        }

    }


}
