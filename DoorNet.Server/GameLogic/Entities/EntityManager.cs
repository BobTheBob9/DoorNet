using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using DoorNet.Server.CoreSystems;
using DoorNet.Shared.Networking;

using static DoorNet.Server.DoorNetServer;

namespace DoorNet.Server.GameLogic
{
	public class NetEntityManager : DoorNetInitialised
	{
		public static SortedDictionary<ushort, NetEntity> Entities;

		public static NetHandler EntityPositionHandler;
		public static NetHandler EntityCreationHandler;
		public static NetHandler EntityDestructionHandler;

		public override void Initialise()
		{
			EntityPositionHandler = NetworkManager.Handle("DoorNet::Entities::Position");
			EntityCreationHandler = NetworkManager.Handle("DoorNet::Entities::Creation");
			EntityDestructionHandler = NetworkManager.Handle("DoorNet::Entities::Destruction");

			//todo: assign entities unique ushort ids on creation which are then used as handles to them from that point
			//also probably make it so that normal classes can't mess with the entity dict at some point
		}
	}
}
