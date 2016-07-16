using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestSharp;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace spotify_nowplaying
{
    class Song
    {
        string _artist;
        public String Artist
        {
            get { return _artist; }
        }

        string _title;
        public String Title
        {
            get { return _title; }
        }

        public static Song ProccessTitle(String windowTitle)
        {
            int splitLocation = windowTitle.IndexOf(" - ");

            if (splitLocation == -1)
            {
                return null;
            }

            String artist = windowTitle.Substring(0, splitLocation);
            String title = windowTitle.Substring(splitLocation + 3);

            Song song = new Song(artist, title);
            return song;
        }

        public Song(String artist, String title)
        {
            this._artist = artist;
            this._title = title;
        }

        public bool DownloadAlbumArt(Config config)
        {
            // Lookup the album art
            RestClient client = new RestClient("http://api.spotify.com");
            RestRequest request = new RestRequest("v1/search", Method.GET);
            request.AddParameter("q", String.Format("artist:{0} track:{1}", _artist, _title));
            request.AddParameter("type", "track");
            request.AddParameter("limit", "1");

            IRestResponse response = client.Execute(request);
            String sResponse = response.Content;

            try
            {
                JObject jLinq = JObject.Parse(sResponse);

                String link = (String)jLinq["tracks"]["items"][0]["album"]["images"][0]["url"];

                if (link.Length != 0)
                {
                    Uri imgUri = new Uri(link);
                    using (WebClient imgClient = new WebClient())
                    {
                        imgClient.DownloadFile(imgUri, Path.Combine(config.Storage, "album.png"));
                        return true;
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Error downloading album art");
            }

            return false;
        }

        private void clearAlbumArt(String path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (StreamWriter stream = new StreamWriter(File.Open(path, FileMode.Create)))
                    {
                        stream.Write("");
                        stream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void CreateStreamPanel(Config config)
        {
            CreateStreamPanel(config, false);
        }

        public void CreateStreamPanel(Config config, bool placeholder)
        {
            Image template = Image.FromFile(Path.Combine(config.Storage, "template.png"));
            Image album = null;
            if (placeholder)
            {
                album = Image.FromFile(Path.Combine(config.Storage, "placeholder.png"));
            }
            else
            {
                album = Image.FromFile(Path.Combine(config.Storage, "album.png"));
            }
            Graphics g = Graphics.FromImage(template);
            g.DrawImage(album, 0, 0, 64, 64);
            g.DrawString(_artist, new Font(FontFamily.GenericMonospace, 12, FontStyle.Regular), new SolidBrush(Color.White), 72, 8);
            g.DrawString(_title, new Font(FontFamily.GenericMonospace, 12, FontStyle.Regular), new SolidBrush(Color.White), 72, 28);
            template.Save(config.Output, ImageFormat.Png);
        }
    }
}
