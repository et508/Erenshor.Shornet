using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace ShorNet
{
    public class NetUI : MonoBehaviour
    {
        public static NetUI Instance { get; private set; }
        private const string BundleFileName  = "netui";
        private const string PrefabAssetName = "netUI";
        private AssetBundle _uiBundle;
        private GameObject  _uiRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadUIFromBundle();
        }

        private void LoadUIFromBundle()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir  = Path.GetDirectoryName(assemblyPath);
            
            string bundlePath = Path.Combine(assemblyDir, BundleFileName);

            if (!File.Exists(bundlePath))
            {
                Debug.LogError($"[NetUI] AssetBundle not found at: {bundlePath}");
                return;
            }

            _uiBundle = AssetBundle.LoadFromFile(bundlePath);
            if (_uiBundle == null)
            {
                Debug.LogError("[NetUI] Failed to load AssetBundle!");
                return;
            }

            GameObject prefab = _uiBundle.LoadAsset<GameObject>(PrefabAssetName);
            if (prefab == null)
            {
                Debug.LogError($"[NetUI] Prefab '{PrefabAssetName}' not found in bundle.");
                return;
            }
            
            _uiRoot = Instantiate(prefab);
            NetUIController.Initialize(_uiRoot);

            DontDestroyOnLoad(_uiRoot);
            
            if (ConfigGenerator._enablePrintInChatWindow.Value)
            {
                _uiRoot.SetActive(false);
            }
            else
            {
                _uiRoot.SetActive(true);
            }
        }
        
        public void ToggleUI()
        {
            if (_uiRoot == null)
                return;
            
            bool shouldBeActive = !_uiRoot.activeSelf;
            _uiRoot.SetActive(shouldBeActive);
        }

        private void OnDestroy()
        {
            if (_uiBundle != null)
            {
                _uiBundle.Unload(false);
                _uiBundle = null;
            }

            if (_uiRoot != null)
            {
                Destroy(_uiRoot);
                _uiRoot = null;
            }

            Instance = null;
        }
    }
}
