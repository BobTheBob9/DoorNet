using System;

namespace DoorNet.Shared.Networking
{
	[Serializable]
	internal struct PacketListSyncronisationPacket
	{
		public string[] Names;
		public bool[] NecessaryIndexes; 
	}
}
