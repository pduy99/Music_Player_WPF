using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Media_Player.UserControls
{
    /// <summary>
    /// Interaction logic for CustomPlaylistControl.xaml
    /// </summary>
    public partial class CustomPlaylistControl : UserControl
    {
        const string audioExtension = "All Media Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;" +
            "*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;" +
            "*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;" +
            "*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV";

        public event RoutedEventHandler PlayAllClick;
        public event RoutedEventHandler SavePlaylistClick;


        Playlist playlist = new Playlist();
        BindingList<Song> listsong = new BindingList<Song>();

        public CustomPlaylistControl()
        {
            InitializeComponent();
        }

        public CustomPlaylistControl(ref Playlist customplaylist)
        {
            InitializeComponent();
            playlist = customplaylist;
            lvListSong.ItemsSource = playlist.Songs;
            tbPlaylistName.Text = playlist.Name;
            tbNumberSong.Text = playlist.Count + " songs";
            
        }

        private void AddSong(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.Filter = audioExtension;
            screen.Multiselect = true;
            if (screen.ShowDialog() == true)
            {
                foreach (var filename in screen.FileNames)
                {
                    try
                    {
                        MediaPlayer media = new MediaPlayer();
                        media.Open(new Uri(filename, UriKind.Absolute));
                        media.MediaOpened += Media_MediaOpened;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Cannot play that file - {ex} ");
                        continue;
                    }
                }
            }
        }

        private void Media_MediaOpened(object sender, EventArgs e)
        {
            MediaPlayer media = sender as MediaPlayer;
            Song song = new Song(media.Source.AbsolutePath);
            song.Duration = media.NaturalDuration.TimeSpan;
            playlist.AddSongs(song);
        }

        private void PlayAll(object sender, RoutedEventArgs e)
        {
            if(playlist.Songs.Count > 0)
            {
                PlayAllClick?.Invoke(this, new RoutedEventArgs());
            }
        }

        private void CheckboxSong_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if(lvListSong.SelectedItems.Count > 0)
            {
                FunctionPanel.Visibility = Visibility.Visible;
                lvListSong.Height = 320;
            }
            else
            {
                FunctionPanel.Visibility = Visibility.Collapsed;
                lvListSong.Height = 380;
            }
            
        }

        private void DelFromPlaylist_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < lvListSong.SelectedItems.Count; i++)
            {
                Song song = (Song)lvListSong.SelectedItems[i];
                playlist.DeleteSong(song);
                i--;
            }

        }

        private void SelectAllSong_Click(object sender, RoutedEventArgs e)
        {
            lvListSong.SelectedItems.Clear();
            foreach(var item in lvListSong.Items)
            {
                lvListSong.SelectedItems.Add(item);
            }
        }

        private void SavePlaylist(object sender, RoutedEventArgs e)
        {
            SavePlaylistClick?.Invoke(this, new RoutedEventArgs());
        }
    }
}
