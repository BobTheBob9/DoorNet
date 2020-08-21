using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;
using UnityEngine;

namespace DoorNet.Shared.Networking
{
	public class NetEntityCreationChannel : NetChannel
	{
		public struct CreationPacket
		{
			public ushort ID;
			public ushort PrefabID;
			public Vector3 Position;
			public Quaternion Rotation;

			public CreationPacket(ushort id, ushort registryId, Vector3 position, Quaternion rotation)
			{
				ID = id;
				PrefabID = registryId;
				Position = position;
				Rotation = rotation;
			}
		}

		public override SerializationResult<byte[]> SerializeData(object objData, NetClient client)
		{
			if (!(objData is CreationPacket))
				return SerializationResult<byte[]>.Failure;

			CreationPacket data = (CreationPacket)objData;

			byte[] fullData = new byte[(sizeof(ushort) * 2) + sizeof(float) * 4 * 2];
			Array.Copy(BitConverter.GetBytes(data.ID), 0, fullData, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes(data.PrefabID), 0, fullData, sizeof(ushort), sizeof(ushort));

			//position
			Array.Copy(BitConverter.GetBytes(data.Position.x), 0, fullData, sizeof(ushort) * 2, sizeof(float));
			Array.Copy(BitConverter.GetBytes(data.Position.y), 0, fullData, (sizeof(ushort) * 2) + sizeof(float), sizeof(float));
			Array.Copy(BitConverter.GetBytes(data.Position.z), 0, fullData, (sizeof(ushort) * 2) + (sizeof(float) * 2), sizeof(float));

			//rotation
			Array.Copy(BitConverter.GetBytes(data.Rotation.x), 0, fullData, (sizeof(ushort) * 2) + (sizeof(float) * 3), sizeof(float));
			Array.Copy(BitConverter.GetBytes(data.Rotation.y), 0, fullData, (sizeof(ushort) * 2) + (sizeof(float) * 4), sizeof(float));
			Array.Copy(BitConverter.GetBytes(data.Rotation.z), 0, fullData, (sizeof(ushort) * 2) + (sizeof(float) * 5), sizeof(float));
			Array.Copy(BitConverter.GetBytes(data.Rotation.w), 0, fullData, (sizeof(ushort) * 2) + (sizeof(float) * 6), sizeof(float));

			return new SerializationResult<byte[]>(true, fullData);
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length != (sizeof(ushort) * 2) + sizeof(float) * 4 * 2)
				return SerializationResult<object>.Failure;

			ushort id = BitConverter.ToUInt16(data, 0);
			ushort registryId = BitConverter.ToUInt16(data, sizeof(ushort));

			Vector3 position = new Vector3(BitConverter.ToSingle(data, sizeof(ushort) * 2),
										   BitConverter.ToSingle(data, (sizeof(ushort) * 2) + sizeof(float)),
										   BitConverter.ToSingle(data, (sizeof(ushort) * 2) + (sizeof(float) * 2)));

			Quaternion rotation = new Quaternion(BitConverter.ToSingle(data, (sizeof(ushort) * 2) + (sizeof(float) * 3)),
												 BitConverter.ToSingle(data, (sizeof(ushort) * 2) + (sizeof(float) * 4)),
												 BitConverter.ToSingle(data, (sizeof(ushort) * 2) + (sizeof(float) * 5)),
												 BitConverter.ToSingle(data, (sizeof(ushort) * 2) + (sizeof(float) * 6)));

			return new SerializationResult<object>(true, new CreationPacket(id, registryId, position, rotation));
		}
	}
}
