using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Harmony;
using BobNet;
using DoorNet.Shared.Modules;
using DoorNet.Shared.Prefabs;

namespace DoorNet.Client.GameLogic
{
	using static GameClient;

	[DoorNetModule(Side.Client)]
	internal static class InfantryPatches
	{
		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			RemoteEntityRegistry.Instance.CreateEntry("DoorNet::Infantry", GTTODPrefabs.GetPrefab("Infantry"));
			RemoteEntityRegistry.Instance.CreateEntry("DoorNet::Brute", GTTODPrefabs.GetPrefab("Brute"));

			if (!ConnectedToLocalServer)
			{
				//do patches to strip away serverside behaviour
			}
		}
	}
}
