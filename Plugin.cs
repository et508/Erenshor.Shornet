using BepInEx;
using HarmonyLib;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace ShorNet
{
    [BepInPlugin("et508.erenshor.shornet", "ShorNet", "0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static NetworkManager _networkManager;
        private static string steamUsername;
        private bool steamChecked = false;

        public void Awake()
        {
            ConfigGenerator.GenerateConfig(this);

            SceneManager.activeSceneChanged += OnSceneWasInitialized;

            // Apply all patches
            var instance = new Harmony("et508.erenshor.shornet");
            instance.PatchAll();
            Logger.LogInfo("All ShorNet patches have been loaded!");

            // Print hash
            Logger.LogInfo($"ShorNet Plugin Hash: {GetModHash()}");
        }

        public static string GetModHash()
        {
            // Path to current DLL
            string dllPath = Assembly.GetExecutingAssembly().Location;

            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException("DLL not found", dllPath);
            }

            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(dllPath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                foreach (var b in hash)
                    sb.Append(b.ToString("x2")); // hex format
                return sb.ToString();
            }
        }

        public static string GetSteamUsername()
        {
            return steamUsername;
        }

        public static NetworkManager GetNetworkManager()
        {
            return _networkManager;
        }

        public void OnSceneWasInitialized(Scene current, Scene next)
        {
            if (!steamChecked)
            {
                if (!SteamManager.Initialized || SteamFriends.GetPersonaName() == null)
                {
                    Logger.LogError("Steam is not initialized. ShorNet requires Steam to be running.");
                    Destroy(this);
                    return;
                }

                steamUsername = SteamFriends.GetPersonaName();
                Logger.LogInfo($"Using {steamUsername} as ShorNet display name.");

                GameObject shorNetObject = new GameObject("ShorNet");
                _networkManager = shorNetObject.AddComponent<NetworkManager>();
                _networkManager.Init(Logger, steamUsername, this.Info);
                DontDestroyOnLoad(shorNetObject);

                steamChecked = true;
            }

            if (SceneValidator.IsValidScene(next.name) && !_networkManager.IsConnected)
            {
                _networkManager.ConnectToGlobalServer();
            }
            else if (_networkManager.IsConnected && !SceneValidator.IsValidScene(next.name))
            {
                _networkManager.SendDisconnectedPackage();
            }
        }

        public void OnApplicationQuit()
        {
            if (_networkManager == null) return;

            _networkManager.SendDisconnectedPackage();
        }

        public static void SendChatLogMessage(string message)
        {
            if (_networkManager == null) return;

            UpdateSocialLog.LogAdd(message);

            if (ConfigGenerator._enablePrintInGlobalChat.Value)
            {
                UpdateSocialLog.LogAdd(message);
            }
        }
    }
}
