using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DoorNet.Shared.Networking
{
	public class NetHandle
	{
		public bool IsConnectedHost { get; internal set; }
		public string Name;

		//client/server stuff
		internal bool InUse = true;
		internal IPEndPoint EP;
		internal bool HasUdp = false;
		internal Guid UdpHandshakeGuid;

		//server only stuff
		internal TcpClient Client;
		internal Thread ClientPollThread;
	}
}
