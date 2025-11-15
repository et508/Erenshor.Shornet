using LiteNetLib;
using Newtonsoft.Json;
using LiteNetLib.Utils;
using BepInEx.Bootstrap;
using Steamworks;

namespace ShorNet
{
    public class MessageSender
    {
        public static void SendPackage(NetPeer peer, PackageData data)
        {
            var writer = new NetDataWriter();
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            writer.Put(JsonConvert.SerializeObject(data, settings));
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void SendChatMessage(string message)
        {
            if (Plugin.GetNetworkManager().GetPeer() == null)
            {
                ChatHandler.PushToUIAndGame("No connection to the ShorNet server. Couldn't send message.");
                return;
            }

            if (message.Length > 255)
            {
                ChatHandler.PushToUIAndGame("Message is too long. Couldn't send message.");
                return;
            }

            var data = new PackageData
            {
                SenderName = Plugin.GetSteamUsername(),
                Type       = PackageData.PackageType.ChatMessage,
                Message    = message,
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            SendPackage(Plugin.GetNetworkManager().GetPeer(), data);
        }

        public static void SendRequestForOnlinePlayers()
        {
            if (Plugin.GetNetworkManager().GetPeer() == null)
            {
                ChatHandler.PushToUIAndGame("No connection to the ShorNet server. Couldn't send message.");
                return;
            }

            var data = new PackageData
            {
                SenderName = Plugin.GetSteamUsername(),
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.PlayersOnline,
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            SendPackage(Plugin.GetNetworkManager().GetPeer(), data);
        }

        public static void SendRequestToKickPlayer(int peerId)
        {
            if (Plugin.GetNetworkManager().GetPeer() == null)
            {
                ChatHandler.PushToUIAndGame("No connection to the ShorNet server. Couldn't send command.");
                return;
            }

            var data = new PackageData
            {
                SenderName = Plugin.GetSteamUsername(),
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.KickPlayer,
                Message    = peerId.ToString(),
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            SendPackage(Plugin.GetNetworkManager().GetPeer(), data);
        }

        public static void SendRequestToBanPlayer(int peerId, int durationInSeconds)
        {
            if (Plugin.GetNetworkManager().GetPeer() == null)
            {
                UpdateSocialLog.LogAdd("No connection to the ShorNet server. Couldn't send command.");
                return;
            }

            var data = new PackageData
            {
                SenderName = Plugin.GetSteamUsername(),
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.BanPlayer,
                Message    = peerId.ToString(),
                Duration   = durationInSeconds,
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            SendPackage(Plugin.GetNetworkManager().GetPeer(), data);
        }

        public static void SendRequestToBanPlayer(int peerId)
        {
            if (Plugin.GetNetworkManager().GetPeer() == null)
            {
                UpdateSocialLog.LogAdd("No connection to the ShorNet server. Couldn't send command.");
                return;
            }

            var data = new PackageData
            {
                SenderName = Plugin.GetSteamUsername(),
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.BanPlayer,
                Message    = peerId.ToString(),
                Duration   = -1,
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            SendPackage(Plugin.GetNetworkManager().GetPeer(), data);
        }

        public static void SendRequestToUnbanPlayer(string steamId)
        {
            if (Plugin.GetNetworkManager().GetPeer() == null)
            {
                UpdateSocialLog.LogAdd("No connection to the ShorNet server. Couldn't send command.");
                return;
            }

            var data = new PackageData
            {
                SenderName = Plugin.GetSteamUsername(),
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.UnbanPlayer,
                Message    = steamId,
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            SendPackage(Plugin.GetNetworkManager().GetPeer(), data);
        }
    }
}
