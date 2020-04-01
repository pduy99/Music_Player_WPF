using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Media_Player
{
    public class Playlist:INotifyPropertyChanged, INotifyCollectionChanged
    {
        private string name;
        private BindingList<Song> songs;
        private int count;
        
        
        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public Playlist(string name)
        {
            this.Name = name;
            Songs = new BindingList<Song>();
            Count = Songs.Count;
        }

        public Playlist(string name, BindingList<Song> songs)
        {
            this.Name = name;
            this.Songs = songs;
            this.Count = Songs.Count;
              
        }

        public Playlist() { }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        public void AddSongs(Song song)
        {
            Songs.Add(song);
            this.Count = Songs.Count;
            RaiseColectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, song));
           
        }

        public void DeleteSong(Song song)
        {
            Songs.Remove(song);
            this.Count = Songs.Count;
            RaiseColectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, song));
            
        }

        public BindingList<Song> Songs
        {
            get { return songs; }
            set
            {
                songs = value;
                RaisePropertyChanged();
            }
        }

        public int Count
        {
            get { return count; }
            set
            {
                count = value;
                RaisePropertyChanged();
            }
        }

        private void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaiseColectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged?.Invoke(this, e);
        }
    }
}
