using Microsoft.Win32;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            slVolume.Value = mePlayer.Volume;
        }


        //Tab Explore
        public class Songs
        {
            public string Song { get; set; }
            public string Genre { get; set; }
            public string Author { get; set; }
            public string Duration { get; set; }

        }
        //Tao mang luu file va duong dan
        //Tao list luu paths, files;
        List<String> paths = new List<String>();
        List<String> files = new List<String>();
        //Tao list luu bai hat add vao datagrid
        List<Songs> songs = new List<Songs>();
        int tmp = 0;
        //Button Import
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Media files (*.mp3;*.mpg;*.mpeg)|*.mp3;*.mpg;*.mpeg|All files (*.*)|*.*";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                files.AddRange(ofd.SafeFileNames);
                paths.AddRange(ofd.FileNames);       
                for (int i = tmp; i < files.Count(); i++)
                {
                    tmp++;
                    songs.Add(new Songs()
                    {
                        Song = files[i],
                        Genre = "Unknow",
                        Author = "Sơn tùng MTP",
                        Duration = "00:00"
                    });
                    dgList.Items.Add(songs[i]);
                }
                dgList.SelectedIndex = 0;
                mePlayer.Source = new Uri(paths[0]);
            }
            
        }

        private void dgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mePlayer.Source = new Uri(paths[dgList.SelectedIndex]);
        }
      
        private void mePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            SongTitleHeader.Text = files[dgList.SelectedIndex];
            tbTitle.Text = files[dgList.SelectedIndex];
        }
        //--------------------------------------------------------------------------------------------------------------------------------

        // Button Exit, Playing, Explore, Albums, Playlists
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnPlaying_Click(object sender, RoutedEventArgs e)
        {
            Thickness margin = indicator.Margin;
            margin.Top = btnPlaying.Margin.Top + 17;
            indicator.Margin = margin;
            tbControl.SelectedIndex = 0;

        }

        private void btnExplore_Click(object sender, RoutedEventArgs e)
        {
            Thickness margin = indicator.Margin;
            margin.Top = btnExplore.Margin.Top + 17;
            indicator.Margin = margin;
            tbControl.SelectedIndex = 1;

        }

        private void btnAlbums_Click(object sender, RoutedEventArgs e)
        {
            Thickness margin = indicator.Margin;
            margin.Top = btnAlbums.Margin.Top + 17;
            indicator.Margin = margin;
        }

        private void btnPlaylists_Click(object sender, RoutedEventArgs e)
        {
            Thickness margin = indicator.Margin;
            margin.Top = btnPlaylists.Margin.Top + 17;
            indicator.Margin = margin;
        }
        //--------------------------------------------------------------------------------------------------------------------------------

        // Button Previous, Play, Pause, Stop, Next
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if(dgList.SelectedIndex == 0)
            {
                dgList.SelectedIndex = files.Count() - 1;
            }
            else if(dgList.SelectedIndex > 0)
            {
                dgList.SelectedIndex -= 1;
            }    
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Play();
            btnPlay.Visibility = Visibility.Hidden;
            btnPause.Visibility = Visibility.Visible;
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Pause();
            btnPlay.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Hidden;
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Stop();
            btnPlay.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Hidden;
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if(dgList.SelectedIndex == files.Count() - 1)
            {
                dgList.SelectedIndex = 0;
            }
            else if (dgList.SelectedIndex < files.Count() - 1)
            {
                dgList.SelectedIndex += 1;
            }
            
        }
        //--------------------------------------------------------------------------------------------------------------------------------

        //Time line
        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan))
            {
                slTimeLine.Minimum = 0;
                slTimeLine.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                slTimeLine.Value = mePlayer.Position.TotalSeconds;
            }
        }
        
        private void slTimeLine_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tbStatus.Text = TimeSpan.FromSeconds(slTimeLine.Maximum - slTimeLine.Value).ToString(@"hh\:mm\:ss");
            mePlayer.Position = TimeSpan.FromSeconds(slTimeLine.Value);
        }
        //--------------------------------------------------------------------------------------------------------------------------------

        //Volume
        private void slVolume_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.05 : -0.05;
            slVolume.Value = mePlayer.Volume;
        }
        private void slVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            slVolume.Minimum = 0;
            slVolume.Maximum = 1;
            mePlayer.Volume = slVolume.Value;
        }

        private void btnMaxVolume_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Volume = 1;
            slVolume.Value = mePlayer.Volume;
        }


        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Volume = 0;
            slVolume.Value = mePlayer.Volume;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
    }
}
