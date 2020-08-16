using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using Reactor.API.Runtime.Patching;
using Reactor.API.Logging;

using UnityEngine;
using DoorNet.Client.Menus;
using DoorNet.Shared.Prefabs;

namespace DoorNet.Client
{
	[ModEntryPoint(ModID)]
	public class Mod : MonoBehaviour
	{
		public const string ModID = "com.github.BobTheBob9/DoorNet.Client";

		public static Log Logger;
		public static IManager Manager;

		public void Initialize(IManager manager)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(2);

			Manager = manager;
			Logger = LogManager.GetForCurrentAssembly();

			CustomConsole.PatchConsole();
			CustomConsole.RegisterDefaultCommands();

			GTTODPrefabs.Initialise();

			Logger.Info("Initialised DoorNet.Client successfully!");
		}
	}
}
