using BepInEx;
using HarmonyLib;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using BepInEx.Logging;

namespace ShorNet
{
    [BepInPlugin("et508.erenshor.shornet", "ShorNet", "0.1.0")]
    [BepInProcess("Erenshor.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static NetworkManager _networkManager;
        private static string steamUsername;
        private bool steamChecked = false;
        
        internal static ManualLogSource Log;

        public void Awake()
        {
            Log = Logger;
            
            ConfigGenerator.GenerateConfig(this);
            
            ShorNetSetup.EnsureInitialized();
            WindowLayoutStore.Load();

            SceneManager.activeSceneChanged += OnSceneWasInitialized;
            
            var harmony = new Harmony("et508.erenshor.shornet");
            harmony.PatchAll();
            
            ShorNetController.Initialize();
            
            Log.LogInfo("All ShorNet patches have been loaded!");
            
            Log.LogInfo($"ShorNet Plugin Hash: {GetModHash()}");
        }

        public static string GetModHash()
        {
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
    }
}
