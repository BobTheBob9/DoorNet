using System;
using System.Reflection;

using UnityEngine;
using Harmony;

using DoorNet.Shared.Networking;
using DoorNet.Server.CoreSystems;

using Reactor.API.Logging;

namespace DoorNet.Server
{
	//test server class to test network connectivity and basic player sync, all temp
	//also this code is disgusting on god
	public static class DoorNetServer
	{
		public static Log Logger;
		public static NetManager NetworkManager;
		public static HarmonyInstance Harmony;

		private static EventPoller Poller;

		public static void Create(Log logger = null)
		{
			Logger = logger;

			Harmony = HarmonyInstance.Create("com.github.BobTheBob9/DoorNet.Server");
			Harmony.PatchAll();

			NetworkManager = new NetManager(ManagerType.Server, 44444);

			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
				if (type.IsAssignableFrom(typeof(DoorNetInitialised)))
					((DoorNetInitialised)Activator.CreateInstance(type)).Initialise();

			NetworkManager.StartServer();

			GameObject gm = new GameObject("DoorNet::Testing::EventPoller");
			Poller = gm.AddComponent<EventPoller>();
		}
	}
}
