namespace ShorNet
{
    public static class ChatHandler
    {
        public static void HandleReceivedData(PackageData data)
        {
            if (data.Type == PackageData.PackageType.ChatMessage)
            {
                Plugin.SendChatLogMessage($"<color=purple>[SHORNET]</color> {data.SenderName}: {data.Message}");
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
                    Plugin.SendChatLogMessage(
                        $"<color=purple>[SHORNET]</color> <color=yellow>{data.SenderName} has <color=green>connected</color> to ShorNet.</color>");
                    break;

                case PackageData.InformationType.PlayerDisconnected:
                    Plugin.SendChatLogMessage(
                        $"<color=purple>[SHORNET]</color> <color=yellow>{data.SenderName} has <color=red>disconnected</color> from ShorNet.</color>");
                    break;

                case PackageData.InformationType.PlayersOnline:
                    Plugin.SendChatLogMessage($"<color=purple>[SHORNET]</color> {data.Message}");
                    break;

                case PackageData.InformationType.VersionMismatch:
                    Plugin.SendChatLogMessage($"<color=purple>[SHORNET]</color> {data.Message}");
                    break;

                case PackageData.InformationType.BlacklistedMessage:
                    Plugin.SendChatLogMessage($"<color=purple>[SHORNET]</color> <color=red>{data.Message}</color>");
                    break;

                case PackageData.InformationType.MessageOfTheDay:
                    Plugin.SendChatLogMessage($"<color=purple>[SHORNET]</color> <color=yellow>{data.Message}</color>");
                    break;

                case PackageData.InformationType.KickPlayer:
                    UpdateSocialLog.LogAdd($"<color=purple>[SHORNET]</color> <color=red>{data.Message}</color>");
                    break;

                case PackageData.InformationType.BanPlayer:
                    UpdateSocialLog.LogAdd($"<color=purple>[SHORNET]</color> <color=green>{data.Message}</color>");
                    break;

                case PackageData.InformationType.UnbanPlayer:
                    UpdateSocialLog.LogAdd($"<color=purple>[SHORNET]</color> <color=green>{data.Message}</color>");
                    break;
            }
        }
    }
}
