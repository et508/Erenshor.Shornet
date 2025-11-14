using System;
using System.Linq;

namespace ShorNet
{
    static class SceneValidator
    {
        public static string[] InvalidScenes = new string[]
        {
            "Menu",
            "LoadScene",
        };

        public static bool IsValidScene(string sceneName)
        {
            return !InvalidScenes.Contains(sceneName, StringComparer.OrdinalIgnoreCase);
        }
    }
}