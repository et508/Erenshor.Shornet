using LiteNetLib;
using BepInEx.Logging;
using Newtonsoft.Json;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine;
using Steamworks;

namespace ShorNet
{
    public class NetworkManager : MonoBehaviour
    {
        private NetManager _netManager;
        private static NetPeer _serverPeer;
        private EventBasedNetListener _listener;
        private ManualLogSource _logger;
        private string _steamUsername;

        public bool IsConnected => _serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected;

        public NetPeer GetPeer()
        {
            return _serverPeer;
        }

        public void Init(ManualLogSource logger, string steamUsername, PluginInfo pluginInfo)
        {
            _logger = logger;
            _steamUsername = steamUsername;
            _listener = new EventBasedNetListener();
            _netManager = new NetManager(_listener);

            _listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
            {
                OnNetworkReceive(peer, reader, deliveryMethod);
            };
            _listener.PeerConnectedEvent += OnPeerConnected;
        }

        public void ConnectToGlobalServer()
        {
            _netManager.Start();
            _serverPeer = _netManager.Connect(
                ConfigGenerator._serverIp.Value,
                ConfigGenerator._serverPort.Value,
                "ShorNetOnlineChat");

            int attempts = 0;
            while (_serverPeer.ConnectionState != ConnectionState.Connected && attempts < 5)
            {
                _logger.LogMessage("Connecting to the ShorNet server...");
                attempts++;
                System.Threading.Thread.Sleep(1000);
            }

            if (_serverPeer.ConnectionState == ConnectionState.Connected)
            {
                ChatHandler.PushToUIAndGame("<color=purple>[SHORNET]</color> <color=green>Successfully connected to the ShorNet server.</color>");
                _logger.LogMessage("Successfully connected to the ShorNet server.");
            }
            else
            {
                _logger.LogMessage("Connecting to the ShorNet server has failed.");
            }
        }

        public void Update()
        {
            if (_serverPeer == null || _netManager == null) return;
            _netManager.PollEvents();

            if (_serverPeer.ConnectionState == ConnectionState.Disconnected)
            {
                _logger.LogError("Lost connection to the ShorNet server.");
                ChatHandler.PushToUIAndGame("<color=purple>[SHORNET]</color> <color=red>Lost connection to the ShorNet server.</color>");
                _serverPeer = null;
            }
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            string message = reader.GetString();
            var data = JsonConvert.DeserializeObject<PackageData>(message);
            ChatHandler.HandleReceivedData(data);
        }

        private void OnPeerConnected(NetPeer peer)
        {
            SendConnectedPackage();
        }

        public void SendConnectedPackage()
        {
            string modVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString();
            string modHash = Plugin.GetModHash();
            string steamId = SteamUser.GetSteamID().ToString();

            var data = new PackageData
            {
                Type = PackageData.PackageType.Information,
                Info = PackageData.InformationType.PlayerConnected,
                SenderName = _steamUsername,
                ModVersion = modVersion,
                ModHash = modHash,
                SteamId = steamId
                // No Signature
            };
            MessageSender.SendPackage(_serverPeer, data);
        }

        public void SendDisconnectedPackage()
        {
            if (_serverPeer == null) return;

            string modVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString();
            string modHash = Plugin.GetModHash();
            string steamId = SteamUser.GetSteamID().ToString();

            var data = new PackageData
            {
                Type = PackageData.PackageType.Information,
                Info = PackageData.InformationType.PlayerDisconnected,
                SenderName = _steamUsername,
                ModVersion = modVersion,
                ModHash = modHash,
                SteamId = steamId
                // No Signature
            };
            MessageSender.SendPackage(_serverPeer, data);
            _serverPeer.Disconnect();
            _serverPeer = null;
        }
    }
}
