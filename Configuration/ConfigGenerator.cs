using BepInEx;
using BepInEx.Configuration;

namespace ShorNet
{
    public static class ConfigGenerator
    {
        // Connection info now lives in ShorNetSetup.ConnectionConfigPath (connection.wtf)
        // We keep only user preferences in the BepInEx config file.

        public static ConfigEntry<bool> _enablePrintInChatWindow;

        public static void GenerateConfig(BaseUnityPlugin baseUnityPlugin)
        {
            _enablePrintInChatWindow = baseUnityPlugin.Config.Bind(
                "Preferences",
                "EnablePrintInChatWindow",
                false,
                "Send ShorNet messages to the game's chat window instead of the ShorNet UI."
            );
        }
    }
}