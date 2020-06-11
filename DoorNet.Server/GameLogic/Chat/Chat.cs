using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DoorNet.Server.CoreSystems;
using DoorNet.Shared.Networking;

using static DoorNet.Server.DoorNetServer;

namespace DoorNet.Server.GameLogic
{
	public class ChatManager : DoorNetInitialised
	{
		public static NetHandler ChatHandler;

		public static void Send(string message, string sender = "SYSTEM")
		{
			string fullMessage = $"[{sender}] {message}";
			Logger.Info(fullMessage);

			byte[] data = Encoding.Unicode.GetBytes(fullMessage);
			ChatHandler.Broadcast(data, SendMode.Tcp);
		}

		public override void Initialise()
		{
			ChatHandler = NetworkManager.Handle("DoorNet::Chat");
			ChatHandler.Listen((byte[] data, NetHandle sender) => Send(Encoding.Unicode.GetString(data), sender.Name));
		}
	}
}
