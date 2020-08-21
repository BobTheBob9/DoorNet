using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Harmony;
using BobNet;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Prefabs;
using DoorNet.Shared.Helpers;
using DoorNet.Shared.Networking;
using UnityEngine;

namespace DoorNet.Server.GameLogic
{
	using static GameServer;

	[DoorNetModule(Side.Server)]
	internal static class InfantryPatches
	{
		private struct EntityState
		{
			public NetEntity Entity;
			public DateTime LastTargetSet;
		}

		private static TimeSpan TargetSetCooldown;
		private static NetChannel TargetChannel;
		private static NetEntityStateCollection<Infantry, EntityState> States = new NetEntityStateCollection<Infantry, EntityState>();

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			TargetSetCooldown = TimeSpan.FromSeconds(2);
			TargetChannel = NetworkManager.CreateChannel("DoorNet::InfantryTargeting", new IDMappedDataChannel(new UShortChannel()));

			NetEntityRegistry.Instance.CreateEntry("DoorNet::Infantry", GTTODPrefabs.GetPrefab("Infantry"));
			NetEntityRegistry.Instance.CreateEntry("DoorNet::Brute", GTTODPrefabs.GetPrefab("Brute"));

			Harmony.Patch(typeof(Infantry).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance),
				postfix: new HarmonyMethod(typeof(InfantryPatches).GetMethod("CreateEntity", BindingFlags.NonPublic | BindingFlags.Static)));
			Harmony.Patch(typeof(Infantry).GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
				prefix: new HarmonyMethod(typeof(InfantryPatches).GetMethod("SetTarget", BindingFlags.NonPublic | BindingFlags.Static)),
				postfix: new HarmonyMethod(typeof(InfantryPatches).GetMethod("UpdatePosition", BindingFlags.NonPublic | BindingFlags.Static)));

			Type ienumeratorClass = ReflectionHelpers.GetIEnumeratorClass(typeof(Infantry), "Fire");
			InstantiationHelpers.PatchInstantiators(Harmony, ienumeratorClass.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Instance));
		}

		private static void CreateEntity(Infantry __instance)
		{
			NetClient[] sendList = null; 
			if (IsLocalServer)
			{
				sendList = new NetClient[NetworkManager.Clients.Count - 1];
				for (int i = 1; i < NetworkManager.Clients.Count; i++)
					sendList[i - 1] = NetworkManager.Clients[i];
			}

			NetEntity entity = NetEntity.CreateEntity(__instance.gameObject, sendList);
			States.Add(entity, __instance, new EntityState { LastTargetSet = DateTime.Now, Entity = entity });
		}

		private static void SetTarget(Infantry __instance)
		{
			EntityState state = States[__instance];
			if (DateTime.Now.Subtract(state.LastTargetSet) < TargetSetCooldown)
				return;

			state.LastTargetSet = DateTime.Now;

			NetPlayer newTarget = AIHelpers.GetClosestPlayer(__instance.transform.position);
			__instance.Target = newTarget.transform;

			NetEntity myEntity = state.Entity;
			TargetChannel.BroadcastSerialized(SendMode.Tcp, new IDMappedDataChannel.Container(myEntity.ID, newTarget.Entity.ID));
		}

		private static void UpdatePosition(Infantry __instance)
		 => States[__instance].Entity.UpdatePosition();
	}
}
