using System;
using System.Reflection;

using UnityEngine;
using Harmony;

using BobNet;
using DoorNet.Shared.Modules;

using Reactor.API.Logging;
using DoorNet.Server.GameLogic;

namespace DoorNet.Server
{
	/// <summary>
	/// Static class for managing the DoorNet gameserver
	/// </summary>
	public class GameServer : MonoBehaviour
	{
		public delegate void OnClientJoinHandler(NetClient client);
		public delegate void OnClientLeaveHandler(NetClient client, string reason);

		public static event OnClientJoinHandler OnClientJoin;
		public static event OnClientLeaveHandler OnClientLeave;

		public static bool Created { get; private set; } = false;

		public static Log Logger { get; private set; }
		public static HarmonyInstance Harmony { get; private set; }
		public static NetManager NetworkManager { get; private set; }

		private static GameServer EventPoller;

		/// <summary>
		/// Starts the DoorNet gameserver
		/// </summary>
		public static void Create()
		{
			Logger = LogManager.GetForCurrentAssembly();
			Logger.Info("Creating server...");

			Harmony = HarmonyInstance.Create("com.github.BobTheBob9/DoorNet.Server");
			NetworkManager = NetManager.CreateServer(44444);
			NetworkManager.ClientTimeoutTime = TimeSpan.Zero;
			NetEntityRegistry.Instance = new NetEntityRegistry();

			ModuleManager.LoadModules(Side.Server);

			EventPoller = new GameObject("DoorNet::EventPoller").AddComponent<GameServer>();
			NetworkManager.StartServer();

			Logger.Info("Server created successfully!");
		}

		/// <summary>
		/// Destroys the DoorNet gameserver
		/// </summary>
		public static void Destroy()
		{
			
		}

		private void Update()
		{
			foreach (NetEvent nEvent in NetworkManager.PollEvents())
			{
				switch (nEvent.EventType)
				{
					case NetEventType.ClientConnected:
						{
							ClientConnectedEvent connectionEvent = (ClientConnectedEvent)nEvent;
							OnClientJoin?.Invoke(connectionEvent.ConnectedClient);
							break;
						}

					case NetEventType.ClientDisconnected:
						{
							ClientDisconnectedEvent disconnectEvent = (ClientDisconnectedEvent)nEvent;
							OnClientLeave?.Invoke(disconnectEvent.DisconnectedClient, disconnectEvent.DisconnectReason);
							break;
						}
				}
			}
		}
	}
}
