using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using Reactor.API.Runtime.Patching;
using Reactor.API.Logging;

using UnityEngine;

using Logger = Reactor.API.Logging.Log;

namespace DoorNet.Client
{
	[ModEntryPoint(ModID)]
	public class Mod : MonoBehaviour
	{
		public const string ModID = "com.github.BobTheBob9/DoorNet.Client";

		public static IManager Manager;
		public static Logger Logger => LogManager.GetForCurrentAssembly();

		public void Initialise(IManager manager)
		{
			Manager = manager;

			//note: these might end up being reworked to use our own harmonyinstance rather than centrifuge's simple patching methods to control whether certain patches do or don't get applied
			RuntimePatcher.RunTranspilers();
			RuntimePatcher.AutoPatch();

			Logger.Info("Initialised DoorNet.Client successfully!");
		}
	}
}
