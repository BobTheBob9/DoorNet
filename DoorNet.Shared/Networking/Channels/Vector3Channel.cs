using System;
using UnityEngine;
using BobNet;

namespace DoorNet.Shared.Networking
{
	public class Vector3Channel : NetChannel
	{
		public override SerializationResult<byte[]> SerializeData(object objData, NetClient client)
		{
			if (!(objData is Vector3))
				return new SerializationResult<byte[]>(false);

			Vector3 data = (Vector3)objData;

			byte[] xData = BitConverter.GetBytes(data.x);
			byte[] yData = BitConverter.GetBytes(data.y);
			byte[] zData = BitConverter.GetBytes(data.z);

			byte[] vecData = new byte[12];
			Array.Copy(xData, 0, vecData, 0, xData.Length);
			Array.Copy(yData, 0, vecData, sizeof(float), xData.Length);
			Array.Copy(zData, 0, vecData, sizeof(float) * 2, xData.Length);

			return new SerializationResult<byte[]>(true, vecData);
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length != sizeof(float) * 3)
				return new SerializationResult<object>(false);

			float x = BitConverter.ToSingle(data, 0);
			float y = BitConverter.ToSingle(data, sizeof(float));
			float z = BitConverter.ToSingle(data, sizeof(float) * 2);

			return new SerializationResult<object>(true, new Vector3(x, y, z));
		}
	}
}
