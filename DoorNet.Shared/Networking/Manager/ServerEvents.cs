using System;

namespace DoorNet.Shared.Networking
{
	public partial class NetManager
	{
		public class ServerEventContainer
		{
			public delegate void OnClientConnectHandler(NetHandle client);
			public delegate void OnClientDisconnectHandler(NetHandle client, string reason);

			public event Action OnServerStart;
			public event Action OnServerClose;
			public event OnClientConnectHandler OnClientConnect;
			public event OnClientDisconnectHandler OnClientDisconnect;

			internal void InvokeOnServerStart() => OnServerStart?.Invoke();
			internal void InvokeOnServerClose() => OnServerClose?.Invoke();
			internal void InvokeOnClientConnect(NetHandle client) => OnClientConnect?.Invoke(client);
			internal void InvokeOnClientDisconnect(NetHandle client, string reason) => OnClientDisconnect?.Invoke(client, reason);
		}
	}
}