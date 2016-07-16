using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace spotify_nowplaying
{
    class Program
    {
        private static Song lastSong; 

        static void Main(string[] args)
        {
            Config config = null;
            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: Must provide a path to the config file");
                Environment.Exit(0);
            }
            else
            {
                config = Config.ParseConfig(args[0]);
            }

            while (true)
            {
                Song song = getNowPlaying();
                if (song != null)
                {
                    if (lastSong == null || (song.Artist != lastSong.Artist || song.Title != lastSong.Title))
                    {
                        Console.WriteLine(song.Artist + " - " + song.Title); 
                        updateFiles(song, config);
                        lastSong = song;
                    }
                }
                else
                {
                    if (lastSong != null)
                    {
                        clearFiles(config);
                        lastSong = null;
                    }
                }
                Thread.Sleep(1000);
            }
        }

        static Song getNowPlaying()
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
            {

                if (!String.IsNullOrEmpty(p.MainWindowTitle) && p.ProcessName == "Spotify")
                {
                    if (p.MainWindowTitle != "Spotify")
                    {
                        return Song.ProccessTitle(p.MainWindowTitle);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Update text files with artist and song title, artist and download the album art
        /// </summary>
        /// <param name="song">Song to process</param>
        private static void updateFiles(Song song, Config config)
        {
            using (StreamWriter stream = new StreamWriter(File.Open(Path.Combine(config.Storage, "nowplaying-artist.txt"), FileMode.Create)))
            {
                stream.Write(song.Artist);
                stream.Close();
            }
            using (StreamWriter stream = new StreamWriter(File.Open(Path.Combine(config.Storage, "nowplaying - title.txt"), FileMode.Create)))
            {
                stream.Write(song.Title);
                stream.Close();
            }

            if (song.DownloadAlbumArt(config))
            {
                song.CreateStreamPanel(config);
            }
            else
            {
                song.CreateStreamPanel(config, true);
            }
        }

        private static void clearFiles(Config config)
        {
            Console.WriteLine("No song detected, clearing files");
            clearFile(Path.Combine(config.Storage, "nowplaying-artist.txt"));
            clearFile(Path.Combine(config.Storage, "nowplaying-title.txt"));
            clearFile(Path.Combine(config.Storage, "album.png"));
            clearFile(config.Output);
        }

        private static void clearFile(String path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (StreamWriter stream = new StreamWriter(File.Open(path, FileMode.Create)))
                    {
                        stream.Write("");
                        stream.Close();
                    }
                } catch (Exception e)
                {
                    ///TODO: Handle this exception properly
                }
            }
        }
    }
}
