using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HyperSearch.Windows.Settings
{
    public class SettingsDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BooleanDataTemplate { get; set; }
        public DataTemplate MultiOptionDataTemplate { get; set; }
        public DataTemplate SliderDataTemplate { get; set; }
        public DataTemplate ActionDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
                   DependencyObject container)
        {
            (container as ContentPresenter).HorizontalAlignment = HorizontalAlignment.Stretch;

            var lvi = item as SettingsListViewItem;

            if (lvi.Property == null) return ActionDataTemplate;

            var attrib = lvi.Property.GetCustomAttributes(typeof(SettingTypeAttribute), false).FirstOrDefault() as SettingTypeAttribute;

            if (attrib != null)
            {
                lvi.Title = attrib.Title;
                lvi.Description = attrib.Description;

                switch (attrib.Type)
                {
                    case SettingsType.TrueFalse:
                        return BooleanDataTemplate;
                    case SettingsType.MultiOption:
                        return MultiOptionDataTemplate;
                    case SettingsType.Slider:
                        return SliderDataTemplate;
                    case SettingsType.FolderPath:
                        return MultiOptionDataTemplate;
                    case SettingsType.FilePath:
                        return MultiOptionDataTemplate;
                    case SettingsType.Action:
                        return ActionDataTemplate;
                    default:
                        break;
                }
            }

            return BooleanDataTemplate;
        }
    }
}
