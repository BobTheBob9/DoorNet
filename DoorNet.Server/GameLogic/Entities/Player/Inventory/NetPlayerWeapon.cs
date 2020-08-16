using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoorNet.Server.GameLogic
{
	public class NetPlayerWeapon
	{
		public NetPlayerInventory Inventory { get; private set; }

		public string Name { get; private set; } 

		public static NetPlayerWeapon FromWeaponscript()
		{
			throw new NotImplementedException();
		}

		public void ShootPrimary()
		{

		}

		public void ShootSecondary()
		{

		}

		public void Reload()
		{

		}
	}
}
