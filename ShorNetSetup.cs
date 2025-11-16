using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;

namespace ShorNet
{
    /// <summary>
    /// One-time setup helper for ShorNet config-related folders/files.
    /// Creates BepInEx/config/ShorNet and windowlayouts.json if missing.
    /// </summary>
    public static class ShorNetSetup
    {
        private static bool _initialized;

        /// <summary>
        /// Folder: BepInEx/config/ShorNet
        /// </summary>
        public static string ShorNetConfigFolder =>
            Path.Combine(Paths.ConfigPath, "ShorNet");

        /// <summary>
        /// File: BepInEx/config/ShorNet/windowlayouts.json
        /// </summary>
        public static string WindowLayoutsPath =>
            Path.Combine(ShorNetConfigFolder, "windowlayouts.json");

        /// <summary>
        /// Ensure folder + windowlayouts.json exist.
        /// Safe to call multiple times; guarded by _initialized flag.
        /// </summary>
        public static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;

            try
            {
                // Ensure folder exists
                Directory.CreateDirectory(ShorNetConfigFolder);

                // Ensure windowlayouts.json exists (start as empty dictionary)
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