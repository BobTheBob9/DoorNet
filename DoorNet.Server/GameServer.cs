using System;
using System.Reflection;

using UnityEngine;
using Harmony;

using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

using Reactor.API.Logging;

namespace DoorNet.Server
{
	/// <summary>
	/// The DoorNet gameserver
	/// </summary>
	public static class GameServer
	{
		public static Log Logger;
		public static NetManager NetworkManager;
		public static HarmonyInstance Harmony;

		private static EventPoller Poller;

		/// <summary>
		/// Starts the DoorNet gameserver
		/// </summary>
		public static void Create()
		{
			Logger = LogManager.GetForCurrentAssembly();

			Harmony = HarmonyInstance.Create("com.github.BobTheBob9/DoorNet.Server");
			Harmony.PatchAll();

			NetworkManager = new NetManager(Side.Server, 44444);

			ModuleManager.LoadModules(Side.Server);

			NetworkManager.StartServer();

			GameObject gm = new GameObject("DoorNet::Testing::EventPoller");
			GameObject.DontDestroyOnLoad(gm);
			Poller = gm.AddComponent<EventPoller>();
		}
	}
}
