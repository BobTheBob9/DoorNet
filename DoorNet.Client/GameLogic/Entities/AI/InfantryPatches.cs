using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Harmony;
using BobNet;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Prefabs;
using DoorNet.Shared.Networking;
using UnityEngine;

namespace DoorNet.Client.GameLogic
{
	using static GameClient;

	[DoorNetModule(Side.Client)]
	internal static class InfantryPatches
	{
		private static NetChannel TargetChannel;
		private static RemoteEntityStateCollection<RemoteEntity, Infantry> ComponentCache = new RemoteEntityStateCollection<RemoteEntity, Infantry>();

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			TargetChannel = NetworkManager.CreateChannel("DoorNet::InfantryTargeting", new IDMappedDataChannel(new UShortChannel()));

			RemoteEntityRegistry.Instance.CreateEntry("DoorNet::Infantry", GTTODPrefabs.GetPrefab("Infantry"));
			RemoteEntityRegistry.Instance.CreateEntry("DoorNet::Brute", GTTODPrefabs.GetPrefab("Brute"));

			if (!ConnectedToLocalServer)
			{
				//do patches to strip away serverside behaviour/do prediction and shit
				Harmony.Patch(typeof(Infantry).GetMethod("Start", BindingFlags.Public | BindingFlags.Instance),
					postfix: new HarmonyMethod(typeof(InfantryPatches).GetMethod("OnCreation", BindingFlags.NonPublic | BindingFlags.Static)));

				TargetChannel.OnRecieveSerialized += (object data, NetClient sender) =>
				{
					IDMappedDataChannel.Container container = (IDMappedDataChannel.Container)data;
					if (!RemoteEntity.Entities.ContainsKey(container.ID))
						return;

					RemoteEntity entity = RemoteEntity.Entities[container.ID];
					if (!ComponentCache.Contains(entity))
						return;

					GameObject targetObj;
					if ((ushort)container.Data == ClientPlayer.EntityID)
						targetObj = ClientPlayer.Instance.gameObject;
					else if (RemoteEntity.Entities.ContainsKey((ushort)container.Data))
						targetObj = RemoteEntity.Entities[(ushort)container.Data].gameObject;
					else return;

					ComponentCache[entity].Target = targetObj.transform;
				};
			}
		}

		private static void OnCreation(Infantry __instance)
		{
			RemoteEntity myEntity = __instance.GetComponent<RemoteEntity>();
			if (myEntity == null)
			{
				GameObject.Destroy(myEntity);
				return;
			}

			ComponentCache.Add(myEntity, myEntity, __instance);
		}
	}
}
