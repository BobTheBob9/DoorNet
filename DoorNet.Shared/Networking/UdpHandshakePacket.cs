using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoorNet.Shared.Networking
{
	public struct UdpHandshakePacket
	{
		public bool Success;
		public Guid ID;
	}
}
