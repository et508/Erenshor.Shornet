using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShorNet
{
    public class SNchatWindow : MonoBehaviour
    {
        public static SNchatWindow Instance { get; private set; }

        private const string PrefabAssetName = "SNchatWindow";

        private GameObject _uiRoot;

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
            // Use shared loader instead of loading bundle directly
            _uiRoot = LoadAssetBundle.CreateUIRoot(PrefabAssetName, true);
            if (_uiRoot == null)
            {
                Debug.LogError("[NetUI] SNchatWindow failed to create UI root from prefab.");
                return;
            }

            // Your existing controller wiring
            SNchatWindowController.Initialize(_uiRoot);

            // Already marked DontDestroyOnLoad in loader, but this is harmless if called again
            DontDestroyOnLoad(_uiRoot);

            // Start hidden by default
            _uiRoot.SetActive(false);

            SceneManager.activeSceneChanged += OnSceneWasInitialized;

            // Apply correct visibility for the current active scene immediately
            var active = SceneManager.GetActiveScene();
            OnSceneWasInitialized(active, active);
        }

        public void OnSceneWasInitialized(Scene current, Scene next)
        {
            if (_uiRoot == null)
                return;

            // Always hide in invalid scenes (Menu, LoadScene, etc.)
            if (!SceneValidator.IsValidScene(next.name))
            {
                _uiRoot.SetActive(false);
                return;
            }

            // Valid gameplay scene: show or hide based on config
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
            SceneManager.activeSceneChanged -= OnSceneWasInitialized;

            if (_uiRoot != null)
            {
                Destroy(_uiRoot);
                _uiRoot = null;
            }

            // NOTE: we do NOT unload the AssetBundle here.
            // It’s shared via LoadAssetBundle and may be used by other UI pieces.
            Instance = null;
        }
    }
}
