using System;
using System.Collections.Generic;
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

        public static string WindowLayoutsPath =>
            Path.Combine(ShorNetConfigFolder, "windowlayouts.json");

        // NEW: separate connection config file (with .wtf extension, for fun)
        public static string ConnectionConfigPath =>
            Path.Combine(ShorNetConfigFolder, "connection.wtf");

        public static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;

            try
            {
                Directory.CreateDirectory(ShorNetConfigFolder);

                // Ensure window layouts exists
                if (!File.Exists(WindowLayoutsPath))
                {
                    var empty = new Dictionary<string, WindowLayout>();
                    string json = JsonConvert.SerializeObject(empty, Formatting.Indented);
                    File.WriteAllText(WindowLayoutsPath, json);
                }

                // Ensure connection.wtf exists with sane defaults
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