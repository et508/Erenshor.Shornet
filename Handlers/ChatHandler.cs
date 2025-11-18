namespace ShorNet
{
    public static class ChatHandler
    {
        public static void HandleReceivedData(PackageData data)
        {
            if (data.Type == PackageData.PackageType.ChatMessage)
            {
                // Timestamp + channel-aware coloring
                string timestamp    = GetTimestamp();
                string channelLabel = GetChannelLabel(data.Channel);
                string colorHex     = GetChannelColorHex(data.Channel);

                // Entire line tinted by channel color
                string msg =
                    $"<color={colorHex}>[{timestamp}] {channelLabel} [{data.SenderName}]: {data.Message}</color>";

                PushToUIAndGame(msg);
            }
            else if (data.Type == PackageData.PackageType.Information)
            {
                HandleInformation(data);
            }
        }

        private static void HandleInformation(PackageData data)
        {
            switch (data.Info)
            {
                case PackageData.InformationType.PlayerConnected:
                {
                    string msg =
                        $"<color=purple>[SHORNET]</color> <color=yellow>{data.SenderName} has <color=green>connected</color> to ShorNet.</color>";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.PlayerDisconnected:
                {
                    string msg =
                        $"<color=purple>[SHORNET]</color> <color=yellow>{data.SenderName} has <color=red>disconnected</color> from ShorNet.</color>";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.PlayersOnline:
                {
                    string msg = $"<color=purple>[SHORNET]</color> {data.Message}";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.VersionMismatch:
                {
                    string msg = $"<color=purple>[SHORNET]</color> {data.Message}";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.BlacklistedMessage:
                {
                    string msg = $"<color=purple>[SHORNET]</color> <color=red>{data.Message}</color>";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.MessageOfTheDay:
                {
                    string msg = $"<color=purple>[SHORNET]</color> <color=yellow>{data.Message}</color>";
                    PushToUIAndGame(msg);
                    break;
                }

                case PackageData.InformationType.KickPlayer:
                {
                    string msg = $"<color=purple>[SHORNET]</color> <color=red>{data.Message}</color>";
                    SNchatWindowController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }

                case PackageData.InformationType.BanPlayer:
                {
                    string msg = $"<color=purple>[SHORNET]</color> <color=green>{data.Message}</color>";
                    SNchatWindowController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }

                case PackageData.InformationType.UnbanPlayer:
                {
                    string msg = $"<color=purple>[SHORNET]</color> <color=green>{data.Message}</color>";
                    SNchatWindowController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }
            }
        }

        // 🔹 Simple timestamp: [HH:MM]
        private static string GetTimestamp()
        {
            return System.DateTime.Now.ToString("HH:mm");
        }

        // 🔹 Channel label: [ALL], [TRADE], etc.
        private static string GetChannelLabel(PackageData.ChatChannel channel)
        {
            switch (channel)
            {
                case PackageData.ChatChannel.Trade:
                    return "[TRADE]";

                case PackageData.ChatChannel.All:
                default:
                    return "[ALL]";
            }
        }

        // 🔹 Channel color used for the entire line
        private static string GetChannelColorHex(PackageData.ChatChannel channel)
        {
            switch (channel)
            {
                case PackageData.ChatChannel.Trade:
                    // Orange-ish
                    return "#FFA500";

                case PackageData.ChatChannel.All:
                default:
                    // Soft blue (same as before)
                    return "#8AAFFF";
            }
        }

        public static void PushToUIAndGame(string msg)
        {
            if (ConfigGenerator._enablePrintInChatWindow.Value)
            {
                SendChatLogMessage(msg);
            }
            else
            {
                SNchatWindowController.AddMessage(msg);
            }
        }
        
        public static void SendChatLogMessage(string message)
        {
            UpdateSocialLog.LogAdd(message);
        }
    }
}
