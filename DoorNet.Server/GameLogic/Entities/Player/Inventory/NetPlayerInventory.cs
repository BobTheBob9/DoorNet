using System;
using System.Collections.Generic;
using System.Text;

using BobNet;
using DoorNet.Shared.Modules;

namespace DoorNet.Server.GameLogic
{
	[DoorNetModule(Side.Server)]
	public class NetPlayerInventory
	{
		public static List<NetPlayerWeapon> GlobalWeapons = new List<NetPlayerWeapon>();

		public NetPlayer Player { get; private set; }

		public int SelectedIndex { get; private set; }
		public NetPlayerWeapon[] Weapons { get; private set; }

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			return;

			foreach (WeaponItem weapon in GameManager.GM.Player.GetComponent<InventoryScript>().Weapons)
				GlobalWeapons.Add(NetPlayerWeapon.FromWeaponscript(weapon));
		}

		public NetPlayerInventory(NetPlayer player)
		{
			Player = player;
		}
	}
}
