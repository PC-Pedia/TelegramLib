using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TelegramLib
{
    public class TelegramClientSettings
    {
        public int ApiId { get; set; }
        public string ApiHash { get; set; }

        public TelegramClientSettings()
        {

        }

        public TelegramClientSettings(string configPath)
        {
            ReadFromJsonConfig(configPath);
        }

        public void ReadFromJsonConfig(string path)
        {
            var conf =  JsonConvert.DeserializeObject<TelegramClientSettings>(File.ReadAllText(path));
            ApiId = conf.ApiId;
            ApiHash = conf.ApiHash;
        }
    }
}
