using System;
using System.IO;
using Newtonsoft.Json;

namespace ShorNet
{
    /// <summary>
    /// Strongly-typed settings for ShorNet server connection.
    /// Stored in ShorNetSetup.ConnectionConfigPath (connection.wtf).
    /// </summary>
    public sealed class ConnectionConfig
    {
        public string ServerIP   { get; set; } = "127.0.0.1";
        public int    ServerPort { get; set; } = 27015;
    }

    /// <summary>
    /// Helper to load/save connection.wtf.
    /// Compatible with .NET Framework 4.7.2 / C# 7.3.
    /// </summary>
    public static class ConnectionConfigStore
    {
        private static readonly object _lock = new object();
        private static ConnectionConfig _cached; // no nullable refs in C# 7.3

        /// <summary>
        /// Load the connection config from disk.
        /// If missing or invalid, returns defaults and writes file.
        /// </summary>
        public static ConnectionConfig Load()
        {
            lock (_lock)
            {
                ShorNetSetup.EnsureInitialized();

                if (_cached != null)
                    return _cached;

                var path = ShorNetSetup.ConnectionConfigPath;

                try
                {
                    if (!File.Exists(path))
                    {
                        var def = new ConnectionConfig
                        {
                            ServerIP   = "165.227.186.68",
                            ServerPort = 27015
                        };

                        string jsonDefault = JsonConvert.SerializeObject(def, Formatting.Indented);
                        File.WriteAllText(path, jsonDefault);

                        _cached = def;
                        return _cached;
                    }

                    string json = File.ReadAllText(path);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        _cached = new ConnectionConfig();
                        return _cached;
                    }

                    var cfg = JsonConvert.DeserializeObject<ConnectionConfig>(json);
                    if (cfg == null)
                    {
                        _cached = new ConnectionConfig();
                        return _cached;
                    }

                    _cached = cfg;
                    return _cached;
                }
                catch (Exception ex)
                {
                    Plugin.Log?.LogError("[ConnectionConfigStore] Failed to load connection config: " + ex);
                    _cached = new ConnectionConfig();
                    return _cached;
                }
            }
        }

        /// <summary>
        /// Save new connection settings to connection.wtf.
        /// </summary>
        public static void Save(ConnectionConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            lock (_lock)
            {
                ShorNetSetup.EnsureInitialized();

                try
                {
                    var path = ShorNetSetup.ConnectionConfigPath;
                    string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(path, json);
                    _cached = config;
                }
                catch (Exception ex)
                {
                    Plugin.Log?.LogError("[ConnectionConfigStore] Failed to save connection config: " + ex);
                }
            }
        }
    }
}
