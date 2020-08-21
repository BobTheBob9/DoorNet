using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;
using UnityEngine;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;
using DoorNet.Shared.Registries;

namespace DoorNet.Server.GameLogic
{
	using static GameServer;

	/// <summary>
	/// Represents a GameObject syncronised across the network
	/// </summary>
	[DoorNetModule(Side.Server)]
	public class NetEntity : MonoBehaviour
	{
		public delegate void OnDestructionHandler();
		public delegate void OnEntityDestructionHandler(NetEntity entity);
		public static event OnEntityDestructionHandler OnEntityDestruction;

		public static SortedDictionary<ushort, NetEntity> Entities { get; private set; } = new SortedDictionary<ushort, NetEntity>();

		public static NetChannel EntityPositionChannel { get; private set; }
		public static NetChannel EntityRotationChannel { get; private set; }
		public static NetChannel EntityCreationChannel { get; private set; }
		public static NetChannel EntityEnablingChannel { get; private set; }
		public static NetChannel EntityDestructionChannel { get; private set; }

		public event OnDestructionHandler OnDestruction;

		public bool Alive { get; private set; } = true;

		public ushort ID { get; private set; }
		public ushort PrefabID { get; private set; }

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			//create handlers
			EntityPositionChannel = NetworkManager.CreateChannel("DoorNet::Entities::Position", new IDMappedDataChannel(new Vector3Channel()));
			EntityRotationChannel = NetworkManager.CreateChannel("DoorNet::Entities::Rotation", new IDMappedDataChannel(new QuaternionChannel()));
			EntityCreationChannel = NetworkManager.CreateChannel("DoorNet::Entities::Creation", new NetEntityCreationChannel());
			EntityEnablingChannel = NetworkManager.CreateChannel("DoorNet::Entities::Enabling", new IDMappedDataChannel(new BooleanChannel()));
			EntityDestructionChannel = NetworkManager.CreateChannel("DoorNet::Entities::Destruction", new UShortChannel());

			OnClientJoin += SendEntitiesToNewClient;
			//todo: probably make it so that normal classes can't mess with the entity dict at some point
		}

		/// <summary>
		/// Creates a networked entity from a preexisting gameobject
		/// </summary>
		/// <param name="obj">The object to be created as a networked entity</param>
		/// <returns>The created networked entity</returns>
		public static NetEntity CreateEntity(GameObject obj, NetClient[] clientsToSendTo = null)
		{
			//generate an id and create an obj
			ushort id = 0;
			foreach (var pair in Entities)
			{
				if (pair.Key != id)
					break; //id is available

				id++;
			}

			var registryContainer = obj.GetComponent<NetEntityRegistry.PrefabRegistryContainer>();

			if (registryContainer == null)
				throw new InvalidOperationException("Cannot create a NetEntity from an unregistered prefab");

			NetEntity createdEntity = obj.AddComponent<NetEntity>();

			createdEntity.ID = id;
			createdEntity.PrefabID = (ushort)registryContainer.ID;

			Entities.Add(id, createdEntity);
			NetEntityCreationChannel.CreationPacket sentPacket = new NetEntityCreationChannel.CreationPacket(id, createdEntity.PrefabID, createdEntity.transform.position, createdEntity.transform.rotation);
			if (clientsToSendTo == null)
				EntityCreationChannel.BroadcastSerialized(SendMode.Tcp, sentPacket);
			else
				foreach (NetClient client in clientsToSendTo)
					EntityCreationChannel.SendSerialized(SendMode.Tcp, sentPacket, client);

			createdEntity.gameObject.SetActive(true);
			return createdEntity;
		}

		private static void SendEntitiesToNewClient(NetClient client)
		{
			foreach (NetEntity entity in Entities.Values)
				EntityCreationChannel.SendSerialized(SendMode.Tcp, new NetEntityCreationChannel.CreationPacket(entity.ID, entity.PrefabID, entity.transform.position, entity.transform.rotation), client);
		}

		/// <summary>
		/// Broadcasts the position of a networked entity to clients
		/// </summary>
		/// <param name="position">The broadcasted position of the entity</param>
		public void UpdatePosition(Vector3 position)
		 => EntityPositionChannel.BroadcastSerialized(SendMode.Udp, new IDMappedDataChannel.Container(ID, position));

		/// <summary>
		/// Broadcasts the current position of a networked entity to clients
		/// </summary>
		public void UpdatePosition()
		 => UpdatePosition(transform.position);

		/// <summary>
		/// Broadcasts the rotation of a networked entity to clients
		/// </summary>
		/// <param name="rotation">The broadcasted rotation of the entity</param>
		public void UpdateRotation(Quaternion rotation)
		 => EntityRotationChannel.BroadcastSerialized(SendMode.Udp, new IDMappedDataChannel.Container(ID, rotation));

		/// <summary>
		/// Broadcasts the current rotation of a networked entity to clients
		/// </summary>
		public void UpdateRotation()
		 => UpdateRotation(transform.rotation);

		private void OnEnable()
		 => EntityEnablingChannel.BroadcastSerialized(SendMode.Tcp, new IDMappedDataChannel.Container(ID, true));

		private void OnDisable()
		 => EntityEnablingChannel.BroadcastSerialized(SendMode.Tcp, new IDMappedDataChannel.Container(ID, false));

		private void OnDestroy()
		{
			Alive = false;
			OnDestruction?.Invoke();
			OnEntityDestruction?.Invoke(this);

			Entities.Remove(ID);
			EntityDestructionChannel.BroadcastSerialized(SendMode.Tcp, ID);
		}
	}
}
