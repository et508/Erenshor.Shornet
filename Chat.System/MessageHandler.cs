using HarmonyLib;
using System;

namespace ShorNet
{
    internal class MessageHandler
    {
        // =========================================================
        // Patch: TypeText.CheckCommands  (handles /shor commands)
        // Mirrors LootManager's LootCommands pattern exactly.
        // =========================================================
        [HarmonyPatch(typeof(TypeText), "CheckCommands")]
        public static class TypeText_CheckCommands_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {
                string command = GameData.TextInput.typed.text;
                if (string.IsNullOrWhiteSpace(command))
                    return true;

                if (!command.StartsWith("/shor", StringComparison.OrdinalIgnoreCase))
                    return true;

                // Make sure command registry is ready
                CommandRegistry.EnsureInitialized();

                string remainder = command.Length <= 5
                    ? string.Empty
                    : command.Substring(5).TrimStart();

                if (string.IsNullOrEmpty(remainder))
                {
                    CommandRegistry.ShowHelp();
                }
                else
                {
                    int spaceIndex = remainder.IndexOf(' ');
                    string cmd  = spaceIndex < 0 ? remainder : remainder.Substring(0, spaceIndex);
                    string args = spaceIndex < 0 ? string.Empty : remainder.Substring(spaceIndex + 1).Trim();

                    if (!CommandRegistry.TryExecute(cmd, GameData.TextInput, args))
                    {
                        ChatHandler.PushToUIAndGame(
                            "<color=purple>[SHORNET]</color> " +
                            "<color=yellow>Unknown subcommand. Try </color><color=white>/shor help</color>");
                    }
                }

                ClearInput();
                return false; // swallow from game
            }

            private static void ClearInput()
            {
                GameData.TextInput.typed.text = "";
                GameData.TextInput.CDFrames   = 10f;
                GameData.TextInput.InputBox.SetActive(false);
                GameData.PlayerTyping         = false;
            }
        }

        // =========================================================
        // Patch: TypeText.CheckInput  (handles chat routing only)
        // Intercepts normal (non-slash) text when global mode is on.
        // =========================================================
        [HarmonyPatch(typeof(TypeText), "CheckInput")]
        public static class TypeText_CheckInput_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(TypeText __instance)
            {
                if (__instance == null || __instance.typed == null)
                    return true;

                CommandRegistry.EnsureInitialized();

                string text = (__instance.typed.text ?? string.Empty).Trim();

                // Only intercept non-slash text when ShorNet mode is active.
                // Slash commands are handled by the CheckCommands patch above.
                if (CommandRegistry.Mode != CommandRegistry.ChatMode.Off &&
                    !string.IsNullOrWhiteSpace(text) &&
                    text[0] != '/')
                {
                    if (MessageHandler._suppressNextChatRoute)
                    {
                        MessageHandler._suppressNextChatRoute = false;
                        return true;
                    }

                    MessageSender.SendChatMessage(text);
                    __instance.typed.text = string.Empty;
                    __instance.CloseInputBox();
                    return false;
                }

                MessageHandler._suppressNextChatRoute = false;
                return true;
            }
        }

        // =========================================================
        // Patch: TypeText.ForceTextInput  (NPC dialog keyword clicks)
        // ForceTextInput sets typed.text to a keyword then calls
        // CheckInput immediately — we don't want that routed to ShorNet.
        // =========================================================
        [HarmonyPatch(typeof(TypeText), "ForceTextInput")]
        public static class TypeText_ForceTextInput_Patch
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                // Temporarily disable chat routing for this one CheckInput call.
                _suppressNextChatRoute = true;
            }
        }

        internal static bool _suppressNextChatRoute = false;
    }
}