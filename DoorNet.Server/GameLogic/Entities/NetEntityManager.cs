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

	/// <summary>
	/// Static class for managing networked entities
	/// </summary>
	[DoorNetModule(Side.Server)]
	public static class NetEntityManager
	{
		public static SortedDictionary<ushort, NetEntity> Entities;

		public static NetHandler EntityPositionHandler;
		public static NetHandler EntityCreationHandler;
		public static NetHandler EntityDestructionHandler;

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			//create handlers
			EntityPositionHandler = NetworkManager.Handle("DoorNet::Entities::Position");
			EntityCreationHandler = NetworkManager.Handle("DoorNet::Entities::Creation");
			EntityDestructionHandler = NetworkManager.Handle("DoorNet::Entities::Destruction");

			//todo: probably make it so that normal classes can't mess with the entity dict at some point
		}

		/// <summary>
		/// Creates a networked entity from a preexisting GameObject
		/// </summary>
		/// <param name="obj">The object to be created as a networked entity</param>
		/// <returns>The created networked entity</returns>
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

			//create obj
			NetEntity entity = new NetEntity(id, obj);
			Entities.Add(id, entity);

			//construct creation packet
			//todo: make system to sync prefabs for client instantiation
			//byte[] data = fuck do this later
			//EntityCreationHandler.Broadcast(data, SendMode.Tcp);

			return entity;
		}
	}
}
