using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using Reactor.API.Runtime.Patching;
using Reactor.API.Logging;

using UnityEngine;
using BobNet;
using DoorNet.Shared.Modules;
using DoorNet.Client.Menus;

namespace DoorNet.Client
{
	[ModEntryPoint(ModID)]
	public class Mod : MonoBehaviour
	{
		public const string ModID = "com.github.BobTheBob9/DoorNet.Client";

		public static Log Logger;
		public static IManager Manager;

		public void Initialise(IManager manager)
		{
			Manager = manager;
			Logger = LogManager.GetForCurrentAssembly();

			CustomConsole.PatchConsole();
			CustomConsole.RegisterDefaultCommands();

			Logger.Info("Initialised DoorNet.Client successfully!");
		}
	}
}
