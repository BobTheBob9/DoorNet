using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Harmony;
using UnityEngine;
using BobNet;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Prefabs;
using DoorNet.Shared.Networking;

namespace DoorNet.Client.GameLogic
{
	using static GameClient;

	[DoorNetModule(Side.Client)]
	internal static class BulletScriptPatches
	{
		private static NetChannel BulletCatchupChannel;
		private static RemoteEntityStateCollection<RemoteEntity, BulletScript> ComponentCache = new RemoteEntityStateCollection<RemoteEntity, BulletScript>();

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			BulletCatchupChannel = NetworkManager.CreateChannel("DoorNet::BulletCatchup", new IDMappedDataChannel(new DateTimeChannel()));

			//player bullets
			//GameManager.GM.Player.GetComponent<InventoryScript>().Weapons[0].
			RemoteEntityRegistry.Instance.CreateEntry("DoorNet::StandardBullet", GTTODPrefabs.GetPrefab("BULLET"));

			//infantry bullets
			RemoteEntityRegistry.Instance.CreateEntry("DoorNet::InfantryPlasmaBullet", GTTODPrefabs.GetPrefab("EnemyPlasma"));

			if (!ConnectedToLocalServer)
			{
				Harmony.Patch(typeof(BulletScript).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance),
					postfix: new HarmonyMethod(typeof(BulletScriptPatches).GetMethod("OnCreation", BindingFlags.NonPublic | BindingFlags.Static)));

				//BulletCatchupChannel.OnRecieveSerialized += (object data, NetClient sender) =>
				//{
				//	IDMappedDataChannel.Container container = (IDMappedDataChannel.Container)data;
				//	if (!RemoteEntity.Entities.ContainsKey(container.ID))
				//		return;
				//
				//	RemoteEntity entity = RemoteEntity.Entities[container.ID];
				//	if (!ComponentCache.Contains(entity))
				//		return;
				//
				//	BulletScript bullet = ComponentCache[entity];
				//	double timeSinceSend = DateTime.Now.Subtract((DateTime)container.Data).TotalSeconds;
				//	bullet.transform.Translate(Vector3.forward * (float)timeSinceSend * bullet.speed);
				//};
			}
		}

		private static void OnCreation(BulletScript __instance)
		{
			RemoteEntity myEntity = __instance.GetComponent<RemoteEntity>();
			if (myEntity == null)
				return;

			ComponentCache.Add(myEntity, myEntity, __instance);
		}
	}
}
