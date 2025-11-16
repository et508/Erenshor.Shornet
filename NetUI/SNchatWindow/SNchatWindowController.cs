using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShorNet
{
    public static class SNchatWindowController
    {
        private static GameObject _uiRoot;

        private static GameObject _container;
        private static RectTransform _containerRect;

        private static GameObject _panelBG;
        private static RectTransform _panelBGRect;

        private static GameObject _messagePanel;
        private static RectTransform _messagePanelRect;

        private static GameObject _dragHandle;
        private static GameObject _resizeHandle;

        private static Transform _messageContent;
        private static TextMeshProUGUI _messageTemplate;
        private static ScrollRect _scrollRect;
        private static RectTransform _messageViewRect;
        private static RectTransform _viewportRect;

        // Layout relationship caches
        private static Vector2 _initialContainerSize;
        private static Vector2 _initialPanelBGSize;
        private static Vector2 _initialMessagePanelSize;
        private static Vector2 _initialMessageViewSize;

        // Margins: parentSize - childSize
        private static Vector2 _panelBGMargin;      // container - panelBG
        private static Vector2 _messagePanelMargin; // panelBG - messagePanel
        private static Vector2 _messageViewMargin;  // messagePanel - messageView

        public static bool IsInitialized { get; private set; }

        public static void Initialize(GameObject uiRoot)
        {
            _uiRoot = uiRoot;

            // container
            _container = UICommon.Find(_uiRoot, "container")?.gameObject;
            if (_container == null)
            {
                Plugin.Log?.LogError("[ShorNet]: 'container' not found on SNchatWindow.");
                return;
            }

            _containerRect = _container.GetComponent<RectTransform>();
            if (_containerRect == null)
            {
                Plugin.Log?.LogError("[ShorNet]: SNchatWindow 'container' has no RectTransform.");
                return;
            }

            // 🔹 Restore saved window position if enabled
            if (ConfigGenerator.WindowPositionEnabled != null &&
                ConfigGenerator.WindowPositionEnabled.Value)
            {
                var restored = new Vector2(
                    ConfigGenerator.WindowPosX.Value,
                    ConfigGenerator.WindowPosY.Value
                );
                _containerRect.anchoredPosition = restored;
                Plugin.Log?.LogInfo($"[ShorNet] Restored chat window position to {restored}.");
            }

            // panelBG
            _panelBG = UICommon.Find(_uiRoot, "container/panelBG")?.gameObject;
            if (_panelBG == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'panelBG' not found under container.");
                return;
            }

            _panelBGRect = _panelBG.GetComponent<RectTransform>();
            if (_panelBGRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'panelBG' has no RectTransform.");
                return;
            }

            // messagePanel
            _messagePanel = UICommon.Find(_uiRoot, "container/panelBG/messagePanel")?.gameObject;
            if (_messagePanel == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messagePanel' not found.");
                return;
            }

            _messagePanelRect = _messagePanel.GetComponent<RectTransform>();
            if (_messagePanelRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messagePanel' has no RectTransform.");
                return;
            }

            // messageView (ScrollRect)
            var messageViewGO = UICommon.Find(_uiRoot, "container/panelBG/messagePanel/messageView")?.gameObject;
            if (messageViewGO == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messageView' not found.");
                return;
            }

            _scrollRect = messageViewGO.GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messageView' has no ScrollRect.");
                return;
            }

            _messageViewRect = messageViewGO.GetComponent<RectTransform>();
            if (_messageViewRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messageView' has no RectTransform.");
                return;
            }

            var viewport = _scrollRect.viewport;
            if (viewport == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: ScrollRect has no viewport assigned.");
                return;
            }

            _viewportRect = viewport.GetComponent<RectTransform>();
            if (_viewportRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: viewport has no RectTransform.");
                return;
            }

            // messageContent
            _messageContent = _viewportRect.Find("messageContent");
            if (_messageContent == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messageContent' not found under messageView/Viewport.");
                return;
            }

            // messageTemplate
            var templateTransform = _messageContent.Find("messageTemplate");
            if (templateTransform == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messageTemplate' not found under messageContent.");
                return;
            }

            _messageTemplate = templateTransform.GetComponent<TextMeshProUGUI>();
            if (_messageTemplate == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messageTemplate' has no TextMeshProUGUI component.");
                return;
            }

            _messageTemplate.gameObject.SetActive(false);

            // Cache initial sizes & margins to preserve visual spacing when resizing
            _initialContainerSize     = _containerRect.sizeDelta;
            _initialPanelBGSize       = _panelBGRect.sizeDelta;
            _initialMessagePanelSize  = _messagePanelRect.sizeDelta;
            _initialMessageViewSize   = _messageViewRect.sizeDelta;

            _panelBGMargin      = _initialContainerSize    - _initialPanelBGSize;
            _messagePanelMargin = _initialPanelBGSize      - _initialMessagePanelSize;
            _messageViewMargin  = _initialMessagePanelSize - _initialMessageViewSize;

            // 🔹 Restore saved window size (after margins are cached so layout math stays correct)
            if (ConfigGenerator.WindowSizeEnabled != null &&
                ConfigGenerator.WindowSizeEnabled.Value &&
                (ConfigGenerator.WindowWidth.Value > 0f || ConfigGenerator.WindowHeight.Value > 0f))
            {
                var savedSize = new Vector2(
                    ConfigGenerator.WindowWidth.Value,
                    ConfigGenerator.WindowHeight.Value
                );

                // Apply now...
                _containerRect.sizeDelta = savedSize;
                RefreshLayout();
                Plugin.Log?.LogInfo($"[ShorNet] Restored chat window size to {savedSize}.");

                // ...and ensure it gets re-applied after Unity's first layout pass.
                var restorer = _container.AddComponent<SizeRestorer>();
                restorer.Target = _containerRect;
                restorer.SavedSize = savedSize;
            }

            // 🔹 Drag handle: use panelBG itself so clicking the border/background moves the window
            _dragHandle = _panelBG;
            if (_dragHandle != null && _containerRect != null)
            {
                var dh = _dragHandle.GetComponent<DragHandler>() ?? _dragHandle.AddComponent<DragHandler>();
                dh.PanelToMove = _containerRect;

                // Save window position when dragging finishes
                dh.OnDragFinished = SaveWindowPosition;
            }
            else
            {
                Plugin.Log?.LogWarning("[ShorNet] SNchatWindow: panelBG not usable as drag handle.");
            }

            // Resize handle
            _resizeHandle = UICommon.Find(_uiRoot, "container/resizeHandle")?.gameObject;
            if (_resizeHandle != null && _containerRect != null)
            {
                var rh = _resizeHandle.GetComponent<ResizeHandler>() ?? _resizeHandle.AddComponent<ResizeHandler>();
                rh.PanelToResize = _containerRect;
            }
            else
            {
                Plugin.Log?.LogWarning("[ShorNet] SNchatWindow: resizeHandle not found; window will not be resizable.");
            }

            IsInitialized = true;
        }

        /// <summary>
        /// Called by ResizeHandler when the main container changes size.
        /// Recomputes child sizes so original border/spacing is preserved.
        /// </summary>
        public static void OnPanelResized(RectTransform panel)
        {
            if (!IsInitialized || panel == null)
                return;

            if (panel != _containerRect)
                return; // only care about the ShorNet container

            RefreshLayout();

            // 🔹 Save updated size to config
            var size = _containerRect.sizeDelta;
            ConfigGenerator.WindowWidth.Value = size.x;
            ConfigGenerator.WindowHeight.Value = size.y;
            ConfigGenerator.WindowSizeEnabled.Value = true;

            Plugin.Log?.LogInfo($"[ShorNet] Saved chat window size: {size}.");
        }

        private static void RefreshLayout()
        {
            if (!IsInitialized)
                return;

            var containerSize = _containerRect.sizeDelta;

            // Maintain relationships using cached margins
            _panelBGRect.sizeDelta      = containerSize - _panelBGMargin;
            _messagePanelRect.sizeDelta = _panelBGRect.sizeDelta - _messagePanelMargin;
            _messageViewRect.sizeDelta  = _messagePanelRect.sizeDelta - _messageViewMargin;
        }

        private static void SaveWindowPosition(Vector2 anchoredPos)
        {
            if (_containerRect == null)
                return;

            ConfigGenerator.WindowPosX.Value = anchoredPos.x;
            ConfigGenerator.WindowPosY.Value = anchoredPos.y;
            ConfigGenerator.WindowPositionEnabled.Value = true;

            Plugin.Log?.LogInfo($"[ShorNet] Saved chat window position: {anchoredPos}.");
        }

        public static void AddMessage(string message)
        {
            if (!IsInitialized || _messageContent == null || _messageTemplate == null)
            {
                Plugin.Log?.LogWarning("[ShorNet] SNchatWindow.AddMessage called before initialization.");
                return;
            }

            var labelGO = Object.Instantiate(_messageTemplate.gameObject, _messageContent);
            var label = labelGO.GetComponent<TextMeshProUGUI>();
            label.text = message;
            labelGO.SetActive(true);

            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        // 🔹 Helper MonoBehaviour: re-apply saved size after Unity finishes its first layout pass.
        private sealed class SizeRestorer : MonoBehaviour
        {
            public RectTransform Target;
            public Vector2 SavedSize;

            private bool _applied;

            private void LateUpdate()
            {
                if (_applied || Target == null || !IsInitialized)
                    return;

                Target.sizeDelta = SavedSize;
                RefreshLayout();

                _applied = true;
                enabled = false;
            }
        }
    }
}
