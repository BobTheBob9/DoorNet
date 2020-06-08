using System.Net.Sockets;

namespace DoorNet.Shared.Networking
{
	public struct TcpThreadPacket
	{
		public byte[] Data;
		public ushort Channel;
		public TcpClient Client;
	}
}
