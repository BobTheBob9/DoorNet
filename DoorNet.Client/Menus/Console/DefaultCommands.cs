using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorNet.Client.Menus
{
	public static partial class CustomConsole
	{
		internal static void RegisterDefaultCommands()
		{
			RegisterCommand("connect", Connect);
			RegisterCommand("createserver", CreateServer);
		}

		private static void Connect(string[] args)
		{
			
		}

		private static void CreateServer(string[] args)
		 => GameClient.HostLocalServer();
	}
}
