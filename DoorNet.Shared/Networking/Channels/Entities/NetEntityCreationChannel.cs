using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;

namespace DoorNet.Shared.Networking
{
	public class NetEntityCreationChannel : NetChannel
	{
		public struct CreationPacket
		{
			public ushort ID;
			public ushort EntityRegistryID;

			public CreationPacket(ushort id, ushort registryId)
			{
				ID = id;
				EntityRegistryID = registryId;
			}
		}

		public override SerializationResult<byte[]> SerializeData(object objData, NetClient client)
		{
			if (!(objData is CreationPacket))
				return SerializationResult<byte[]>.Failure;

			CreationPacket data = (CreationPacket)objData;

			byte[] fullData = new byte[sizeof(ushort) * 2];
			Array.Copy(BitConverter.GetBytes(data.ID), 0, fullData, 0, sizeof(ushort));
			Array.Copy(BitConverter.GetBytes(data.EntityRegistryID), 0, fullData, sizeof(ushort), sizeof(ushort));

			return new SerializationResult<byte[]>(true, fullData);
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length != sizeof(ushort) * 2)
				return SerializationResult<object>.Failure;

			ushort id = BitConverter.ToUInt16(data, 0);
			ushort registryId = BitConverter.ToUInt16(data, sizeof(ushort));

			return new SerializationResult<object>(true, new CreationPacket(id, registryId));
		}
	}
}
