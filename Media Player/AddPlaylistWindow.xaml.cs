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
using System.Windows.Shapes;

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for AddPlaylistWindow.xaml
    /// </summary>
    public partial class AddPlaylistWindow : Window
    {
        public static string PLAYLISTNAME;
        public AddPlaylistWindow()
        {
            InitializeComponent();
        }

        private void BtnAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            PLAYLISTNAME = tbPlaylist.Text;
            DialogResult = true;
            Close();
        }
    }
}
