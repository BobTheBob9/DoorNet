using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;

namespace DoorNet.Shared.Networking
{
	public class StringChannel : NetChannel
	{
		public Encoding Encoding { get; private set; }

		private StringChannel() { }
		public StringChannel(Encoding encoding)
		{
			Encoding = encoding;
		}

		public override SerializationResult<byte[]> SerializeData(object data, NetClient client)
		{
			if (!(data is string))
				return SerializationResult<byte[]>.Failure;

			byte[] encoded = Encoding.GetBytes((string)data);
			return new SerializationResult<byte[]>(true, encoded);
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			string decoded = Encoding.GetString(data);
			return new SerializationResult<object>(true, decoded);
		}
	}
}
