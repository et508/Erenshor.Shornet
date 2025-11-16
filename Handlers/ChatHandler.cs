using System;

namespace ShorNet
{
    public static class ChatHandler
    {
        public static void HandleReceivedData(PackageData data)
        {
            if (data.Type == PackageData.PackageType.ChatMessage)
            {
                // 🔹 Channel-aware tag for future multichannel support
                string channelTag = GetChannelTag(data.Channel);
                // No [SHORNET] prefix for normal chat messages
                string msg = $"{channelTag}{data.SenderName}: {data.Message}";
                PushToUIAndGame(msg);
            }
            else if (data.Type == PackageData.PackageType.Information)
            {
                HandleInformation(data);
            }
        }

        private static void HandleInformation(PackageData data)
        {
            // 🔹 Only show [SHORNET] for server-originated messages
            string prefix = data.SenderName == "[SERVER]"
                ? "<color=purple>[SHORNET]</color> "
                : string.Empty;

            switch (data.Info)
            {
                case PackageData.InformationType.PlayerConnected:
                {
                    string msg =
                        $"{prefix}<color=yellow>{data.SenderName} has <color=green>connected</color> to ShorNet.</color>";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.PlayerDisconnected:
                {
                    string msg =
                        $"{prefix}<color=yellow>{data.SenderName} has <color=red>disconnected</color> from ShorNet.</color>";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.PlayersOnline:
                {
                    string msg = $"{prefix}{data.Message}";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.VersionMismatch:
                {
                    string msg = $"{prefix}{data.Message}";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.BlacklistedMessage:
                {
                    string msg = $"{prefix}<color=red>{data.Message}</color>";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.MessageOfTheDay:
                {
                    string msg = $"{prefix}<color=yellow>{data.Message}</color>";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.KickPlayer:
                {
                    string msg = $"{prefix}<color=red>{data.Message}</color>";
                    msg = WithTimestamp(msg);
                    NetUIController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }

                case PackageData.InformationType.BanPlayer:
                {
                    string msg = $"{prefix}<color=green>{data.Message}</color>";
                    msg = WithTimestamp(msg);
                    NetUIController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }

                case PackageData.InformationType.UnbanPlayer:
                {
                    string msg = $"{prefix}<color=green>{data.Message}</color>";
                    msg = WithTimestamp(msg);
                    NetUIController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }
            }
        }

        // 🔹 Channel label helper
        private static string GetChannelTag(PackageData.ChatChannel channel)
        {
            switch (channel)
            {
                case PackageData.ChatChannel.Trade:
                    // Future-proof: this will show once we actually route /trade messages
                    return "<color=#FFA500>[Trade]</color> ";

                case PackageData.ChatChannel.All:
                default:
                    return "<color=#8AAFFF>[All]</color> ";
            }
        }

        public static void PushToUIAndGame(string msg)
        {
            string stamped = WithTimestamp(msg);

            if (ConfigGenerator._enablePrintInChatWindow.Value)
            {
                SendChatLogMessage(stamped);
            }
            else
            {
                NetUIController.AddMessage(stamped);
            }
        }
        
        public static void SendChatLogMessage(string message)
        {
            UpdateSocialLog.LogAdd(message);
        }

        /// <summary>
        /// Prepends a timestamp like [12:41] in a soft grey so it doesn't fight the channel colors.
        /// </summary>
        private static string WithTimestamp(string inner)
        {
            var now = DateTime.Now;
            return $"<color=#888888>[{now:HH:mm}]</color> {inner}";
        }
    }
}
