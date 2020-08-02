using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;
using UnityEngine;
using DoorNet.Shared.Modules;

namespace DoorNet.Client.GameLogic
{
	[DoorNetModule(Side.Client)]
	public class RemotePlayer : MonoBehaviour
	{
		[DoorNetModuleInitialiser]
		private static void Initialise()
		{
			GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			prefab.SetActive(false);
			prefab.AddComponent<RemotePlayer>();

			RemoteEntityRegistry.Instance.CreateEntry("DoorNet::Player", prefab);
		}
	}
}
