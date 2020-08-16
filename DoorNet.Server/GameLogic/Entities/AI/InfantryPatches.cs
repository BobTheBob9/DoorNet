using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Harmony;
using BobNet;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Prefabs;

namespace DoorNet.Server.GameLogic
{
	using static GameServer;

	[DoorNetModule(Side.Server)]
	internal static class InfantryPatches
	{
		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			NetEntityRegistry.Instance.CreateEntry("DoorNet::Infantry", GTTODPrefabs.GetPrefab("Infantry"));
			NetEntityRegistry.Instance.CreateEntry("DoorNet::Brute", GTTODPrefabs.GetPrefab("Brute"));

			Harmony.Patch(typeof(Infantry).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance),
				postfix: new HarmonyMethod(typeof(InfantryPatches).GetMethod("CreateEntity", BindingFlags.NonPublic | BindingFlags.Static)));
			Harmony.Patch(typeof(Infantry).GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
				prefix: new HarmonyMethod(typeof(InfantryPatches).GetMethod("SetTarget", BindingFlags.NonPublic | BindingFlags.Static)));
		}

		private static void CreateEntity(Infantry __instance)
		{
			Logger.Info($"Server: Created a derek");

			NetClient[] sendList = null;
			if (NetworkManager.Clients[0].IsLocal) //slightly painful check
			{
				sendList = new NetClient[NetworkManager.Clients.Count - 1];
				for (int i = 1; i < NetworkManager.Clients.Count; i++)
					sendList[i - 1] = NetworkManager.Clients[i];
			}

			NetEntity.CreateEntity(__instance.gameObject, sendList);
		}

		private static void SetTarget(Infantry __instance)
		 => __instance.Target = AIHelpers.GetClosestPlayer(__instance.transform.position)?.transform;
	}
}
