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

		public static NetPlayerWeapon FromWeaponscript(WeaponItem item)
		{
			WeaponScript weaponObj = item.WeaponObject;
			if (weaponObj.PrimaryBullet != null && !NetEntityRegistry.Instance.ContainsItem(weaponObj.PrimaryBullet))
				NetEntityRegistry.Instance.CreateEntry($"DoorNet::WeaponBullet{weaponObj.PrimaryBullet.name}", weaponObj.PrimaryBullet);

			if (weaponObj.SecondaryBullet != null && !NetEntityRegistry.Instance.ContainsItem(weaponObj.SecondaryBullet))
				NetEntityRegistry.Instance.CreateEntry($"DoorNet::WeaponBullet{weaponObj.SecondaryBullet.name}", weaponObj.SecondaryBullet);

			//todo convert into netplayerweapon

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
