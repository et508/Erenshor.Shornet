using System;
using System.IO;
using Newtonsoft.Json;

namespace ShorNet
{
    /// <summary>
    /// Runtime settings for ShorNet, persisted as JSON alongside connection.wtf.
    /// Intentionally separate from BepInEx config so the ImGui UI can write it
    /// without going through ConfigEntry machinery.
    /// </summary>
    public static class ShorNetSettings
    {
        public static string ChatOutputWindow { get; set; } = "MAINCHAT";
        public static int    ChatOutputTab    { get; set; } = 0;

        private static string FilePath => ShorNetSetup.SettingsPath;

        private sealed class SettingsData
        {
            public string ChatOutputWindow { get; set; } = "MAINCHAT";
            public int    ChatOutputTab    { get; set; } = 0;
        }

        public static void Load()
        {
            try
            {
                ShorNetSetup.EnsureInitialized();
                if (!File.Exists(FilePath)) return;

                string json = File.ReadAllText(FilePath);
                if (string.IsNullOrWhiteSpace(json)) return;

                var data = JsonConvert.DeserializeObject<SettingsData>(json);
                if (data == null) return;

                ChatOutputWindow = data.ChatOutputWindow ?? "MAINCHAT";
                ChatOutputTab    = data.ChatOutputTab;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError("[ShorNet] Failed to load settings: " + ex.Message);
            }
        }

        public static void Save()
        {
            try
            {
                ShorNetSetup.EnsureInitialized();
                var data = new SettingsData
                {
                    ChatOutputWindow = ChatOutputWindow,
                    ChatOutputTab    = ChatOutputTab
                };
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError("[ShorNet] Failed to save settings: " + ex.Message);
            }
        }
    }
}