﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using TVS.API;
using System.Management;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace TVSPlayer {
    class Helper {

        /// <summary>
        /// Path to cached data
        /// </summary>
        public static string data = @"C:\Users\Public\Documents\TVS-Player\";

        /// <summary>
        /// Link for retrieving poster data
        /// </summary>
        public static string posterLink = "https://www.thetvdb.com/banners/";

        /// <summary>
        /// Generates default name for episode. SeriesName - SxxExx - EpisodeName
        /// </summary>
        /// <param name="episode">Which episode to generate for</param>
        /// <param name="series">Which season to generate for</param>
        public static string GenerateName(Series series, Episode episode) {
            string name = null;
            if (episode.airedSeason < 10) {
                name = episode.airedEpisodeNumber < 10 ? series.seriesName + " - S0" + episode.airedSeason + "E0" + episode.airedEpisodeNumber + " - " + episode.episodeName : name = series.seriesName + " - S0" + episode.airedSeason + "E" + episode.airedEpisodeNumber + " - " + episode.episodeName;
            } else if (episode.airedSeason >= 10) {
                name = episode.airedEpisodeNumber < 10 ? series.seriesName + " - S" + episode.airedSeason + "E0" + episode.airedEpisodeNumber + " - " + episode.episodeName : series.seriesName + " - S" + episode.airedSeason + "E" + episode.airedEpisodeNumber + " - " + episode.episodeName;
            }
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid) {
                name = name.Replace(c.ToString(), "");
            }
            return name;
        }

        /// <summary>
        /// Returns SxxExx according to Episode
        /// </summary>
        /// <param name="episode">Any Episode</param>
        /// <returns></returns>
        public static string GenerateName(Episode episode) {
            if (episode.airedSeason < 10) {
                return episode.airedEpisodeNumber < 10 ? "S0" + episode.airedSeason + "E0" + episode.airedEpisodeNumber : "S0" + episode.airedSeason + "E" + episode.airedEpisodeNumber;
            } else {
                return episode.airedEpisodeNumber < 10 ? "S" + episode.airedSeason + "E0" + episode.airedEpisodeNumber : "S" + episode.airedSeason + "E" + episode.airedEpisodeNumber;
            }
        }

        /// <summary>
        /// Get string in format h:mm:ss from player media lenght or current position
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetTime(long value) {
            int minutes, seconds, hours;
            minutes = seconds = hours = 0;
            value = value / 10000000;
            hours = Convert.ToInt32(Math.Floor((double)(value / 60 / 60)));
            minutes = Convert.ToInt32(Math.Floor((double)(value / 60 - 60 * hours)));

            seconds = Convert.ToInt32(Math.Floor((double)(value - ((60 * 60 * hours) + (60 * minutes)))));
            string hoursString = hours > 0 ? hours + ":" : "";
            string minutesString = minutes >= 10 ? minutes.ToString() + ":" : "0" + minutes + ":";
            string secondsString = seconds >= 10 ? seconds.ToString() : "0" + seconds;
            return hoursString + minutesString + secondsString;
        }

        /// <summary>
        /// Cheks if TVSPlyer is already running
        /// </summary>
        /// <returns></returns>
        public static bool CheckRunning() {
            var procName = Process.GetCurrentProcess();
            List<Process> processes = Process.GetProcessesByName(procName.ProcessName).ToList();
            processes.Remove(procName);
            if (processes.Count > 1) {
                foreach (var proc in processes) {
                    BringProcessToFront(proc);
                }
                return false;
            } else {
                return true;
            }
        }

        public static DateTime ParseAirDate(string date) {
            return DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Brings any process to front
        /// </summary>
        /// <param name="process"></param>
        public static void BringProcessToFront(Process process) {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle)) {
                ShowWindow(handle, SW_RESTORE);
            }
            SetForegroundWindow(handle);
        }

        /// <summary>
        /// Disables Windows screen saver
        /// </summary>
        public static void DisableScreenSaver() {
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);
        }

        /// <summary>
        /// Enables Windows screen saver
        /// </summary>
        public static void EnableScreenSaver() {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        /// <summary>
        /// Only use this on first launch. If only Intel GPU is present some UI elements won't load
        /// </summary>
        public static void SetPerformanceMode() {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            ManagementObjectCollection collection = mos.Get();
            bool quality = false;
            foreach (var gpu in collection) {
                string name = gpu["Name"].ToString();
                if (gpu["Name"].ToString().ToLower().Contains("radeon") || gpu["Name"].ToString().ToLower().Contains("nvidia")) {
                    quality = true;
                }
            }
            if (quality) {
                Settings.PerformanceMode = false;
            } else {
                Settings.PerformanceMode = true;
            }
        }


        [DllImport("kernel32.dll")]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        enum EXECUTION_STATE : uint {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        const int SW_RESTORE = 9;

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);
        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

    }

    static class Extensions {

        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color color) {
            return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static BitmapImage ToBitmapImage(this Bitmap src) {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        /// <summary>
        /// Waits for all Tasks in IEnumerable to complete
        /// </summary>
        /// <param name="tasks"></param>
        public static void WaitAll(this IEnumerable<Task> tasks) {
            Task.WaitAll(tasks.ToArray());
        }


        #region MoreLINQ MaxBy Extension Method 

        /*
         * Copyright MoreLINQ 2018
         * https://github.com/morelinq/MoreLINQ/blob/master/MoreLinq/MaxBy.cs       
         * */

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
           Func<TSource, TKey> selector) {
            return source.MaxBy(selector, null);
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            comparer = comparer ?? Comparer<TKey>.Default;
            return ExtremumBy(source, selector, (x, y) => comparer.Compare(x, y));
        }

        static TSource ExtremumBy<TSource, TKey>(IEnumerable<TSource> source,
            Func<TSource, TKey> selector, Func<TKey, TKey, int> comparer) {
            using (var sourceIterator = source.GetEnumerator()) {
                if (!sourceIterator.MoveNext())
                    throw new InvalidOperationException("Sequence contains no elements");

                var extremum = sourceIterator.Current;
                var key = selector(extremum);
                while (sourceIterator.MoveNext()) {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer(candidateProjected, key) > 0) {
                        extremum = candidate;
                        key = candidateProjected;
                    }
                }

                return extremum;
            }
        }
        #endregion
    }
}
