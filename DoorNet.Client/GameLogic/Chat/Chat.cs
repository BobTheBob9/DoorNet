using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;
using DoorNet.Client;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

namespace DoorNet.Client.GameLogic
{
	using static GameClient;

	[DoorNetModule(Side.Client)]
	public static class Chat
	{
		public delegate void OnRecieveMessageHandler(string message);
		public static event OnRecieveMessageHandler OnRecieveMessage;

		public static NetChannel ChatChannel;

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			ChatChannel = NetworkManager.CreateChannel("DoorNet::Chat", new StringChannel(Encoding.Unicode));
			ChatChannel.OnRecieveSerialized += (object data, NetClient client)
			 => OnRecieveMessage?.Invoke((string)data);
		}

		public static void Send(string message)
		 => ChatChannel.SendSerialized(SendMode.Tcp, message, NetworkManager.Server);
	}
}
