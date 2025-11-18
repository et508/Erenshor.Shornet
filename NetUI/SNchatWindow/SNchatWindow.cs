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
            _uiRoot = LoadAssetBundle.CreateUIRoot(PrefabAssetName, true);
            if (_uiRoot == null)
            {
                Debug.LogError("[NetUI] SNchatWindow failed to create UI root from prefab.");
                return;
            }
            
            SNchatWindowController.Initialize(_uiRoot);
            
            DontDestroyOnLoad(_uiRoot);
            
            _uiRoot.SetActive(false);

            SceneManager.activeSceneChanged += OnSceneWasInitialized;
            
            var active = SceneManager.GetActiveScene();
            OnSceneWasInitialized(active, active);
        }

        public void OnSceneWasInitialized(Scene current, Scene next)
        {
            if (_uiRoot == null)
                return;
            
            if (!SceneValidator.IsValidScene(next.name))
            {
                _uiRoot.SetActive(false);
                return;
            }
            
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
            
            Instance = null;
        }
    }
}
