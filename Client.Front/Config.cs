using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Client.Front
{
    public class Config
    {
        [JsonProperty(nameof(LastLeftTabSize))]
        public short LastLeftTabSize { get; set; } = 280;
        public static Config Read()
        {
            //string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Settings");
            string directoryPath = Directory.GetCurrentDirectory();
            string configPath = Path.Combine(directoryPath, "Config.json");
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