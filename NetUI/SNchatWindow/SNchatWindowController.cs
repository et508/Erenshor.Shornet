using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShorNet
{
    public static class SNchatWindowController
    {
        private const string WindowId = "SNchatWindow";

        private static GameObject      _uiRoot;
        private static GameObject      _container;
        private static RectTransform   _containerRect;

        private static GameObject      _panelBG;
        private static RectTransform   _panelBGRect;

        private static DebounceInvoker _debouncer;

        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Initialize ShorNet chat window.
        /// 
        /// uiRoot          = instantiated SNchatWindow root GameObject
        /// tabButtonPrefab = prefab "ChatTabButton" loaded from the same AssetBundle
        /// </summary>
        public static void Initialize(GameObject uiRoot, GameObject tabButtonPrefab)
        {
            IsInitialized = false;

            _uiRoot = uiRoot;

            if (_uiRoot == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindowController.Initialize: uiRoot is null.");
                return;
            }

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

            // Load saved window position/size if available
            if (WindowLayoutStore.TryGetLayout(WindowId, out WindowLayout layout))
            {
                Vector2 pos = new Vector2(layout.PosX, layout.PosY);
                _containerRect.anchoredPosition = pos;

                if (layout.SizeX > 0f || layout.SizeY > 0f)
                {
                    Vector2 size = new Vector2(layout.SizeX, layout.SizeY);
                    _containerRect.sizeDelta = size;
                }
            }

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

            // Dragging / edge snapping
            var dragHandle = _panelBG;
            if (dragHandle != null && _containerRect != null)
            {
                var dh = dragHandle.GetComponent<DragHandler>() ?? dragHandle.AddComponent<DragHandler>();
                dh.PanelToMove  = _containerRect;

                // Use panelBG for snap sizing, so snapping respects its visual size instead of the outer container.
                dh.SnapSizeRect = _panelBGRect;

                dh.OnDragFinished = SaveWindowPosition;
            }
            else
            {
                Plugin.Log?.LogWarning("[ShorNet] SNchatWindow: panelBG not usable as drag handle.");
            }

            // Tab bar container (where tab buttons will live)
            var tabContainer = UICommon.Find(
                _uiRoot,
                "container/panelBG/chatPanel/TabBar/TabContainer"
            )?.transform;

            if (tabContainer == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: TabContainer not found (container/panelBG/chatPanel/TabBar/TabContainer).");
                return;
            }

            // Default tab content (TabContent_Default)
            var defaultTabRoot = UICommon.Find(
                _uiRoot,
                "container/panelBG/chatPanel/ScrollViewArea/TabContent_Default"
            )?.transform;

            if (defaultTabRoot == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: TabContent_Default not found.");
                return;
            }

            var defaultScrollRect = defaultTabRoot.GetComponent<ScrollRect>();
            if (defaultScrollRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: TabContent_Default has no ScrollRect.");
                return;
            }

            var defaultViewport = defaultTabRoot.Find("Viewport");
            if (defaultViewport == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: Default tab has no Viewport child.");
                return;
            }

            var defaultMessageContent = defaultViewport.Find("messageContent");
            if (defaultMessageContent == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: messageContent not found under TabContent_Default/Viewport.");
                return;
            }

            var defaultTemplateTransform = defaultMessageContent.Find("messageTemplate");
            if (defaultTemplateTransform == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: messageTemplate not found under messageContent.");
                return;
            }

            var defaultTemplate = defaultTemplateTransform.GetComponent<TextMeshProUGUI>();
            if (defaultTemplate == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: messageTemplate has no TextMeshProUGUI.");
                return;
            }

            // Debouncer used for batching messages
            _debouncer = DebounceInvoker.Attach(_uiRoot);

            // Initialize tab system (for now, only default "ALL" tab)
            ChatTabManager.Initialize(
                tabContainer,
                tabButtonPrefab,
                defaultTabRoot,
                defaultMessageContent,
                defaultTemplate,
                defaultScrollRect,
                _debouncer
            );

            IsInitialized = true;
        }

        /// <summary>
        /// Persist window position (and current size) into the WindowLayoutStore.
        /// </summary>
        private static void SaveWindowPosition(Vector2 anchoredPos)
        {
            if (_containerRect == null)
                return;

            var size = _containerRect.sizeDelta;
            WindowLayoutStore.SetLayout(WindowId, anchoredPos, size);
        }

        /// <summary>
        /// Adds a message to the ShorNet chat window (current active tab).
        /// </summary>
        public static void AddMessage(string message)
        {
            if (!IsInitialized)
            {
                Plugin.Log?.LogWarning("[ShorNet] SNchatWindow.AddMessage called before initialization.");
                return;
            }

            ChatTabManager.AddMessage(message);
        }
    }
}
