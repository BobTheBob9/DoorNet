using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;

namespace DoorNet.Shared.Networking
{
	public class UShortChannel : NetChannel
	{
		public override SerializationResult<byte[]> SerializeData(object data, NetClient client)
		{
			if (!(data is ushort))
				return SerializationResult<byte[]>.Failure;

			return new SerializationResult<byte[]>(true, BitConverter.GetBytes((ushort)data));
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length != sizeof(ushort))
				return SerializationResult<object>.Failure;

			return new SerializationResult<object>(true, BitConverter.ToUInt16(data, 0));
		}
	}
}
