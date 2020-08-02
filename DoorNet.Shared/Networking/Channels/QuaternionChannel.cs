using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using BobNet;

namespace DoorNet.Shared.Networking
{
	public class QuaternionChannel : NetChannel
	{
		public override SerializationResult<byte[]> SerializeData(object objData, NetClient client)
		{
			if (!(objData is Quaternion))
				return SerializationResult<byte[]>.Failure;

			Quaternion data = (Quaternion)objData;

			byte[] xData = BitConverter.GetBytes(data.x);
			byte[] yData = BitConverter.GetBytes(data.y);
			byte[] zData = BitConverter.GetBytes(data.z);
			byte[] wData = BitConverter.GetBytes(data.w);
			
			byte[] vecData = new byte[sizeof(float) * 4];
			Array.Copy(xData, 0, vecData, 0, xData.Length);
			Array.Copy(yData, 0, vecData, sizeof(float), yData.Length);
			Array.Copy(zData, 0, vecData, sizeof(float) * 2, zData.Length);
			Array.Copy(wData, 0, vecData, sizeof(float) * 3, wData.Length);

			return new SerializationResult<byte[]>(true, vecData);
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length != sizeof(float) * 4)
				return SerializationResult<object>.Failure;

			float x = BitConverter.ToSingle(data, 0);
			float y = BitConverter.ToSingle(data, sizeof(float));
			float z = BitConverter.ToSingle(data, sizeof(float) * 2);
			float w = BitConverter.ToSingle(data, sizeof(float) * 3);

			return new SerializationResult<object>(true, new Quaternion(x, y, z, w));
		}
	}
}
