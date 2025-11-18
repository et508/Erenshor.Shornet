using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;

namespace ShorNet
{
    public class WindowLayout
    {
        public float PosX;
        public float PosY;
        public float SizeX;
        public float SizeY;
    }
    
    public static class WindowLayoutStore
    {
        private static bool _loaded;
        private static Dictionary<string, WindowLayout> _layouts =
            new Dictionary<string, WindowLayout>(StringComparer.OrdinalIgnoreCase);

        private static string LayoutFilePath => ShorNetSetup.WindowLayoutsPath;
        
        public static void Load()
        {
            if (_loaded)
                return;

            _loaded = true;

            try
            {
                ShorNetSetup.EnsureInitialized();

                if (!File.Exists(LayoutFilePath))
                {
                    Plugin.Log?.LogInfo("[WindowLayoutStore] No existing layout file found; starting empty.");
                    return;
                }

                string json = File.ReadAllText(LayoutFilePath);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, WindowLayout>>(json);

                if (dict != null)
                {
                    _layouts = dict;
                }

                Plugin.Log?.LogInfo($"[WindowLayoutStore] Loaded {_layouts.Count} window layout(s).");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[WindowLayoutStore] Failed to load layouts: {ex}");
            }
        }
        
        public static void Save()
        {
            try
            {
                ShorNetSetup.EnsureInitialized();

                string json = JsonConvert.SerializeObject(_layouts, Formatting.Indented);
                File.WriteAllText(LayoutFilePath, json);

                Plugin.Log?.LogInfo("[WindowLayoutStore] Layouts saved.");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[WindowLayoutStore] Failed to save layouts: {ex}");
            }
        }
        
        public static bool TryGetLayout(string key, out WindowLayout layout)
        {
            Load();
            return _layouts.TryGetValue(key, out layout);
        }
        
        public static void SetLayout(string key, Vector2 position, Vector2 size)
        {
            Load();

            _layouts[key] = new WindowLayout
            {
                PosX = position.x,
                PosY = position.y,
                SizeX = size.x,
                SizeY = size.y
            };

            Save();
        }
        
        public static bool ApplyToRectTransform(string key, RectTransform rect)
        {
            if (rect == null)
                return false;

            if (!TryGetLayout(key, out var layout))
                return false;

            rect.anchoredPosition = new Vector2(layout.PosX, layout.PosY);
            rect.sizeDelta        = new Vector2(layout.SizeX, layout.SizeY);

            return true;
        }
    }
}
