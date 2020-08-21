using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;

namespace DoorNet.Shared.Networking
{
	public class DateTimeChannel : NetChannel
	{
		public override SerializationResult<byte[]> SerializeData(object objData, NetClient client)
		{
			if (!(objData is DateTime))
				return SerializationResult<byte[]>.Failure;

			DateTime time = (DateTime)objData;
			byte[] data = BitConverter.GetBytes(time.Ticks);

			return new SerializationResult<byte[]>(true, data);
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length != sizeof(long))
				return SerializationResult<object>.Failure;

			long ticks = BitConverter.ToInt64(data, 0);
			DateTime time = new DateTime(ticks);

			return new SerializationResult<object>(true, time);
		}
	}
}
