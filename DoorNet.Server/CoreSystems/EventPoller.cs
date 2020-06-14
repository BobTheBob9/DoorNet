using UnityEngine;

namespace DoorNet.Server
{
	public class EventPoller : MonoBehaviour
	{
		private void Update()
		{
			GameServer.NetworkManager.PollEvents();
		}
	}
}
