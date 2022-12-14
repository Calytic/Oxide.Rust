using Facepunch;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Globalization;
using System.IO;
using System.Net;

namespace Oxide.Game.Rust.Libraries.Covalence
{
    /// <summary>
    /// Represents the server hosting the game instance
    /// </summary>
    public class RustServer : IServer
    {
        #region Initialiation

        internal readonly Server Server = new Server();

        #endregion Initialiation

        #region Information

        /// <summary>
        /// Gets/sets the public-facing name of the server
        /// </summary>
        public string Name
        {
            get => ConVar.Server.hostname;
            set => ConVar.Server.hostname = value;
        }

        private static IPAddress address;

        /// <summary>
        /// Gets the public-facing IP address of the server, if known
        /// </summary>
        public IPAddress Address
        {
            get
            {
                try
                {
                    if (address == null)
                    {
                        WebClient webClient = new WebClient();
                        IPAddress.TryParse(webClient.DownloadString("http://api.ipify.org"), out address);
                        return address;
                    }

                    return address;
                }
                catch (Exception ex)
                {
                    RemoteLogger.Exception("Couldn't get server IP address", ex);
                    return new IPAddress(0);
                }
            }
        }

        /// <summary>
        /// Gets the public-facing network port of the server, if known
        /// </summary>
        public ushort Port => (ushort)ConVar.Server.port;

        /// <summary>
        /// Gets the version or build number of the server
        /// </summary>
        public string Version => BuildInfo.Current.Build.Number;

        /// <summary>
        /// Gets the network protocol version of the server
        /// </summary>
        public string Protocol => global::Rust.Protocol.printable;

        /// <summary>
        /// Gets the language set by the server
        /// </summary>
        public CultureInfo Language => CultureInfo.InstalledUICulture;

        /// <summary>
        /// Gets the total of players currently on the server
        /// </summary>
        public int Players => BasePlayer.activePlayerList.Count;

        /// <summary>
        /// Gets/sets the maximum players allowed on the server
        /// </summary>
        public int MaxPlayers
        {
            get => ConVar.Server.maxplayers;
            set => ConVar.Server.maxplayers = value;
        }

        /// <summary>
        /// Gets/sets the current in-game time on the server
        /// </summary>
        public DateTime Time
        {
            get => TOD_Sky.Instance.Cycle.DateTime;
            set => TOD_Sky.Instance.Cycle.DateTime = value;
        }

        /// <summary>
        /// Gets information on the currently loaded save file
        /// </summary>
        public SaveInfo SaveInfo { get; } = SaveInfo.Create(SaveRestore.SaveFileName);

        #endregion Information

        #region Administration

        /// <summary>
        /// Bans the player for the specified reason and duration
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <param name="duration"></param>
        public void Ban(string id, string reason, TimeSpan duration = default(TimeSpan))
        {
            if (!IsBanned(id))
            {
                ServerUsers.Set(ulong.Parse(id), ServerUsers.UserGroup.Banned, Name, reason);
                ServerUsers.Save();
            }
        }

        /// <summary>
        /// Gets the amount of time remaining on the player's ban
        /// </summary>
        /// <param name="id"></param>
        public TimeSpan BanTimeRemaining(string id) => IsBanned(id) ? TimeSpan.MaxValue : TimeSpan.Zero;

        /// <summary>
        /// Gets if the player is banned
        /// </summary>
        /// <param name="id"></param>
        public bool IsBanned(string id) => ServerUsers.Is(ulong.Parse(id), ServerUsers.UserGroup.Banned);

        /// <summary>
        /// Saves the server and any related information
        /// </summary>
        public void Save()
        {
            ConVar.Server.save(null);
            File.WriteAllText(string.Concat(ConVar.Server.GetServerFolder("cfg"), "/serverauto.cfg"), ConsoleSystem.SaveToConfigString(true));
            ServerUsers.Save();
        }

        /// <summary>
        /// Unbans the player
        /// </summary>
        /// <param name="id"></param>
        public void Unban(string id)
        {
            if (IsBanned(id))
            {
                ServerUsers.Remove(ulong.Parse(id));
                ServerUsers.Save();
            }
        }

        #endregion Administration

        #region Chat and Commands

        /// <summary>
        /// Broadcasts the specified chat message and prefix to all players
        /// </summary>
        /// <param name="message"></param>
        /// <param name="prefix"></param>
        /// <param name="args"></param>
        public void Broadcast(string message, string prefix, params object[] args)
        {
            Server.Broadcast(message, prefix, 0, args);
        }

        /// <summary>
        /// Broadcasts the specified chat message to all players
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(string message) => Broadcast(message, null);

        /// <summary>
        /// Runs the specified server command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public void Command(string command, params object[] args)
        {
            Server.Command(command, args);
        }

        #endregion Chat and Commands
    }
}
