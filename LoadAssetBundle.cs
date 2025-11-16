using System.IO;
using System.Reflection;
using UnityEngine;

namespace ShorNet
{
    public static class LoadAssetBundle
    {
        private const string BundleFileName = "netui";

        private static AssetBundle _bundle;
        private static string _bundlePath;
        private static bool _failed;
        
        private static AssetBundle GetBundle()
        {
            if (_failed)
                return null;

            if (_bundle != null)
                return _bundle;
            
            if (string.IsNullOrEmpty(_bundlePath))
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string assemblyDir  = Path.GetDirectoryName(assemblyPath);
                _bundlePath         = Path.Combine(assemblyDir ?? string.Empty, BundleFileName);
            }

            if (!File.Exists(_bundlePath))
            {
                Debug.LogError($"[NetUI] AssetBundle not found at: {_bundlePath}");
                _failed = true;
                return null;
            }

            _bundle = AssetBundle.LoadFromFile(_bundlePath);
            if (_bundle == null)
            {
                Debug.LogError("[NetUI] Failed to load AssetBundle!");
                _failed = true;
            }
            else
            {
                Debug.Log($"[NetUI] Loaded AssetBundle from '{_bundlePath}'.");
            }

            return _bundle;
        }
        
        public static GameObject CreateUIRoot(string prefabName, bool dontDestroyOnLoad = true)
        {
            var bundle = GetBundle();
            if (bundle == null)
            {
                Debug.LogError("[NetUI] Cannot create UI root because bundle is not loaded.");
                return null;
            }

            GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);
            if (prefab == null)
            {
                Debug.LogError($"[NetUI] Prefab '{prefabName}' not found in bundle.");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab);
            if (dontDestroyOnLoad)
            {
                Object.DontDestroyOnLoad(instance);
            }

            return instance;
        }
        
        public static void Unload(bool unloadAllLoadedObjects = false)
        {
            if (_bundle != null)
            {
                _bundle.Unload(unloadAllLoadedObjects);
                _bundle = null;
                _failed = false;
                Debug.Log("[NetUI] AssetBundle unloaded.");
            }
        }
    }
}
