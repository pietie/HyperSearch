using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperSearch.Windows.Settings
{
    public enum SettingsType
    {
        Ignore,
        TrueFalse,
        MultiOption,
        Slider,
        FolderPath,
        FilePath,
        Action
    }

    public enum SettingActionType
    {
        None,
        ClearSearchCache,
        ConfigureButtonLayout,
        SetKeyMultiple,
        SetKeySingle

    }


    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SettingCreateEmptyAttribute : Attribute
    {
    }


    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Class)]
    public class SettingSectionAttribute : Attribute
    {
        public SettingSectionAttribute() { }

        public SettingSectionAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SettingTypeAttribute : Attribute
    {
        public SettingsType Type { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        //public string MutliValueCsv { get; set; }
        public Type EnumSource { get; set; }

        public string ActionTitle { get; set; }

        public SettingActionType ActionType { get; set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SettingCompositeAttribute : Attribute
    {
        
    }


        [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SettingsSectionOrderByAttribute : Attribute
    {
        public string SectionNameCsv { get; set; }
        public SettingsSectionOrderByAttribute(string sectionCsv)
        {
            SectionNameCsv = sectionCsv;
        }
    }
}
