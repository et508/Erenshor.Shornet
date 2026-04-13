using System;
using System.IO;
using BepInEx;
using Newtonsoft.Json;

namespace ShorNet
{
    public static class ShorNetSetup
    {
        private static bool _initialized;

        public static string ShorNetConfigFolder =>
            Path.Combine(Paths.ConfigPath, "ShorNet");

        public static string ConnectionConfigPath =>
            Path.Combine(ShorNetConfigFolder, "connection.wtf");

        public static string SettingsPath =>
            Path.Combine(ShorNetConfigFolder, "settings.json");

        public static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;

            try
            {
                Directory.CreateDirectory(ShorNetConfigFolder);

                if (!File.Exists(ConnectionConfigPath))
                {
                    var defaultConfig = new ConnectionConfig
                    {
                        ServerIP   = "165.227.186.68",
                        ServerPort = 27015
                    };

                    string json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                    File.WriteAllText(ConnectionConfigPath, json);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[ShorNetSetup] Failed to initialize config paths: {ex}");
            }
        }
    }
}