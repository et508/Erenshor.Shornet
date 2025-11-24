using LiteNetLib;
using Newtonsoft.Json;
using LiteNetLib.Utils;
using BepInEx.Bootstrap;
using Steamworks;

namespace ShorNet
{
    public static class MessageSender
    {
        public static void SendPackage(NetPeer peer, PackageData data)
        {
            if (peer == null)
                return;

            var writer   = new NetDataWriter();
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            writer.Put(JsonConvert.SerializeObject(data, settings));
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        // ----------------------------------------------------
        //  Chat Message
        // ----------------------------------------------------
        public static void SendChatMessage(string message)
        {
            var peer = Plugin.GetNetworkManager().GetPeer();
            if (peer == null)
            {
                ChatHandler.PushToUIAndGame("No connection to the ShorNet server. Couldn't send message.");
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
                return;

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
                Channel    = PackageData.ChatChannel.All,
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            SendPackage(peer, data);
        }

        // ----------------------------------------------------
        //  /shor online
        // ----------------------------------------------------
        public static void SendRequestForOnlinePlayers()
        {
            var peer = Plugin.GetNetworkManager().GetPeer();
            if (peer == null)
            {
                ChatHandler.PushToUIAndGame("No connection to the ShorNet server. Couldn't send request.");
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

            SendPackage(peer, data);
        }
    }
}
