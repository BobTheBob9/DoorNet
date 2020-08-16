using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using UnityEngine;
using DoorNet.Shared.Prefabs;

namespace DoorNet.Client.Menus
{
	public static partial class CustomConsole
	{
		private static Task<IPHostEntry> DnsTask;
		private static int ConnectingPort;

		internal static void RegisterDefaultCommands()
		{
			RegisterCommand("connect", Connect);
			RegisterCommand("createserver", CreateServer);
			RegisterCommand("dumpprefabs", DumpPrefabs);
			RegisterCommand("instantiate", Instantiate);
		}

		private static void Connect(string[] args)
		{
			if (args.Length < 0)
				return;

			string epString = args[0];
			string ipString;
			if (epString.Contains(':'))
			{
				string[] split = epString.Split(':');
				ipString = split[0];
				ConnectingPort = int.Parse(split[1]);
			}
			else
			{
				ipString = epString;
				ConnectingPort = 44444;
			}

			DnsTask = Dns.GetHostEntryAsync(ipString);
		}

		private static void CheckDnsTask()
		{
			if (DnsTask?.IsCompleted ?? false && DnsTask.Status == TaskStatus.RanToCompletion)
			{
				foreach (IPAddress address in DnsTask.Result.AddressList)
				{
					if (address.AddressFamily == AddressFamily.InterNetworkV6/* && !Socket.OSSupportsIPv6*/)
						continue;

					GameClient.Connect(new IPEndPoint(address, ConnectingPort));
					DnsTask = null;
					break;
				}
			}
		}

		private static void CreateServer(string[] args)
		 => GameClient.HostLocalServer();

		//testing commands (not actually multiplayer-related)

		private static void DumpPrefabs(string[] args)
		{
			string[] names = GTTODPrefabs.GetNames();
			StringBuilder sb = new StringBuilder();
			foreach (string name in names)
			{
				sb.Append(name);
				sb.Append(": [");
				GameObject obj = GTTODPrefabs.GetPrefab(name);
				if (obj != null)
				{
					foreach (Component behaviour in obj.GetComponents(typeof(Component)))
					{
						sb.Append(behaviour.GetType().Name);
						sb.Append(", ");
					}
				}
				sb.AppendLine();
			}

			System.IO.File.WriteAllText("prefabdump.txt", sb.ToString());
		}

		private static void Instantiate(string[] args)
		{
			if (args.Length < 1)
				return;

			GameObject.Instantiate(GTTODPrefabs.GetPrefab(args[0]), GameManager.GM.Player.transform.position, GameManager.GM.transform.rotation);
		}
	}
}
