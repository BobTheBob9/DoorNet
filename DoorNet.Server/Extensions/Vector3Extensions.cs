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
		/// <summary>
		/// Converts a Vector3 to a byte array
		/// </summary>
		/// <param name="vec">The vector to be converted</param>
		/// <returns>The converted byte array</returns>
		public static byte[] ToByteArray(this Vector3 vec)
		{
			//serialize floats
			byte[] x = BitConverter.GetBytes(vec.x);
			byte[] y = BitConverter.GetBytes(vec.y);
			byte[] z = BitConverter.GetBytes(vec.z);

			//append byte arrays
			return x.Append(y).Append(z);
		}
	}
}
