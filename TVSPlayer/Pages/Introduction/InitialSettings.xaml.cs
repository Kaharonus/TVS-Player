﻿using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Interaction logic for InitialSettings.xaml
    /// </summary>
    public partial class InitialSettings : Page {
        public InitialSettings() {
            InitializeComponent();
        }
        private void ThirdScan_GotFocus(object sender, RoutedEventArgs e) {
            ThirdScan.GotFocus -= ThirdScan_GotFocus;
            ThirdScan.Text = "";
        }

        private void ThirdFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            var fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                ThirdScan.Focus();
                ThirdScan.Text = fbd.SelectedPath;
            }
        }

        private void FirstFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            var fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                FirstScan.Focus();
                FirstScan.Text = fbd.SelectedPath;
            }
        }

        private void FirstScan_GotFocus(object sender, RoutedEventArgs e) {
            FirstScan.GotFocus -= FirstScan_GotFocus;
            FirstScan.Text = "";
        }

        private void SecondScan_GotFocus(object sender, RoutedEventArgs e) {
            SecondScan.GotFocus -= SecondScan_GotFocus;
            SecondScan.Text = "";
        }

        private void SecondFolder_MouseUp(object sender, MouseButtonEventArgs e) {
            var fbd = new VistaFolderBrowserDialog();
            if ((bool)fbd.ShowDialog()) {
                SecondScan.Focus();
                SecondScan.Text = fbd.SelectedPath;
            }
        }
        /// <summary>
        /// Hide this function in VS and do not look at it.. ever
        /// </summary>
        /// <returns>true if everything is fine, false if it isn't</returns>
        private bool DoChecks() {
            if (FirstScan.Text == SecondScan.Text && FirstScan.Text != "Select directory") { return false; }
            if (FirstScan.Text == ThirdScan.Text && ThirdScan.Text != "Select directory") { return false; }
            if (SecondScan.Text == ThirdScan.Text && SecondScan.Text != "Select directory") { return false; }
            if (FirstScan.Text != "Select directory" && !String.IsNullOrEmpty(FirstScan.Text)) {
                if (!Directory.Exists(FirstScan.Text)) { return false; }
            }
            if (SecondScan.Text != "Select directory" && !String.IsNullOrEmpty(SecondScan.Text)) {
                if (!Directory.Exists(SecondScan.Text)) { return false; }
            }
            if (ThirdScan.Text != "Select directory" && !String.IsNullOrEmpty(ThirdScan.Text)) {
                if (!Directory.Exists(ThirdScan.Text)) { return false; }
            }
            if (CacheFolder.Text != "Select directory" && !String.IsNullOrEmpty(CacheFolder.Text)) {
                if ((CacheFolder.Text != FirstScan.Text && CacheFolder.Text != SecondScan.Text && CacheFolder.Text != ThirdScan.Text)) {
                    if (!Directory.Exists(CacheFolder.Text)) { return false; }
                }

            }
            return true;
        }

        private void SaveLocations() {
            if (Directory.Exists(FirstScan.Text)) {
                Properties.Settings.Default.FirstScan = FirstScan.Text;
                Properties.Settings.Default.Save();
            }
            if (Directory.Exists(SecondScan.Text)) {
                Properties.Settings.Default.SecondScan = SecondScan.Text;
                Properties.Settings.Default.Save();
            }
            if (Directory.Exists(ThirdScan.Text)) {
                Properties.Settings.Default.ThirdScan = ThirdScan.Text;
                Properties.Settings.Default.Save();
            }
            if ((bool)AutoDownload.IsChecked) {
                Properties.Settings.Default.CacheLocation = CacheFolder.Text;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.AutoDownload = true;
                Properties.Settings.Default.Save();
            }
        }

        private void CacheSelect_MouseUp(object sender, MouseButtonEventArgs e) {
            if ((bool)AutoDownload.IsChecked) {
                var fbd = new VistaFolderBrowserDialog();
                if ((bool)fbd.ShowDialog()) {
                    CacheFolder.Focus();
                    CacheFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private void CacheFolder_GotFocus(object sender, RoutedEventArgs e) {
            CacheFolder.GotFocus -= CacheFolder_GotFocus;
            CacheFolder.Text = "";
        }

        private void AutoDownload_Checked(object sender, RoutedEventArgs e) {
            MessageBoxResult res = MessageBox.Show("This feature will download new episodes using torrent.\nDepending on where you live this might be illegal in your country.\nI am not responsible for your actions.\n\n Are you sure you want to continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes) {
                CacheFolder.IsEnabled = true;
                Storyboard sb = (Storyboard)FindResource("ShowButton");
                sb.Begin(CacheFolder);
                Storyboard sbOp = (Storyboard)FindResource("OpacityUp");
                sbOp.Begin(CacheSelect);
            } else {
                AutoDownload.IsChecked = false;
            }

        }

        private void AutoDownload_Unchecked(object sender, RoutedEventArgs e) {
            CacheFolder.IsEnabled = false;
            Storyboard sb = (Storyboard)FindResource("HideButton");
            sb.Begin(CacheFolder);
            Storyboard sbOp = (Storyboard)FindResource("OpacityDown");
            sbOp.Begin(CacheSelect);
        }

        private void Continue_MouseUp(object sender, MouseButtonEventArgs e) {
            if (DoChecks()) {
                SaveLocations();
                MainWindow.RemovePage();
                MainWindow.AddPage(new StartUp());
            } else {
                MessageBox.Show("Either some directory does not exist or some of the directories are the same");
            }
        }

        private void Back_MouseUp(object sender, MouseButtonEventArgs e) {
            MainWindow.RemovePage();
            MainWindow.AddPage(new SelectThemeStartUp());
        }
    }
}
