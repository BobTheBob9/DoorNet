using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoorNet.Server.GameLogic
{
	public class NetPlayerInventory
	{
		public NetPlayer Player { get; private set; }

		public int SelectedIndex { get; private set; }
		public NetPlayerWeapon[] Weapons { get; private set; }

		public NetPlayerInventory(NetPlayer player)
		{
			Player = player;
		}
	}
}
