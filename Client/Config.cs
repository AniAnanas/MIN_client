using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm;

namespace Client
{
    public class Config
    {
        [JsonProperty(nameof(LastLeftTabSize))]
        public short LastLeftTabSize { get; set; } = 280;

        [JsonProperty("Path to Sqlite DB")]
        public string DatabasePath { get; set; } = "Data/MIN.sqlite";

        [JsonProperty("Interval of DB saving in secs")]
        public int DBSaveInterval { get; set; } = 120;

        [JsonProperty("Server Host")]
        public string ServerHost { get; set; } = "localhost";

        [JsonProperty("Server Port")]
        public int ServerPort { get; set; } = 5555;

        public static Config Read()
        {
            //string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Settings");
            string directoryPath = Environment.CurrentDirectory;
            Directory.CreateDirectory(Path.Combine(directoryPath, "Data"));
            string configPath = Path.Combine(directoryPath, "Data", "Config.json");
            Directory.CreateDirectory(directoryPath);

            try
            {
                Config config = DefaultConfig();

                if (!File.Exists(configPath))
                {
                    File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
                }
                return JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath)) ?? config;
            }

            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return DefaultConfig();
            }
        }

        private static Config DefaultConfig()
        {
            var defaultConfig = new Config
            {

            };

            return defaultConfig;
        }
    }
}