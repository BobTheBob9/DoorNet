using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

namespace DoorNet.Server.GameLogic
{
	using static GameServer;

	/// <summary>
	/// Static class for handling the sending of chat messages
	/// </summary>
	[DoorNetModule(Side.Server)]
	public static class ChatManager
	{
		public static NetHandler ChatHandler;

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			//init a handler for chat and listen for messages
			ChatHandler = NetworkManager.Handle("DoorNet::Chat");
			ChatHandler.Listen((byte[] data, NetHandle sender) 
				=> Send(Encoding.Unicode.GetString(data), sender.Name));
		}

		/// <summary>
		/// Broadcasts a chat message to all clients
		/// </summary>
		/// <param name="message">The message to be sent</param>
		/// <param name="sender">The message's displayed sender, "SYSTEM" by default</param>
		public static void Send(string message, string sender = "SYSTEM")
		{
			//construct, log and send a message to all clients
			string fullMessage = $"[{sender}] {message}";
			Logger.Info(fullMessage);

			byte[] data = Encoding.Unicode.GetBytes(fullMessage);
			ChatHandler.Broadcast(data, SendMode.Tcp);
		}
	}
}
