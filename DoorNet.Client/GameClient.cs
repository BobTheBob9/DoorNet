using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using UnityEngine;
using Harmony;

using BobNet;
using DoorNet.Server;
using DoorNet.Shared.Modules;
using DoorNet.Client.GameLogic;

using Reactor.API.Logging;

namespace DoorNet.Client
{
	/// <summary>
	/// 
	/// </summary>
	public class GameClient : MonoBehaviour
	{
		public static bool Created { get; private set; } = false;
		public static bool ConnectedToLocalServer { get; private set; } = false;

		public static Log Logger { get; private set; }
		public static HarmonyInstance Harmony { get; private set; } = HarmonyInstance.Create(Mod.ModID);
		public static NetManager NetworkManager { get; private set; }

		private static GameClient EventPoller;

		/// <summary>
		/// 
		/// </summary>
		public static void Connect(IPEndPoint connectedEP)
		{
			Create();
			NetworkManager.Connect(connectedEP);
		}

		/// <summary>
		/// 
		/// </summary>
		public static void HostLocalServer()
		{
			ConnectedToLocalServer = true;
			Create();

			GameServer.Create();
			NetworkManager.ConnectLocal(GameServer.NetworkManager);
		}

		/// <summary>
		/// 
		/// </summary>
		public static void Destroy()
		{

		}

		private static void Create()
		{
			if (Created)
				return;

			Logger = LogManager.GetForCurrentAssembly();
			Logger.Info("Creating client...");

			Created = true;

			NetworkManager = NetManager.CreateClient();

			RemoteEntityRegistry.Instance = new RemoteEntityRegistry();
			RemoteEntityRegistry.SyncChannel = NetworkManager.CreateChannel("DoorNet::EntityRegistrySync", new SyncableStringArrayChannel());

			ModuleManager.LoadModules(Side.Client);

			EventPoller = new GameObject("DoorNet::EventPoller").AddComponent<GameClient>();

			Logger.Info("Client created successfully!");
		}

		private void Update()
		{
			foreach (NetEvent nEvent in NetworkManager.PollEvents())
			{
				switch (nEvent.EventType)
				{
					case NetEventType.ConnectionComplete:
						{
							RemoteEntityRegistry.Instance.SyncroniseEntries((SyncableStringArrayChannel)RemoteEntityRegistry.SyncChannel);
							break;
						}

					case NetEventType.DisconnectedSelf:
						{

							break;
						}
				}
			}
		}
	}
}
