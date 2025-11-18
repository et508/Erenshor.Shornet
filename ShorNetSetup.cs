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
        
        public static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;

            try
            {
                
                Directory.CreateDirectory(ShorNetConfigFolder);
                
                if (!File.Exists(WindowLayoutsPath))
                {
                    var empty = new Dictionary<string, WindowLayout>();
                    string json = JsonConvert.SerializeObject(empty, Formatting.Indented);
                    File.WriteAllText(WindowLayoutsPath, json);

                    Plugin.Log?.LogInfo($"[ShorNetSetup] Created layout file at: {WindowLayoutsPath}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[ShorNetSetup] Failed to initialize config paths: {ex}");
            }
        }
    }
}