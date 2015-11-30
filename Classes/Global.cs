using HyperSearch.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HyperSearch
{
    public static class Global
    {
        public static string AlternativeSystemWheelSourceFolder;
        public static string AlternativeGameWheelSourceFolder;

        public static string BuildFilePathInHyperspinDir(string filename, params object[] args)
        {
            return Path.Combine(HyperSearchSettings.Instance().General.HyperSpinPathAbsolute ?? "", string.Format(filename, args));
        }

        public static string BuildFilePathInAppDir(string filename)
        {
            return string.Format("{0}{1}", System.AppDomain.CurrentDomain.BaseDirectory, filename.TrimStart(new char[] { '\\', '/' }));
        }

        public static string BuildFilePathInResourceDir(string filename, params object[] args)
        {
            return BuildFilePathInAppDir(string.Format("Resources\\{0}", string.Format(filename, args)));
        }
    }
}
