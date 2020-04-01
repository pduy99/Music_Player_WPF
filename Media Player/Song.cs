using Id3;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Media_Player
{
    public class Song: INotifyPropertyChanged
    {
        private string path = null;
        private string name = "Unknown";
        private string singer = "Unknown";
        private TimeSpan duration;
        private string album;
        private BitmapImage thumbnail;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }
        public BitmapImage Thumbnail
        {
            get { return thumbnail; }
            set
            {
                thumbnail = value;
                RaisePropertyChanged();
            }
        }
        public string Singer
        {
            get { return singer; }
            set
            {   
                singer = value;
                RaisePropertyChanged();
            }
        }
        public TimeSpan Duration
        {
            get { return duration; }
            set
            {
                duration = value;
                RaisePropertyChanged();
            }
        }
        public string Album
        {
            get { return album; }
            set
            {
                album = value;
                RaisePropertyChanged();
            }
        }
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                RaisePropertyChanged();
            }
        }

        public Song(string path)
        {
            this.path = path;
            try
            {
                var mp3 = new Mp3(path);
                Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);
                this.Name = tag.Title;
                this.Album = tag.Album;
                this.Singer = tag.Artists;
                this.Thumbnail = LoadImage(tag.Pictures[0].PictureData);
            }
            catch (Exception)
            {
                if(this.Name == "Unknown")
                {
                    GetNameFromPath();
                }
            }
        }

        public Song()
        {
            this.name = "";
            this.singer = "";
            this.duration = TimeSpan.FromSeconds(0);
            this.album = "";
        }

        public Song(string name, string singer, string path)
        {
            this.name = name;
            this.singer = singer;
            this.path = path;
        }

        public void Update(Song song)
        {
            this.Name = song.Name;
            this.Singer = song.Singer;
            this.Album = song.Album;
            this.Duration = song.Duration;
            this.Thumbnail = song.Thumbnail;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void GetNameFromPath()
        {
            var tokens = this.path.Split(new string[] { "\\" },
                StringSplitOptions.None);
            if(tokens.Length > 1)
            {
                this.Name = tokens[tokens.Length - 1];
            }
            else
            {
                tokens = this.path.Split(new string[] { "/" }, StringSplitOptions.None);
                this.Name = tokens[tokens.Length - 1];
            }
        }
    }
}

