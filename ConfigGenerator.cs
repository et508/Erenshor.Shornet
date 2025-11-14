using BepInEx;
using BepInEx.Configuration;
using System;

namespace ShorNet
{
    public static class ConfigGenerator
    {
        public static ConfigEntry<string> _serverIp;
        public static ConfigEntry<int> _serverPort;
        public static ConfigEntry<bool> _enablePrintInGlobalChat;

        public static void GenerateConfig(BaseUnityPlugin baseUnityPlugin)
        {
            _serverIp = baseUnityPlugin.Config.Bind(
                "Connection",
                "ServerIP",
                "45.131.109.73",
                "The IP address of the ShorNet server."
            );

            _serverPort = baseUnityPlugin.Config.Bind(
                "Connection",
                "ServerPort",
                9055,
                "The port of the ShorNet server."
            );

            _enablePrintInGlobalChat = baseUnityPlugin.Config.Bind(
                "Preferences",
                "EnablePrintInShorNetChat",
                false,
                "Also print all ShorNet messages into the game's Global Chat tab instead of only the local Social Log."
            );
        }
    }
}