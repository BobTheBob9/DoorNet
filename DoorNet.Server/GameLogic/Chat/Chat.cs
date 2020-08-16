using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BobNet;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Networking;

namespace DoorNet.Server.GameLogic
{
	using static GameServer;

	/// <summary>
	/// Static class for handling the sending of chat messages
	/// </summary>
	[DoorNetModule(Side.Server)]
	public static class Chat
	{
		public static NetChannel ChatChannel;

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			//create a channel for chat and listen for messages
			ChatChannel = NetworkManager.CreateChannel("DoorNet::Chat", new StringChannel(Encoding.Unicode));
			ChatChannel.OnRecieveSerialized += (object data, NetClient client)
			 => Send((string)data, client.GetPlayer().Name);
		}

		/// <summary>
		/// Broadcasts a chat message to all clients
		/// </summary>
		/// <param name="message">The message to be sent</param>
		/// <param name="sender">The message's displayed sender, "SYSTEM" by default</param>
		public static void Send(string message, string sender = "SYSTEM")
		{
			//construct, log and send a message to all clients
			string fullMessage;

			if (sender.Length > 0)
				fullMessage = $"[{sender}] {message}";
			else
				fullMessage = message;

			Logger.Info(fullMessage);
			ChatChannel.BroadcastSerialized(SendMode.Tcp, fullMessage);
		}

		public static void SendTo(NetClient reciever, string message, string sender = "SYSTEM")
		{
			string fullMessage;

			if (sender.Length > 0)
				fullMessage = $"[{sender} => {reciever.GetPlayer().Name}] {message}";
			else
				fullMessage = message;

			Logger.Info(fullMessage);
			ChatChannel.SendSerialized(SendMode.Tcp, fullMessage, reciever);
		}
	}
}
