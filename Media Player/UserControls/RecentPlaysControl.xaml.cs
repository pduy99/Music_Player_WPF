using System;
using System.Collections.Generic;
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

namespace Media_Player.UserControls
{
    /// <summary>
    /// Interaction logic for RecentPlaysControl.xaml
    /// </summary>
    public partial class RecentPlaysControl : UserControl
    {
        Playlist playlist = new Playlist();
        Song song = new Song();

        public event RoutedEventHandler ContinuePlaying_Click;

        public RecentPlaysControl(Playlist recentplays, Song currentsong)
        {
            InitializeComponent();
            if (recentplays == null)
            {
                PlaylistInfo.Visibility = Visibility.Collapsed;
            }
            else
            {
                playlist = recentplays;
                song = currentsong;

                lvListSong.ItemsSource = playlist.Songs;
                tbPlaylistName.Text = playlist.Name;
                tbNumberSong.DataContext = playlist.Count + " songs";
                
                for(int i = 0; i < playlist.Songs.Count; i++)
                {
                    if(currentsong.Name == playlist.Songs[i].Name)
                    {
                        lvListSong.SelectedIndex = i;
                        break;
                    }
                }

            }
        }

        private void ContinuePlay(object sender, RoutedEventArgs e)
        {
            ContinuePlaying_Click?.Invoke(this, new RoutedEventArgs());
        }

       

        
    }
}
