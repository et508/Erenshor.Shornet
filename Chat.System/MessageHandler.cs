using HarmonyLib;
using System;

namespace ShorNet
{
    internal class MessageHandler
    {
        /// <summary>
        /// When true, the next TypeText.CheckInput call will be allowed
        /// to run completely unpatched (vanilla behavior).
        /// Used for NPC dialog keyword clicks, etc.
        /// </summary>
        internal static bool IgnoreNextCheckInput = false;

        // ==========================
        // Patch: TypeText.CheckInput
        // ==========================
        [HarmonyPatch(typeof(TypeText), "CheckInput")]
        public static class TypeText_CheckInput_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(TypeText __instance)
            {
                if (__instance == null || __instance.typed == null)
                    return true;

                // Allow one "vanilla" CheckInput (for NPC dialog clicks, etc.)
                if (IgnoreNextCheckInput)
                {
                    IgnoreNextCheckInput = false;
                    return true;
                }

                string raw  = __instance.typed.text ?? string.Empty;
                string text = raw.Trim();

                // 1) ShorNet slash commands: /shor ...
                if (text.StartsWith("/shor", StringComparison.OrdinalIgnoreCase))
                {
                    HandleShorCommand(__instance, text);
                    ResetPlayerUI(__instance);
                    return false; // don't let the base game see /shor commands
                }

                // 2) Normal text AND ShorNet chat mode is active:
                //    route this line into ShorNet instead of base game, as long
                //    as it isn't a different slash command.
                if (CommandRegistry.Mode != CommandRegistry.ChatMode.Off &&
                    !string.IsNullOrWhiteSpace(text) &&
                    text[0] != '/') // avoid intercepting game commands
                {
                    SendChatAccordingToMode(text);
                    ResetPlayerUI(__instance);
                    return false;
                }

                // 3) Otherwise, let the game handle input as usual
                return true;
            }
        }

        // =====================================
        // Patch: IDLog.OnClick (NPC dialog link)
        // =====================================
        [HarmonyPatch(typeof(IDLog), "OnClick")]
        public static class IDLog_OnClick_Patch
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                // The click handler will call GameData.TextInput.ForceTextInput(word),
                // which internally calls TypeText.CheckInput().
                // We want THAT call to behave like vanilla local chat, not ShorNet.
                IgnoreNextCheckInput = true;
            }
        }

        // ==========================
        // Core helpers
        // ==========================

        private static void ResetPlayerUI(TypeText __instance)
        {
            if (__instance == null)
                return;

            __instance.typed.text = string.Empty;
            __instance.CloseInputBox(); // lets TypeText restore scroll & layout properly
        }

        /// <summary>
        /// Sends a chat line to ShorNet based on the current mode.
        /// Right now the server treats everything as Global, but this
        /// is future-proof for Trade/etc.
        /// </summary>
        private static void SendChatAccordingToMode(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            // TODO: When channel support is wired client-side,
            // pass the channel into MessageSender based on CommandRegistry.Mode.
            MessageSender.SendChatMessage(message);
        }

        // ==========================
        // /shor command parsing
        // ==========================

        private static void HandleShorCommand(TypeText __instance, string full)
        {
            // full is like:
            // "/shor"
            // "/shor all"
            // "/shor say hello world"
            // "/shor online"
            // "/shor connect"

            string trimmed = full.Trim();

            // Strip leading "/shor"
            string remainder = trimmed.Length <= 5
                ? string.Empty
                : trimmed.Substring(5).TrimStart(); // remove "/shor" + any space

            // No subcommand → show help
            if (string.IsNullOrEmpty(remainder))
            {
                CommandRegistry.ShowHelp();
                return;
            }

            // Split into first token + rest
            int spaceIndex = remainder.IndexOf(' ');
            string cmd = (spaceIndex < 0)
                ? remainder
                : remainder.Substring(0, spaceIndex);

            string args = (spaceIndex < 0)
                ? string.Empty
                : remainder.Substring(spaceIndex + 1).Trim();

            if (CommandRegistry.TryExecute(cmd, __instance, args))
                return;

            // Unknown command
            ChatHandler.PushToUIAndGame(
                "<color=purple>[SHORNET]</color> " +
                "<color=yellow>Unknown subcommand. Try </color><color=white>/shor help</color>");
        }
    }
}
