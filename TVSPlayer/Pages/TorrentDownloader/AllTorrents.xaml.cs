﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TVSPlayer {
    /// <summary>
    /// Interaction logic for AllTorrents.xaml
    /// </summary>
    public partial class AllTorrents : Page {
        public AllTorrents() {
            InitializeComponent();
        }
        Dictionary<TorrentUserControl, TorrentDownloader> userControls = new Dictionary<TorrentUserControl, TorrentDownloader>();
        Dictionary<FinishedTorrentUserControl, Torrent> uielements = new Dictionary<FinishedTorrentUserControl, Torrent>();

        private void Panel_Loaded(object sender, RoutedEventArgs e) {
            Render();
        }

        private async void Render() {
            while (IsLoaded) {
                var list = TorrentDownloader.GetTorrents();
                if (list.Count > userControls.Count) {
                    var torrents = new List<Torrent>();
                    userControls.Values.ToList().ForEach(x => torrents.Add(x.TorrentSource));
                    List<TorrentDownloader> changes = list.Where(x => !torrents.Contains(x.TorrentSource)).ToList();
                    foreach (var item in changes) {
                        Dispatcher.Invoke(() => {
                            TorrentUserControl tcu = new TorrentUserControl(item, Panel);
                            tcu.Height = 65;
                            tcu.Opacity = 0;
                            tcu.Margin = new Thickness(10, 0, 10, 0);
                            Panel.Children.Add(tcu);
                            userControls.Add(tcu, item);
                            Storyboard sb = (Storyboard)FindResource("OpacityUp");
                            sb.Begin(tcu);
                        });
                    }
                }
                if (list.Count < userControls.Count) {
                    var torrents = new List<Torrent>();
                    var secondList = new List<Torrent>();
                    list.ForEach(x => secondList.Add(x.TorrentSource));
                    userControls.Values.ToList().ForEach(x => torrents.Add(x.TorrentSource));
                    List<Torrent> changes = torrents.Except(secondList).ToList();
                    Dictionary<TorrentUserControl, TorrentDownloader> values = userControls.Where(x => changes.Contains(x.Value.TorrentSource)).ToDictionary(x => x.Key, x => x.Value);
                    foreach (var item in values) {
                        Storyboard sb = (Storyboard)FindResource("OpacityDown");
                        var clone = sb.Clone();
                        clone.Completed += (s, ev) => {
                            Dispatcher.Invoke(() => {
                                Panel.Children.Remove(item.Key);
                            });
                        };
                        clone.Begin(item.Key);
                        userControls.Remove(item.Key);

                    }
                }
                await Task.Run(() => {
                    Thread.Sleep(250);
                });
            }
        }

        private void RenderOnFinished() {
            Task.Run(() => {
                bool isLoaded = true;
                Dispatcher.Invoke(() => { isLoaded = IsLoaded; });
                while (isLoaded) {
                    var list = TorrentDatabase.Load().Where(x => x.HasFinished).ToList();
                    list.Reverse();
                    if (list.Count > uielements.Count) {
                        var torrents = new List<Torrent>();
                        uielements.Values.ToList().ForEach(x => torrents.Add(x));
                        List<Torrent> changes = list.Where(x => !torrents.Contains(x)).ToList();
                        foreach (var item in changes) {
                            Dispatcher.Invoke(() => {
                                FinishedTorrentUserControl tcu = new FinishedTorrentUserControl(item);
                                tcu.Height = 65;
                                tcu.Margin = new Thickness(10, 0, 10, 0);
                                tcu.Opacity = 0;
                                SecondPanel.Children.Add(tcu);
                                var sb = (Storyboard)FindResource("OpacityUp");
                                sb.Begin(tcu);
                                uielements.Add(tcu, item);
                            });
                        }
                    }
                    Thread.Sleep(2000);
                    Dispatcher.Invoke(() => { isLoaded = IsLoaded; });
                }
            });
        }

        private void SecondPanel_Loaded(object sender, RoutedEventArgs e) {
            RenderOnFinished();
        }
    }
}
