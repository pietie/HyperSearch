using System;
using System.Windows.Controls;

namespace HyperSearch.Classes
{
    public class SystemSoundPlayer
    {
        private static SystemSoundPlayer _instance;
        private MediaElement _mediaElement;
        private string _sourceDir;

        private SystemSoundPlayer() { throw new NotImplementedException(); }

        private SystemSoundPlayer(Panel host, string sourceDir)
        {
            _mediaElement = new MediaElement();
            _mediaElement.LoadedBehavior = MediaState.Manual;
            //_mediaElement.Volume = 0.006; // TODO: From config!

            this._sourceDir = sourceDir;
            host.Children.Add(_mediaElement);
        }

        public static SystemSoundPlayer Instance()
        {
            if (_instance == null) throw new Exception("SystemSoundPlayer not yet properly initialised.");

            return _instance;
        }

        public static SystemSoundPlayer Init(Panel host, string sourceDir)
        {
            if (_instance == null)
            {
                _instance = new SystemSoundPlayer(host, sourceDir);
            }

            return _instance;
        }

        public void PlaySound(SystemSound sound)
        {
            string filename = null;

            switch (sound)
            {
                case SystemSound.WheelOut:
                    filename = "Sound_Wheel_Out.mp3";
                    break;
                case SystemSound.WheelIn:
                    filename = "Sound_Wheel_In.mp3";
                    break;
                case SystemSound.WheelJump:
                    filename = "Sound_Wheel_Jump.mp3";
                    break;
                case SystemSound.ScreenClick:
                    filename = "Sound_Screen_Click.mp3";
                    break;
                case SystemSound.ScreenIn:
                    filename = "Sound_Screen_In.mp3";
                    break;
                case SystemSound.ScreenOut:
                    filename = "Sound_Screen_Out.mp3";
                    break;
                case SystemSound.LetterClick:
                    filename = "Sound_Letter_Click.mp3";
                    break;
                default:
                    throw new Exception("Unsupported SystemSound: " + sound);
            }

            if (filename == null) throw new Exception("filename not set");

            _mediaElement.Source = new Uri(string.Format("{0}\\{1}", _sourceDir.TrimEnd(new char[] { '\\', '/' }), filename.TrimStart(new char[] { '\\', '/' })), UriKind.Absolute);
            _mediaElement.Play();
        }


    }

    public enum SystemSound
    {
        WheelOut,
        WheelIn,
        WheelJump,
        ScreenClick,
        ScreenIn,
        ScreenOut,
        LetterClick
    }
}
