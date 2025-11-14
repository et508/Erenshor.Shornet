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
        private enum ShorNetConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

        private NetManager _netManager;
        private static NetPeer _serverPeer;
        private EventBasedNetListener _listener;
        private ManualLogSource _logger;
        private string _steamUsername;

        // Connection state tracking
        private ShorNetConnectionState _state = ShorNetConnectionState.Disconnected;

        // Simple connection timeout so we can report failure correctly
        private const float ConnectTimeoutSeconds = 10f;
        private float _connectTimer;

        public bool IsConnected =>
            _serverPeer != null &&
            _serverPeer.ConnectionState == ConnectionState.Connected &&
            _state == ShorNetConnectionState.Connected;

        public NetPeer GetPeer()
        {
            return _serverPeer;
        }

        public void Init(ManualLogSource logger, string steamUsername, PluginInfo pluginInfo)
        {
            _logger = logger;
            _steamUsername = steamUsername;

            _listener = new EventBasedNetListener();
            _netManager = new NetManager(_listener)
            {
                IPv6Enabled = false,
                NatPunchEnabled = false,
                DisconnectTimeout = 15000
            };

            _listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
            {
                OnNetworkReceive(peer, reader, deliveryMethod);
            };

            _listener.PeerConnectedEvent += OnPeerConnected;
        }

        /// <summary>
        /// Starts a non-blocking connection attempt to the ShorNet server.
        /// Actual success/failure is handled via events and Update().
        /// </summary>
        public void ConnectToGlobalServer()
        {
            if (_netManager == null)
            {
                _logger.LogError("[ShorNet] NetManager is not initialized; cannot connect.");
                return;
            }

            if (_state == ShorNetConnectionState.Connected && IsConnected)
            {
                _logger.LogMessage("[ShorNet] Already connected to the ShorNet server.");
                return;
            }

            if (_state == ShorNetConnectionState.Connecting)
            {
                _logger.LogMessage("[ShorNet] Already attempting to connect to the ShorNet server.");
                return;
            }

            if (!_netManager.IsRunning)
            {
                _netManager.Start();
            }

            _logger.LogMessage("[ShorNet] Connecting to the ShorNet server...");
            _serverPeer = _netManager.Connect(
                ConfigGenerator._serverIp.Value,
                ConfigGenerator._serverPort.Value,
                "ShorNet"
            );

            _state = ShorNetConnectionState.Connecting;
            _connectTimer = ConnectTimeoutSeconds;
        }

        private void Update()
        {
            if (_netManager == null)
                return;

            _netManager.PollEvents();

            // Handle connection timeout while in Connecting state
            if (_state == ShorNetConnectionState.Connecting)
            {
                _connectTimer -= Time.deltaTime;

                if (_serverPeer == null ||
                    _serverPeer.ConnectionState == ConnectionState.Disconnected ||
                    _connectTimer <= 0f)
                {
                    _logger.LogMessage("[ShorNet] Connection attempt to ShorNet server has failed or timed out.");
                    Plugin.SendChatLogMessage("<color=purple>[SHORNET]</color> <color=red>Could not connect to the ShorNet server.</color>");

                    _state = ShorNetConnectionState.Disconnected;
                    _serverPeer = null;
                }
            }

            // Detect lost connection after being fully connected
            if (_state == ShorNetConnectionState.Connected)
            {
                if (_serverPeer == null || _serverPeer.ConnectionState == ConnectionState.Disconnected)
                {
                    _logger.LogError("[ShorNet] Lost connection to the ShorNet server.");
                    Plugin.SendChatLogMessage("<color=purple>[SHORNET]</color> <color=red>Lost connection to the ShorNet server.</color>");

                    _state = ShorNetConnectionState.Disconnected;
                    _serverPeer = null;
                }
            }
        }

        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            string message = reader.GetString();
            var data = JsonConvert.DeserializeObject<PackageData>(message);
            ChatHandler.HandleReceivedData(data);
        }

        private void OnPeerConnected(NetPeer peer)
        {
            _state = ShorNetConnectionState.Connected;

            _logger.LogMessage("[ShorNet] Successfully connected to the ShorNet server.");
            Plugin.SendChatLogMessage("<color=purple>[SHORNET]</color> <color=green>Successfully connected to the ShorNet server.</color>");

            SendConnectedPackage();
        }

        public void SendConnectedPackage()
        {
            if (_serverPeer == null)
                return;

            var data = new PackageData
            {
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.PlayerConnected,
                SenderName = _steamUsername,
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            MessageSender.SendPackage(_serverPeer, data);
        }

        /// <summary>
        /// Graceful disconnect called by Plugin when leaving valid scenes or quitting.
        /// </summary>
        public void SendDisconnectedPackage()
        {
            if (_serverPeer == null)
            {
                _state = ShorNetConnectionState.Disconnected;
                return;
            }

            var data = new PackageData
            {
                Type       = PackageData.PackageType.Information,
                Info       = PackageData.InformationType.PlayerDisconnected,
                SenderName = _steamUsername,
                ModVersion = Chainloader.PluginInfos["et508.erenshor.shornet"].Metadata.Version.ToString(),
                ModHash    = Plugin.GetModHash(),
                SteamId    = SteamUser.GetSteamID().ToString()
            };

            MessageSender.SendPackage(_serverPeer, data);

            _serverPeer.Disconnect();
            _serverPeer = null;
            _state = ShorNetConnectionState.Disconnected;
        }
    }
}
