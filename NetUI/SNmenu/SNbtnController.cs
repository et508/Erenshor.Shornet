using UnityEngine;
using UnityEngine.UI;

namespace ShorNet
{
    public static class SNbtnController
    {
        // Unique ID for layout persistence in windowlayouts.json
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

        /// <summary>
        /// Initialize the ShorNet menu button controller.
        /// Expects hierarchy: SNmenu(root)/SNbtn/panelBG/Button
        /// </summary>
        public static void Initialize(GameObject uiRoot)
        {
            _uiRoot = uiRoot;

            // SNbtn (movable root)
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

            // Try restore layout (position + size) from WindowLayoutStore
            WindowLayout layout;
            if (WindowLayoutStore.TryGetLayout(WindowId, out layout))
            {
                var pos = new Vector2(layout.PosX, layout.PosY);
                _btnRect.anchoredPosition = pos;

                // Size may be default; only apply if non-zero
                if (layout.SizeX > 0f || layout.SizeY > 0f)
                {
                    var size = new Vector2(layout.SizeX, layout.SizeY);
                    _btnRect.sizeDelta = size;
                    Plugin.Log?.LogInfo("[ShorNet] Restored SNmenu button layout: pos "
                        + pos + " size " + size + ".");
                }
                else
                {
                    Plugin.Log?.LogInfo("[ShorNet] Restored SNmenu button position to " + pos + ".");
                }
            }

            // panelBG (drag handle)
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

            // Button (clickable control – future menu popup hook)
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
                // Placeholder: we can wire this up to open the future ShorNet menu later.
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnMenuButtonClicked);
            }

            // 🔹 Drag handle: panelBG moves the SNbtn root (with snap-to-edge from DragHandler)
            var dh = _panelBG.GetComponent<DragHandler>() ?? _panelBG.AddComponent<DragHandler>();
            dh.PanelToMove = _btnRect;

            // Save layout when drag finishes
            dh.OnDragFinished = SaveWindowLayout;

            IsInitialized = true;
            Plugin.Log?.LogInfo("[ShorNet] SNbtnController initialized successfully.");
        }

        /// <summary>
        /// Save the SNmenu button layout (position + size) via WindowLayoutStore.
        /// </summary>
        private static void SaveWindowLayout(Vector2 anchoredPos)
        {
            if (_btnRect == null)
                return;

            var size = _btnRect.sizeDelta;
            WindowLayoutStore.SetLayout(WindowId, anchoredPos, size);

            Plugin.Log?.LogInfo("[ShorNet] Saved SNmenu button layout: pos "
                + anchoredPos + " size " + size + ".");
        }

        /// <summary>
        /// Placeholder for the actual menu-open behavior (EQ-style pop-up).
        /// We'll wire this into a proper ShorNet main menu UI later.
        /// </summary>
        private static void OnMenuButtonClicked()
        {
            // For now, just log so we know it's wired.
            Plugin.Log?.LogInfo("[ShorNet] SNmenu button clicked (menu popup not implemented yet).");
            // Future:
            // SNmainMenuController.ToggleMenu();
        }
    }
}
