using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShorNet
{
    public static class NetUIController
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
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'container' not found on netUI.");
                return;
            }

            _containerRect = _container.GetComponent<RectTransform>();
            if (_containerRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'container' has no RectTransform.");
                return;
            }

            // panelBG
            _panelBG = UICommon.Find(_uiRoot, "container/panelBG")?.gameObject;
            if (_panelBG == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'panelBG' not found under container.");
                return;
            }

            _panelBGRect = _panelBG.GetComponent<RectTransform>();
            if (_panelBGRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'panelBG' has no RectTransform.");
                return;
            }

            // messagePanel
            _messagePanel = UICommon.Find(_uiRoot, "container/panelBG/messagePanel")?.gameObject;
            if (_messagePanel == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'messagePanel' not found.");
                return;
            }

            _messagePanelRect = _messagePanel.GetComponent<RectTransform>();
            if (_messagePanelRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'messagePanel' has no RectTransform.");
                return;
            }

            // messageView (ScrollRect)
            var messageViewGO = UICommon.Find(_uiRoot, "container/panelBG/messagePanel/messageView")?.gameObject;
            if (messageViewGO == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'messageView' not found.");
                return;
            }

            _scrollRect = messageViewGO.GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'messageView' has no ScrollRect.");
                return;
            }

            _messageViewRect = messageViewGO.GetComponent<RectTransform>();
            if (_messageViewRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'messageView' has no RectTransform.");
                return;
            }

            var viewport = _scrollRect.viewport;
            if (viewport == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: ScrollRect has no viewport assigned.");
                return;
            }

            _viewportRect = viewport.GetComponent<RectTransform>();
            if (_viewportRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: viewport has no RectTransform.");
                return;
            }

            // messageContent
            _messageContent = _viewportRect.Find("messageContent");
            if (_messageContent == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'messageContent' not found under messageView/Viewport.");
                return;
            }

            // messageTemplate
            var templateTransform = _messageContent.Find("messageTemplate");
            if (templateTransform == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'messageTemplate' not found under messageContent.");
                return;
            }

            _messageTemplate = templateTransform.GetComponent<TextMeshProUGUI>();
            if (_messageTemplate == null)
            {
                Plugin.Log?.LogError("[ShorNet] NetUIController: 'messageTemplate' has no TextMeshProUGUI component.");
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
            

            // 🔹 Drag handle: use panelBG itself so clicking the border/background moves the window
            _dragHandle = _panelBG;
            if (_dragHandle != null && _containerRect != null)
            {
                var dh = _dragHandle.GetComponent<DragHandler>() ?? _dragHandle.AddComponent<DragHandler>();
                dh.PanelToMove = _containerRect;
            }
            else
            {
                Plugin.Log?.LogWarning("[ShorNet] NetUIController: panelBG not usable as drag handle.");
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
                Plugin.Log?.LogWarning("[ShorNet] NetUIController: resizeHandle not found; window will not be resizable.");
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

        public static void AddMessage(string message)
        {
            if (!IsInitialized || _messageContent == null || _messageTemplate == null)
            {
                Plugin.Log?.LogWarning("[ShorNet] NetUIController.AddMessage called before initialization.");
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
    }
}
