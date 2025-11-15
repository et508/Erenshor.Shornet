namespace ShorNet
{
    public static class ChatHandler
    {
        public static void HandleReceivedData(PackageData data)
        {
            if (data.Type == PackageData.PackageType.ChatMessage)
            {
                string msg = $"<color=purple>[SHORNET]</color> {data.SenderName}: {data.Message}";
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
                    NetUIController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }

                case PackageData.InformationType.BanPlayer:
                {
                    string msg = $"<color=purple>[SHORNET]</color> <color=green>{data.Message}</color>";
                    NetUIController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }

                case PackageData.InformationType.UnbanPlayer:
                {
                    string msg = $"<color=purple>[SHORNET]</color> <color=green>{data.Message}</color>";
                    NetUIController.AddMessage(msg);
                    UpdateSocialLog.LogAdd(msg);
                    break;
                }
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
                NetUIController.AddMessage(msg);
            }
        }
        
        public static void SendChatLogMessage(string message)
        {
            UpdateSocialLog.LogAdd(message);
        }
    }
}
