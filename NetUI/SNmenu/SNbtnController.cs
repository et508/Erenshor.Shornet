using UnityEngine;
using UnityEngine.UI;

namespace ShorNet
{
    public static class SNbtnController
    {
        private const string WindowId = "SNbtn";

        private static GameObject _uiRoot;

        private static GameObject _btnRoot;
        private static RectTransform _btnRect;

        private static GameObject _panelBG;
        private static RectTransform _panelBGRect;

        private static GameObject _buttonGO;
        private static RectTransform _buttonRect;
        private static Button _button;

        public static bool IsInitialized { get; private set; }

        
        public static void Initialize(GameObject uiRoot)
        {
            _uiRoot = uiRoot;
            
            _btnRoot = UICommon.Find(_uiRoot, "SNbtn")?.gameObject;
            if (_btnRoot == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNbtnController: 'SNbtn' not found under SNmenu root.");
                return;
            }

            _btnRect = _btnRoot.GetComponent<RectTransform>();
            if (_btnRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNbtnController: 'SNbtn' has no RectTransform.");
                return;
            }
            
            WindowLayout layout;
            if (WindowLayoutStore.TryGetLayout(WindowId, out layout))
            {
                var pos = new Vector2(layout.PosX, layout.PosY);
                _btnRect.anchoredPosition = pos;
                
                if (layout.SizeX > 0f || layout.SizeY > 0f)
                {
                    var size = new Vector2(layout.SizeX, layout.SizeY);
                    _btnRect.sizeDelta = size;
                }
            }
            
            _panelBG = UICommon.Find(_uiRoot, "SNbtn/panelBG")?.gameObject;
            if (_panelBG == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNbtnController: 'SNbtn/panelBG' not found.");
                return;
            }

            _panelBGRect = _panelBG.GetComponent<RectTransform>();
            if (_panelBGRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNbtnController: 'panelBG' has no RectTransform.");
                return;
            }
            
            _buttonGO = UICommon.Find(_uiRoot, "SNbtn/panelBG/Button")?.gameObject;
            if (_buttonGO == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNbtnController: 'SNbtn/panelBG/Button' not found.");
                return;
            }

            _buttonRect = _buttonGO.GetComponent<RectTransform>();
            if (_buttonRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNbtnController: 'Button' has no RectTransform.");
                return;
            }

            _button = _buttonGO.GetComponent<Button>();
            if (_button == null)
            {
                Plugin.Log?.LogWarning("[ShorNet] SNbtnController: 'Button' has no Button component (click logic will be added later).");
            }
            else
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnMenuButtonClicked);
            }
            
            var dh = _panelBG.GetComponent<DragHandler>() ?? _panelBG.AddComponent<DragHandler>();
            dh.PanelToMove = _btnRect;
            
            dh.OnDragFinished = SaveWindowLayout;

            IsInitialized = true;
        }
        
        private static void SaveWindowLayout(Vector2 anchoredPos)
        {
            if (_btnRect == null)
                return;

            var size = _btnRect.sizeDelta;
            WindowLayoutStore.SetLayout(WindowId, anchoredPos, size);
        }

       
        private static void OnMenuButtonClicked()
        {
            Plugin.Log?.LogInfo("[ShorNet] SNmenu button clicked (menu popup not implemented yet).");
        }
    }
}
