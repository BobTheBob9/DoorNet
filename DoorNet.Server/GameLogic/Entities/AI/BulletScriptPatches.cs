using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Harmony;
using BobNet;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;
using DoorNet.Shared.Prefabs;

namespace DoorNet.Server.GameLogic
{
	using static GameServer;

	[DoorNetModule(Side.Server)]
	internal static class BulletScriptPatches
	{
		private static NetChannel BulletCatchupChannel;

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			BulletCatchupChannel = NetworkManager.CreateChannel("DoorNet::BulletCatchup", new IDMappedDataChannel(new DateTimeChannel()));

			//player bullets
			//GameManager.GM.Player.GetComponent<InventoryScript>().Weapons[0].
			NetEntityRegistry.Instance.CreateEntry("DoorNet::StandardBullet", GTTODPrefabs.GetPrefab("BULLET"));

			//infantry bullets
			NetEntityRegistry.Instance.CreateEntry("DoorNet::InfantryPlasmaBullet", GTTODPrefabs.GetPrefab("EnemyPlasma"));

			Harmony.Patch(typeof(BulletScript).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance),
				postfix: new HarmonyMethod(typeof(BulletScriptPatches).GetMethod("CreateEntity", BindingFlags.NonPublic | BindingFlags.Static)));

			//if (IsLocalServer)
			//	InstantiationHelpers.PatchInstantiators(typeof(WeaponScript).GetMethod("NewPrimaryFire", BindingFlags.Public | BindingFlags.Instance));
		}

		private static void CreateEntity(BulletScript __instance)
		{
			NetClient[] sendList = null;
			if (IsLocalServer)
			{
				sendList = new NetClient[NetworkManager.Clients.Count - 1];
				for (int i = 1; i < NetworkManager.Clients.Count; i++)
					sendList[i - 1] = NetworkManager.Clients[i];
			}

			NetEntity entity = NetEntity.CreateEntity(__instance.gameObject, sendList);

			var container = new IDMappedDataChannel.Container(entity.ID, DateTime.Now);
			BulletCatchupChannel.BroadcastSerialized(SendMode.Tcp, container);
		}
	}
}
