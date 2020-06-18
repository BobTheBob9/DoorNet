using UnityEngine;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

namespace DoorNet.Server
{
	[DoorNetModule(Side.Server)]
	public class EventPoller : MonoBehaviour
	{
		public static EventPoller Poller;

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			GameObject gm = new GameObject("DoorNet::Testing::EventPoller");
			DontDestroyOnLoad(gm);
			Poller = gm.AddComponent<EventPoller>();
		}

		private void Update()
		{
			GameServer.NetworkManager.PollEvents();
		}
	}
}
