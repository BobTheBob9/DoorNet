using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;
using DoorNet.Shared.Modules;

namespace DoorNet.Server.GameLogic
{
	[DoorNetModule(Side.Server)]
	public class NetPlayerInventory
	{
		public NetPlayer Player { get; private set; }

		public int SelectedIndex { get; private set; }
		public NetPlayerWeapon[] Weapons { get; private set; }

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			//convert andre weaponscripts into NetPlayerWeapons
		}

		public NetPlayerInventory(NetPlayer player)
		{
			Player = player;
		}
	}
}
