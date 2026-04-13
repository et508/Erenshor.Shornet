using BepInEx;
using HarmonyLib;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BepInEx.Logging;

namespace ShorNet
{
    [BepInPlugin("et508.erenshor.shornet", "ShorNet", "0.1.1")]
    [BepInProcess("Erenshor.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private static NetworkManager _networkManager;
        private static string steamUsername;
        private bool steamChecked = false;

        internal static ManualLogSource Log;

        // ImGui
        internal ImGuiRenderer _imgui;
        private  ShorNetWindow _window;

        internal static Plugin Instance;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern System.IntPtr LoadLibrary(string path);

        public void Awake()
        {
            Log = Logger;
            Instance = this;

            ConfigGenerator.GenerateConfig(this);
            ShorNetSetup.EnsureInitialized();
            ShorNetSettings.Load();

            SceneManager.activeSceneChanged += OnSceneWasInitialized;

            var harmony = new Harmony("et508.erenshor.shornet");
            harmony.PatchAll();

            ShorNetController.Initialize();

            Log.LogInfo($"ShorNet Plugin Hash: {GetModHash()}");
        }

        private void Start()
        {
            string assemblyDir = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location) ?? BepInEx.Paths.PluginPath;
            string cimguiPath = Path.Combine(assemblyDir, "cimgui.dll");

            if (!File.Exists(cimguiPath))
            {
                Logger.LogError("[ShorNet] cimgui.dll not found at: " + cimguiPath);
                return;
            }

            var hLib = LoadLibrary(cimguiPath);
            if (hLib == System.IntPtr.Zero)
            {
                Logger.LogError("[ShorNet] Failed to pre-load cimgui.dll (Win32 error " +
                    Marshal.GetLastWin32Error() + ")");
                return;
            }

            Logger.LogInfo("[ShorNet] cimgui.dll loaded at 0x" + hLib.ToString("X"));
            InitImGuiTypes();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitImGuiTypes()
        {
            float detectedScale = Mathf.Clamp((float)Screen.height / 1080f, 0.5f, 4f);

            _window = new ShorNetWindow();
            _imgui  = new ImGuiRenderer(Logger);
            _imgui.UiScale = detectedScale;
            _imgui.OnLayout = () => _window.Draw();

            if (!_imgui.Init())
            {
                Logger.LogError("[ShorNet] ImGui init failed — settings UI will not render.");
            }
            else
            {
                PointerOverUIPatch.Renderer = _imgui;
                _window.Scale = detectedScale;

                var mute = gameObject.GetComponent<ImGuiInputMute>()
                           ?? gameObject.AddComponent<ImGuiInputMute>();
                mute.Renderer = _imgui;

                Logger.LogInfo("[ShorNet] ImGui UI ready.");
            }
        }

        public static void OpenSettingsWindow()
        {
            Instance?._window?.Show();
        }

        public static string GetModHash()
        {
            string dllPath = Assembly.GetExecutingAssembly().Location;

            if (!File.Exists(dllPath))
                throw new FileNotFoundException("DLL not found", dllPath);

            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(dllPath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                var sb = new StringBuilder();
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
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
                    // Steam not ready yet on this scene change — skip networking, try next time.
                    return;
                }

                steamUsername = SteamFriends.GetPersonaName();
                Logger.LogInfo($"Using {steamUsername} as ShorNet display name.");

                GameObject shorNetObject = new GameObject("ShorNet");
                _networkManager = shorNetObject.AddComponent<NetworkManager>();
                _networkManager.Init(Logger, steamUsername);
                DontDestroyOnLoad(shorNetObject);

                steamChecked = true;
            }

            if (_networkManager == null) return;

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

        private void OnGUI()
        {
            _imgui?.OnGUI();
        }

        private void OnDestroy()
        {
            _imgui?.Dispose();
        }
    }
}