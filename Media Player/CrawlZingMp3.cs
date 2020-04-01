using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using xNet;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Media_Player
{
    public class CrawlZingMp3: MainWindow
    {
        const string songURL = "https://zing-mp3.glitch.me/?url=mp3.zing.vn";
        private Playlist zingChartAlbum = new Playlist("Top-chart ZingMp3");

        private string getURL(string id)
        {
            try
            {
                string link = songURL + id;
                HttpRequest request = new HttpRequest();
                string stringResponse = request.Get(link).ToString();
                JObject response = JObject.Parse(stringResponse);

                string URL = response.SelectToken("source.audio.128.view").ToString();
                return URL;
            }
            catch(Exception e)
            {
                Debug.Write("Loi get URL bai hat:" + e.Message);
                return null;
            }
            
        }

        public Playlist createAlbum()
        {
            Crawl();
            return zingChartAlbum;
        }

        private void Crawl()
        {
            
            HttpRequest request = new HttpRequest();
            string bodyHTML = request.Get(@"mp3.zing.vn/zing-chart-tuan/bai-hat-Viet-Nam/IWZ9Z08I.html").ToString();
            string chartPattern = @"<ul class=""tracking-page-session""(.*?)</ul>";
            var HtmlChart = Regex.Matches(bodyHTML, chartPattern, RegexOptions.Singleline);
            var HtmlListSong = Regex.Matches(HtmlChart[0].ToString(), @"<li (.*?)</li>", RegexOptions.Singleline);

            for (int i = 0; i < 20; i++)
            {   
                try
                {
                    
                    var SongNameAndSinger = Regex.Matches(HtmlListSong[i].ToString(), @"<a\s\S(.*?)</a>", RegexOptions.Singleline);
                    string songName = getSongName(SongNameAndSinger[1].ToString());
                    string singer = getSinger(SongNameAndSinger[2].ToString());

                    string songID = Regex.Match(HtmlListSong[i].ToString().ToString(), @"/bai-hat(.*?)""", RegexOptions.Singleline).ToString();
                    string URL = getURL(songID);

                    Song song = new Song(songName, singer, URL);
                    //Debug.WriteLine(song.Name);
                    zingChartAlbum.AddSongs(song);

                }
                catch (Exception e)
                {
                    Debug.WriteLine("Loi lay danh sach bang xep hang MP3" + e.Message);
                }
            }
        }

        private string getSongName(string s)
        {
            string songName;
            int from = s.IndexOf(">") + 1;
            int to = s.LastIndexOf("</a");
            songName = s.Substring(from, to - from);

            return songName;
        }

        private string getSinger(string s)
        {
            string artist;
            int from = s.IndexOf(">") + 1;
            int to = s.LastIndexOf("</a");
            artist = s.Substring(from, to - from);

            return artist;
        }
    }

}
