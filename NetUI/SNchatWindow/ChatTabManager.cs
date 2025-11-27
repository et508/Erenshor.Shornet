using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShorNet
{
    /// <summary>
    /// Manages ShorNet chat tabs and their content areas.
    /// For now, only a single default "GLOBAL" tab is created and used.
    /// </summary>
    public static class ChatTabManager
    {
        private class ChatTab
        {
            public string Id;
            public Transform ContentRoot;
            public Transform MessageContent;
            public TextMeshProUGUI MessageTemplate;
            public ScrollRect ScrollRect;
            public readonly List<string> PendingMessages = new List<string>();
        }

        private static Transform _tabContainer;
        private static GameObject _tabButtonPrefab;
        private static DebounceInvoker _debouncer;

        private static readonly Dictionary<string, ChatTab> _tabs = new Dictionary<string, ChatTab>();
        private static ChatTab _activeTab;

        private const int MaxMessages = 500;

        /// <summary>
        /// Default tab ID is now "global"
        /// </summary>
        private const string DefaultTabId = "global";

        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Initialize the tab manager with the default GLOBAL tab.
        /// </summary>
        public static void Initialize(
            Transform tabContainer,
            GameObject tabButtonPrefab,
            Transform defaultContentRoot,
            Transform defaultMessageContent,
            TextMeshProUGUI defaultTemplate,
            ScrollRect defaultScrollRect,
            DebounceInvoker debouncer)
        {
            IsInitialized = false;
            _tabs.Clear();
            _activeTab = null;

            _tabContainer = tabContainer;
            _tabButtonPrefab = tabButtonPrefab;
            _debouncer = debouncer;

            if (_tabContainer == null || _tabButtonPrefab == null)
            {
                Plugin.Log?.LogError("[ShorNet] ChatTabManager.Initialize: Missing tabContainer or tabButtonPrefab.");
                return;
            }

            if (defaultContentRoot == null ||
                defaultMessageContent == null ||
                defaultTemplate == null ||
                defaultScrollRect == null)
            {
                Plugin.Log?.LogError("[ShorNet] ChatTabManager.Initialize: Default tab components missing.");
                return;
            }

            // Ensure the template is disabled so clones can be created correctly.
            defaultTemplate.gameObject.SetActive(false);

            // Create the default GLOBAL tab.
            var globalTab = new ChatTab
            {
                Id = DefaultTabId,
                ContentRoot = defaultContentRoot,
                MessageContent = defaultMessageContent,
                MessageTemplate = defaultTemplate,
                ScrollRect = defaultScrollRect
            };

            _tabs[globalTab.Id] = globalTab;
            _activeTab = globalTab;

            // Visible label is now "Global"
            CreateTabButton(globalTab, "Global");

            IsInitialized = true;
        }

        /// <summary>
        /// Adds a message to the currently active tab.
        /// Currently only the GLOBAL tab exists.
        /// </summary>
        public static void AddMessage(string message)
        {
            if (!IsInitialized || string.IsNullOrEmpty(message))
                return;

            if (_activeTab == null)
            {
                if (_tabs.TryGetValue(DefaultTabId, out var recovered))
                {
                    _activeTab = recovered;
                }
                else
                {
                    Plugin.Log?.LogWarning("[ShorNet] ChatTabManager.AddMessage: No active tab.");
                    return;
                }
            }

            _activeTab.PendingMessages.Add(message);

            if (_debouncer != null)
            {
                var tab = _activeTab;
                _debouncer.Schedule(() => FlushPendingForTab(tab), 0.03f);
            }
            else
            {
                FlushPendingForTab(_activeTab);
            }
        }

        public static void SetActiveTab(string tabId)
        {
            if (!IsInitialized)
                return;

            if (!_tabs.TryGetValue(tabId, out var tab))
            {
                Plugin.Log?.LogWarning($"[ShorNet] ChatTabManager.SetActiveTab: Unknown tab '{tabId}'.");
                return;
            }

            _activeTab = tab;
        }

        // ==========================
        // Internal helpers
        // ==========================

        private static void FlushPendingForTab(ChatTab tab)
        {
            if (tab == null ||
                tab.MessageContent == null ||
                tab.MessageTemplate == null)
                return;

            if (tab.PendingMessages.Count == 0)
                return;

            bool addedAny = false;

            while (tab.PendingMessages.Count > 0)
            {
                var text = tab.PendingMessages[0];
                tab.PendingMessages.RemoveAt(0);

                var labelGO = Object.Instantiate(tab.MessageTemplate.gameObject, tab.MessageContent);
                var label = labelGO.GetComponent<TextMeshProUGUI>();
                label.text = text;
                labelGO.SetActive(true);

                addedAny = true;
            }

            if (!addedAny)
                return;

            // First child = template, others = messages
            int realRowCount = tab.MessageContent.childCount - 1;
            int over = realRowCount - MaxMessages;

            while (over > 0 && tab.MessageContent.childCount > 1)
            {
                var oldest = tab.MessageContent.GetChild(1);
                Object.Destroy(oldest.gameObject);
                over--;
            }

            if (tab.ScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                tab.ScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private static void CreateTabButton(ChatTab tab, string labelText)
        {
            if (_tabContainer == null || _tabButtonPrefab == null || tab == null)
                return;

            var tabGO = Object.Instantiate(_tabButtonPrefab, _tabContainer);
            tabGO.name = $"Tab_{labelText}";

            var label = tabGO.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = labelText;

            var button = tabGO.GetComponent<Button>();
            if (button != null)
            {
                string capturedId = tab.Id;
                button.onClick.AddListener(() => SetActiveTab(capturedId));
            }
        }
    }
}
