﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TVS.API;

namespace TVSPlayer
{
    class UpdateDatabase {

        /// <summary>
        /// Starts checking if all files are where they are supposed to be and if database is updated
        /// </summary>
        public async static void StartUpdateBackground(bool startNow = true) {
            if (startNow) {
                await CheckFiles();
                await Update();
            }
            Timer checktimer = new Timer(600000);
            checktimer.Elapsed += async (s, ev) => await CheckFiles();
            checktimer.Start();
            Timer timer = new Timer(3600000);
            timer.Elapsed += async (s, ev) => await Update();
            timer.Start();
        }

        /// <summary>
        /// Cheks if files in database actually exist on hdd
        /// </summary>
        /// <returns></returns>
        public async static Task CheckFiles() {
            await Task.Run(() => {
                foreach (var series in Database.GetSeries()) {
                    foreach (var episode in Database.GetEpisodes(series.id)) {
                        for (int i = episode.files.Count - 1; i >= 0; i--) {
                            if (!File.Exists(episode.files[i].NewName)) {
                                episode.files.RemoveAt(i);
                                Database.EditEpisode(series.id, episode.id, episode);
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Updates database if it hasnt been updated in the last day and autodownloads every Series that has autodownload enabled
        /// </summary>
        /// <returns></returns>
        public async static Task Update() {
            if (Settings.LastCheck.AddDays(1).Date <= DateTime.Now.Date) {
                await Task.Run(async () => {
                    List<int> ids = Series.GetUpdates(Settings.LastCheck);
                    List<Series> series = Database.GetSeries();
                    ids = ids.Where(x => series.Any(y => y.id == x)).ToList();
                    foreach (int id in ids) {
                       await UpdateFullSeries(id);
                    }
                });
                await DownloadLastWeek();             
                Settings.LastCheck = DateTime.Now;
            }
            await Renamer.ScanAndRename(Database.GetSeries());
         }

        private async static Task DownloadLastWeek() {
            await Task.Run(async() => {
                var series = Database.GetSeries().Where(x=>x.autoDownload);
                var episodes = new Dictionary<Series,List<Episode>>();
                foreach (var se in series) {
                    //adds episodes that dont have files and have been released in last week
                    episodes.Add(se, Database.GetEpisodes(se.id).Where(x => x.files.Count == 0 && !String.IsNullOrEmpty(x.firstAired) && Helper.ParseAirDate(x.firstAired) > DateTime.Now.AddDays(-7) && Helper.ParseAirDate(x.firstAired).AddDays(1) < DateTime.Now).ToList());
                }
                foreach (var combination in episodes) {
                    foreach (var episode in combination.Value) {
                        if (TorrentDatabase.Load().Where(x => x.Episode.id == episode.id).ToList().Count == 0) {
                            TorrentDownloader downloader = new TorrentDownloader(await Torrent.SearchSingle(combination.Key, episode, Settings.DownloadQuality));
                            await downloader.Download();
                        }
                    }

                }
            });
        }

        public async static Task UpdateFullSeries(Series series) {
            await UpdateFullSeries(series.id);
        }

        public async static Task UpdateFullSeries(int id) {
            await Task.Run(() => {
                List<Task> tasks = new List<Task>();
                tasks.Add(UpdateSeries(id));
                tasks.Add(UpdateEpisodes(id));
                tasks.Add(UpdatePosters(id));
                tasks.Add(UpdateActors(id));
                tasks.WaitAll();
            });
        }

        private static async Task UpdateSeries(int id) {
            await Task.Run(() => {
                var series = Database.GetSeries(id);
                var newseries = Series.GetSeries(id);
                if (series.Compare(newseries)) {
                    series.Update(newseries);
                    Database.EditSeries(id, series);
                }
            });
        }

        private static async Task UpdateEpisodes(int id) {
            await Task.Run(() => {
                var list = Episode.GetEpisodes(id);
                var oldList = Database.GetEpisodes(id);
                foreach (var episode in list) {
                    var ep = oldList.Where(x => x.id == episode.id).FirstOrDefault();
                    if (ep != null) {
                        if (ep.Compare(episode)) {
                            ep.Update(episode);
                            Database.EditEpisode(id, ep.id, ep);
                        }
                    } else {
                        Database.AddEpisode(id, episode);
                    }
                }
            });
        }

        private static async Task UpdateActors(int id) {
            await Task.Run(() => {
                var list = Actor.GetActors(id);
                var oldList = Database.GetActors(id);
                foreach (var actor in list) {
                    var ac = oldList.Where(x => x.id == actor.id).FirstOrDefault();
                    if (ac != null) {
                        if (ac.Compare(actor)) {
                            ac.Update(actor);
                            Database.EditActor(id, ac.id, ac);
                        }
                    } else {
                        Database.AddActor(id, actor);
                    }
                }

            });
        }

        private static async Task UpdatePosters(int id) {
            await Task.Run(() => {
                var list = Poster.GetPosters(id);
                var oldList = Database.GetPosters(id);
                foreach (var poster in list) {
                    var po = oldList.Where(x => x.id == poster.id).FirstOrDefault();
                    if (po != null) {
                        if (po.Compare(poster)) {
                            po.Update(poster);
                            Database.EditPoster(id, po.id, po);
                        }
                    } else {
                        Database.AddPoster(id, poster);
                    }
                }
                //Remove results that got deleted
                oldList.Where(x => !list.Any(y => y.id == x.id)).ToList().ForEach(x =>
                                Database.RemovePoster(id, x.id));
            });
        }

    }
}
