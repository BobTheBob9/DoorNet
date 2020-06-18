using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using DoorNet.Server.Extensions;
using DoorNet.Shared.Networking;
using DoorNet.Shared.Networking.Extensions;

namespace DoorNet.Server.GameLogic
{
	/// <summary>
	/// Represents a GameObject syncronised across clients and servers
	/// </summary>
	public class NetEntity
	{
		public ushort ID;
		public GameObject GameObject;

		internal NetEntity(ushort id, GameObject obj)
		{
			ID = id;
			GameObject = obj;
		}

		/// <summary>
		/// Updates the position of a networked entity, broadcasting the update to clients
		/// </summary>
		/// <param name="pos">The new position of the entity</param>
		/// <param name="rot">The new rotation of the entity</param>
		public void UpdatePosition(Vector3 pos, Quaternion rot)
		{
			//construct packet
			byte[] idData = BitConverter.GetBytes(ID);

			byte[] posData = pos.ToByteArray();
			byte[] rotData = rot.eulerAngles.ToByteArray();

			byte[] fullData = idData.Append(posData).Append(rotData);

			//broadcast
			NetEntityManager.EntityPositionHandler.Broadcast(fullData, SendMode.Udp);
		}
	}
}
