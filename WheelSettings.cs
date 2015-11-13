using Gajatko.IniFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HyperSearch
{
    public class WheelSettings
    {
        public Filters FilterSection { get; set; }
        public VideoDefaults VideoDefaultsSection { get; set; }
        public void InitFromIniFile(string iniFilepath)
        {
            var ini = IniFile.FromFile(iniFilepath);

            this.FilterSection = new Filters();
            this.VideoDefaultsSection = new VideoDefaults();

            InitSections(ini, this.FilterSection, this.VideoDefaultsSection);
        }

        [IniHeader(Name = "filters")]
        public class Filters
        {
            [IniField(Name = "parents_only")]
            public bool ParentsOnly { get; set; }
            
            [IniField(Name = "themes_only")]
            public bool ThemesOnly{ get; set; }
            
            [IniField(Name = "wheels_only")]
            public bool WheelsOnly { get; set; }
            
            [IniField(Name = "roms_only")]
            public bool RomsOnly { get; set; }
        }

        [IniHeader(Name = "video defaults")]
        public class VideoDefaults
        {
            [IniField(Name = "path")]
            public string CustomVideoPath { get; set; }
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
                        throw;
                    }

                    //iniField.Name
                    //var aa = prop.CustomAttributes.ToList();

                }
            }
        }
    }
}
