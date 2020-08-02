using System;
using System.Reflection;

using UnityEngine;
using Harmony;

using BobNet;
using DoorNet.Shared.Modules;

using Reactor.API.Logging;

namespace DoorNet.Server
{
	/// <summary>
	/// Static class for managing the DoorNet gameserver
	/// </summary>
	public class GameServer : MonoBehaviour
	{
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
			Harmony = HarmonyInstance.Create("com.github.BobTheBob9/DoorNet.Server");
			NetworkManager = NetManager.CreateServer(44444);

			ModuleManager.LoadModules(Side.Server);

			EventPoller = new GameObject("DoorNet::EventPoller").AddComponent<GameServer>();
			NetworkManager.StartServer();
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
				//idk lol do this later
			}
		}
	}
}
