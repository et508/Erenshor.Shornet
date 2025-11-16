using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json;
using UnityEngine;

namespace ShorNet
{
    /// <summary>
    /// Simple serializable layout record for a window.
    /// </summary>
    public class WindowLayout
    {
        public float PosX;
        public float PosY;
        public float SizeX;
        public float SizeY;
    }

    /// <summary>
    /// Central store for all window layouts (positions & sizes).
    /// Backed by BepInEx/config/ShorNet/windowlayouts.json.
    /// </summary>
    public static class WindowLayoutStore
    {
        private static bool _loaded;
        private static Dictionary<string, WindowLayout> _layouts =
            new Dictionary<string, WindowLayout>(StringComparer.OrdinalIgnoreCase);

        private static string LayoutFilePath => ShorNetSetup.WindowLayoutsPath;

        /// <summary>
        /// Load layouts from JSON (no-op if already loaded).
        /// </summary>
        public static void Load()
        {
            if (_loaded)
                return;

            _loaded = true;

            try
            {
                // Ensure base folder/file exist
                ShorNetSetup.EnsureInitialized();

                if (!File.Exists(LayoutFilePath))
                {
                    // Nothing to load yet
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

        /// <summary>
        /// Save current layouts to JSON.
        /// </summary>
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

        /// <summary>
        /// Try to get a stored layout for a given window key.
        /// </summary>
        public static bool TryGetLayout(string key, out WindowLayout layout)
        {
            Load();
            return _layouts.TryGetValue(key, out layout);
        }

        /// <summary>
        /// Set/update layout for a given window key and immediately save.
        /// </summary>
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

        /// <summary>
        /// Helper to apply a stored layout (if present) to a RectTransform.
        /// </summary>
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
