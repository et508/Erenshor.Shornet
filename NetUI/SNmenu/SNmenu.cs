using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShorNet
{
    public class SNmenu : MonoBehaviour
    {
        public static SNmenu Instance { get; private set; }

        private const string PrefabAssetName = "SNmenu";

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
                Debug.LogError("[NetUI] SNmenu failed to create UI root from prefab.");
                return;
            }
            
            SNbtnController.Initialize(_uiRoot);
            
            _uiRoot.SetActive(false);
            
            SceneManager.activeSceneChanged += OnSceneWasInitialized;

            
            var active = SceneManager.GetActiveScene();
            OnSceneWasInitialized(active, active);
        }
        
        public void OnSceneWasInitialized(Scene current, Scene next)
        {
            if (_uiRoot == null)
                return;
            
            if (SceneValidator.IsValidScene(next.name))
            {
                _uiRoot.SetActive(false); //TODO: change when ready to release menu
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
            if (_uiRoot != null)
            {
                Destroy(_uiRoot);
                _uiRoot = null;
            }
            
            Instance = null;
        }
    }
}