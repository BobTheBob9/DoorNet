using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;
using UnityEngine;

namespace DoorNet.Shared.Networking
{
	public class ShootingChannel : NetChannel
	{
		public struct ShootingPacket
		{
			public bool IsSecondary;
			public Quaternion Rotation;

			public ShootingPacket(bool secondary, Quaternion rotation)
			{
				IsSecondary = secondary;
				Rotation = rotation;
			}
		}

		public override SerializationResult<byte[]> SerializeData(object objData, NetClient client)
		{
			if (!(objData is ShootingPacket))
				return SerializationResult<byte[]>.Failure;

			ShootingPacket packet = (ShootingPacket)objData;

			Quaternion rot = packet.Rotation;

			byte secondaryData = (byte)(packet.IsSecondary ? 1 : 0);

			byte[] xData = BitConverter.GetBytes(rot.x);
			byte[] yData = BitConverter.GetBytes(rot.y);
			byte[] zData = BitConverter.GetBytes(rot.z);
			byte[] wData = BitConverter.GetBytes(rot.w);

			byte[] data = new byte[sizeof(byte) + sizeof(float) * 4];

			data[0] = secondaryData;
			Array.Copy(xData, 0, data, sizeof(byte), xData.Length);
			Array.Copy(yData, 0, data, sizeof(byte) + sizeof(float), yData.Length);
			Array.Copy(zData, 0, data, sizeof(byte) + sizeof(float) * 2, zData.Length);
			Array.Copy(wData, 0, data, sizeof(byte) + sizeof(float) * 3, wData.Length);

			return new SerializationResult<byte[]>(true, data);
		}

		public override SerializationResult<object> DeserializeData(byte[] data, NetClient client)
		{
			if (data.Length != sizeof(byte) + sizeof(float) * 4 ||
				(data.Length == sizeof(byte) + sizeof(float) * 4 && data[0] != 0 && data[0] != 1))
				return SerializationResult<object>.Failure;

			bool secondary = data[0] == 1;
			float x = BitConverter.ToSingle(data, sizeof(byte));
			float y = BitConverter.ToSingle(data, sizeof(byte) + sizeof(float));
			float z = BitConverter.ToSingle(data, sizeof(byte) + sizeof(float) * 2);
			float w = BitConverter.ToSingle(data, sizeof(byte) + sizeof(float) * 3);

			return new SerializationResult<object>(true, new ShootingPacket(secondary, new Quaternion(x, y, z, w)));
		}
	}
}
