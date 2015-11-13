using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HyperSearch
{
    public class ErrorHandler
    {
        public static void HandleException(Exception ex)
        {
            LogException(ex);
        }

        public static void LogException(Exception ex)
        {
            Console.WriteLine("ERROR: {0}", ex.ToString());
            MainWindow.LogStatic("ERROR: {0}", ex.ToString());
        }

        public static void ClearSessionFile()
        {
            try
            {
                string logFile = Global.BuildFilePathInAppDir("HyperSearchLog.txt");

                File.Create(logFile).Close();
            }
            catch
            {
                //nothing we can do...
            }
        }
        public static void LogRawLineToSessionFile(string line)
        {
            try
            {
                string logFile = Global.BuildFilePathInAppDir("HyperSearchLog.txt");

                File.AppendAllText(logFile, line);
            }
            catch
            {
                //nothing we can do...
            }
        }
    }
}
