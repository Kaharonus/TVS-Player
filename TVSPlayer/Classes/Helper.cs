﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TVS.API;

namespace TVSPlayer {
    class Helper {
        public static string data = @"C:\Users\Public\Documents\TVS-Player\";
        public static string posterLink = "https://www.thetvdb.com/banners/";

        /// <summary>
        /// Generates default name for episode.
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
            return name;
        }
        public static string GenerateName(Episode episode) {
            if (episode.airedSeason < 10) {
                return episode.airedEpisodeNumber < 10 ? "S0" + episode.airedSeason + "E0" + episode.airedEpisodeNumber : "S0" + episode.airedSeason + "E" + episode.airedEpisodeNumber;
            } else {
                return episode.airedEpisodeNumber < 10 ? "S" + episode.airedSeason + "E0" + episode.airedEpisodeNumber : "S" + episode.airedSeason + "E" + episode.airedEpisodeNumber;
            }
        }


    }
    static class Extensions{
        public static void WaitAll(this IEnumerable<Task> tasks) {
            Task.WaitAll(tasks.ToArray());
        }
    }
}
