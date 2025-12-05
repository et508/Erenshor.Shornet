using System;
using System.Text;
using LiteNetLib;
using LiteNetLib.Utils;
using BepInEx.Logging;
using Newtonsoft.Json;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine;
using Steamworks;
using Erenshor.ShorNet.Protocol;

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
            _logger        = logger;
            _steamUsername = steamUsername;

            _listener   = new EventBasedNetListener();
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
                _netManager.PollEvents();
            }

            if (_serverPeer.ConnectionState == ConnectionState.Connected)
            {
                _logger.LogMessage("Connection to ShorNet server established; waiting for validation response...");
            }
            else
            {
                ChatHandler.PushToUIAndGame("<color=purple>[SHORNET]</color> <color=red>Unable to connect to the ShorNet server.</color>");
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

        /// <summary>
        /// Server currently sends pure UTF-8 JSON bytes (no header).
        /// We read the whole payload and deserialize into PackageData.
        /// </summary>
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            try
            {
                int available = reader.AvailableBytes;
                if (available <= 0)
                {
                    _logger?.LogWarning("Received empty packet from ShorNet server, ignoring.");
                    return;
                }

                // Read ALL remaining bytes as the JSON payload (no subsystem/opcode header yet)
                byte[] payloadBytes = reader.GetRemainingBytes();

                string message;
                try
                {
                    message = Encoding.UTF8.GetString(payloadBytes, 0, payloadBytes.Length);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Failed to decode UTF-8 payload (len={payloadBytes.Length}): {ex}");
                    return;
                }

                PackageData data;
                try
                {
                    data = JsonConvert.DeserializeObject<PackageData>(message);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Failed to deserialize PackageData JSON: {ex}\nJSON: {message}");
                    return;
                }

                ChatHandler.HandleReceivedData(data);
            }
            finally
            {
                // Always recycle the reader back to LiteNetLib pool
                reader.Recycle();
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            SendConnectedPackage();
        }

        public void SendConnectedPackage()
        {
            string modVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString();
            string modHash    = Plugin.GetModHash();
            string steamId    = SteamUser.GetSteamID().ToString();

            var data = new PackageData
            {
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.PlayerConnected,
                SenderName = _steamUsername,
                ModVersion = modVersion,
                ModHash    = modHash,
                SteamId    = steamId
            };
            MessageSender.SendPackage(_serverPeer, data);
        }

        public void SendDisconnectedPackage()
        {
            if (_serverPeer == null) return;

            string modVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString();
            string modHash    = Plugin.GetModHash();
            string steamId    = SteamUser.GetSteamID().ToString();

            var data = new PackageData
            {
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.PlayerDisconnected,
                SenderName = _steamUsername,
                ModVersion = modVersion,
                ModHash    = modHash,
                SteamId    = steamId
            };
            MessageSender.SendPackage(_serverPeer, data);
            _serverPeer.Disconnect();
            _serverPeer = null;
        }
    }
}
