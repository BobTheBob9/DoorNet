using System.Net;

namespace DoorNet.Shared.Networking
{
	internal struct UdpThreadPacket
	{
		public byte[] Data;
		public ushort Channel;
		public IPEndPoint Sender;
	}
}
