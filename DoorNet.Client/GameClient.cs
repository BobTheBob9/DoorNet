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
		public static HarmonyInstance Harmony { get; private set; }
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
			Create();

			GameServer.Create();
			NetworkManager.ConnectLocal(GameServer.NetworkManager);

			ConnectedToLocalServer = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void Destroy()
		{

		}

		private static void Create()
		{
			Created = true;

			Logger = LogManager.GetForCurrentAssembly();
			Harmony = HarmonyInstance.Create(Mod.ModID);
			NetworkManager = NetManager.CreateClient();

			ModuleManager.LoadModules(Side.Client);

			EventPoller = new GameObject("DoorNet::EventPoller").AddComponent<GameClient>();
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
