using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace HyperSearch
{    public class RandomWheelSoundPlayer
    {
        private static RandomWheelSoundPlayer _instance;
        private MediaElement _mediaElement;
        private Random _rand;
        private Uri[] _soundFiles;

        private RandomWheelSoundPlayer() { throw new NotImplementedException(); }

        private RandomWheelSoundPlayer(Panel host, string sourceDir)
        {
            _mediaElement = new MediaElement();
            _mediaElement.LoadedBehavior = MediaState.Manual;
            //_mediaElement.Volume = 0.006; // TODO: From config!

            _rand = new Random();

            _soundFiles = Directory.EnumerateFiles(sourceDir, "*.mp3").Select<string, Uri>((s, u) => new Uri(s, UriKind.Absolute)).ToArray();

            host.Children.Add(_mediaElement);
        }

        public static RandomWheelSoundPlayer Instance()
        {
            if (_instance == null) throw new Exception("RandomWheelSoundPlayer not properly initialised yet");

            return _instance;
        }

        public static RandomWheelSoundPlayer Init(Panel host, string sourceDir)
        {
            if (_instance == null)
            {
                _instance = new RandomWheelSoundPlayer(host, sourceDir);
            }

            return _instance;
        }

        public void PlayRandomSound()
        {
            if (_mediaElement != null) _mediaElement.Stop();

            _mediaElement.Source = _soundFiles[_rand.Next(0, _soundFiles.Length - 1)];
            _mediaElement.Play();
        }

    }
}
