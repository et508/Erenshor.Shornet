using HarmonyLib;
using UnityEngine;

namespace ShorNet
{
    /// <summary>
    /// Manages ShorNet's chat filter mask on the target IDLog window.
    /// Identical pattern to LootManager's ChatFilterInjector:
    /// strip our flag from all tabs, apply it only to the configured tab.
    /// Re-applies after each IDLog.Start so scene reloads stay correct.
    /// </summary>
    [HarmonyPatch(typeof(IDLog), "Start")]
    public static class ChatFilterInjector
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            ApplyChatMask();
        }

        public static void ApplyChatMask()
        {
            // Strip our flag from every window/tab (clean slate).
            foreach (var win in UpdateSocialLog.ChatWindows)
            {
                for (int t = 0; t < win.FilterMasks.Length; t++)
                    win.FilterMasks[t] &= ~(ChatLogLine.LogType)ChatHandler.ShorNetLogTypeFlag;
            }

            var target = GetTargetWindow(ShorNetSettings.ChatOutputWindow);
            if (target == null) return;

            int tab = Mathf.Clamp(ShorNetSettings.ChatOutputTab, 0, target.activeTabs - 1);
            target.FilterMasks[tab] |= (ChatLogLine.LogType)ChatHandler.ShorNetLogTypeFlag;
        }

        public static IDLog GetTargetWindow(string windowName)
        {
            foreach (var win in UpdateSocialLog.ChatWindows)
            {
                if (win.WindowName == windowName) return win;
            }
            if (UpdateSocialLog.ChatWindows.Count > 0)
                return UpdateSocialLog.ChatWindows[0];
            return null;
        }
    }
}