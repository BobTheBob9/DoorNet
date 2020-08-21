using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using BobNet;
using DoorNet.Client;
using DoorNet.Shared.Modules;
using System.Collections.ObjectModel;

namespace DoorNet.Client.GameLogic
{
	using static GameClient;

	[DoorNetModule(Side.Client)]
	public class ChatGui : MonoBehaviour
	{
		public static ChatGui Instance { get; private set; }

		public static int DisplayCount
		{
			get => _DisplayCount;
			set
			{
				_DisplayCount = value;
				RefreshDisplayedMessages();
			}
		}
		public static ReadOnlyCollection<string> Messages
		{
			get => _Messages.AsReadOnly();
		}

		private static string[] DisplayedMessages;
		private static List<string> _Messages = new List<string>();
		private static int _DisplayCount;

		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			Instance = new GameObject().AddComponent<ChatGui>();
			Chat.OnRecieveMessage += AddMessage;
			DisplayCount = 10;
		}

		public static void AddMessage(string message)
		{
			Logger.Info(message);
			_Messages.Add(message);
			RefreshDisplayedMessages();
		}

		private static void RefreshDisplayedMessages()
		{
			DisplayedMessages = new string[Messages.Count > DisplayCount ? DisplayCount : Messages.Count];
			Array.Copy(_Messages.ToArray(), Messages.Count - DisplayedMessages.Length, DisplayedMessages, 0, DisplayedMessages.Length);
		}

		private void OnGUI()
		{
			for (int i = 0; i < DisplayedMessages.Length; i++)
			{
				GUI.Label(new Rect(1, 20 * (i + 2) , Screen.width, 20), DisplayedMessages[i]);
			}
		}
	}
}
