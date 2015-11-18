using HyperSearch.Windows.Settings;
using System.Configuration;
using System.Linq;

namespace HyperSearch.Classes
{
    [SettingsSectionOrderBy("Input, Abc, Def")]
    public class HyperSearchSettings
    {
        private static HyperSearchSettings _instance = new HyperSearchSettings();

        private HyperSearchSettings()
        {
            this.P1KeyboardControlsSection = new InputControls();
            this.GeneralSection = new General();

            var highLevelProperties = from p in this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                      select new
                                      {
                                          Property = p,
                                          Children = (from c in p.GetGetMethod().ReturnType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                                      select new
                                                      {
                                                          Property = c,
                                                          SettingsTypeAttribute = c.GetCustomAttributes(typeof(SettingTypeAttribute), false).FirstOrDefault() as SettingTypeAttribute,
                                                          SectionAttribute = c.GetCustomAttributes(typeof(SettingSectionAttribute), false).FirstOrDefault() as SettingSectionAttribute,
                                                          Parent = p
                                                      }),
                                          SectionAttribute = p.GetGetMethod().ReturnType.GetCustomAttributes(typeof(SettingSectionAttribute), false).FirstOrDefault() as SettingSectionAttribute
                                      };

            var _sectionsWithSettingsDict = ((from setting in
                                              (from h in highLevelProperties
                                               from c in h.Children
                                               where c.Parent == h.Property && h.SectionAttribute != null
                                               select new 
                                               {
                                                   Property = c.Property,
                                                   ParentObject = c.Parent.GetGetMethod().Invoke(this, null),
                                                   Section = c.SectionAttribute == null ? h.SectionAttribute : c.SectionAttribute, // inherit if necessary
                                                       SettingsType = c.SettingsTypeAttribute
                                               })
                                          group setting by setting.Section.Name into grp
                                          select grp).ToDictionary(g => g.Key, g => g.ToList()));

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
               // var xx = _sectionsWithSettingsDict.Values.FirstOrDefault(v => v.FirstOrDefault(x => x.SettingsType.AppConfigKey.EqualsCI(key)) != null);

                if (key == "Keys.Trigger.Search")
                {

                }

            }

        }
        public static HyperSearchSettings Instance() { return _instance; }

        public KeyList ActionKey;
        public KeyList BackKey;
        public KeyList ExitKey;
        public KeyList MinimizeKey;

        public KeyList UpKey;
        public KeyList RightKey;
        public KeyList DownKey;
        public KeyList LeftKey;

        public General GeneralSection { get; set; }

        public InputControls P1KeyboardControlsSection { get; set; }

        public static void InitFromAppConfig()
        {

        }


        [SettingSection("Input")]
        public class InputControls 
        {
            [SettingType(Type = SettingsType.Action, Title = "Button layout", Description = "Configure your button layout and styles", ActionTitle = "Set", ActionType = SettingActionType.ConfigureButtonLayout)]
            public bool ConfigureButtonLayout { get; set; }

            public string ButtonLayoutTemplateFilename { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Search", Description = "Shows the main search window.", ActionType = SettingActionType.SetKeyMultiple, AppConfigKey= "Keys.Trigger.Search")]
            public string Search { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Favourites", Description = "Shows the favourites search window.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Favourites { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Genre", Description = "Shows the genre search window.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Genre { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Settings", Description = "Shows this settings window.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Settings { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Up", Description = "Move up.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Up { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Right", Description = "Move right.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Right { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Down", Description = "Move down.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Down { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Left", Description = "Move left.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Left { get; set; }


            [SettingType(Type = SettingsType.MultiOption, Title = "Action", Description = "Starts a game or confirm a selection.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Action { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Back", Description = "Go back to previous screen, close a window or cancel an action.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Back { get; set; }


            [SettingType(Type = SettingsType.MultiOption, Title = "Minimize", Description = "Minimizes the current search window. Does not affect other windows.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Minimize { get; set; }


            [SettingType(Type = SettingsType.MultiOption, Title = "Exit", Description = "Closes the current window.", ActionType = SettingActionType.SetKeyMultiple)]
            public string Exit { get; set; }
        }


        [SettingSection("General")]
        public class General
        {

            [SettingType(Type = SettingsType.FolderPath, Title = "HyperSpin path", Description = "Specifies the location of the main HyperSpin folder.")]
            public string HyperSpinPath { get; set; }

            [SettingType(Type = SettingsType.FilePath, Title = "Rocketlauncher path", Description = "Specifies the location of the Rocketlauncher exe.")]
            public string RocketLauncherExePath { get; set; }

            [SettingType(Type = SettingsType.MultiOption, Title = "Keyboard type", Description = "Specifies the keyboard type to use for search criteria input.", MutliValueCsv = "AtoZ,Qwerty,Azerty,Orb")]
            public string KeyboardType { get; set; }
            [SettingType(Type = SettingsType.TrueFalse, Title = "Cab Mode", Description = "If enabled input from your physical keyboard will be ignored when inputting search criteria.")]
            public string CabMode { get; set; }
            

            [SettingType(Type = SettingsType.TrueFalse, Title = "Hide mouse cursor", Description = "Hides the mouse cursor if enabled.")]
            public string HideMouseCursor { get; set; }


            [SettingType(Type = SettingsType.TrueFalse, Title = "Standalone mode", Description = "If enabled allows HyperSearch to launch outside of Hyperspin.")]
            public string StandAloneMode { get; set; }
        }
    }
}
