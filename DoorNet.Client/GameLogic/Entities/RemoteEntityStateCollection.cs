using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace DoorNet.Client.GameLogic
{
	public class RemoteEntityStateCollection<KT, VT> where KT : Component
	{
		private Dictionary<KT, VT> ObjectDictionary = new Dictionary<KT, VT>();

		public void Add(RemoteEntity entity, KT key, VT value)
		{
			ObjectDictionary.Add(key, value);
			entity.OnDestruction += () => ObjectDictionary.Remove(key);
		}

		public VT GetState(KT key)
		 => ObjectDictionary[key];

		public VT this[KT key]
		 => GetState(key);

		public bool Contains(KT key)
		=> ObjectDictionary.ContainsKey(key);
	}
}
