using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;

namespace DoorNet.Shared.Networking
{
	public class IDMappedDataChannel : NetChannel
	{
		public struct Container
		{
			public ushort ID;
			public object Data;

			public Container(ushort id, object data)
			{
				ID = id;
				Data = data;
			}
		}

		public NetChannel InheritedChannel { get; private set; }
		 
		private IDMappedDataChannel() { }
		public IDMappedDataChannel(NetChannel inheritedChannel)
		{
			InheritedChannel = inheritedChannel;
		}

		public override SerializationResult<byte[]> SerializeData(object objData, NetClient client)
		{
			if (!(objData is Container))
				return SerializationResult<byte[]>.Failure;

			Container data = (Container)objData;

			byte[] idData = BitConverter.GetBytes(data.ID);
			SerializationResult<byte[]> inheritedData = InheritedChannel.SerializeData(data.Data, client);

			if (!inheritedData.ShouldSend)
				return SerializationResult<byte[]>.Failure;

			byte[] fullData = new byte[sizeof(ushort) + inheritedData.Data.Length];
			Array.Copy(idData, 0, fullData, 0, idData.Length);
			Array.Copy(inheritedData.Data, 0, idData, sizeof(ushort), inheritedData.Data.Length);

			return new SerializationResult<byte[]>(true, fullData);
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length < sizeof(ushort))
				return SerializationResult<object>.Failure;

			ushort id = BitConverter.ToUInt16(data, 0);

			byte[] dataWithoutID = new byte[data.Length - sizeof(ushort)];
			Array.Copy(data, sizeof(ushort), dataWithoutID, 0, dataWithoutID.Length);

			return InheritedChannel.DeserializeData(dataWithoutID, client);
		}
	}
}
