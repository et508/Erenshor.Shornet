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

        // 🔹 New: UI window position persistence
        public static ConfigEntry<bool> WindowPositionEnabled;
        public static ConfigEntry<float> WindowPosX;
        public static ConfigEntry<float> WindowPosY;

        // 🔹 New: UI window size persistence
        public static ConfigEntry<bool> WindowSizeEnabled;
        public static ConfigEntry<float> WindowWidth;
        public static ConfigEntry<float> WindowHeight;

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
                "Send ShorNet messages to the game's chat window."
            );

            // UI Position config
            WindowPositionEnabled = baseUnityPlugin.Config.Bind(
                "UI",
                "WindowPositionEnabled",
                false,
                "If true, ShorNet will restore the chat window position from the saved values."
            );

            WindowPosX = baseUnityPlugin.Config.Bind(
                "UI",
                "WindowPosX",
                0f,
                "Saved ShorNet window X position (anchoredPosition.x)."
            );

            WindowPosY = baseUnityPlugin.Config.Bind(
                "UI",
                "WindowPosY",
                0f,
                "Saved ShorNet window Y position (anchoredPosition.y)."
            );

            // UI Size config
            WindowSizeEnabled = baseUnityPlugin.Config.Bind(
                "UI",
                "WindowSizeEnabled",
                false,
                "If true, ShorNet will restore the chat window size from the saved values."
            );

            WindowWidth = baseUnityPlugin.Config.Bind(
                "UI",
                "WindowWidth",
                0f,
                "Saved ShorNet window width (container sizeDelta.x)."
            );

            WindowHeight = baseUnityPlugin.Config.Bind(
                "UI",
                "WindowHeight",
                0f,
                "Saved ShorNet window height (container sizeDelta.y)."
            );
        }
    }
}
