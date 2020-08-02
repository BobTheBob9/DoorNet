using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;

namespace DoorNet.Shared.Networking
{
	public class BooleanChannel : NetChannel
	{
		public override SerializationResult<byte[]> SerializeData(object data, NetClient client)
		{
			if (!(data is bool))
				return SerializationResult<byte[]>.Failure;

			return new SerializationResult<byte[]>(true, ((bool)data) ? new byte[] { 1 } : new byte[] { 0 });
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length != 1 || (data.Length == 1 && data[0] != 0 && data[0] != 1))
				return SerializationResult<object>.Failure;

			return new SerializationResult<object>(true, data[0] == 1);
		}
	}
}
