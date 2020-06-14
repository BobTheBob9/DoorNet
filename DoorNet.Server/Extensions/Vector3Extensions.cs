using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using DoorNet.Shared.Networking.Extensions;

namespace DoorNet.Server.Extensions
{
	public static class Vector3Extensions
	{
		public static byte[] ToByteArray(this Vector3 vec)
		{
			byte[] x = BitConverter.GetBytes(vec.x);
			byte[] y = BitConverter.GetBytes(vec.y);
			byte[] z = BitConverter.GetBytes(vec.z);

			return x.Append(y).Append(z);
		}
	}
}
