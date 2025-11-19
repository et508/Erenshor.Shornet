using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShorNet
{
    public static class SNchatWindowController
    {
        private const string WindowId = "SNchatWindow";

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
        
        private static Vector2 _initialContainerSize;
        private static Vector2 _initialPanelBGSize;
        private static Vector2 _initialMessagePanelSize;
        private static Vector2 _initialMessageViewSize;
        
        private static Vector2 _panelBGMargin;      
        private static Vector2 _messagePanelMargin; 
        private static Vector2 _messageViewMargin;  

        // 🔹 Debounce + message limit
        private static DebounceInvoker _debouncer;
        private const int MaxMessages = 500;

        // CHANGED: Use List<string> instead of Queue<string> to avoid Unity's Queue<> resolution issue
        private static readonly List<string> _pendingMessages = new List<string>();

        public static bool IsInitialized { get; private set; }

        public static void Initialize(GameObject uiRoot)
        {
            _uiRoot = uiRoot;
            
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
            
            WindowLayout savedLayout = null;
            if (WindowLayoutStore.TryGetLayout(WindowId, out var layout))
            {
                savedLayout = layout;
                Vector2 pos = new Vector2(layout.PosX, layout.PosY);
                _containerRect.anchoredPosition = pos;
                Plugin.Log?.LogInfo($"[ShorNet] Restored chat window position to {pos}.");
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
            
            _messageContent = _viewportRect.Find("messageContent");
            if (_messageContent == null)
            {
                Plugin.Log?.LogError("[ShorNet] SNchatWindow: 'messageContent' not found under messageView/Viewport.");
                return;
            }
            
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
            
            _initialContainerSize     = _containerRect.sizeDelta;
            _initialPanelBGSize       = _panelBGRect.sizeDelta;
            _initialMessagePanelSize  = _messagePanelRect.sizeDelta;
            _initialMessageViewSize   = _messageViewRect.sizeDelta;

            _panelBGMargin      = _initialContainerSize    - _initialPanelBGSize;
            _messagePanelMargin = _initialPanelBGSize      - _initialMessagePanelSize;
            _messageViewMargin  = _initialMessagePanelSize - _initialMessageViewSize;
            
            if (savedLayout != null && (savedLayout.SizeX > 0f || savedLayout.SizeY > 0f))
            {
                var savedSize = new Vector2(savedLayout.SizeX, savedLayout.SizeY);

                _containerRect.sizeDelta = savedSize;
                RefreshLayout();
                Plugin.Log?.LogInfo($"[ShorNet] Restored chat window size to {savedSize}.");
                
                var restorer = _container.AddComponent<SizeRestorer>();
                restorer.Target    = _containerRect;
                restorer.SavedSize = savedSize;
            }
            
            _dragHandle = _panelBG;
            if (_dragHandle != null && _containerRect != null)
            {
                var dh = _dragHandle.GetComponent<DragHandler>() ?? _dragHandle.AddComponent<DragHandler>();
                dh.PanelToMove  = _containerRect;      
                dh.SnapSizeRect = _messagePanelRect;   
                
                dh.OnDragFinished = SaveWindowPosition;
            }
            else
            {
                Plugin.Log?.LogWarning("[ShorNet] SNchatWindow: panelBG not usable as drag handle.");
            }
            
            _resizeHandle = UICommon.Find(_uiRoot, "container/resizeHandle")?.gameObject;
            if (_resizeHandle != null && _containerRect != null)
            {
                var rh = _resizeHandle.GetComponent<ResizeHandler>() ?? _resizeHandle.AddComponent<ResizeHandler>();
                rh.PanelToResize = _containerRect;
                
                rh.OnResizing       = OnPanelResized;
                rh.OnResizeFinished = OnPanelResized;
            }
            else
            {
                Plugin.Log?.LogWarning("[ShorNet] SNchatWindow: resizeHandle not found; window will not be resizable.");
            }

            // 🔹 Attach debounce invoker for batched UI updates
            _debouncer = DebounceInvoker.Attach(_uiRoot);

            IsInitialized = true;
        }
        
        public static void OnPanelResized(RectTransform panel)
        {
            if (!IsInitialized || panel == null)
                return;

            if (panel != _containerRect)
                return;

            RefreshLayout();

            var size = _containerRect.sizeDelta;
            var pos  = _containerRect.anchoredPosition;
            
            WindowLayoutStore.SetLayout(WindowId, pos, size);

            Plugin.Log?.LogInfo($"[ShorNet] Saved chat window size: {size} (pos {pos}).");
        }

        private static void RefreshLayout()
        {
            if (!IsInitialized)
                return;

            var containerSize = _containerRect.sizeDelta;
            
            _panelBGRect.sizeDelta      = containerSize - _panelBGMargin;
            _messagePanelRect.sizeDelta = _panelBGRect.sizeDelta - _messagePanelMargin;
            _messageViewRect.sizeDelta  = _messagePanelRect.sizeDelta - _messageViewMargin;
        }

        private static void SaveWindowPosition(Vector2 anchoredPos)
        {
            if (_containerRect == null)
                return;

            var size = _containerRect.sizeDelta;
            
            WindowLayoutStore.SetLayout(WindowId, anchoredPos, size);

            Plugin.Log?.LogInfo($"[ShorNet] Saved chat window position: {anchoredPos} (size {size}).");
        }

        // 🔹 Flush pending messages: instantiate, trim to MaxMessages, scroll to bottom
        private static void FlushPendingMessages()
        {
            if (!IsInitialized || _messageContent == null || _messageTemplate == null)
                return;

            bool addedAny = false;

            // CHANGED: Consume from List<string> as FIFO
            while (_pendingMessages.Count > 0)
            {
                var text = _pendingMessages[0];
                _pendingMessages.RemoveAt(0);

                var labelGO = Object.Instantiate(_messageTemplate.gameObject, _messageContent);
                var label   = labelGO.GetComponent<TextMeshProUGUI>();
                label.text  = text;
                labelGO.SetActive(true);

                addedAny = true;
            }

            if (!addedAny)
                return;

            // child[0] is template, real rows start at index 1
            int realRowCount = _messageContent.childCount - 1;
            int over = realRowCount - MaxMessages;
            while (over > 0 && _messageContent.childCount > 1)
            {
                var oldest = _messageContent.GetChild(1);
                Object.Destroy(oldest.gameObject);
                over--;
            }

            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public static void AddMessage(string message)
        {
            if (!IsInitialized || _messageContent == null || _messageTemplate == null)
            {
                Plugin.Log?.LogWarning("[ShorNet] SNchatWindow.AddMessage called before initialization.");
                return;
            }

            // CHANGED: List add instead of Queue.Enqueue
            _pendingMessages.Add(message);

            if (_debouncer != null)
            {
                _debouncer.Schedule(FlushPendingMessages, 0.03f);
            }
            else
            {
                FlushPendingMessages();
            }
        }
        
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
