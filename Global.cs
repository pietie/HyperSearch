using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HyperSearch
{
    public static class Global
    {
        public static string HsPath;
        public static string LauncherFullPath;
        public static System.Windows.Input.Key? MinimizeKeyOld;
        public static string AlternativeGameWheelSourceFolder;


        public static KeyList ActionKey;
        public static KeyList BackKey;
        public static KeyList ExitKey;
        public static KeyList MinimizeKey;

        public static KeyList UpKey;
        public static KeyList RightKey;
        public static KeyList DownKey;
        public static KeyList LeftKey;


        public static string BuildFilePathInHyperspinDir(string filename, params object[] args)
        {
            var str1=  string.Format("{0}\\{1}", HsPath, string.Format(filename.TrimStart(new char[] { '\\', '/' }), args));

            var str2 = Path.Combine(HsPath, string.Format(filename, args));

            System.Diagnostics.Debug.Assert(str1.Equals(str2));

            return str1;
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
