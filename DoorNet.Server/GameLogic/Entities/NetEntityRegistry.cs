using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using DoorNet.Shared.Registries;

namespace DoorNet.Server.GameLogic
{
	public class NetEntityRegistry : Registry<GameObject>
	{
		public class PrefabRegistryContainer : MonoBehaviour
		{
			public int ID;
		}

		public static NetEntityRegistry Instance;

		public override void CreateEntry(string id, GameObject item)
		{
			item.AddComponent<PrefabRegistryContainer>().ID = _Entries.Count;
			base.CreateEntry(id, item);
		}
	}
}
