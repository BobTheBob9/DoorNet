using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace DoorNet.Server.GameLogic
{
	public static class AIHelpers
	{
		public static NetPlayer GetClosestPlayer(Vector3 currentPosition)
		{
			if (NetPlayer.Players.Count == 0)
				return null;

			NetPlayer closestPlayer = NetPlayer.Players[0];
			float closestDistance = Vector3.Distance(currentPosition, closestPlayer.transform.position);
			for (int i = 1; i < NetPlayer.Players.Count; i++)
			{
				float dist = Vector3.Distance(currentPosition, NetPlayer.Players[i].transform.position);
				if (dist < closestDistance)
				{
					closestDistance = dist;
					closestPlayer = NetPlayer.Players[i];
				}
			}

			return closestPlayer;
		}


	}
}
