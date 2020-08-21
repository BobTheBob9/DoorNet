using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BobNet;
using UnityEngine;
using DoorNet.Shared.Registries;

namespace DoorNet.Client.GameLogic
{
	public class RemoteEntityRegistry : Registry<GameObject>
	{
		public class PrefabRegistryContainer : MonoBehaviour
		{
			public int ID;
		}

		public static RemoteEntityRegistry Instance;

		internal static NetChannel SyncChannel;

		public override void CreateEntry(string id, GameObject item)
		{
			item.SetActive(false);
			item.AddComponent<PrefabRegistryContainer>().ID = _Entries.Count;
			base.CreateEntry(id, item);
		}
	}
}
