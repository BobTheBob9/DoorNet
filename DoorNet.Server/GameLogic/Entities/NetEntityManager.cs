using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

namespace DoorNet.Server.GameLogic
{
	using static GameServer;

	[DoorNetModule(Side.Server)]
	public static class NetEntityManager
	{
		public static SortedDictionary<ushort, NetEntity> Entities;

		public static NetHandler EntityPositionHandler;
		public static NetHandler EntityCreationHandler;
		public static NetHandler EntityDestructionHandler;

		public static void Initialise()
		{
			EntityPositionHandler = NetworkManager.Handle("DoorNet::Entities::Position");
			EntityCreationHandler = NetworkManager.Handle("DoorNet::Entities::Creation");
			EntityDestructionHandler = NetworkManager.Handle("DoorNet::Entities::Destruction");

			//todo: assign entities unique ushort ids on creation which are then used as handles to them from that point
			//also probably make it so that normal classes can't mess with the entity dict at some point
		}

		public static NetEntity CreateEntity(GameObject obj)
		{
			//generate an id and create an obj
			ushort id = 0;
			foreach (var pair in Entities)
			{
				if (pair.Key != id)
					break; //id is available

				id++;
			}

			NetEntity entity = new NetEntity(id, obj);
			Entities.Add(id, entity);

			//construct creation packet
			//todo: make system to sync prefabs for client instantiation
			//byte[] data = fuck do this later
			//EntityCreationHandler.Broadcast(data, SendMode.Tcp);

			return entity; //create obj
		}
	}
}
