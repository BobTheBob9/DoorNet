using System;

namespace DoorNet.Shared.Networking
{
	public partial class NetManager
	{
		public class ClientEventContainer
		{
			public delegate void OnDisconnectHandler(bool causedBySelf, string reason);
	
			public event Action OnConnect;
			public event OnDisconnectHandler OnDisconnect;

			internal void InvokeOnConnect() => OnConnect?.Invoke();
			internal void InvokeOnDisconnect(bool causedBySelf, string reason) => OnDisconnect?.Invoke(causedBySelf, reason);
		}
	}
}
