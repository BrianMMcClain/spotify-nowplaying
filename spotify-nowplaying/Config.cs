using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spotify_nowplaying
{
    class Config
    {
        private String _template;
        public String Template
        {
            get { return _template; }
            set { _template = value; }
        }

        private String _output;
        public String Output
        {
            get { return _output; }
            set { _output = value; }
        }

        private String _storage;
        public String Storage
        {
            get { return _storage; }
            set { _storage = value; }
        }

        private String _placeholder;
        public String Placeholder
        {
            get { return _placeholder; }
            set { _placeholder = value; }
        }

        public static Config ParseConfig(String path)
        {
            if (File.Exists(path))
            {
                Config conf = new Config();
                using (StreamReader reader = new StreamReader(File.Open(path, FileMode.Open)))
                {
                    String line = reader.ReadLine();
                    while (line != null)
                    {
                        String key = line.Split('=')[0];
                        String value = line.Split('=')[1];
                        switch (key)
                        {
                            case "template":
                                conf.Template = value;
                                break;
                            case "output":
                                conf.Output = value;
                                break;
                            case "storage":
                                conf.Storage = value;
                                break;
                            case "placeholder":
                                conf._placeholder = value;
                                break;
                        }
                        line = reader.ReadLine();
                    }
                }
                return conf;
            }
            else
            {
                // File doesn't exsit
                Console.WriteLine("Config file " + path + " does not exist. Exiting.");
                return null;
            }
        }

        public Config()
        {

        }
    }
}
