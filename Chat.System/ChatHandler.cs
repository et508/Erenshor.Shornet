using Erenshor.ShorNet.Protocol;

namespace ShorNet
{
    public static class ChatHandler
    {
        public static void HandleReceivedData(PackageData data)
        {
            if (data.Type == PackageData.PackageType.ChatMessage)
            {
                string timestamp    = GetTimestamp();
                string channelLabel = GetChannelLabel(data.Channel);
                string colorHex     = GetChannelColorHex(data.Channel);

                string msg =
                    $"<color=#ADADAD>[{timestamp}]</color> <color={colorHex}>{channelLabel} [{data.SenderName}]: {data.Message}</color>";

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
                    string msg = $"<color=purple>[SHORNET]</color> <color=red>{data.Message}</color>";
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
            }
        }

        /// <summary>
        /// The ShorNet-exclusive log type flag.
        /// Uses bit 30 (2^30 = 1073741824) to avoid collision with the game's defined
        /// ChatLogLine.LogType values (highest is 2^22) and other mods grabbing nearby slots.
        /// </summary>
        public const int ShorNetLogTypeFlag = 1073741824; // 2^30

        /// <summary>
        /// Sends a ShorNet message to the game's social log using the ShorNet-exclusive
        /// log type flag so it routes only to the ShorNet tab filter.
        /// </summary>
        public static void PushToUIAndGame(string msg)
        {
            UpdateSocialLog.LogAdd(new ChatLogLine(msg, (ChatLogLine.LogType)ShorNetLogTypeFlag, ""));
        }

        private static string GetTimestamp()
        {
            return System.DateTime.Now.ToString("HH:mm");
        }

        private static string GetChannelLabel(PackageData.ChatChannel channel)
        {
            switch (channel)
            {
                case PackageData.ChatChannel.Trade:
                    return "[TRADE]";

                case PackageData.ChatChannel.Global:
                default:
                    return "[GLOBAL]";
            }
        }

        private static string GetChannelColorHex(PackageData.ChatChannel channel)
        {
            switch (channel)
            {
                case PackageData.ChatChannel.Trade:
                    return "#FFA500";

                case PackageData.ChatChannel.Global:
                default:
                    return "#8AAFFF";
            }
        }
    }
}