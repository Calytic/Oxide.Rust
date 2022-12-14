using Facepunch;
using Facepunch.Extend;
using Network;
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Unity;
using Oxide.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Oxide.Game.Rust
{
    /// <summary>
    /// The extension class that represents this extension
    /// </summary>
    public class RustExtension : Extension
    {
        internal static Assembly Assembly = Assembly.GetExecutingAssembly();
        internal static AssemblyName AssemblyName = Assembly.GetName();
        internal static VersionNumber AssemblyVersion = new VersionNumber(AssemblyName.Version.Major, AssemblyName.Version.Minor, AssemblyName.Version.Build);
        internal static string AssemblyAuthors = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly, typeof(AssemblyCompanyAttribute), false)).Company;

        /// <summary>
        /// Gets whether this extension is for a specific game
        /// </summary>
        public override bool IsGameExtension => true;

        /// <summary>
        /// Gets the name of this extension
        /// </summary>
        public override string Name => "Rust";

        /// <summary>
        /// Gets the author of this extension
        /// </summary>
        public override string Author => AssemblyAuthors;

        /// <summary>
        /// Gets the version of this extension
        /// </summary>
        public override VersionNumber Version => AssemblyVersion;

        /// <summary>
        /// Gets the branch of this extension
        /// </summary>
        public override string Branch => "public"; // TODO: Handle this programmatically

        /// <summary>
        /// Default game-specific references for use in plugins
        /// </summary>
        internal static readonly HashSet<string> DefaultReferences = new HashSet<string>
        {
            "ApexAI", "ApexShared", "Facepunch.Network", "Facepunch.Steamworks", "Facepunch.System", "Facepunch.UnityEngine", "Rust.Data", "Rust.Global",
            "Rust.Workshop", "System.Drawing", "UnityEngine.AIModule", "UnityEngine.AssetBundleModule", "UnityEngine.CoreModule", "UnityEngine.GridModule",
            "UnityEngine.ImageConversionModule", "UnityEngine.Networking", "UnityEngine.PhysicsModule", "UnityEngine.TerrainModule",
            "UnityEngine.TerrainPhysicsModule", "UnityEngine.UI", "UnityEngine.UIModule", "UnityEngine.UIElementsModule",
            "UnityEngine.UnityWebRequestWWWModule", "UnityEngine.VehiclesModule", "UnityEngine.WebModule"
        };

        /// <summary>
        /// List of assemblies allowed for use in plugins
        /// </summary>
        public override string[] WhitelistAssemblies => new[]
        {
            "Assembly-CSharp", "Assembly-CSharp-firstpass", "DestMath", "Facepunch.Network", "Facepunch.System", "Facepunch.UnityEngine", "mscorlib",
            "Oxide.Core", "Oxide.Rust", /* < Needed for non-C# plugins for some reason */ "RustBuild", "Rust.Data", "Rust.Global", "System", "System.Core",
            "UnityEngine"
        };

        /// <summary>
        /// List of namespaces allowed for use in plugins
        /// </summary>
        public override string[] WhitelistNamespaces => new[]
        {
            "ConVar", "Dest", "Facepunch", "Network", "Oxide.Game.Rust.Cui", "ProtoBuf", "PVT", "Rust", "Steamworks", "System.Collections",
            "System.Security.Cryptography", "System.Text", "UnityEngine"
        };

        /// <summary>
        /// List of filter matches to apply to console output
        /// </summary>
        public static string[] Filter =
        {
            "alphamapResolution is clamped to the range of",
            "AngryAnt Behave version",
            "Floating point textures aren't supported on this device",
            "HDR RenderTexture format is not supported on this platform.",
            "Image Effects are not supported on this platform.",
            "Missing projectileID",
            "Motion vectors not supported on a platform that does not support",
            "The image effect Main Camera",
            "The image effect effect -",
            "Unable to find shaders",
            "Unsupported encoding: 'utf8'",
            "Warning, null renderer for ScaleRenderer!",
            "[AmplifyColor]",
            "[AmplifyOcclusion]",
            "[CoverageQueries] Disabled due to unsupported",
            "[CustomProbe]",
            "[Manifest] URI IS",
            "[SpawnHandler] populationCounts"
        };

        /// <summary>
        /// Initializes a new instance of the RustExtension class
        /// </summary>
        /// <param name="manager"></param>
        public RustExtension(ExtensionManager manager) : base(manager)
        {
        }

        /// <summary>
        /// Loads this extension
        /// </summary>
        public override void Load()
        {
            Manager.RegisterLibrary("Rust", new Libraries.Rust());
            Manager.RegisterLibrary("Command", new Libraries.Command());
            Manager.RegisterLibrary("Item", new Libraries.Item());
            Manager.RegisterLibrary("Player", new Libraries.Player());
            Manager.RegisterLibrary("Server", new Libraries.Server());
            Manager.RegisterPluginLoader(new RustPluginLoader());
        }

        /// <summary>
        /// Loads plugin watchers used by this extension
        /// </summary>
        /// <param name="directory"></param>
        public override void LoadPluginWatchers(string directory)
        {
        }

        /// <summary>
        /// Called when all other extensions have been loaded
        /// </summary>
        public override void OnModLoad()
        {
            CSharpPluginLoader.PluginReferences.UnionWith(DefaultReferences);

            if (Interface.Oxide.EnableConsole())
            {
                Output.OnMessage += HandleLog;
            }
        }

        internal static void ServerConsole()
        {
            if (Interface.Oxide.ServerConsole == null)
            {
                return;
            }

            Interface.Oxide.ServerConsole.Title = () => $"{BasePlayer.activePlayerList.Count} | {ConVar.Server.hostname}";

            Interface.Oxide.ServerConsole.Status1Left = () =>
            {
                string hostname = ConVar.Server.hostname.Length > 30 ? ConVar.Server.hostname.Truncate(30) : ConVar.Server.hostname;
                return $"{hostname} [{(Interface.Oxide.Config.Options.Modded ? "Modded" : "Community")}]";
            };
            Interface.Oxide.ServerConsole.Status1Right = () => $"{Performance.current.frameRate}fps, {((ulong)Time.realtimeSinceStartup).FormatSeconds()}";

            Interface.Oxide.ServerConsole.Status2Left = () =>
            {
                string players = $"{BasePlayer.activePlayerList.Count}/{ConVar.Server.maxplayers} players";
                int sleepers = BasePlayer.sleepingPlayerList.Count;
                int entities = BaseNetworkable.serverEntities.Count;
                return $"{players}, {sleepers + (sleepers.Equals(1) ? " sleeper" : " sleepers")}, {entities + (entities.Equals(1) ? " entity" : " entities")}";
            };
            Interface.Oxide.ServerConsole.Status2Right = () =>
            {
                if (Net.sv == null || !Net.sv.IsConnected())
                {
                    return "not connected";
                }

                ulong bytesReceived = Net.sv.GetStat(null, NetworkPeer.StatTypeLong.BytesReceived_LastSecond);
                ulong bytesSent = Net.sv.GetStat(null, NetworkPeer.StatTypeLong.BytesSent_LastSecond);
                return $"{Utility.FormatBytes(bytesReceived) ?? "0"}/s in, {Utility.FormatBytes(bytesSent) ?? "0"}/s out";
            };

            Interface.Oxide.ServerConsole.Status3Left = () =>
            {
                string gameTime = (TOD_Sky.Instance?.Cycle?.DateTime != null ? TOD_Sky.Instance.Cycle.DateTime : DateTime.Now).ToString("h:mm tt");
                return $"{gameTime.ToLower()}, {ConVar.Server.level} [{ConVar.Server.worldsize}, {ConVar.Server.seed}]";
            };
            Interface.Oxide.ServerConsole.Status3Right = () => $"Oxide.Rust {AssemblyVersion}";
            Interface.Oxide.ServerConsole.Status3RightColor = ConsoleColor.Yellow;

            Interface.Oxide.ServerConsole.Input += ServerConsoleOnInput;
            Interface.Oxide.ServerConsole.Completion = input =>
            {
                if (!string.IsNullOrEmpty(input))
                {
                    if (!input.Contains("."))
                    {
                        input = string.Concat("global.", input);
                    }

                    return ConsoleSystem.Index.All.Where(c => c.FullName.StartsWith(input.ToLower())).ToList().ConvertAll(c => c.FullName).ToArray();
                }

                return null;
            };
        }

        private static void ServerConsoleOnInput(string input)
        {
            input = input.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                ConsoleSystem.Run(ConsoleSystem.Option.Server, input);
            }
        }

        private static void HandleLog(string message, string stackTrace, LogType logType)
        {
            if (!string.IsNullOrEmpty(message) && !Filter.Any(message.Contains))
            {
                Interface.Oxide.RootLogger.HandleMessage(message, stackTrace, logType.ToLogType());
            }
        }
    }
}
