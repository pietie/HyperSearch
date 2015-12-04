using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace HyperSearch.Classes
{
    public static class FFMPEG
    {
        public class FFMetadata
        {
            public TimeSpan Duration { get; set; }
            public double? FramesPerSecond { get; set; }
            public string WidthxHeight { get; set; }
        }

        public static Process CreateNewProcess(string args)
        {
            var ffmpeg = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.FileName = Global.BuildFilePathInAppDir(@"ffmpeg\ffmpeg.exe");
            startInfo.Arguments = args;

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            ffmpeg.StartInfo = startInfo;
            ffmpeg.EnableRaisingEvents = true;

            return ffmpeg;
        }

        public static FFMetadata ExtractMetadata(string inputFilePath)
        {
            FFMetadata ffmetadata = new FFMetadata();

            var ffmpegMetaData = FFMPEG.CreateNewProcess(string.Format("-i \"{0}\" -f ffmetadata -", inputFilePath));

            ffmpegMetaData.Start();

            var consoleOutput = ffmpegMetaData.StandardError.ReadToEnd();



            // TODO: Find a better (more robust) way of determining the frame rate);
            //
            // Sample:
            //  Duration: 00:00:01.20, start: 0.000000, bitrate: 1980 kb/s
            //    Stream #0:0: Video: vp6a, yuva420p, 720x486, 1536 kb/s, 30 tbr, 1k tbn, 1k tbc
            //
            if (consoleOutput != null)
            {
                double? framesPerSecond = null;
                string widthHeight = null;
                string durationStr = null;

                var durationRegEx = new Regex("Duration: \\d\\d:\\d\\d:\\d\\d(\\.\\d\\d\\d?)?", RegexOptions.IgnoreCase);

                var durMatch = durationRegEx.Match(consoleOutput);

                if (durMatch.Success)
                {
                    durationStr = durMatch.Value.ToLower().Replace(" ", "").Replace("duration:", "");
                }

                var lines = consoleOutput.Split('\n');
                //?Stream #0:0(eng): Video:
                var videoStreamMatcher = new Regex("^(\\s+)?Stream\\s?#0:0.+Video:", RegexOptions.IgnoreCase);

                var videoStream = lines.FirstOrDefault(l => videoStreamMatcher.IsMatch(l));

                var widthHeightMatcher = new Regex("^[0-9]+x[0-9]+", RegexOptions.IgnoreCase);
                var fpsMatcher = new Regex("^(\\d+(\\.\\d{1,2})?)+k?\\s?(fps|tbr)", RegexOptions.IgnoreCase);

                if (videoStream != null)
                {
                    var csv = videoStream.Split(',');
                    var possibleFps = "";

                    foreach (var el in csv)
                    {
                        if (string.IsNullOrEmpty(el)) continue;

                        var element = el.Trim();

                        var m1 = widthHeightMatcher.Match(element);

                        if (widthHeight == null && m1.Success)
                        {
                            widthHeight = m1.Value;
                        }

                        var m2 = fpsMatcher.Match(element);

                        if (framesPerSecond == null && m2.Success)
                        {
                            try
                            {
                                framesPerSecond = double.Parse(m2.Value.ToLower().Replace("fps", "").Replace("tbr", ""));
                            }
                            catch { }
                        }

                        //var pairs = element.Split(' ');

                        //if (pairs.Length < 2) continue;

                        //if (pairs[1].Equals("fps", StringComparison.OrdinalIgnoreCase) || pairs[1].Equals("tbr", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    possibleFps = pairs[0];
                        //    break;
                        //}
                    }

                    int tmp;

                    if (int.TryParse(possibleFps, out tmp))
                    {
                        framesPerSecond = tmp;
                    }
                }
                else
                {
                    // error out? Or just log and fallback to default
                }

                if (durationStr != null)
                {
                    try
                    {
                        var us = new System.Globalization.CultureInfo("en-US");

                        ffmetadata.Duration = TimeSpan.ParseExact(durationStr, "g", us);
                    }
                    catch
                    {
                        // ignore any failure to parse duration
                    }
                }

                ffmetadata.FramesPerSecond = framesPerSecond;
                ffmetadata.WidthxHeight = widthHeight;
            }

            return ffmetadata;
        }

        public static byte[] ExtractFrame(string inputFilePath)
        {
            //ffmpeg  -y -itsoffset -10  -i daphne.mp4 -vcodec mjpeg -vframes 1 -an -f rawvideo -s 320x240 test.jpg
            //var ffmpeg = FFMPEG.CreateNewProcess(string.Format("-y -itsoffset -10 -i \"{0}\" -s 320x240 -vcodec png -vframes 1 -an -f image2pipe -", inputFilePath));
            var ffmpeg = FFMPEG.CreateNewProcess(string.Format("-y -itsoffset -10 -i \"{0}\" -s 320x240 -vcodec png -vframes 1 -f image2pipe -", inputFilePath));

            ffmpeg.Start();

            byte[] buffer = new byte[2048];

            List<byte> dataArray = new List<byte>();
            var read = ffmpeg.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);

            while (read > 0)
            {
                byte[] d = new byte[read];

                Array.Copy(buffer, d, read);

                dataArray.AddRange(d);

                read = ffmpeg.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
            }

            var consoleOutput = ffmpeg.StandardError.ReadToEnd();

            return dataArray.ToArray();
        }
    }
}
