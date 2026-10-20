using System;
using System.IO;
using System.Text.Json;
using FamilySupermarketPOS.Models;

namespace FamilySupermarketPOS.Services
{
    public static class ConfigService
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public static AppConfig Current { get; set; } = new();

        static ConfigService()
        {
            Load();
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var cfg = JsonSerializer.Deserialize<AppConfig>(json);
                    if (cfg != null)
                    {
                        Current = cfg;
                        return;
                    }
                }
            }
            catch
            {
                // Fall back to default
            }

            Current = new AppConfig();
        }

        public static void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch
            {
                // Non-critical
            }
        }
    }
}
