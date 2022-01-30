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
using NAudio.Wave;


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

            timer2.Interval = TimeSpan.FromMilliseconds(0.1);
            timer2.Tick += Timer2_Tick;
            timer2.IsEnabled = false;
        }
        //Tab PCM va FFT
        DispatcherTimer timer2 = new DispatcherTimer();
        private int RATE = 44100; // sample rate
        byte[] frames;
        int frameSize = 0;
        long tmpBuffer = 0;
        long bufferSize = (long)Math.Pow(2, 13);

        public void ReadFileAudio(string url)
        {
            tmpBuffer = 0;
            plotPCM.Plot.Clear();
            plotFFT.Plot.Clear();
            AudioFileReader afr = new AudioFileReader(url);
            // read the bytes from the stream
            frameSize = (int)afr.Length;
            frames = new byte[frameSize];
            afr.Read(frames, 0, frameSize);
            timer2.IsEnabled = true;
        }

        public double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length];
            System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                fftComplex[i] = new System.Numerics.Complex(data[i], 0.0);
            }
            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);

            for (int i = 0; i < data.Length; i++)
            {
                fft[i] = fftComplex[i].Magnitude;
            }

            return fft;
        }

        public void UpdateAudioGraph()
        {

            int SAMPLE_RESOLUTION = 16;

            int BYTE_PER_POINT = SAMPLE_RESOLUTION / 8;
            Int32[] vals = new Int32[bufferSize / BYTE_PER_POINT];
            double[] Ys = new double[bufferSize / BYTE_PER_POINT];
            double[] Xs = new double[bufferSize / BYTE_PER_POINT];
            double[] Ys2 = new double[bufferSize / BYTE_PER_POINT];
            double[] Xs2 = new double[bufferSize / BYTE_PER_POINT];
            for (int i = 0; i < vals.Length; i++)
            {
                //bit shift the byte buffer into the right variable format
                byte hByte = frames[i * 2 + 1 + tmpBuffer];
                byte lByte = frames[i * 2 + 0 + tmpBuffer];
                //Console.Out.WriteLine("i: {2};  Hbyte:{0};    Lbyte:{1} ", hByte, lByte, i);
                vals[i] = (int)(short)((hByte << 8) | lByte);
                Xs[i] = i;
                Ys[i] = vals[i];
                Xs2[i] = (double)i / Ys.Length * RATE / 1000.0; // units are in kHz
            }
            //plot PCM
            plotPCM.Plot.SetAxisLimits(0, 2000, -60000, 60000);
            plotPCM.Plot.AddScatterLines(Xs, Ys, color: System.Drawing.Color.Purple);
            plotPCM.Refresh();
            Ys2 = FFT(Ys);
            //plot FFT
            plotFFT.Plot.SetAxisLimits(-2, 25, -50, 3000);
            plotFFT.Plot.AddScatterLines(Xs2.Take(Xs2.Length / 2).ToArray(), Ys2.Take(Ys2.Length / 2).ToArray(), color: System.Drawing.Color.Purple);
            plotFFT.Refresh();
        }
        //--------------------------------------------------------------------------------------------------------------------------------

        //Tab Explore
        public class Songs
        {
            public string Song { get; set; }
            public string Genre { get; set; }
            public string Author { get; set; }
            public string Duration { get; set; }

        }
        //Tao list luu file va duong dan
        //Tao list luu paths, files;
        List<String> paths = new List<String>();
        List<String> files = new List<String>();
        List<String> duration = new List<String>();
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
                for (int i = 0; i < files.Count(); i++)
                {
                    AudioFileReader wfr = new AudioFileReader(paths[i]);
                    TimeSpan ts = wfr.TotalTime;
                    if(ts.Hours != 0)
                    {
                        duration.Add(ts.Hours.ToString() + ":" + ts.Minutes.ToString() + ":" + ts.Seconds.ToString());
                    } 
                    else
                    {
                        duration.Add(ts.Minutes.ToString() + ":" + ts.Seconds.ToString());
                    }
                }
                    
                for (int i = tmp; i < files.Count(); i++)
                {
                    tmp++;
                    songs.Add(new Songs()
                    {
                        Song = files[i],
                        Genre = "Unknow",
                        Author = "Sơn tùng MTP",
                        Duration = duration[i]
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
            ReadFileAudio(paths[dgList.SelectedIndex]);
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

        private void btnRawDataPCM_Click(object sender, RoutedEventArgs e)
        {
            Thickness margin = indicator.Margin;
            margin.Top = btnRawDataPCM.Margin.Top + 17;
            indicator.Margin = margin;
            tbControl.SelectedIndex = 2;
        }
        private void btnFFT_Click(object sender, RoutedEventArgs e)
        {
            Thickness margin = indicator.Margin;
            margin.Top = btnFFT.Margin.Top + 17;
            indicator.Margin = margin;
            tbControl.SelectedIndex = 3;
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
            ReadFileAudio(paths[dgList.SelectedIndex]);
            mePlayer.Play();
            btnPlay.Visibility = Visibility.Hidden;
            btnPause.Visibility = Visibility.Visible;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            timer2.IsEnabled = true;
            mePlayer.Play();
            btnPlay.Visibility = Visibility.Hidden;
            btnPause.Visibility = Visibility.Visible; 
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            timer2.IsEnabled = false;
            mePlayer.Pause();
            btnPlay.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Hidden;
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            mePlayer.Stop();
            btnPlay.Visibility = Visibility.Visible;
            btnPause.Visibility = Visibility.Hidden;
            ReadFileAudio(paths[dgList.SelectedIndex]);
            UpdateAudioGraph();
            timer2.IsEnabled = false;
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
            ReadFileAudio(paths[dgList.SelectedIndex]);
            mePlayer.Play();
            btnPlay.Visibility = Visibility.Hidden;
            btnPause.Visibility = Visibility.Visible;
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

        private void Timer2_Tick(object sender, EventArgs e)
        {
            if (tmp + bufferSize <= frameSize)
            {
                UpdateAudioGraph();
                plotPCM.Plot.Clear();
                plotFFT.Plot.Clear();
                tmpBuffer += bufferSize;
            }
            else
            {
                plotPCM.Plot.Clear();
                plotFFT.Plot.Clear();
                plotPCM.Refresh();
                plotFFT.Refresh();
                timer2.IsEnabled = false;
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
