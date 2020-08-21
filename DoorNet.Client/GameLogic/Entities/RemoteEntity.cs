using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;
using UnityEngine;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

namespace DoorNet.Client.GameLogic
{
	using static GameClient;

	/// <summary>
	/// 
	/// </summary>
	[DoorNetModule(Side.Client)]
	public class RemoteEntity : MonoBehaviour
	{
		public delegate void OnDestructionHandler();
		public delegate void OnEntityDestructionHandler(RemoteEntity entity);
		public static event OnEntityDestructionHandler OnEntityDestruction;

		public static Dictionary<ushort, RemoteEntity> Entities { get; private set; } = new Dictionary<ushort, RemoteEntity>();

		public static NetChannel EntityPositionChannel { get; private set; }
		public static NetChannel EntityRotationChannel { get; private set; }
		public static NetChannel EntityCreationChannel { get; private set; }
		public static NetChannel EntityEnablingChannel { get; private set; }
		public static NetChannel EntityDestructionChannel { get; private set; }

		public event OnDestructionHandler OnDestruction;

		public ushort ID { get; private set; }
		public ushort PrefabID { get; private set; }
		public Vector3 NetworkPosition { get; private set; }
		public Quaternion NetworkRotation { get; private set; }

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			EntityPositionChannel = NetworkManager.CreateChannel("DoorNet::Entities::Position", new IDMappedDataChannel(new Vector3Channel()));
			EntityRotationChannel = NetworkManager.CreateChannel("DoorNet::Entities::Rotation", new IDMappedDataChannel(new QuaternionChannel()));
			EntityCreationChannel = NetworkManager.CreateChannel("DoorNet::Entities::Creation", new NetEntityCreationChannel());
			EntityEnablingChannel = NetworkManager.CreateChannel("DoorNet::Entities::Enabling", new IDMappedDataChannel(new BooleanChannel()));
			EntityDestructionChannel = NetworkManager.CreateChannel("DoorNet::Entities::Destruction", new UShortChannel());

			EntityPositionChannel.OnRecieveSerialized += (object objData, NetClient sender) =>
			{
				IDMappedDataChannel.Container container = (IDMappedDataChannel.Container)objData;
				if (!Entities.ContainsKey(container.ID) || Entities[container.ID] == null)
					return;

				Vector3 position = (Vector3)container.Data;

				RemoteEntity entity = Entities[container.ID];
				entity.NetworkPosition = position;
				entity.transform.position = position;
			};

			EntityRotationChannel.OnRecieveSerialized += (object objData, NetClient sender) =>
			{
				IDMappedDataChannel.Container container = (IDMappedDataChannel.Container)objData;
				if (!Entities.ContainsKey(container.ID) || Entities[container.ID] == null)
					return;

				Quaternion rotation = (Quaternion)container.Data;
				RemoteEntity entity = Entities[container.ID];
				entity.NetworkRotation = rotation;
				entity.transform.rotation = rotation;
			};

			EntityCreationChannel.OnRecieveSerialized += (object objData, NetClient sender) =>
			{
				NetEntityCreationChannel.CreationPacket creationPacket = (NetEntityCreationChannel.CreationPacket)objData;

				GameObject gm = Instantiate(RemoteEntityRegistry.Instance.Items[creationPacket.PrefabID]);
				CreateEntity(creationPacket.ID, creationPacket.PrefabID, gm);

				gm.transform.position = creationPacket.Position;
				gm.transform.rotation = creationPacket.Rotation;
			};

			EntityEnablingChannel.OnRecieveSerialized += (object objData, NetClient sender) =>
			{
				IDMappedDataChannel.Container container = (IDMappedDataChannel.Container)objData;
				if (!Entities.ContainsKey(container.ID) || Entities[container.ID] == null)
					return;

				Entities[container.ID].gameObject.SetActive((bool)container.Data);
			};

			EntityDestructionChannel.OnRecieveSerialized += (object objData, NetClient sender) =>
			{
				ushort id = (ushort)objData;
				if (!Entities.ContainsKey(id))
					return;

				if (Entities[id] != null)
				{
					Entities[id].OnDestruction?.Invoke();
					OnEntityDestruction?.Invoke(Entities[id]);
					Destroy(Entities[id].gameObject);
				}

				Entities.Remove(id);
			};
		}

		public static void ReplaceEntity(ushort id, ushort prefabId, GameObject newObject)
		{
			RemoteEntity oldEntity = Entities[id];
			Entities.Remove(id);
			Destroy(oldEntity);

			CreateEntity(id, prefabId, newObject);
		}

		private static void CreateEntity(ushort id, ushort prefabId, GameObject obj)
		{
			RemoteEntity entity = obj.AddComponent<RemoteEntity>();
			entity.ID = id;
			entity.PrefabID = prefabId;
			Entities.Add(id, entity);
			entity.gameObject.SetActive(true);

			Logger.Info($"Client: Created entity {RemoteEntityRegistry.Instance.Entries[prefabId]}");
		}
	}
}
