using Gma.System.MouseKeyHook;
using Media_Player.UserControls;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum PlayMode
        {
            NOT_REPLAY,
            REPLAY_ONE_TIME,
            REPLAY_FOREVER,
        }
        PlayMode playmode = PlayMode.NOT_REPLAY;

        string PlaylistsPath = AppDomain.CurrentDomain.BaseDirectory + "Playlists" ;
        string DataPath = AppDomain.CurrentDomain.BaseDirectory + "Data";

        MediaPlayer media = new MediaPlayer();
        /// <summary>
        /// Đối tượng bài hát để UI binding dữ liệu
        /// </summary>
        Song SuperSong = new Song();

        /// <summary>
        /// Bài hát đang chơi
        /// </summary>
        Song currentSong = new Song();

        /// <summary>
        /// Bài hát cuối cùng chơi khi chương trình tắt
        /// </summary>
        Song lastplaysong = new Song();

        /// <summary>
        /// Playlist được chơi lần cuối khi chương trình tắt 
        /// </summary>
        Playlist recentplays = new Playlist();

        /// <summary>
        /// Hàng đợi bài hát sắp chơi
        /// </summary>
        BindingList<Song> playlist = new BindingList<Song>();

        /// <summary>
        /// Danh sách các playlist do người dùng tạo
        /// </summary>
        BindingList<Playlist> customplaylist = new BindingList<Playlist>();

        /// <summary>
        /// Chứa danh sách các bài hát crawl từ zingmp3
        /// </summary>
        Playlist ZingMp3Playlist = new Playlist();

        Playlist selectedPlaylist = new Playlist();

        /// <summary>
        /// Biến đếm số lần đã chơi của playlist
        /// </summary>
        int playcount = 1;

        DispatcherTimer _timer;
        bool isPlaying = false;
        bool isPlayingRandomly = false;
        
        /// <summary>
        /// Hook bàn phím
        /// </summary>
        private IKeyboardMouseEvents _hook;

        /// <summary>
        /// Các định dạng file người dùng được chọn
        /// </summary>
        const string audioExtension = "All Media Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;" +
            "*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;" +
            "*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;" +
            "*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV";

        public MainWindow()
        {
            InitializeComponent();
            //UI
            tbSongName.DataContext = SuperSong;
            tbArtist.DataContext = SuperSong;

            //Timer
            media.MediaEnded += Media_MediaEnded;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += _timer_Tick;

            //Slider
            TimeSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(TimeSlider_MouseLeftButtonUp), true);
            TimeSlider.ValueChanged += TimeSlider_ValueChanged;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            VolumeSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(VolumnSlider_MouseLeftButtonUp), true);

            //Playlist
            lvNowPlaying.ItemsSource = playlist;
            lvCustomPlaylist.ItemsSource = customplaylist;

            //Tooltip
            btnRepeatMode.DataContext = playmode;
            btnPlayRandomMode.DataContext = isPlayingRandomly;

            //Hook
            _hook = Hook.GlobalEvents();
            _hook.KeyUp += _hook_KeyUp;

            
        }

        /// <summary>
        /// Sự kiện hook bàn phím
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _hook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if(e.Control && e.Alt && (e.KeyCode == Keys.N))
            {
                //Ctrl + Alt + N --> Next song
                BtnSkipNext_Click(this, new RoutedEventArgs());
            }

            if(e.Control && e.Alt && (e.KeyCode == Keys.P))
            {
                //Ctrl + Alt + P --> Previous song
                BtnSkipPrevious_Click(this, new RoutedEventArgs());
            }

            if(e.Control && e.Alt && (e.KeyCode == Keys.D))
            {
                //Ctrl + Alt + D --> Pause/Resume
                BtnPlayPause_Click(this, new RoutedEventArgs());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(PlaylistsPath);
            Directory.CreateDirectory(DataPath);
            LoadPlaylist();
            LoadRecentPlays();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _hook.KeyUp -= _hook_KeyUp;
            _hook.Dispose();
            SaveReccentPlays();

            Environment.Exit(0);
        }

        /*
         * Các hàm xử lý âm thanh
         */
        private void VolumnSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var volumn = sender as Slider;
            media.Volume = volumn.Value;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var volumn = sender as Slider;
            media.Volume = volumn.Value;
        }

        /// <summary>
        /// Mute/Unmute volumn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (media.IsMuted == true)
            {
                iconVolumn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Audio;
                VolumeSlider.Value = 0.5;
            }
            else
            {
                iconVolumn.Kind = MaterialDesignThemes.Wpf.PackIconKind.Mute;
                VolumeSlider.Value = 0;
            }
            media.IsMuted = !media.IsMuted;

        }

        /*
         * Các hàm xử lý slider timeline của bài hát 
        */
        private void TimeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (media.Source != null)
            {
                if (currentSong.Duration.TotalSeconds > 0)
                {
                    var totalTime = currentSong.Duration.TotalSeconds;
                    media.Position = TimeSpan.FromSeconds(TimeSlider.Value * totalTime);
                }
            }
        }

        private void TimeSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (media.Source != null)
            {
                if (currentSong.Duration.TotalSeconds > 0)
                {
                    var totalTime = currentSong.Duration.TotalSeconds;
                    media.Position = TimeSpan.FromSeconds(TimeSlider.Value * totalTime);
                }
            }
            
        }

        /// <summary>
        /// Event Handler for updating the current position of media player to UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Tick(object sender, EventArgs e)
        {
            if (media.Source != null)
            {
                if (currentSong.Duration.TotalSeconds > 0)
                {
                    var currenpost = media.Position;
                    var totalTime = currentSong.Duration.TotalSeconds;

                    tbCurrentPos.Text = currenpost.ToString(@"mm\:ss");
                    TimeSlider.Value = currenpost.TotalSeconds / totalTime;
                }
            }
        }

        /// <summary>
        /// Sự kiện khi bài hát kết thúc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_MediaEnded(object sender, EventArgs e)
        {
            int indexCurrentSong = playlist.IndexOf(currentSong);
            isPlaying = false;
            
            
            if(indexCurrentSong < (playlist.Count() -1)) //Nếu bài hát vừa kết thúc không phải bài cuối cùng trong playlist
            {
                if (isPlayingRandomly == false)
                {
                    int index = indexCurrentSong + 1;
                    lvNowPlaying.SelectedIndex = index;
                    PlaySelectedSong(playlist[index]);
                   
                }
                else
                {
                    Random rnd = new Random();
                    int count = playlist.Count();
                    int index = rnd.Next(count);
                    lvNowPlaying.SelectedIndex = index;
                    PlaySelectedSong(playlist[index]);
                    
                }

            }
            //Bài hát là bài cuối trong playlist
            else 
            {
                int index;
                if (playmode == PlayMode.REPLAY_FOREVER)
                {
                    if(isPlayingRandomly == false)
                    {
                        index = 0;
                        lvNowPlaying.SelectedIndex = index;
                        PlaySelectedSong(playlist[index]);
                        
                    }
                    else
                    {
                        Random rnd = new Random();
                        int count = playlist.Count();
                        index = rnd.Next(count);
                        lvNowPlaying.SelectedIndex = index;
                        PlaySelectedSong(playlist[index]);
                        
                    }
                }
                if(playmode == PlayMode.REPLAY_ONE_TIME)
                {
                    if(playcount < 2)
                    {
                        if (isPlayingRandomly == false)
                        {
                            index = 0;
                            lvNowPlaying.SelectedIndex = index;
                            PlaySelectedSong(playlist[index]);
                            
                        }
                        else
                        {
                            Random rnd = new Random();
                            int count = playlist.Count();
                            index = rnd.Next(count);
                            lvNowPlaying.SelectedIndex = index;
                            PlaySelectedSong(playlist[index]);
                            
                        }
                        playcount++;
                    }
                    else
                    {
                        isPlaying = true;
                        BtnPlayPause_Click(this, new RoutedEventArgs());
                    }
                }
                if(playmode == PlayMode.NOT_REPLAY)
                {
                    isPlaying = true;
                    BtnPlayPause_Click(this, new RoutedEventArgs());
                }
            }
        }

        /// <summary>
        /// Sự kiện bài hát được load hoàn tất
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentSong_MediaOpened(object sender, EventArgs e)
        {
            currentSong.Duration = media.NaturalDuration.TimeSpan;
            tbDuration.Text = currentSong.Duration.ToString(@"mm\:ss");


            BtnPlayPause_Click(this, new RoutedEventArgs());
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            //Chưa hỗ trợ chức năng này
        }

        /// <summary>
        /// Tạo một custom playlist mới
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddCustomPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var screen = new AddPlaylistWindow();
            if(screen.ShowDialog() == true)
            {
                string playlistName = AddPlaylistWindow.PLAYLISTNAME;
                if(playlistName != "")
                {
                    Playlist playlist = new Playlist(playlistName);
                    customplaylist.Add(playlist);
                }   
            }
        }

        /// <summary>
        /// Đóng Tab Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            btnCloseMenu.Visibility = Visibility.Collapsed;
            btnOpenMenu.Visibility = Visibility.Visible;
            MenuBody.Visibility = Visibility.Collapsed;
            CustomPlaylistTitle.Visibility = Visibility.Collapsed;
            lvCustomPlaylist.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Mở Tab Menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            btnOpenMenu.Visibility = Visibility.Collapsed;
            btnCloseMenu.Visibility = Visibility.Visible;
            MenuBody.Visibility = Visibility.Visible;
            CustomPlaylistTitle.Visibility = Visibility.Visible;
            lvCustomPlaylist.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Chơi bài hát trước
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSkipPrevious_Click(object sender, RoutedEventArgs e)
        {
            
            int indexCurrentSong = playlist.IndexOf(currentSong);
            if (indexCurrentSong > 0)
            {
                int index = indexCurrentSong - 1;
                lvNowPlaying.SelectedIndex = index;
                isPlaying = false;
                PlaySelectedSong(playlist[index]);
            }
            
        }

        /// <summary>
        /// Chơi/ngưng chơi bài hát dựa vào biến isPlaying
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (media.Source != null)
            {
                if (isPlaying == true) //Pause
                {
                    iconPlayPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlayCircleFilled;
                    PauseRotation();
                    _timer.Stop();
                    media.Pause();

                    isPlaying = false;

                }
                else //Play
                {
                    iconPlayPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.PauseCircleFilled;
                    BeginOrResumeRotation();
                    _timer.Start();
                    media.Play();
                    isPlaying = true;

                }
            }
        }

        /// <summary>
        /// Kết thúc chơi một bài hát
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {

            if (media.Source != null)
            {
                
                media.Stop();
                Media_MediaEnded(this, new EventArgs());
                StopRoation();
                iconPlayPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlayCircleFilled;
            }

        }

        /// <summary>
        /// Chơi bài hát kế tiếp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSkipNext_Click(object sender, RoutedEventArgs e)
        {
            Media_MediaEnded(this, new EventArgs());
        }
 
        /// <summary>
        /// Thay đổi chế độ chơi lặp lại
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRepeatMode_Click(object sender, RoutedEventArgs e)
        {
            //Change in order: NOT_REPLAY --> REPLAY_ONE_TIME --> REPLAY_FORVER

            if (playmode == PlayMode.NOT_REPLAY)
            {
                playmode = PlayMode.REPLAY_ONE_TIME;
                iconRepeat.Kind = MaterialDesignThemes.Wpf.PackIconKind.RepeatOne;
                iconRepeat.Foreground = Brushes.LightGreen;
            }
            else if(playmode == PlayMode.REPLAY_ONE_TIME)
            {
                playmode = PlayMode.REPLAY_FOREVER;
                iconRepeat.Kind = MaterialDesignThemes.Wpf.PackIconKind.Repeat;
                iconRepeat.Foreground = Brushes.LightGreen;
            }
            else
            {
                playmode = PlayMode.NOT_REPLAY;
                iconRepeat.Kind = MaterialDesignThemes.Wpf.PackIconKind.RepeatOff;
                iconRepeat.Foreground = Brushes.White;
            }
            btnRepeatMode.DataContext = playmode;
        }

        /// <summary>
        /// Thay đổi chế độ chơi ngẫu nhiên hoặc tuần tự
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlayRandomMode_Click(object sender, RoutedEventArgs e)
        {
           
            if (isPlayingRandomly == false)
            {
                iconShuffel.Foreground = Brushes.LightGreen;
            }
            else
            {
                iconShuffel.Foreground = Brushes.White;
            }
            isPlayingRandomly = !isPlayingRandomly;
            btnPlayRandomMode.DataContext = isPlayingRandomly;
        }

        /// <summary>
        /// Mở chọn file chơi mà không tại playlist mới
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFile_Click(object sender, MouseButtonEventArgs e)
        {
            var screen = new Microsoft.Win32.OpenFileDialog
            {
                Filter = audioExtension,
                Multiselect = true
            };
            if (screen.ShowDialog() == true)
            {
                foreach(var filename in screen.FileNames)
                {
                    try
                    {
                        isPlaying = false;
                        Song song = new Song(filename);
                        AddToPlaylist(song);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Cannot play that file - {ex} ");
                        continue;
                    }
                }
                if (playlist.Count() > 0)
                {
                    media.Open(new Uri(playlist[0].Path, UriKind.Absolute));
                    currentSong = playlist[0];
                    SuperSong.Update(currentSong);
                    UpdateThumbnail();
                    media.MediaOpened += CurrentSong_MediaOpened;
                }
            }
        }

        /// <summary>
        /// Chơi một bài hát 
        /// </summary>
        /// <param name="song"></param>
        private void PlaySelectedSong(Song song)
        {
            try
            {
                media.Open(new Uri(song.Path, UriKind.Absolute));
                currentSong = song;
                media.MediaOpened += CurrentSong_MediaOpened;
                SuperSong.Update(currentSong);
                UpdateThumbnail();
            }
            catch (Exception e)
            {
                Debug.Write("Loi phat bai hat: " + e.Message);
            }
        }

        /*
         Các hàm xử lý animation đĩa xoay 
        */

        /// <summary>
        /// Xoay hoặc ngừng xoay đĩa 
        /// </summary>
        private void BeginOrResumeRotation()
        {
            if(DiscImageRotate.Angle == 0)
            {
                ((Storyboard)Resources["RotateImage"]).Begin();
            }
            else
            {
                ((Storyboard)Resources["RotateImage"]).Resume();
            }
            
        }

        /// <summary>
        /// Tạm ngưng xoay đĩa
        /// </summary>
        private void PauseRotation()
        {
            ((Storyboard)Resources["RotateImage"]).Pause();
        }

        /// <summary>
        /// Kết thúc xoay đĩa và đặt lại angle của đĩa = 0
        /// </summary>
        private void StopRoation()
        {
            ((Storyboard)Resources["RotateImage"]).Stop();
        }

        

        /// <summary>
        /// Cập nhập hình ảnh của đĩa xoay theo thumbnail của bài đang phát
        /// </summary>
        private void UpdateThumbnail()
        {
            //WPF sucks that ImageBrush can't use binding data
            if (currentSong.Thumbnail != null)
            {
                ThumbnailImage.ImageSource = currentSong.Thumbnail;
            }
            else
            {
                ThumbnailImage.ImageSource = FindResource("Disc") as ImageSource;
            }
        }

        /// <summary>
        /// Mở Tab Now Playing và hiện control xoay đĩa 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NowPlaying_Click(object sender, MouseButtonEventArgs e)
        {
            MenuTab.Visibility = Visibility.Collapsed;
            lvCustomPlaylist.UnselectAll();
            ((Storyboard)Resources["MenuClose"]).Begin();
            ((Storyboard)Resources["PlaylistTabOpen"]).Begin();

            if (!MainGrid.Children.Contains(NowPlayingGrid))
            {
                MainGrid.Children.Clear();
                MainGrid.Children.Add(NowPlayingGrid);
            }

        }

        /// <summary>
        /// Đóng Tab Now playing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClosePlaylistTab_Click(object sender, RoutedEventArgs e)
        {
            ((Storyboard)Resources["PlaylistTabClose"]).Begin();
            ((Storyboard)Resources["MenuOpen"]).Begin();
            MenuTab.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Lựa chọn bài hát trong tab Now playing để chơi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedSongFromPlaylist(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ListView listview = sender as System.Windows.Controls.ListView;
            int index = listview.SelectedIndex;
            isPlaying = false;
            PlaySelectedSong(playlist[index]);
            MoveCursorPlaylist(index);

        }

        /// <summary>
        /// Cập nhật lại vị trí của con trỏ chỉ bài hát đang chơi
        /// </summary>
        /// <param name="index"></param>
        private void MoveCursorPlaylist(int index)
        {
            TrainsitionigContentSlide.OnApplyTemplate();
            GridCursor.Margin = new Thickness(0, 0 + (35 * index), 0, 0);
        }

        /// <summary>
        /// Xóa một custom playlist được chọn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelCustomPlaylist_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button cmd = (System.Windows.Controls.Button)sender;
            if (cmd.DataContext is Playlist playlist)
            {
                customplaylist.Remove(playlist);
                DeletePlaylist(playlist);
            }

        }

        /// <summary>
        /// Mở control custom playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LvCustomPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainGrid.Children.Clear();

            int index = lvCustomPlaylist.SelectedIndex;
            if(index >= 0)
            {
                Playlist p = customplaylist[index];
                selectedPlaylist = p;
                var CustomPlaylistController = new CustomPlaylistControl(ref p);
                MainGrid.Children.Add(CustomPlaylistController);
                CustomPlaylistController.PlayAllClick += CustomPlaylistController_PlayAllClick;
                CustomPlaylistController.SavePlaylistClick += CustomPlaylistController_SavePlaylistClick;
            }
        }

        /// <summary>
        /// Sự kiện nút lưu custom playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomPlaylistController_SavePlaylistClick(object sender, RoutedEventArgs e)
        {
            var p = customplaylist[lvCustomPlaylist.SelectedIndex];
            SavePlaylist(p);
        }

        /// <summary>
        /// Phát tất cả các bài hát trong custom playlist được chọn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomPlaylistController_PlayAllClick(object sender, RoutedEventArgs e)
        {
            playlist = selectedPlaylist.Songs;
            recentplays = selectedPlaylist;
            lvNowPlaying.ItemsSource = playlist;
            PlaySelectedSong(playlist[0]);
            playcount = 1;
        }

        /// <summary>
        /// Lưu một custom playlist xuống file
        /// </summary>
        /// <param name="p"></param>
        private void SavePlaylist(Playlist p)
        {
            string filename = PlaylistsPath +"//" + p.Name + ".playlist";
            var writer = new StreamWriter(filename);
            foreach(var song in p.Songs)
            {
                writer.WriteLine(song.Path);
            }
            writer.Close();
            System.Windows.MessageBox.Show("Saved");
        }

        /// <summary>
        /// Nạp lại tất cả các playlist đã lưu
        /// </summary>
        private void LoadPlaylist()
        {
            string[] playlists = Directory.GetFiles(PlaylistsPath, "*.playlist");
            foreach(var item in playlists)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(item);
                Playlist p = new Playlist(name);

                string[] songs = File.ReadAllLines(item);
                foreach(var s in songs)
                {
                    Song song = new Song(s);
                    p.Songs.Add(song);
                }

                customplaylist.Add(p);
            }
        }

        /// <summary>
        /// Lưu lại playlist và bài hát chơi lần cuối xuống file
        /// </summary>
        private void SaveReccentPlays()
        {
            if (recentplays != null && currentSong.Path != null)
            {
                string filename = DataPath + "\\" + "data.dat";
                var writer = new StreamWriter(filename);
                writer.WriteLine(recentplays.Name);
                writer.Write(currentSong.Name);
                writer.Close();
            }

        }

        /// <summary>
        /// Nạp lại playlist và bài hát chơi lần cuối từ file
        /// </summary>
        private void LoadRecentPlays()
        {
            var filename = DataPath + "//" + "data.dat";
            
            try
            {
                string[] lines = File.ReadAllLines(filename);
                if (lines.Length == 2)
                {
                    foreach (var item in customplaylist) //Already load saved custom playlist
                    {
                        if (item.Name == lines[0])
                        {
                            recentplays = item;
                            foreach (var song in item.Songs)
                            {
                                if (song.Name == lines[1])
                                {
                                    lastplaysong = song;
                                }
                            }
                            return;
                        }
                    }
                }
                recentplays = null;
                lastplaysong = null;
            }
            catch
            {
                File.Create(filename);
            }
            

            
        }

        /// <summary>
        /// Xóa một custom playlist khỏi file
        /// </summary>
        /// <param name="p"></param>
        private void DeletePlaylist(Playlist p)
        {
            string[] playlists = Directory.GetFiles(PlaylistsPath, "*.playlist");
            foreach(var item in playlists)
            {
                if(System.IO.Path.GetFileNameWithoutExtension(item) == p.Name)
                {
                    File.Delete(item);
                }
            }
        }

        /// <summary>
        /// Mở control Recent plays
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecentPlays_Click(object sender, MouseButtonEventArgs e)
        {
            lvCustomPlaylist.UnselectAll();
            MainGrid.Children.Clear();
            var RecentPlaysController = new RecentPlaysControl(recentplays,lastplaysong);
            MainGrid.Children.Add(RecentPlaysController);

            RecentPlaysController.ContinuePlaying_Click += RecentPlaysController_ContinuePlaying_Click;
        }

        /// <summary>
        /// Chơi lại playlist đã chơi lần cuối
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecentPlaysController_ContinuePlaying_Click(object sender, RoutedEventArgs e)
        {
            playlist = recentplays.Songs;
            lvNowPlaying.ItemsSource = playlist;
            PlaySelectedSong(lastplaysong);
            for(int i = 0; i < playlist.Count; i++)
            {
                if(playlist[i].Name == lastplaysong.Name)
                {
                    MoveCursorPlaylist(i);
                }
            }
            playcount = 1;

        }

        /// <summary>
        /// Thêm một bài hát vào playlist
        /// </summary>
        /// <param name="song"></param>
        private void AddToPlaylist(Song song)
        {
            if (!playlist.Contains(song))
            {
                playlist.Add(song);
            }
        }

        private void CrawlZingMp3(object sender, RoutedEventArgs e)
        {
            CrawlZingMp3 crawler = new CrawlZingMp3();
            if(ZingMp3Playlist.Songs == null)
            {
                Thread thread = new Thread(() =>
                {
                    ZingMp3Playlist = crawler.createAlbum();
                    
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        MainGrid.Children.Clear();
                        var CustomPlaylistController = new CustomPlaylistControl(ref ZingMp3Playlist);
                        selectedPlaylist = ZingMp3Playlist;
                        MainGrid.Children.Add(CustomPlaylistController);
                        CustomPlaylistController.PlayAllClick += CustomPlaylistController_PlayAllClick;
                        CustomPlaylistController.SavePlaylistClick += CustomPlaylistController_SavePlaylistClick;
                        BusyBar.IsBusy = false;
                        
                    }));  
                });
                thread.IsBackground = true;
                thread.Start();
                BusyBar.IsBusy = true;
            }
            else
            {
                MainGrid.Children.Clear();
                var CustomPlaylistController = new CustomPlaylistControl(ref ZingMp3Playlist);
                selectedPlaylist = ZingMp3Playlist;
                MainGrid.Children.Add(CustomPlaylistController);
                CustomPlaylistController.PlayAllClick += CustomPlaylistController_PlayAllClick;
                CustomPlaylistController.SavePlaylistClick += CustomPlaylistController_SavePlaylistClick;
                BusyBar.IsBusy = false;
                
            }
        }
    }
}
