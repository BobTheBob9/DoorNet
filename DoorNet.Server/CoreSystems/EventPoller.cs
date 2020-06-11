using UnityEngine;

namespace DoorNet.Server.CoreSystems
{
	public class EventPoller : MonoBehaviour
	{
		private void Update()
		{
			DoorNetServer.NetworkManager.PollEvents();
		}
	}
}
