using BepInEx;
using BepInEx.Configuration;
using System;

namespace ShorNet
{
    public static class ConfigGenerator
    {
        public static ConfigEntry<string> _serverIp;
        public static ConfigEntry<int> _serverPort;
        public static ConfigEntry<bool> _enablePrintInChatWindow;

        public static void GenerateConfig(BaseUnityPlugin baseUnityPlugin)
        {
            _serverIp = baseUnityPlugin.Config.Bind(
                "Connection",
                "ServerIP",
                "127.0.0.1", //TODO: Change to actual server IP
                "The IP address of the ShorNet server."
            );

            _serverPort = baseUnityPlugin.Config.Bind(
                "Connection",
                "ServerPort",
                27015,
                "The port of the ShorNet server."
            );

            _enablePrintInChatWindow = baseUnityPlugin.Config.Bind(
                "Preferences",
                "EnablePrintInChatWindow",
                false,
                "Also print all ShorNet messages into the game's chat window."
            );
        }
    }
}