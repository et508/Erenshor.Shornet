using System;
using System.Collections.Generic;
using System.Text;

namespace ShorNet
{
    internal static class CommandRegistry
    {
        internal enum ChatMode
        {
            Off    = 0,
            Global = 1,
            Trade  = 2
        }

        internal static ChatMode Mode { get; private set; } = ChatMode.Off;

        internal delegate void ShorCommandHandler(TypeText context, string args);

        private static readonly Dictionary<string, ShorCommandHandler> _handlers =
            new Dictionary<string, ShorCommandHandler>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> _descriptions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static CommandRegistry()
        {
            // Core chat routing
            RegisterCommand(
                "global",
                (ctx, args) => SetMode(ChatMode.Global),
                "Route normal chat into ShorNet [GLOBAL]."
            );

            RegisterCommand(
                "trade",
                (ctx, args) => SetMode(ChatMode.Trade),
                "Route normal chat into ShorNet [TRADE]. (future)"
            );

            RegisterCommand(
                "off",
                (ctx, args) => SetMode(ChatMode.Off),
                "Stop sending normal chat to ShorNet."
            );

            RegisterCommand(
                "none",
                (ctx, args) => SetMode(ChatMode.Off),
                "Alias for /shor off."
            );

            // One-off send
            RegisterCommand(
                "say",
                HandleSay,
                "Send one message to ShorNet without changing mode."
            );

            // Info / connectivity
            RegisterCommand(
                "online",
                (ctx, args) => HandleOnline(),
                "Request list of players online."
            );

            RegisterCommand(
                "connect",
                (ctx, args) => HandleConnect(),
                "Connect/reconnect to ShorNet."
            );

            // Help
            RegisterCommand(
                "help",
                (ctx, args) => ShowHelp(),
                "Show ShorNet command help."
            );
        }

        internal static void RegisterCommand(string name, ShorCommandHandler handler, string description)
        {
            if (string.IsNullOrWhiteSpace(name) || handler == null)
                return;

            _handlers[name]     = handler;
            _descriptions[name] = description ?? string.Empty;
        }

        internal static bool TryExecute(string name, TypeText context, string args)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            ShorCommandHandler handler;
            if (_handlers.TryGetValue(name, out handler))
            {
                handler(context, args);
                return true;
            }

            return false;
        }

        internal static void ShowHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<color=purple>[SHORNET]</color> <color=yellow>Commands:</color>");

            foreach (var kvp in _descriptions)
            {
                var cmd  = kvp.Key;
                var desc = kvp.Value;
                sb.AppendLine($"<color=white>/shor {cmd}</color> - {desc}");
            }

            ChatHandler.PushToUIAndGame(sb.ToString());
        }

        private static void SetMode(ChatMode mode)
        {
            Mode = mode;

            switch (mode)
            {
                case ChatMode.Global:
                    ChatHandler.PushToUIAndGame(
                        "<color=purple>[SHORNET]</color> " +
                        "<color=yellow>Normal chat will now be sent to ShorNet [GLOBAL].</color>");
                    break;

                case ChatMode.Trade:
                    ChatHandler.PushToUIAndGame(
                        "<color=purple>[SHORNET]</color> " +
                        "<color=yellow>Normal chat will now be sent to ShorNet [TRADE]. (Channel support WIP)</color>");
                    break;

                case ChatMode.Off:
                default:
                    ChatHandler.PushToUIAndGame(
                        "<color=purple>[SHORNET]</color> " +
                        "<color=yellow>ShorNet chat routing disabled. Game chat restored.</color>");
                    break;
            }
        }

        private static void HandleSay(TypeText ctx, string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ChatHandler.PushToUIAndGame(
                    "<color=purple>[SHORNET]</color> " +
                    "<color=red>Usage: /shor say &lt;message&gt;</color>");
                return;
            }

            MessageSender.SendChatMessage(args);
        }

        private static void HandleOnline()
        {
            MessageSender.SendRequestForOnlinePlayers();
        }

        private static void HandleConnect()
        {
            Plugin.GetNetworkManager().ConnectToGlobalServer();
        }
    }
}
