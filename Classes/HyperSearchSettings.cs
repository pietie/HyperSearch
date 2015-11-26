using HyperSearch.Windows.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace HyperSearch.Classes
{
    [SettingsSectionOrderBy("Input, Abc, Def")]
    public class HyperSearchSettings
    {
        private static HyperSearchSettings _instance = null;

        [JsonProperty]
        public GeneralSection General { get; set; }
        [JsonProperty]
        public InputControls Input { get; set; }

        [JsonProperty]
        public MiscSection Misc { get; set; }


        private HyperSearchSettings()
        {
            this.Input = new InputControls();
            this.General = new GeneralSection();
            this.Misc = new MiscSection();
        }

        public static HyperSearchSettings Instance()
        {
            if (_instance == null)
            {
                Load();

                if (_instance == null) _instance = new HyperSearchSettings();
            }

            return _instance;

        }


        public void Save()
        {
            try
            {
                var filePath = Global.BuildFilePathInAppDir("Settings.json");

                var json = JsonConvert.SerializeObject(this, new JsonSerializerSettings()
                {
                    Converters = new List<JsonConverter>() { new KeyListConverter() },
                    ContractResolver = new SerializeWithJsonAttributesContractResolver(),
                    Formatting = Formatting.Indented
                });

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

       public static void Load()
        {
            try
            {
                var filePath = Global.BuildFilePathInAppDir("Settings.json");
                string json;


                if (File.Exists(filePath)) 
                {
                    json = File.ReadAllText(filePath);
                }
                else
                { // create json using all top-level expected objects so that the DefaultValue attribute can take over
                    //var allExpectedSections = (from p in typeof(HyperSearchSettings).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    //                           select string.Format("\"{0}\": {{ }}", p.Name));

                    var highLevelProperties = (from p in typeof(HyperSearchSettings).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                               select new
                                               {
                                                   p.Name,
                                                   Children = (from c in p.GetGetMethod().ReturnType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                                               where c.IsDefined(typeof(SettingCreateEmptyAttribute),false)
                                                               select new { c.Name }
                                                               )
                                               });

                    var allExpectedSections = from p in highLevelProperties
                              select string.Format("\"{0}\": {{ {1} }}", p.Name, (
                                string.Join(",", (from c in p.Children
                                select string.Format("\"{0}\": {{  }}", c.Name)))
                              ));


                    json = "{" + string.Join(",", allExpectedSections.ToArray()) + "}";
                }

                JsonSerializerSettings ss = new JsonSerializerSettings();
                
                ss.Converters = new List<JsonConverter> { new KeyListConverter() };
                ss.DefaultValueHandling = DefaultValueHandling.Populate;

                var settings = JsonConvert.DeserializeObject<HyperSearchSettings>(json, ss);

                var nullSections = (from p in settings.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                    where p.GetValue(settings, null) == null
                                    select p);

                foreach(var p in nullSections)
                {
                    // create an empty section
                    var emptySection = Activator.CreateInstance(p.PropertyType);

                    p.SetValue(settings, emptySection, null);
                }


                _instance = settings;

                // TODO: Find a better way to create these objects on demand
                if (_instance.Input.Triggers == null) _instance.Input.Triggers = new Triggers();

            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException(ex);
            }
        }

        [SettingSection("Input")]
        public class InputControls
        {
            public InputControls()
            {
                LayoutConfig = new LayoutConfig();
                Triggers = new Triggers();
            }

            [JsonIgnore]
            [SettingType(Type = SettingsType.Action, Title = "Button layout", Description = "Configure your button layout and styles", ActionTitle = "Set", ActionType = SettingActionType.ConfigureButtonLayout)]
            public bool ConfigureButtonLayout { get; set; }

            [JsonProperty]
            [SettingCreateEmpty]
            [SettingComposite]
            public Triggers Triggers { get; set; }

            [JsonProperty]
            [DefaultValue(Key.Up)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Up", Description = "Move up.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Up { get; set; }

            [JsonProperty]
            [DefaultValue(Key.Right)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Right", Description = "Move right.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Right { get; set; }

            [JsonProperty]
            [DefaultValue(Key.Down)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Down", Description = "Move down.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Down { get; set; }

            [JsonProperty]
            [DefaultValue(Key.Left)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Left", Description = "Move left.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Left { get; set; }

            [JsonProperty]
            [DefaultValue(Key.Enter)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Action", Description = "Starts a game or confirm a selection.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Action { get; set; }

            [JsonProperty]
            [DefaultValue(Key.Back)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Back", Description = "Go back to previous screen, close a window or cancel an action.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Back { get; set; }

            [JsonProperty]
            [DefaultValue(Key.Delete)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Clear", Description = "Used to clear settings or input boxes.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Clear { get; set; }

            [JsonProperty]
            [DefaultValue(Key.OemTilde)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Minimize", Description = "Minimizes the current search window. Does not affect other windows.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Minimize { get; set; }

            [JsonProperty]
            [DefaultValue(Key.Escape)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Exit", Description = "Closes the current window.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Exit { get; set; }

            [SettingCreateEmpty]
            [JsonProperty]
            public LayoutConfig LayoutConfig { get; set; }

        }

        public class Triggers
        {
            [JsonProperty]
            [DefaultValue(Key.F3)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Search", Description = "Shows the main search window.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Search { get; set; }

            [JsonProperty]
            [DefaultValue(0)]
            public int SearchTriggerDelayInMilliseconds { get; set; }

            [JsonProperty]
            [DefaultValue(Key.F4)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Favourites", Description = "Shows the favourites search window.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Favourites { get; set; }
            [JsonProperty]
            [DefaultValue(0)]
            public int FavouritesTriggerDelayInMilliseconds { get; set; }


            [JsonProperty]
            [DefaultValue(Key.F5)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Genre", Description = "Shows the genre search window.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Genre { get; set; }

            [JsonProperty]
            [DefaultValue(0)]
            public int GenreTriggerDelayInMilliseconds { get; set; }

          


            [JsonProperty]
            [DefaultValue(Key.F10)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Settings", Description = "Shows this settings window.", ActionType = SettingActionType.SetKeyMultiple)]
            public KeyList Settings { get; set; }

            [JsonProperty]
            [DefaultValue(0)]
            public int SettingsTriggerDelayInMilliseconds { get; set; }

        }


        [SettingSection("General")]
        public class GeneralSection
        {
            [JsonProperty]
            [SettingType(Type = SettingsType.FolderPath, Title = "HyperSpin path", Description = "Specifies the location of the main HyperSpin folder.")]
            public string HyperSpinPath { get; set; }

            [JsonProperty]
            [SettingType(Type = SettingsType.FilePath, Title = "Rocketlauncher path", Description = "Specifies the location of the Rocketlauncher exe.")]
            public string RocketLauncherExePath { get; set; }

            [JsonProperty]
            [DefaultValue(TextInputType.Qwerty)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Keyboard type", Description = "Specifies the keyboard type to use for search criteria input.", EnumSource = typeof(TextInputType))]
            public TextInputType? KeyboardType { get; set; }

            [JsonProperty]
            [DefaultValue(SearchMode.Contains)]
            [SettingType(Type = SettingsType.MultiOption, Title = "Search mode", Description = "'Contains' search is slower but yields the best results.", EnumSource =typeof(SearchMode))]
            public SearchMode SearchMode { get; set; }

            [JsonProperty]
            [DefaultValue(false)]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Cab Mode", Description = "If enabled input from your physical keyboard will be ignored when inputting search criteria.")]
            public bool? CabMode { get; set; }

            [JsonProperty]
            [DefaultValue(false)]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Hide mouse cursor", Description = "Hides the mouse cursor if enabled.")]
            public bool? HideMouseCursor { get; set; }

            [JsonProperty]
            [DefaultValue(false)]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Standalone mode", Description = "If enabled allows HyperSearch to launch outside of Hyperspin.")]
            public bool? StandAloneMode { get; set; }

            [JsonProperty]
            [DefaultValue(1600)]
            public int StandaloneWidth { get; set; }

            [JsonProperty]
            [DefaultValue(900)]
            public int StandaloneHeight { get; set; }

            [JsonProperty]
            [DefaultValue(0)]
            public int BalloonToolTipTimeOutInMilliseconds { get; set; }

        }


        [SettingSection("Misc")]
        public class MiscSection
        {
            [JsonProperty]
            [DefaultValue(true)]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Show game videos", Description = "If enabled displays the selected game's video if available.")]
            public bool EnableGameVideos { get; set; }

            [DefaultValue(900)]
            public int GameVideoPopupTimeoutInMilliseconds { get; set; }

            [JsonProperty]
            [DefaultValue(true)]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Show system images on system filter", Description = "If enabled displays system images on the system filter list.")]
            public bool ShowSystemImagesOnFilter { get; set; }

            [JsonProperty]
            [DefaultValue(true)]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Show system images on results", Description = "If enabled displays system images on the results view.")]
            public bool ShowSystemImagesOnResults { get; set; }

            [JsonProperty]
            [DefaultValue(true)]
            [SettingType(Type = SettingsType.TrueFalse, Title = "Show wheel images on results", Description = "If enabled displays wheel images on the results view.")]
            public bool ShowWheelImagesOnResults { get; set; }
        }

}

    public class KeyListConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            KeyList kl = (KeyList)value;
            var keyCodes = kl.ToKeyCodeList();
            JArray.FromObject(keyCodes).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var keyList = new KeyList();

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray array = JArray.Load(reader);

                var keyCodes = array.ToObject<int[]>();

                for (int i = 0; i < keyCodes.Length; i++)
                {
                    // TODO: Add error handling for invalid values? Or just ignore invalids with a warning in the log
                    var key = (System.Windows.Input.Key)keyCodes[i];

                    keyList.Add(key);
                }

            }

            return keyList;
        }

        public override bool CanRead { get { return true; } }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(KeyList);
        }
    }

    public enum TextInputType
    {
        AtoZ,
        Qwerty,
        Azerty,
        Orb
    }

    public enum SearchMode
    {
        Contains,
        StartsWith
    }
}
